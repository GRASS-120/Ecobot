using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Reflection;
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

namespace Bot
{
    public class BotBase : MonoBehaviour, IInteractable, IInventoryHolder
    {
        [Title("Components")]
        [SerializeField] private BotMovementController movementController;
        [SerializeField] private BotCommandController commandController;
        [SerializeField] private BotProgrammingController programmingController;

        // ОБЯЗАТЕЛЬНО повесь этот компонент в инспекторе
        [SerializeField] private BotInteractor botInteractor;

        [Title("Inventory")]
        [SerializeField] [Min(1)] private int inventorySize = 8;

        [Title("UI / Overlay")]
        [SerializeField] private Transform programmingOverlayParent;

        private GameManager _gameManager;
        private WindowManager _windowManager;
        private StateMachine _stateMachine;
        private InventorySystem _inventorySystem;

        public BotMovementController MovementController => movementController;
        public BotCommandController CommandController => commandController;
        public BotProgrammingController ProgrammingController => programmingController;
        public InventorySystem InventorySystem => _inventorySystem;

        // это нужно нодам
        public BotInteractor Interactor => botInteractor;

        public void Init(GridMap gridMap, GameManager gameManager, WindowManager windowManager)
        {
            _gameManager = gameManager;
            _windowManager = windowManager;

            // инвентарь бота
            _inventorySystem = new InventorySystem(inventorySize);

            movementController.Init(this, gridMap);
            commandController.Init(this);
            programmingController.Init(this);

            _stateMachine = new StateMachine();
            _stateMachine.SetState(new BotStateIdle());

            // ВАЖНО: связать интерактор и инвентарь
            if (botInteractor != null)
            {
                botInteractor.Init(this);
            }
            else
            {
                Debug.LogWarning("[BotBase] BotInteractor is NULL — бот будет майнить, но лут не сохранит.");
            }
        }

        public void Interact(IInteractor interactor)
        {
            _gameManager.FSM.SetState(_gameManager.ProgrammingMode);
            Debug.Log("[BotBase] Enter ProgrammingMode.");

            if (programmingOverlayParent == null)
            {
                Debug.LogError("[BotBase] programmingOverlayParent is NULL — назначь в инспекторе.");
                return;
            }

            // Holder ссылается на конкретного BotProgrammingController
            var holder = programmingOverlayParent.GetComponent<ProgrammingOverlayBotHolder>();
            if (holder == null)
            {
                holder = programmingOverlayParent.gameObject.AddComponent<ProgrammingOverlayBotHolder>();
                Debug.Log($"[BotBase] Added ProgrammingOverlayBotHolder on '{programmingOverlayParent.name}'");
            }
            holder.Set(programmingController);

            // Открываем окно. ВАЖНО: Больше НЕ биндим граф здесь, это делает сам OverlayController.
            _windowManager.OpenWindow<GUI.Programming.Windows.ProgrammingOverlayController>();
            Debug.Log("[BotBase] Opened ProgrammingOverlayController (overlay).");
        }

        public void AltInteract(IInteractor interactor) => OpenInventory();

        private void OpenInventory()
        {
            var storageWindow = _windowManager.GetController<StorageInventoryWindowController>();
            if (storageWindow == null) return;

            if (storageWindow.IsOpen)
                _windowManager.CloseWindow<StorageInventoryWindowController>();
            else
            {
                storageWindow.SetStorage(_inventorySystem);
                _windowManager.OpenWindow<StorageInventoryWindowController>();
            }
        }

        // это требует IInventoryHolder
        public bool TryAddToInventory(InventoryItemData data, int amount)
        {
            return _inventorySystem.TryAddToInventory(data, amount);
        }
    }

    public class ProgrammingOverlayBotHolder : MonoBehaviour
    {
        [SerializeField] private BotProgrammingController botProgramming;
        public BotProgrammingController BotProgramming => botProgramming;

        public void Set(BotProgrammingController value)
        {
            botProgramming = value;
            Debug.Log(value
                ? $"[BotHolder] Set BotProgramming='{value.name}' on holder '{name}'"
                : $"[BotHolder] Cleared BotProgramming on holder '{name}'");
        }
    }
}
