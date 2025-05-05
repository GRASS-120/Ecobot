using Bot.Command;
using Game;
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

        private GameManager _gameManager;
        // private 
        
        public BotMovementController MovementController => movementController;
        public BotCommandController CommandController => commandController;

        public void Init(GridMap gridMap, GameManager gameManager)
        {
            movementController.Init(this, gridMap);
            commandController.Init(this);

            _gameManager = gameManager;
        }

        public void Interact(IInteractor interactor)
        {
            _gameManager.FSM.SetState(_gameManager.ProgrammingMode);
            // _uiManager.OpenProgrammingOverlay();
        }
        
        public void AltInteract(IInteractor interactor)
        {
            Debug.Log(interactor);
        }
    }
}
