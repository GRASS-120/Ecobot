using GUI.UIFramework;
using Inventory.UI;
using UnityEngine;

namespace GUI.Gameplay.Windows.View
{
    public class InventoryWindowView : PopupView
    {
        [Header("Components")]
        [SerializeField] private MainInventoryUI playerInventoryUI;
        [SerializeField] private MouseInventoryItemUI mouseInventoryItemUI;
        
        public MainInventoryUI PlayerInventoryUI => playerInventoryUI;
        public MouseInventoryItemUI MouseInventoryItemUI => mouseInventoryItemUI;
    }
}