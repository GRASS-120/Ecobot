using Game;
using GUI.Gameplay;
using GUI.Programming;
using GUI.UIFramework;
using Player;
using UnityEngine;

namespace GUI.Core
{
    public class GameUIRootView : UIRootView
    {
        [Header("UI Managers")]
        public GameplayUIManager gameplayUIManager;
        public ProgrammingUIManager programmingUIManager;

        public void Init(GameManager gameManager, PlayerManager player)
        {
            var rootViewModel = new GameUIRootViewModel();
            
            Dispatch(rootViewModel);
            
            gameplayUIManager.Init(rootViewModel, gameManager, player);
            programmingUIManager.Init(rootViewModel, gameManager);
        }
    }
}