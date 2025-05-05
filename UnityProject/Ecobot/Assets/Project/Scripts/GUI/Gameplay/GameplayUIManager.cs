using Game;
using Game.Mods;
using Game.Mods.Core;
using GUI.Core;
using GUI.Gameplay.Windows;
using GUI.Main;
using GUI.UIFramework;
using UnityEngine;

namespace GUI.Gameplay
{
    /// <summary>
    /// Менеджер всех окон, связанных с Gameplay. Создает ViewModel для всех окон
    /// </summary>
    public class GameplayUIManager : MonoBehaviour, IUIManager
    {
        private GameUIRootViewModel _rootViewModel;
        private GameMode _mode;
        private GameManager _gameManager;

        public void Init(GameUIRootViewModel rootViewModel, GameMode mode, GameManager gameManager)
        {
            _rootViewModel = rootViewModel;
            _mode = mode;
            
            _mode.OnEnterEvent += OpenOverlay;
        }

        public void OpenOverlay()
        {
            var viewModel = new GameplayOverlayViewModel(this);
           
            _rootViewModel.OpenOverlay(viewModel);
        }
        
        public PopupAViewModel OpenPopupA()
        {
            var viewModel = new PopupAViewModel();
            
            _rootViewModel.OpenPopup(viewModel);
            
            return viewModel;
        }
    }
}