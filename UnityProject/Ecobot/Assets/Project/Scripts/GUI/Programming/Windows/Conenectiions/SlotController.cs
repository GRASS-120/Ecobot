using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using GUI.Programming.Windows.Nodes;

namespace GUI.Programming.Windows.Slots
{
    public class SlotController : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("Slot Configuration")]
        [SerializeField] private SlotDirection _slotDirection = SlotDirection.Input;
        [SerializeField] private SlotContentType _slotContentType = SlotContentType.Stream;

        [Header("Connection Point")]
        [SerializeField] private RectTransform _connectionPoint;

        [Header("Visual Feedback (optional, для самого слота)")]
        [SerializeField] private Image slotImage;
        [SerializeField] private Material defaultMaterial;
        [SerializeField] private Material validMaterial;
        [SerializeField] private Material invalidMaterial;

        [Header("Semantic Keying")]
        [SerializeField] private string _slotKey;
        [SerializeField] private string _dataTypeName;

        public string SlotKey => string.IsNullOrWhiteSpace(_slotKey) ? gameObject.name : _slotKey;
        public string DataTypeName => _dataTypeName;

        public string SlotId => SlotKey;

        public enum SlotDirection { Input, Output }
        public enum SlotContentType { Stream, Data }

        public SlotDirection Direction => _slotDirection;
        public SlotContentType ContentType => _slotContentType;
        public RectTransform ConnectionPoint => _connectionPoint;

        public System.Action<SlotController> OnSlotPressed;
        public System.Action<SlotController> OnSlotReleased;

        public NodeController Owner { get; set; }

        private bool _isHovered;
        private int _connectedCount = 0;

        public int ConnectedCount => _connectedCount;
        public bool IsConnected => _connectedCount > 0;
        public bool IsHovered => _isHovered;

        private void Awake()
        {
            if (_connectionPoint == null)
                _connectionPoint = GetComponent<RectTransform>();
            if (slotImage == null)
                slotImage = GetComponent<Image>();
            if (slotImage != null)
                slotImage.raycastTarget = true;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left) return;
            OnSlotPressed?.Invoke(this);
            eventData.Use();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left) return;
            if (_isHovered) OnSlotReleased?.Invoke(this);
            eventData.Use();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _isHovered = true;
            UpdateVisualState();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _isHovered = false;

            // сброс локальной подсветки слота
            ApplyDefaultMaterial();

            // и сброс предпросмотра линии
            var activeNode = NodeController.ActiveConnectionNode;
            var line = activeNode?.GetCurrentPreviewLine();
            if (line != null)
                line.ClearHoverPreview();
        }

        private void ApplyDefaultMaterial()
        {
            if (slotImage == null) return;
            slotImage.material = defaultMaterial != null ? Instantiate(defaultMaterial) : null;
        }

        private void UpdateVisualState()
        {
            if (slotImage == null) return;

            var activeNode = NodeController.ActiveConnectionNode;
            if (activeNode != null && activeNode.HasActiveConnection)
            {
                var output = activeNode.GetActiveOutputSlot();
                var line   = activeNode.GetCurrentPreviewLine();
                if (output != null)
                {
                    bool canConnect = output.CanConnectWith(this);

                    // опционально подсвечиваем сам слот материалом
                    if (canConnect) { if (validMaterial   != null) slotImage.material = Instantiate(validMaterial); }
                    else            { if (invalidMaterial != null) slotImage.material = Instantiate(invalidMaterial); }

                    // главное — подсветка на линии
                    if (line != null)
                        line.SetHoverPreview(canConnect);

                    return;
                }
            }

            ApplyDefaultMaterial();

            // если нет активного предпросмотра — сказать линии сбросить состояние
            var maybeLine = activeNode?.GetCurrentPreviewLine();
            if (maybeLine != null)
                maybeLine.ClearHoverPreview();
        }

        public bool CanConnectWith(SlotController otherSlot)
        {
            if (otherSlot == null) return false;
            if (otherSlot == this) return false;
            if (otherSlot.Direction == this.Direction) return false;
            if (otherSlot.ContentType != this.ContentType) return false;
            if (otherSlot.Owner == this.Owner) return false;

            if (ContentType == SlotContentType.Data &&
                !string.IsNullOrWhiteSpace(DataTypeName) &&
                !string.IsNullOrWhiteSpace(otherSlot.DataTypeName))
            {
                if (DataTypeName != otherSlot.DataTypeName &&
                    DataTypeName != "System.Object" &&
                    otherSlot.DataTypeName != "System.Object")
                    return false;
            }

            return true;
        }

        // учёт соединений
        public void SetConnected(bool connected)
        {
            int before = _connectedCount;
            _connectedCount = connected ? Mathf.Max(_connectedCount, 1) : 0;
            if (before != _connectedCount)
                Debug.Log($"[Slot] SetConnected: {gameObject.name}[{Direction}/{ContentType}] {before} → {_connectedCount}");
        }

        public void IncrementConnected()
        {
            _connectedCount++;
            Debug.Log($"[Slot] IncrementConnected: {gameObject.name}[{Direction}/{ContentType}] => count={_connectedCount}");
        }

        public void DecrementConnected()
        {
            int before = _connectedCount;
            _connectedCount = Mathf.Max(0, _connectedCount - 1);
            Debug.Log($"[Slot] DecrementConnected: {gameObject.name}[{Direction}/{ContentType}] {before} → {_connectedCount}");
        }

        public void ResetVisual()
        {
            ApplyDefaultMaterial();
            Debug.Log($"[Slot] ResetVisual: {gameObject.name}[{Direction}/{ContentType}] (count={_connectedCount})");
        }
    }
}
