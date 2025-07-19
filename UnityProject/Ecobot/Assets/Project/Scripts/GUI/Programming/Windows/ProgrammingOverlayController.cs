using Game;
using GUI.UIFramework;

namespace GUI.Programming.Windows
{
    public class ProgrammingOverlayController : WindowController
    {
        private readonly GameManager _gameManager;
        public override string Id => "ProgrammingOverlay";

        public ProgrammingOverlayController(GameManager gameManager)
        {
            _gameManager = gameManager;
        }
        
        public void RequestCloseOverlay()
        {
            _gameManager.FSM.GoToPreviousState();
        }
    }
}