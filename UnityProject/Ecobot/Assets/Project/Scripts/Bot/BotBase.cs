using Bot.Command;
using Bot.Programming;
using Bot.States;
using FiniteStateMachine;
using Game;
using Grid;
using Grid.Base;
using GUI.Programming;
using InteractionSystem;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace Bot
{
    public class BotBase : MonoBehaviour, IInteractable {
        [Title("Components")]
        [SerializeField] private BotMovementController movementController;
        [SerializeField] private BotCommandController commandController;
        [SerializeField] private BotProgrammingController programmingController;

        private GameManager _gameManager;
        private StateMachine _stateMachine;
        private BotStateIdle _stateIdle;
        private BotStateWorking _stateWorking;
        private BotStateWaiting _stateWaiting;
        
        public BotMovementController MovementController => movementController;
        public BotCommandController CommandController => commandController;
        public BotProgrammingController ProgrammingController => programmingController;

        public void Init(GridMap gridMap, GameManager gameManager)
        {
            _stateMachine = new StateMachine();
            _stateIdle = new BotStateIdle();
            _stateWorking = new BotStateWorking();
            _stateWaiting = new BotStateWaiting();
            
            movementController.Init(this, gridMap);
            commandController.Init(this);
            programmingController.Init(this);

            _gameManager = gameManager;
            
            _stateMachine.AddAnyTransition(_stateIdle, new FuncPredicate(() => true));
            _stateMachine.SetState(_stateIdle);
        }

        public void Interact(IInteractor interactor)
        {
            // todo: temp
            _gameManager.FSM.SetState(_gameManager.ProgrammingMode);
        }
        
        public void AltInteract(IInteractor interactor)
        {
            Debug.Log(interactor);
        }
    }
}
