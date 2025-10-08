using GUI.Gameplay.Windows.View;
using GUI.UIFramework;
using Player;
using R3;
using UnityEngine;

namespace GUI.Gameplay.Windows.Controller
{
    [Window(WindowType.Overlay, "GameplayOverlay")]
    public class GameplayOverlayController : WindowController<GameplayOverlayView>
    {
        public override string Id => "GameplayOverlay";
        
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

            // View.StorageInventoryUI.Init(_playerInventoryHolder.HotbarInventorySystem, Subs);
        }
    }
}