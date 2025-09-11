using GUI.UIFramework;
using Inventory.UI;
using UnityEngine;

namespace GUI.Gameplay.Windows.View
{
    public class InventoryWindowView : PopupView
    {
        [Header("Components")]
        [SerializeField] private InventorySlotUI slotPrefab;
        [SerializeField] private RectTransform slotsRoot;
        
        public void ClearVisual()
        {
            foreach (Transform child in slotsRoot)
                Destroy(child.gameObject);
        }

        public InventorySlotUI CreateSlotVisual()
        {
            return Instantiate(slotPrefab, slotsRoot);
        }
    }
}