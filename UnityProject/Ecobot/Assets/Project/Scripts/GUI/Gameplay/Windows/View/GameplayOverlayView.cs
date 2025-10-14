using GUI.UIFramework;
using Inventory.UI;
using UnityEngine;

namespace GUI.Gameplay.Windows.View
{
    public class GameplayOverlayView : OverlayView
    {
        [Header("Inventory")]
        [SerializeField] private HotbarUI playerHotbarUI;
        // [SerializeField] private MouseInventoryItemUI mouseInventoryItemUI;

        public HotbarUI PlayerHotbarUI => playerHotbarUI;
        // public MouseInventoryItemUI MouseInventoryItemUI => mouseInventoryItemUI;
    }
}