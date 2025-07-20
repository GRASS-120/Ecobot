using Game;
using GUI.UIFramework;
using R3;

namespace GUI.Programming.Windows
{
    public class ProgrammingOverlayController : WindowController<ProgrammingOverlayView>
    {
        private readonly GameManager _gameManager;
        public override string Id => "ProgrammingOverlay";

        public ProgrammingOverlayController(GameManager gameManager)
        {
            _gameManager = gameManager;
        }

        public override void OnOpen()
        {
            View.BtnClose.OnClickAsObservable().Subscribe(_ => RequestCloseOverlay()).AddTo(Subs);
        }

        private void RequestCloseOverlay()
        {
            _gameManager.FSM.GoToPreviousState();
        }
    }
}