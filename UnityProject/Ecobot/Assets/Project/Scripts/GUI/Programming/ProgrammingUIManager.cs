using Game;
using Game.Mods.Core;
using GUI.Core;
using GUI.Gameplay;
using GUI.Programming.Windows;
using GUI.UIFramework;
using UnityEngine;

namespace GUI.Programming
{
    public class ProgrammingUIManager : MonoBehaviour
    {
        private GameUIRootController _rootController;
        private GameMode _mode;
        private GameManager _gameManager;

        // virtual + не передавать mode, а его прямо здесь выставлять. то есть биндить тут
        public void Init(GameUIRootController rootController, GameManager gameManager)
        {
            _gameManager = gameManager;
            _rootController = rootController;
            _mode = _gameManager.ProgrammingMode;
            
            _mode.OnEnterEvent += OpenOverlay;
        }

        public void OpenOverlay()
        {
            var controller = new ProgrammingOverlayController(_gameManager);
            
            _rootController.OpenOverlay(controller);
        }
    }
}