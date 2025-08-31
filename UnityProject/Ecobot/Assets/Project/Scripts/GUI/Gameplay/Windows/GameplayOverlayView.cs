using GUI.UIFramework;
using Inventory;
using Inventory.UI;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace GUI.Gameplay.Windows
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