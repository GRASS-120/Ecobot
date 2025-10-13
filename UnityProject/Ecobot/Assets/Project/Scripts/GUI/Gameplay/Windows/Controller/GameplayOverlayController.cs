using GUI.Gameplay.Windows.View;
using GUI.UIFramework;
using Inventory.UI;
using Player;
using R3;
using UnityEngine;

namespace GUI.Gameplay.Windows.Controller
{
    [Window(WindowType.Overlay, "GameplayOverlay")]
    public class GameplayOverlayController : WindowController<GameplayOverlayView>
    {
        public override string Id => "GameplayOverlay";
        public MouseInventoryItemUI MouseUI { get; private set; }

        private PlayerInventoryHolder _inventoryHolder;
        private WindowManager _windowManager;

        public void Init(PlayerInventoryHolder inventoryHolder, WindowManager windowManager)
        {
            _inventoryHolder = inventoryHolder;
            _windowManager = windowManager;
        }

        public override void OnOpen()
        {
            View.MouseInventoryItemUI.Init(Subs);
            
            View.PlayerHotbarUI.Init(
                _inventoryHolder.HotbarInventorySystem, 
                _inventoryHolder.InventorySelectionService,
                View.MouseInventoryItemUI, 
                Subs,
                quickMoveTarget: _inventoryHolder.MainInventory);

            var inventoryWindowController = _windowManager.GetController<InventoryWindowController>();
            inventoryWindowController.Init(
                _inventoryHolder.MainInventory,
                _inventoryHolder.InventorySelectionService,
                View.MouseInventoryItemUI,
                _inventoryHolder.HotbarInventorySystem);

            var storageWindowController = _windowManager.GetController<StorageInventoryWindowController>();
            storageWindowController.Init(
                _inventoryHolder.InventorySelectionService,
                View.MouseInventoryItemUI,
                _inventoryHolder.MainInventory); 
            
            MouseUI = View.MouseInventoryItemUI;
        }
    }
}