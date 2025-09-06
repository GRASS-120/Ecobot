using GUI.UIFramework;
using Inventory.UI;
using UnityEngine;

namespace GUI.Gameplay.Windows.View
{
    public class GameplayOverlayView : OverlayView
    {
        [Header("Inventory")]
        [SerializeField] private HotbarUI playerHotbarUI;
        [SerializeField] private MainInventoryUI playerInventoryUI;
        [SerializeField] private MainInventoryUI storageInventoryUI;
        [SerializeField] private MouseInventoryItemUI mouseInventoryItemUI;

        public HotbarUI PlayerHotbarUI => playerHotbarUI;
        public MainInventoryUI PlayerInventoryUI => playerInventoryUI;
        public MainInventoryUI StorageInventoryUI => storageInventoryUI;
        public MouseInventoryItemUI MouseInventoryItemUI => mouseInventoryItemUI;
    }
}