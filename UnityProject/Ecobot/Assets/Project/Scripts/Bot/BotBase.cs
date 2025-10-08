using System.Collections;
using Bot.Command;
using Bot.Programming;
using Bot.States;
using FiniteStateMachine;
using Game;
using Grid;
using Grid.Base;
using GUI.Gameplay.Windows.Controller;
using GUI.Programming;
using GUI.UIFramework;
using InteractionSystem;
using Inventory;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace Bot
{
    public class BotBase : MonoBehaviour, IInteractable, IInventoryHolder
    {
        [Title("Components")]
        [SerializeField] private BotMovementController movementController;
        [SerializeField] private BotCommandController commandController;
        [SerializeField] private BotProgrammingController programmingController;

        [Title("Inventory")]
        [SerializeField] [Min(1)] private int inventorySize = 8;
        
        private GameManager _gameManager;
        private WindowManager _windowManager;
        private StateMachine _stateMachine;
        private BotStateIdle _stateIdle;
        private BotStateWorking _stateWorking;
        private BotStateWaiting _stateWaiting;
        private InventorySystem _inventorySystem;
        
        public BotMovementController MovementController => movementController;
        public BotCommandController CommandController => commandController;
        public BotProgrammingController ProgrammingController => programmingController;
        public InventorySystem InventorySystem => _inventorySystem;

        public void Init(GridMap gridMap, GameManager gameManager, WindowManager windowManager)
        {
            _stateMachine = new StateMachine();
            _stateIdle = new BotStateIdle();
            _stateWorking = new BotStateWorking();
            _stateWaiting = new BotStateWaiting();
            
            movementController.Init(this, gridMap);
            commandController.Init(this);
            programmingController.Init(this);
            
            _gameManager = gameManager;
            _windowManager = windowManager;
            _inventorySystem = new InventorySystem(inventorySize);
            
            _stateMachine.AddAnyTransition(_stateIdle, new FuncPredicate(() => true));
            _stateMachine.SetState(_stateIdle);
        }

        public void Interact(IInteractor interactor)
        {
            // Открытие режима программирования
            _gameManager.FSM.SetState(_gameManager.ProgrammingMode);
        }

        public void AltInteract(IInteractor interactor)
        {
            OpenInventory();
        }

        public void HoldInteractionCancel(IInteractor interactor)
        {
            // Можно добавить визуальную обратную связь об отмене
            Debug.Log("Bot inventory opening cancelled");
        }

        private void OpenInventory()
        {
            if (_windowManager == null)
            {
                Debug.LogError("WindowManager is null! Bot was not initialized properly.");
                return;
            }

            var storageWindow = _windowManager.GetController<StorageInventoryWindowController>();
            
            if (storageWindow == null)
            {
                Debug.LogError("StorageInventoryWindowController not found in WindowManager!");
                return;
            }

            if (storageWindow.IsOpen)
            {
                _windowManager.CloseWindow<StorageInventoryWindowController>();
            }
            else
            {
                storageWindow.SetStorage(_inventorySystem);
                _windowManager.OpenWindow<StorageInventoryWindowController>();
            }
        }

        public bool TryAddToInventory(InventoryItemData data, int amount)
        {
            return _inventorySystem.TryAddToInventory(data, amount);
        }
    }
}
