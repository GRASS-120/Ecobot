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
        [Header("Components")] 
        [SerializeField] private WindowManager windowManager;
        
        [Header("UI Managers")]
        [SerializeField] private GameplayUIManager gameplayUIManager;
        [SerializeField] private ProgrammingUIManager programmingUIManager;

        public void Init(GameManager gameManager, PlayerManager player)
        {
            var rootViewModel = new GameUIRootController();
            
            Dispatch(rootViewModel);
            
            windowManager.Init(rootViewModel);
            gameplayUIManager.Init(windowManager, gameManager, player);
            
            
            programmingUIManager.Init(rootViewModel, gameManager);
        }
    }
}