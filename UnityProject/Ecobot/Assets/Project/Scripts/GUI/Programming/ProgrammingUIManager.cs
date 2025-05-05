using Game;
using Game.Mods.Core;
using GUI.Core;
using GUI.Gameplay;
using GUI.Programming.Windows;
using GUI.UIFramework;
using UnityEngine;

namespace GUI.Programming
{
    public class ProgrammingUIManager : MonoBehaviour, IUIManager
    {
        private GameUIRootViewModel _rootViewModel;
        private GameMode _mode;
        private GameManager _gameManager;

        // virtual
        public void Init(GameUIRootViewModel rootViewModel, GameMode mode, GameManager gameManager)
        {
            _gameManager = gameManager;
            _rootViewModel = rootViewModel;
            _mode = mode;
            
            _mode.OnEnterEvent += OpenOverlay;
        }

        public void OpenOverlay()
        {
            var viewModel = new ProgrammingOverlayViewModel(_gameManager);
            
            _rootViewModel.OpenOverlay(viewModel);
        }
    }
}