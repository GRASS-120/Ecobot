using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;
using GUI.Programming.Windows.Slots;
using GUI.Programming.Graph;

namespace GUI.Programming.Windows.Nodes
{
    public class NodeController : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        public enum UINodeKind { IdleStart, IdleEnd, FindBuilding, FindOre, MoveTo, Mine, Put }

        private class ConnectionInfo
        {
            public SlotController Output;
            public SlotController Input;
            public UIBezierConnection Line;
        }

        [Header("Node Kind")]
        [SerializeField] private UINodeKind nodeKind = UINodeKind.IdleStart;

        [Header("Connections Setup")]
        [SerializeField] private RectTransform connectionsContainer;
        [SerializeField] private UIBezierConnection connectionPrefab;

        [Header("Selection")]
        [SerializeField] private Outline selectionOutline;

        [Header("Drag / Select Handle")]
        [Tooltip("Перетаскивать И ВЫДЕЛЯТЬ узел можно ТОЛЬКО за этот заголовок. Если пусто — за любое место.")]
        [SerializeField] private RectTransform titleDragHandle;

        [Header("Permissions")]
        [SerializeField] private bool canBeDeleted = true;

        private RectTransform _rectTransform;
        private RectTransform _parentRect;
        private NodeGraphController _graph;

        private bool _isSelected;
        private bool _isDragging;
        private bool _canDragOrSelectThisGesture; // ← один флаг: и для драг, и для select
        private Vector2 _offset;
        
        private UIBezierConnection _activeConnection;
        private SlotController _activeOutputSlot;

        public UIBezierConnection GetCurrentPreviewLine() => _activeConnection;
        public SlotController GetActiveOutputSlot() => _activeOutputSlot;

        private RectTransform _tempEndPoint;

        private readonly List<SlotController> _inputSlots = new();
        private readonly List<SlotController> _outputSlots = new();
        private readonly List<ConnectionInfo> _connections = new();

        public static NodeController ActiveConnectionNode { get; private set; }
        public bool HasActiveConnection => _activeConnection != null;

        private string Pfx => $"[Node:{name}]";

        // ===== Dropdown binding (lazy + logs) =====
        private NodeDropdownBinding _dropdownBinding;
        private NodeDropdownBinding DropdownBinding
        {
            get
            {
                if (_dropdownBinding == null)
                {
                    _dropdownBinding = GetComponentInChildren<NodeDropdownBinding>(true);
                    Debug.Log($"{Pfx} DropdownBinding resolved: {(_dropdownBinding ? _dropdownBinding.name : "NULL")} (includeInactive=true)");
                    if (_dropdownBinding != null) _dropdownBinding.DebugDump();
                }
                return _dropdownBinding;
            }
        }

        public UINodeKind NodeType => nodeKind;
        public IReadOnlyList<SlotController> InputSlots  => _inputSlots;
        public IReadOnlyList<SlotController> OutputSlots => _outputSlots;

        public bool TryGetDropdownTechnical(out string technical)
        {
            Debug.Log($"{Pfx} TryGetDropdownTechnical: ENTER");
            technical = null;
            var binding = DropdownBinding;
            if (binding == null)
            {
                Debug.LogWarning($"{Pfx} TryGetDropdownTechnical: binding=NULL");
                return false;
            }

            bool ok = binding.TryGetTechnical(out technical);
            Debug.Log($"{Pfx} TryGetDropdownTechnical: {(ok ? $"OK '{technical}'" : "FAIL")}");
            return ok;
        }

        public bool TryGetDropdownVisual(out string visual)
        {
            Debug.Log($"{Pfx} TryGetDropdownVisual: ENTER");
            visual = null;
            var binding = DropdownBinding;
            if (binding == null)
            {
                Debug.LogWarning($"{Pfx} TryGetDropdownVisual: binding=NULL");
                return false;
            }

            bool ok = binding.TryGetVisual(out visual);
            Debug.Log($"{Pfx} TryGetDropdownVisual: {(ok ? $"OK '{visual}'" : "FAIL")}");
            return ok;
        }

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _parentRect = _rectTransform.parent as RectTransform;

            // пробуем найти граф через иерархию (может быть null во время Rebuild)
            _graph = GetComponentInParent<NodeGraphController>();
            if (_graph == null)
                Debug.Log($"{Pfx} Awake: No NodeGraphController found (will wait InjectGraph).");

            if (connectionsContainer == null && _graph != null)
                connectionsContainer = _graph.ConnectionsContainer;

            FindAllSlots();
            SetupSlotsEvents();

            _dropdownBinding = GetComponentInChildren<NodeDropdownBinding>(true);
            Debug.Log($"{Pfx} Awake: initial dropdown binding = {(_dropdownBinding ? _dropdownBinding.name : "NULL")}");
            if (_dropdownBinding != null) _dropdownBinding.DebugDump();

            if (selectionOutline != null)
                selectionOutline.enabled = false;

