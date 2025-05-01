using GUI.Gameplay;
using GUI.UIFramework;
using UnityEngine;

namespace GUI.Main
{
    public class GameScreenViewModel : WindowViewModel
    {
        private readonly GameplayUIManager _uiManager;
        public override string Id => "GameplayScreen";

        public GameScreenViewModel(GameplayUIManager uiManager)
        {
            _uiManager = uiManager;
        }
        
        public void RequestOpenPopupA()
        {
            _uiManager.OpenPopupA();
        }
    }
}