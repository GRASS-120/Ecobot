using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;
using GUI.Programming.Windows.Slots;
using Bot.Programming.Nodes.Base;
using Bot.Programming.Nodes.Slots;

namespace GUI.Programming.Windows.Nodes
{
    public class NodeController : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        private RectTransform _rectTransform;
        private RectTransform _parentRect;
        private Canvas _canvas;
        private Vector2 _offset;
        private bool _isDragging;

        [Header("Connections Setup")]
        [SerializeField] private RectTransform connectionsContainer;
        [SerializeField] private UIBezierConnection connectionPrefab;

        [Header("Selection")]
        [SerializeField] private Outline selectionOutline;

        [Header("Permissions")]
        [SerializeField] private bool canBeDeleted = true;

        private bool _isSelected;
        public static NodeController SelectedNode { get; private set; }

        private UIBezierConnection _activeConnection;
        private SlotController _activeOutputSlot;
        private RectTransform _tempEndPoint;

        private readonly List<SlotController> _inputSlots = new();
        private readonly List<SlotController> _outputSlots = new();

        private class ConnectionInfo
        {
            public SlotController Output;
            public SlotController Input;
            public UIBezierConnection Line;
        }

        private readonly List<ConnectionInfo> _connections = new();
        private static readonly List<ConnectionInfo> _allConnections = new();

        public static NodeController ActiveConnectionNode { get; private set; }
        public bool HasActiveConnection => _activeConnection != null;

        // === Привязка к программной ноде ===
        public ProgNodeBase LinkedProgramNode { get; private set; }

        public void Initialize(ProgNodeBase programNode)
        {
            LinkedProgramNode = programNode;
        }

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _parentRect = _rectTransform.parent as RectTransform;
            _canvas = GetComponentInParent<Canvas>();

            // === Найти контейнер соединений через родителя ===
            if (connectionsContainer == null)
            {
                var nodesContainer = GetComponentInParent<NodesContainer>();
                if (nodesContainer != null)
                {
                    connectionsContainer = nodesContainer.GetConnectionsContainer();
                }
                else
                {
                    Debug.LogWarning($"[NodeController] No NodesContainer found for {name}, connections will not work!");
                }
            }

            FindAllSlots();
            SetupSlotsEvents();