            // регистрируемся только если граф уже найден; иначе — ждём InjectGraph(...)
            _graph?.RegisterNode(this);
        }

        private void OnDestroy()
        {
            _graph?.UnregisterNode(this);
            Debug.Log($"{Pfx} OnDestroy: removed.");
        }

        private void Update()
        {
            if (_activeConnection != null)
                UpdateActiveConnectionToMouse();

            if (_isSelected && canBeDeleted && (Input.GetKeyDown(KeyCode.Delete) || Input.GetKeyDown(KeyCode.X)))
            {
                DeleteNode();
            }
        }

        public Vector2 GetUIPosition()
        {
            var rt = GetComponent<RectTransform>();
            return rt != null ? rt.anchoredPosition : Vector2.zero;
        }

        // === НОВОЕ: безопасная инъекция графа, когда он известен вызывающему ===
        public void InjectGraph(NodeGraphController graph)
        {
            _graph = graph;
            if (connectionsContainer == null && _graph != null)
                connectionsContainer = _graph.ConnectionsContainer;

            // если ещё не были зарегистрированы — зарегистрируемся сейчас
            _graph?.RegisterNode(this);
        }

        private void FindAllSlots()
        {
            var slots = GetComponentsInChildren<SlotController>(true);
            _inputSlots.Clear();
            _outputSlots.Clear();

            foreach (var slot in slots)
            {
                slot.Owner = this;
                if (slot.Direction == SlotController.SlotDirection.Input)
                    _inputSlots.Add(slot);
                else
                    _outputSlots.Add(slot);
            }

            Debug.Log($"{Pfx} FindAllSlots: inputs={_inputSlots.Count} outputs={_outputSlots.Count}");
        }

        private void SetupSlotsEvents()
        {
            foreach (var slot in _inputSlots)
            {
                slot.OnSlotPressed  += OnInputSlotPressed;
                slot.OnSlotReleased += OnInputSlotReleased;
            }

            foreach (var slot in _outputSlots)
            {
                slot.OnSlotPressed  += OnOutputSlotPressed;
                slot.OnSlotReleased += OnOutputSlotReleased;
            }
        }

        // --- Slot handlers ---
        private void OnOutputSlotPressed(SlotController outputSlot)
        {
            CancelActiveConnection();
            StartNewConnection(outputSlot);
            ActiveConnectionNode = this;
        }

        private void OnInputSlotPressed(SlotController inputSlot)  => TryCompleteConnection(inputSlot);
        private void OnInputSlotReleased(SlotController inputSlot) => TryCompleteConnection(inputSlot);
        private void OnOutputSlotReleased(SlotController outputSlot) { }

        private void StartNewConnection(SlotController outputSlot)
        {
            CancelActiveConnection();
            _activeOutputSlot = outputSlot;

            if (connectionPrefab == null || connectionsContainer == null)
            {
                Debug.LogWarning($"{Pfx} StartNewConnection: Missing connection prefab or container.");
                return;
            }

            _activeConnection = Instantiate(connectionPrefab, connectionsContainer);
            _activeConnection.name = $"Connection_{outputSlot.gameObject.name}_Preview";
            _activeConnection.SetContainer(connectionsContainer);
            _activeConnection.SetStartSlot(outputSlot.ConnectionPoint);
            _activeConnection.SetGraph(_graph);
            _activeConnection.SetInteractable(false);

            Debug.Log($"{Pfx} StartNewConnection: preview line spawned from {outputSlot.gameObject.name}");
        }

        private void TryCompleteConnection(SlotController inputSlot)
        {
            if (ActiveConnectionNode == null || ActiveConnectionNode._activeConnection == null)
                return;

            var activeOutput = ActiveConnectionNode._activeOutputSlot;
            var activeLine   = ActiveConnectionNode._activeConnection;

            if (activeOutput != null && activeOutput.CanConnectWith(inputSlot))
            {
                activeLine.SetEndSlot(inputSlot.ConnectionPoint);
                activeLine.SetInteractable(true);
                activeLine.AssociatedOutput = activeOutput;
                activeLine.AssociatedInput  = inputSlot;

                bool added = _graph != null && _graph.RegisterConnection(activeOutput, inputSlot, activeLine);
                if (added)
                {
                    _connections.Add(new ConnectionInfo { Output = activeOutput, Input = inputSlot, Line = activeLine });

                    Debug.Log($"{Pfx} TryCompleteConnection: COMMIT " +
                              $"{activeOutput.gameObject.name}[{activeOutput.Direction}/{activeOutput.ContentType}] → " +
                              $"{inputSlot.gameObject.name}[{inputSlot.Direction}/{inputSlot.ContentType}] " +
                              $"line={activeLine.name}@{activeLine.GetInstanceID()}");

                    ActiveConnectionNode.ClearTempEndPoint();
                    ActiveConnectionNode._activeConnection = null;
                    ActiveConnectionNode._activeOutputSlot = null;
                    ActiveConnectionNode = null;
                }
                else
                {
                    Debug.Log($"{Pfx} TryCompleteConnection: REJECT (duplicate/failed) — destroying preview line.");
                    activeLine.SetInteractable(false);
                    Destroy(activeLine.gameObject);
                    ActiveConnectionNode._activeConnection = null;
                    ActiveConnectionNode._activeOutputSlot = null;
                    ActiveConnectionNode.ClearTempEndPoint();
                    ActiveConnectionNode = null;
                }
            }
            else
            {
                Debug.Log($"{Pfx} TryCompleteConnection: incompatible slots — cancel preview.");
                ActiveConnectionNode.CancelActiveConnection();
            }
        }

        public void CancelActiveConnection()
        {
            if (_activeConnection != null)
            {
                Debug.Log($"{Pfx} CancelActiveConnection: destroying preview line {_activeConnection.name}");
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
            if (_activeConnection == null || connectionsContainer == null) return;

            if (_tempEndPoint == null)
            {
                var go = new GameObject("TempEndPoint", typeof(RectTransform));
                _tempEndPoint = go.GetComponent<RectTransform>();
                _tempEndPoint.SetParent(connectionsContainer, false);
                _activeConnection.SetEndSlot(_tempEndPoint);
                Debug.Log($"{Pfx} UpdateActiveConnectionToMouse: temp end point created.");
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
                Debug.Log($"{Pfx} ClearTempEndPoint: removed temp end point.");
            }
        }

        private void DeleteNode()
        {
            Debug.Log($"{Pfx} DeleteNode: start, local connections={_connections.Count}");
            if (_graph != null && _connections.Count > 0)
            {
                var copy = new List<ConnectionInfo>(_connections);
                foreach (var c in copy)
                {
                    if (c?.Line != null)
                    {
                        Debug.Log($"{Pfx} DeleteNode: request remove line {c.Line.name}@{c.Line.GetInstanceID()}");
                        _graph.RequestRemoveConnection(c.Line);
                    }
                }
            }

            _connections.Clear();
            _graph?.UnregisterNode(this);
            Debug.Log($"{Pfx} DeleteNode: destroying GO");
            Destroy(gameObject);
        }

        // === локальный учёт связей ===
        internal void AddLocalConnectionIfMissing(SlotController output, SlotController input, UIBezierConnection line)
        {
            for (int i = 0; i < _connections.Count; i++)
            {
                var c = _connections[i];
                if (c.Line == line) return;
                if (c.Output == output && c.Input == input && c.Line == line) return;
            }

            _connections.Add(new ConnectionInfo { Output = output, Input = input, Line = line });
            Debug.Log($"{Pfx} AddLocalConnectionIfMissing: added line={line.name}@{line.GetInstanceID()}");
        }

        internal void RemoveLocalConnectionByLine(UIBezierConnection line)
        {
            if (line == null) return;
            for (int i = _connections.Count - 1; i >= 0; i--)
            {
                if (_connections[i].Line == line)
                {
                    _connections.RemoveAt(i);
                    Debug.Log($"{Pfx} RemoveLocalConnectionByLine: removed line={line.name}@{line.GetInstanceID()}");
                }
            }
        }

        // --- UGUI events (drag & select only by title) ---
        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left) return;

            _isDragging = false;

            // Разрешаем и перетаскивание, и выделение ТОЛЬКО если клик по заголовку (или если handle не задан)
            _canDragOrSelectThisGesture =
                titleDragHandle == null ||
                RectTransformUtility.RectangleContainsScreenPoint(
                    titleDragHandle,
                    eventData.position,
                    eventData.pressEventCamera);

            if (_canDragOrSelectThisGesture)
            {
                _rectTransform.SetAsLastSibling();

                if (_parentRect != null)
                {
                    RectTransformUtility.ScreenPointToLocalPointInRectangle(
                        _parentRect, eventData.position, eventData.pressEventCamera,
                        out Vector2 pointerLocalPoint);

                    _offset = _rectTransform.anchoredPosition - pointerLocalPoint;
                }
            }

            eventData.Use();
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!_canDragOrSelectThisGesture) return; // тянем только если жест начат на title
            if (_parentRect == null) return;

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    _parentRect, eventData.position, eventData.pressEventCamera,
                    out Vector2 pointerLocalPoint))
            {
                _rectTransform.anchoredPosition = pointerLocalPoint + _offset;
                _isDragging = true;
            }

            eventData.Use();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            // Выделяем/снимаем выделение только если клик был по title
            if (_canDragOrSelectThisGesture && !_isDragging)
            {
                if (_isSelected) Deselect();
                else Select();
            }

            _isDragging = false;
            _canDragOrSelectThisGesture = false; // сброс на конец жеста
            eventData.Use();
        }

        private void Select()
        {
            if (selectionOutline != null) selectionOutline.enabled = true;
            _isSelected = true;
        }

        private void Deselect()
        {
            if (selectionOutline != null) selectionOutline.enabled = false;
            _isSelected = false;
        }
    }
}
