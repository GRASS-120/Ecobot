using Game;
using GUI.Gameplay;
using GUI.Programming;
using GUI.UIFramework;
using UnityEngine;

namespace GUI.Core
{
    // типо главная панелька
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

            // todo: временно. Я В АХУЕ с того, какая это непонятная система, гайд хуйни просто посмотрел... теперь
            // сидеть все переписывать... но это позже
            gameplayUIManager.Init(rootViewModel, _gameManager.GameplayMode, _gameManager);
            programmingUIManager.Init(rootViewModel, _gameManager.ProgrammingMode, _gameManager);
        }
    }
}