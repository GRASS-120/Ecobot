using Game;
using GUI.Gameplay;
using GUI.Programming;
using GUI.UIFramework;
using UnityEngine;

namespace GUI.Core
{
    public class GameUIRootView : UIRootView
    {
        [Header("UI Managers")]
        public GameplayUIManager gameplayUIManager;
        public ProgrammingUIManager programmingUIManager;
        
        private GameManager _gameManager;
        
        // concrete active manager depends on game mode
        // и все? -> в целом можно использовать UIManager для переброса инфы между манагерами + проброс в них
        // данных

        public void Init(GameManager gameManager, GameUIRootViewModel rootViewModel)
        {
            _gameManager = gameManager;
            
            Dispatch(rootViewModel);
            
            gameplayUIManager.Init(rootViewModel, _gameManager);
            programmingUIManager.Init(rootViewModel, _gameManager);
        }
    }
}