using GUI.UIFramework.Base;
using UnityEngine;

namespace GUI.Main
{
    public class GameScreenViewModel : WindowViewModel
    {
        private readonly GameplayUIManager _uiManager;
        public override string Id => "ScreenGame";

        public GameScreenViewModel(GameplayUIManager uiManager)
        {
            _uiManager = uiManager;
        }
        
        public void RequestOpenPopupA()
        {
            _uiManager.OpenPopupA();
        }

        public void RequestOpenPopupB()
        {
            _uiManager.OpenPopupB();

        }

        public void RequestGoToMainMenu()
        {
            _uiManager.OpenScreenGame();
        }
    }
}