            if (selectionOutline != null)
                selectionOutline.enabled = false;
        }
        
        public void SetConnectionsContainer(RectTransform container)
        {
            connectionsContainer = container;
        }

        private void FindAllSlots()
        {
            var slots = GetComponentsInChildren<SlotController>();
            foreach (var slot in slots)
            {
                if (slot.Direction == SlotController.SlotDirection.Input)
                    _inputSlots.Add(slot);
                else
                    _outputSlots.Add(slot);
            }
        }

        private void SetupSlotsEvents()
        {
            foreach (var slot in _inputSlots)
            {
                slot.OnSlotPressed += OnInputSlotPressed;
                slot.OnSlotReleased += OnInputSlotReleased;
            }

            foreach (var slot in _outputSlots)
            {
                slot.OnSlotPressed += OnOutputSlotPressed;
                slot.OnSlotReleased += OnOutputSlotReleased;
            }
        }

        private void Update()
        {
            if (_activeConnection != null)
                UpdateActiveConnectionToMouse();

            // Удаление выделенной ноды (если разрешено)
            if (_isSelected && canBeDeleted && (Input.GetKeyDown(KeyCode.Delete) || Input.GetKeyDown(KeyCode.X)))
            {
                DeleteNode();
            }
        }

        #region Slot Handlers

        private void OnOutputSlotPressed(SlotController outputSlot)
        {
            CancelActiveConnection();
            StartNewConnection(outputSlot);
            ActiveConnectionNode = this;
        }

        private void OnInputSlotPressed(SlotController inputSlot)
        {
            TryCompleteConnection(inputSlot);
        }

        private void OnInputSlotReleased(SlotController inputSlot)
        {
            TryCompleteConnection(inputSlot);
        }

        private void OnOutputSlotReleased(SlotController outputSlot)
        {
        }

        #endregion

        #region Connection Management

        private void StartNewConnection(SlotController outputSlot)
        {
            CancelActiveConnection();
            _activeOutputSlot = outputSlot;
            _activeConnection = Instantiate(connectionPrefab, connectionsContainer);

            _activeConnection.SetContainer(connectionsContainer);
            _activeConnection.SetStartSlot(outputSlot.ConnectionPoint);
            _activeConnection.SetInteractable(false);
        }

        private void TryCompleteConnection(SlotController inputSlot)
        {
            if (ActiveConnectionNode == null || ActiveConnectionNode._activeConnection == null)
                return;

            var activeOutput = ActiveConnectionNode._activeOutputSlot;
            var activeLine = ActiveConnectionNode._activeConnection;

            if (activeOutput != null && activeOutput.CanConnectWith(inputSlot))
            {
                bool alreadyConnected = _allConnections.Exists(c =>
                    (c.Output == activeOutput && c.Input == inputSlot) ||
                    (c.Output == inputSlot && c.Input == activeOutput));

                if (alreadyConnected)
                {
                    ActiveConnectionNode.CancelActiveConnection();
                    return;
                }

                activeLine.SetEndSlot(inputSlot.ConnectionPoint);
                activeLine.SetInteractable(true);
                activeLine.AssociatedOutput = activeOutput;
                activeLine.AssociatedInput = inputSlot;

                var connection = new ConnectionInfo
                {
                    Output = activeOutput,
                    Input = inputSlot,
                    Line = activeLine
                };

                ActiveConnectionNode._connections.Add(connection);
                _allConnections.Add(connection);

                activeOutput.SetConnected(true);
                inputSlot.SetConnected(true);

                TryLinkProgramSlots(activeOutput, inputSlot);

                ActiveConnectionNode.ClearTempEndPoint();
                ActiveConnectionNode._activeConnection = null;
                ActiveConnectionNode._activeOutputSlot = null;
                ActiveConnectionNode = null;
            }
            else
            {
                ActiveConnectionNode.CancelActiveConnection();
            }
        }

        private void TryLinkProgramSlots(SlotController output, SlotController input)
        {
            if (output.LinkedSlot == null || input.LinkedSlot == null)
                return;

            var outSlot = output.LinkedSlot;
            var inSlot = input.LinkedSlot;

            if (outSlot.CanConnect(input.LinkedSlot.Owner))
            {
                outSlot.Connect(input.LinkedSlot.Owner);
                Debug.Log($"[NodeController] Linked flow: {outSlot.SlotName} -> {input.LinkedSlot.SlotName}");
            }
            else if (outSlot is ProgNodeDataSlot<object> dataOut && inSlot is ProgNodeDataSlot<object> dataIn)
            {
                dataIn.ConnectToDataSlot(dataOut);
                Debug.Log($"[NodeController] Linked data: {dataOut.SlotName} -> {dataIn.SlotName}");
            }
        }

        public void RemoveConnection(UIBezierConnection line)
        {
            _connections.RemoveAll(c => c.Line == line);
            _allConnections.RemoveAll(c => c.Line == line);
        }

        public static void RemoveGlobalConnection(UIBezierConnection line)
        {
            var toRemove = _allConnections.Find(c => c.Line == line);
            if (toRemove != null)
            {
                if (toRemove.Output != null) toRemove.Output.SetConnected(false);
                if (toRemove.Input != null) toRemove.Input.SetConnected(false);
                _allConnections.Remove(toRemove);
            }
        }

        public SlotController GetActiveOutputSlot() => _activeOutputSlot;

        public void CancelActiveConnection()
        {
            if (_activeConnection != null)
            {
                Destroy(_activeConnection.gameObject);
                _activeConnection = null;
                _activeOutputSlot = null;
                ClearTempEndPoint();
            }

            if (ActiveConnectionNode == this)
                ActiveConnectionNode = null;
        }

        private void UpdateActiveConnectionToMouse()
        {
            if (_activeConnection == null) return;

            if (_tempEndPoint == null)
            {
                var go = new GameObject("TempEndPoint", typeof(RectTransform));
                _tempEndPoint = go.GetComponent<RectTransform>();
                _tempEndPoint.SetParent(connectionsContainer, false);
                _activeConnection.SetEndSlot(_tempEndPoint);
            }

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                connectionsContainer,
                Input.mousePosition,
                null,
                out Vector2 localPoint);

            _tempEndPoint.anchoredPosition = localPoint;
        }

        private void ClearTempEndPoint()
        {
            if (_tempEndPoint != null)
            {
                Destroy(_tempEndPoint.gameObject);
                _tempEndPoint = null;
            }
        }

        #endregion

        #region Selection Logic

        private void ToggleSelection()
        {
            if (_isSelected)
                Deselect();
            else
                Select();
        }

        private void Select()
        {
            if (SelectedNode != null && SelectedNode != this)
                SelectedNode.Deselect();

            _isSelected = true;
            SelectedNode = this;

            if (selectionOutline != null)
                selectionOutline.enabled = true;
        }

        private void Deselect()
        {
            _isSelected = false;

            if (SelectedNode == this)
                SelectedNode = null;

            if (selectionOutline != null)
                selectionOutline.enabled = false;
        }

        private void DeleteNode()
        {
            if (!canBeDeleted)
                return;

            // === Удаляем активное соединение, если оно тянется ===
            if (_activeConnection != null)
            {
                CancelActiveConnection();
            }

            // === Удаляем все связи этой ноды (входящие и исходящие) ===
            var connectionsToRemove = new List<ConnectionInfo>();

            foreach (var conn in _allConnections)
            {
                if ((conn.Input != null && _inputSlots.Contains(conn.Input)) ||
                    (conn.Output != null && _outputSlots.Contains(conn.Output)))
                {
                    connectionsToRemove.Add(conn);
                }
            }

            foreach (var conn in connectionsToRemove)
            {
                if (conn.Line != null)
                    Destroy(conn.Line.gameObject);

                _allConnections.Remove(conn);
            }

            _connections.Clear();

            Deselect();
            Destroy(gameObject);
        }

        #endregion

        #region Pointer Handlers

        private bool _isPointerDown;
        private Vector2 _pointerDownPosition;
        private const float DragThreshold = 5f;

        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            _isPointerDown = true;
            _pointerDownPosition = eventData.position;

            _rectTransform.SetAsLastSibling();

            if (_parentRect != null)
            {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    _parentRect,
                    eventData.position,
                    eventData.pressEventCamera,
                    out Vector2 pointerLocalPoint);

                _offset = _rectTransform.anchoredPosition - pointerLocalPoint;
            }

            eventData.Use();
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!_isPointerDown || _parentRect == null)
                return;

            if (!_isDragging && Vector2.Distance(_pointerDownPosition, eventData.position) > DragThreshold)
            {
                _isDragging = true;
            }

            if (_isDragging)
            {
                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                        _parentRect,
                        eventData.position,
                        eventData.pressEventCamera,
                        out Vector2 pointerLocalPoint))
                {
                    _rectTransform.anchoredPosition = pointerLocalPoint + _offset;
                }
            }

            eventData.Use();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (_isPointerDown && !_isDragging)
            {
                ToggleSelection();
            }

            _isPointerDown = false;
            _isDragging = false;
            eventData.Use();
        }

        #endregion
    }
}
