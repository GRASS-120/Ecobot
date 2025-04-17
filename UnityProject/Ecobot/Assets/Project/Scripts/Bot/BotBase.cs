using Bot.Command;
using Grid;
using Grid.Base;
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

        public BotMovementController MovementController => movementController;
        public BotCommandController CommandController => commandController;

        public void Init(GridMap gridMap)
        {
            movementController.Init(this, gridMap);
            commandController.Init(this);
        }

        public void StartInteraction(IInteractor interactor)
        {
            Debug.Log(interactor);
        }

        public void StopInteraction(IInteractor interactor)
        {
            throw new System.NotImplementedException();
        }
    }
}
