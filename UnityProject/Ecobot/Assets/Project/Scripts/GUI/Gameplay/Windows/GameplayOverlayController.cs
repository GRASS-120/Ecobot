using GUI.UIFramework;
using Inventory;

namespace GUI.Gameplay.Windows
{
    public class GameplayOverlayController : WindowController<GameplayOverlayView>
    {
        private readonly GameplayUIManager _uiManager;
        private readonly PlayerInventoryHolder _playerInventoryHolder; 
        
        public override string Id => "GameplayOverlay";

        public GameplayOverlayController(GameplayUIManager uiManager, PlayerInventoryHolder playerInventoryHolder)
        {
            _uiManager = uiManager;
            _playerInventoryHolder = playerInventoryHolder;
        }

        public override void OnOpen()
        {
            View.HotbarDisplay.Init(_playerInventoryHolder.HotbarInventorySystem, Subs);
        }
    }
}