
using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using GUI.Programming.Windows.Slots;

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

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _parentRect = _rectTransform.parent as RectTransform;
            _canvas = GetComponentInParent<Canvas>();

            FindAllSlots();
            SetupSlotsEvents();
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

        #region Pointer Handlers

        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            _rectTransform.SetAsLastSibling();
            _isDragging = true;

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
            if (!_isDragging || _parentRect == null)
                return;

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    _parentRect,
                    eventData.position,
                    eventData.pressEventCamera,
                    out Vector2 pointerLocalPoint))
            {
                _rectTransform.anchoredPosition = pointerLocalPoint + _offset;
            }

            eventData.Use();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _isDragging = false;
            eventData.Use();
        }

        #endregion

        private void OnDestroy()
        {
            foreach (var slot in _inputSlots)
            {
                slot.OnSlotPressed -= OnInputSlotPressed;
                slot.OnSlotReleased -= OnInputSlotReleased;
            }

            foreach (var slot in _outputSlots)
            {
                slot.OnSlotPressed -= OnOutputSlotPressed;
                slot.OnSlotReleased -= OnOutputSlotReleased;
            }

            _allConnections.RemoveAll(c =>
                c.Output == null || c.Input == null);

            if (ActiveConnectionNode == this)
                ActiveConnectionNode = null;
        }
    }
}
