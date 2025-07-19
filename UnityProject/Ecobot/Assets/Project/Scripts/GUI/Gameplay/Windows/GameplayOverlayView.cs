using GUI.UIFramework;
using Inventory.UI;
using UnityEngine;
using UnityEngine.UI;

namespace GUI.Gameplay.Windows
{
    public class GameplayOverlayView : OverlayView
    {
        [Header("Inventory")]
        [SerializeField] private InventoryUIController inventoryController;
    }
}