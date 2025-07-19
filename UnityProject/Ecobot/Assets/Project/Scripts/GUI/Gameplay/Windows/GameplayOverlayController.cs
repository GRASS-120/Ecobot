using GUI.UIFramework;

namespace GUI.Gameplay.Windows
{
    public class GameplayOverlayController : WindowController
    {
        private readonly GameplayUIManager _uiManager;
        public override string Id => "GameplayOverlay";

        public GameplayOverlayController(GameplayUIManager uiManager)
        {
            _uiManager = uiManager;
        }
    }
}