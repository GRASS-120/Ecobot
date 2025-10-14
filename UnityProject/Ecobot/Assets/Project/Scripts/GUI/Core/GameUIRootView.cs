using Game;
using GUI.Gameplay;
using GUI.Programming;
using GUI.UIFramework;
using Inventory.UI;
using Player;
using R3;
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
        
        [Header("UI Managers")]
        [SerializeField] private MouseInventoryItemUI mouse;

        private CompositeDisposable _disposables = new CompositeDisposable();
        
        public void Init(GameManager gameManager, PlayerManager player)
        {
            var rootViewModel = new GameUIRootController();
            
            Dispatch(rootViewModel);
            
            windowManager.Init(rootViewModel);
            
            mouse.Init(_disposables);
            gameplayUIManager.Init(windowManager, gameManager, player, mouse);
            programmingUIManager.Init(rootViewModel, gameManager);
        }

        private void OnDestroy()
        {
            _disposables?.Dispose();
        }
    }
}