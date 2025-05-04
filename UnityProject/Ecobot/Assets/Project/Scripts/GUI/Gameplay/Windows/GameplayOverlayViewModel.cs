using GUI.UIFramework;

namespace GUI.Gameplay.Windows
{
    public class GameplayOverlayViewModel : WindowViewModel
    {
        private readonly GameplayUIManager _uiManager;
        public override string Id => "GameplayOverlay";

        public GameplayOverlayViewModel(GameplayUIManager uiManager)
        {
            _uiManager = uiManager;
        }
        
        public void RequestOpenPopupA()
        {
            _uiManager.OpenPopupA();
        }
    }
}