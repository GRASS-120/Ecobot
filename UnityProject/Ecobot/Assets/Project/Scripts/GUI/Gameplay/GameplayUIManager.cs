using Game;
using Game.Mods;
using Game.Mods.Core;
using GUI.Core;
using GUI.Gameplay.Windows;
using GUI.Gameplay.Windows.Controller;
using GUI.UIFramework;
using Handbook;
using Inventory;
using Inventory.UI;
using Player;
using UnityEngine;

namespace GUI.Gameplay
{
    /// <summary>
    /// Менеджер всех окон, связанных с Gameplay. Создает ViewModel для всех окон
    /// </summary>
    public class GameplayUIManager : MonoBehaviour
    {
        private WindowManager _windowManager;
        private GameMode _mode;
        private GameManager _gameManager;
        private PlayerInventoryHolder _inventoryHolder;
        private MouseInventoryItemUI _mouse;

        public void Init(
            WindowManager windowManager,
            GameManager gameManager,
            PlayerManager player,
            MouseInventoryItemUI mouse)
        {
            _gameManager = gameManager;
            _windowManager = windowManager;
            _mode = _gameManager.GameplayMode;
            _inventoryHolder = player.Inventory;
            _mouse = mouse;
            
            _mode.OnEnterEvent += OpenOverlay;
        }
        
        public void OpenOverlay()
        {
            _windowManager.OpenWindow<GameplayOverlayController>(controller =>
            {
                controller.Init(_inventoryHolder, _windowManager, _mouse);
            });
        }
    }
}