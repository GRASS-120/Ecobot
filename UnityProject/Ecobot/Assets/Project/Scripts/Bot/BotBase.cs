using Bot.Command;
using Grid;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Bot
{
    // todo: добавить FSM? 1. idle, 2. выполняет команду, 3. ожидает и тп...
    public class BotBase : MonoBehaviour {
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
    }
}
