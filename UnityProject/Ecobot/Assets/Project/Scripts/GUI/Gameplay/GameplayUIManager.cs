using Game;
using Game.Mods;
using Game.Mods.Core;
using GUI.Core;
using GUI.Gameplay.Windows;
using GUI.Gameplay.Windows.Controller;
using GUI.Main;
using GUI.UIFramework;
using Inventory;
using Player;
using UnityEngine;

namespace GUI.Gameplay
{
    /// <summary>
    /// Менеджер всех окон, связанных с Gameplay. Создает ViewModel для всех окон
    /// </summary>
    public class GameplayUIManager : MonoBehaviour
    {
        private GameUIRootController _rootController;
        private GameMode _mode;
        private GameManager _gameManager;
        private PlayerInventoryHolder _inventoryHolder;

        public void Init(GameUIRootController rootController, GameManager gameManager, PlayerManager player)
        {
            _gameManager = gameManager;
            _rootController = rootController;
            _mode = _gameManager.GameplayMode;
            _inventoryHolder = player.Inventory;
            
            _mode.OnEnterEvent += OpenOverlay;
        }
        
        public void OpenOverlay()
        {
            var controller = new GameplayOverlayController(_inventoryHolder);
           
            _rootController.OpenOverlay(controller);
        }

        public void OpenPlayerInventory()
        {
            var controller = new InventoryWindowController(_inventoryHolder);
            
            // ok, этот функционал стоит оставить, так как может понадобиться для случаев, когда неважно какое окно
            // например, закрыть последний popup или открыть overlay
            
            _rootController.OpenPopup(controller);
            
            // два способа закрыть...
            // _rootController.ClosePopup(controller);
            //
            // _rootController.OpenedOverlay.CurrentValue.Close();
        }
    }
}