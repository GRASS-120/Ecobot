using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Inventory.UI
{
    public class InventorySlotUI : MonoBehaviour
    {
        [SerializeField] private Image image;
        [SerializeField] private TextMeshProUGUI count;
        [SerializeField] private InventorySlot assignedSlot;

        private Button _button;

        public InventorySlot AssignedSlot => assignedSlot;
        public InventoryDisplay Display { get; private set; }

        private void Awake()
        {
            ClearSlot();

            _button = GetComponent<Button>();
            _button.onClick.AddListener(OnSlotClick);
            Display = GetComponentInParent<InventoryDisplay>();
        }

        public void Init(InventorySlot slot)
        {
            assignedSlot = slot;
            UpdateSlotUI(slot);
        }

        public void UpdateSlotUI(InventorySlot slot)
        {
            if (slot.ItemData != null)
            {
                image.sprite = slot.ItemData.icon;
                image.color = Color.white;
                
                count.text = slot.StackSize > 1 ? slot.StackSize.ToString() : "";
            }
            else
            {
                ClearSlot();
            }
        }

        public void UpdateSlotUI()
        {
            if (assignedSlot != null) UpdateSlotUI(assignedSlot);
        }

        public void ClearSlot()
        {
            assignedSlot?.ClearSlot();
            image.sprite = null;
            image.color = Color.clear;
            count.text = "";
        }

        private void OnSlotClick()
        {
            Display.SlotClicked(this);
        }
    }
}
