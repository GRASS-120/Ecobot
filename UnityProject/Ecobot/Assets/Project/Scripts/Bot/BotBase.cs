using Bot.Command;
using Grid;
using Grid.Base;
using GUI.Programming;
using InteractionSystem;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Bot
{
    // todo: добавить FSM? 1. idle, 2. выполняет команду, 3. ожидает и тп...
    public class BotBase : MonoBehaviour, IInteractable {
        [Title("Components")]
        [SerializeField] private BotMovementController movementController;
        [SerializeField] private BotCommandController commandController;

        private ProgrammingUIManager _uiManager;
        // private 
        
        public BotMovementController MovementController => movementController;
        public BotCommandController CommandController => commandController;

        public void Init(GridMap gridMap, ProgrammingUIManager uiManager)
        {
            movementController.Init(this, gridMap);
            commandController.Init(this);

            _uiManager = uiManager;
        }

        public void Interact(IInteractor interactor)
        {
            // _uiManager.OpenProgrammingOverlay();
        }
        
        public void AltInteract(IInteractor interactor)
        {
            Debug.Log(interactor);
        }
    }
}
