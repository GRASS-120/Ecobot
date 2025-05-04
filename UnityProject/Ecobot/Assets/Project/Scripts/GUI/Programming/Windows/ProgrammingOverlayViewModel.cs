using GUI.UIFramework;

namespace GUI.Programming.Windows
{
    public class ProgrammingOverlayViewModel : WindowViewModel
    {
        private readonly ProgrammingUIManager _uiManager;
        public override string Id => "ProgrammingOverlay";

        public ProgrammingOverlayViewModel(ProgrammingUIManager uiManager)
        {
            _uiManager = uiManager;
        }
        
        // public void RequestOpenPopupA()
        // {
        //     _uiManager.OpenPopupA();
        // }
    }
}