using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using GUI.Programming.Windows.Nodes;
using Bot.Programming.Nodes.Base; 

namespace GUI.Programming.Windows.Slots
{
    public class SlotController : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("Slot Configuration")]
        [SerializeField] private SlotDirection _slotDirection = SlotDirection.Input;
        [SerializeField] private SlotContentType _slotContentType = SlotContentType.Stream;

        [Header("Connection Point")]
        [SerializeField] private RectTransform _connectionPoint;

        [Header("Visual Feedback")]
        [SerializeField] private Image slotImage;
        [SerializeField] private Material defaultMaterial;
        [SerializeField] private Material validMaterial;
        [SerializeField] private Material invalidMaterial;

        public SlotDirection Direction => _slotDirection;
        public SlotContentType ContentType => _slotContentType;
        public RectTransform ConnectionPoint => _connectionPoint;
        public string SlotId => $"{gameObject.name}_{_slotDirection}_{_slotContentType}";

        public System.Action<SlotController> OnSlotPressed;
        public System.Action<SlotController> OnSlotReleased;

        private bool _isHovered;
        private bool _isConnected;

        public enum SlotDirection { Input, Output }
        public enum SlotContentType { Stream, Data }
        
        public ProgNodeSlotBase LinkedSlot { get; private set; }
        
        public void Initialize(ProgNodeSlotBase slot)
        {
            LinkedSlot = slot;
        }
        
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
            ApplyDefaultMaterial();
        }

        private void ApplyDefaultMaterial()
        {
            if (slotImage == null) return;
            if (defaultMaterial != null)
                slotImage.material = Instantiate(defaultMaterial);
            else
                slotImage.material = null;
        }

        private void UpdateVisualState()
        {
            if (slotImage == null) return;

            var activeNode = NodeController.ActiveConnectionNode;
            if (activeNode != null && activeNode.HasActiveConnection)
            {
                var output = activeNode.GetActiveOutputSlot();
                if (output != null)
                {
                    bool canConnect = output.CanConnectWith(this);
                    if (canConnect)
                    {
                        if (validMaterial != null) slotImage.material = Instantiate(validMaterial);
                    }
                    else
                    {
                        if (invalidMaterial != null) slotImage.material = Instantiate(invalidMaterial);
                    }
                    return;
                }
            }

            ApplyDefaultMaterial();
        }

        public bool CanConnectWith(SlotController otherSlot)
        {
            if (otherSlot == null) return false;
            return otherSlot != this &&
                   otherSlot.Direction != this.Direction &&
                   otherSlot.ContentType == this.ContentType &&
                   otherSlot.transform.parent != this.transform.parent;
        }

        public void SetConnected(bool connected) => _isConnected = connected;
        public bool IsConnected => _isConnected;
        public bool IsHovered => _isHovered;
    }
}
