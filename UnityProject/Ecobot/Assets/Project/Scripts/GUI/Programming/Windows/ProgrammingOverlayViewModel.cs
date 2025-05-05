using Game;
using GUI.UIFramework;

namespace GUI.Programming.Windows
{
    public class ProgrammingOverlayViewModel : WindowViewModel
    {
        private readonly GameManager _gameManager;
        public override string Id => "ProgrammingOverlay";

        public ProgrammingOverlayViewModel(GameManager gameManager)
        {
            _gameManager = gameManager;
        }
        
        public void RequestCloseOverlay()
        {
            _gameManager.FSM.GoToPreviousState();
        }
    }
}