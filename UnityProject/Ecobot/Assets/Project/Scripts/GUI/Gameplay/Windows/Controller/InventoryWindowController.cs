using GUI.Gameplay.Windows.View;
using GUI.UIFramework;
using Player;

namespace GUI.Gameplay.Windows.Controller
{
    public class InventoryWindowController : WindowController<InventoryWindowView>
    {
        public override string Id => "InventoryWindow";

        private readonly PlayerInventoryHolder _inventoryHolder; 
        
        public InventoryWindowController(PlayerInventoryHolder inventoryHolder)
        {
            _inventoryHolder = inventoryHolder;
        }
        
        public override void OnOpen()
        {
            View.PlayerInventoryUI.Init(
                _inventoryHolder.MainInventory, 
                View.MouseInventoryItemUI, 
                Subs,
                quickMoveTarget: _inventoryHolder.HotbarInventorySystem);
        }
    }
}