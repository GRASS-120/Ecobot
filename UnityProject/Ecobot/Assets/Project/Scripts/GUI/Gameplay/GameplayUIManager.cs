using Game;
using Game.Mods;
using Game.Mods.Core;
using GUI.Core;
using GUI.Gameplay.Windows;
using GUI.Main;
using GUI.UIFramework;
using Player;
using UnityEngine;

namespace GUI.Gameplay
{
    /// <summary>
    /// Менеджер всех окон, связанных с Gameplay. Создает ViewModel для всех окон
    /// </summary>
    public class GameplayUIManager : MonoBehaviour
    {
        private GameUIRootViewModel _rootViewModel;
        private GameMode _mode;
        private GameManager _gameManager;
        private PlayerManager _player;

        public void Init(GameUIRootViewModel rootViewModel, GameManager gameManager, PlayerManager player)
        {
            _gameManager = gameManager;
            _rootViewModel = rootViewModel;
            _mode = _gameManager.GameplayMode;
            _player = player;
            
            _mode.OnEnterEvent += OpenOverlay;
        }
        
        public void OpenOverlay()
        {
            var viewModel = new GameplayOverlayViewModel(this);
           
            _rootViewModel.OpenOverlay(viewModel);
        }
    }
}