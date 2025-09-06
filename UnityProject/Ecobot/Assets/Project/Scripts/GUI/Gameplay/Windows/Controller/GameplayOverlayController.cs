using GUI.Gameplay.Windows.View;
using GUI.UIFramework;
using Player;
using R3;
using UnityEngine;

namespace GUI.Gameplay.Windows.Controller
{
    public class GameplayOverlayController : WindowController<GameplayOverlayView>
    {
        public override string Id => "GameplayOverlay";
        
        private readonly PlayerInventoryHolder _inventoryHolder; 
        
        public GameplayOverlayController(PlayerInventoryHolder inventoryHolder)
        {
            _inventoryHolder = inventoryHolder;
        }

        public override void OnOpen()
        {
            View.MouseInventoryItemUI.Init(Subs);
            
            View.PlayerHotbarUI.Init(
                _inventoryHolder.HotbarInventorySystem, 
                View.MouseInventoryItemUI, 
                Subs,
                quickMoveTarget: _inventoryHolder.MainInventory);

            // View.PlayerInventoryUI.Init(
            //     _inventoryHolder.MainInventory, 
            //     View.MouseInventoryItemUI, 
            //     Subs,
            //     quickMoveTarget: _inventoryHolder.HotbarInventorySystem);
            
            // View.StorageInventoryUI.Init(_playerInventoryHolder.HotbarInventorySystem, Subs);
        }
    }
}