using UnityEngine;

namespace Bot.Command.Types
{
    public class BotCommandMove : BotCommand {
        private readonly BotMovementController _mover;
        private readonly Vector3 _target;
        
        public BotCommandMove(BotMovementController mover, Vector3 target)
        {
            _mover = mover;
            _target = target;
        }
        
        public override void Execute()
        {
            _mover.StartCoroutine(_mover.Move(_target));
        }

        public override bool CanBeExecuted() {
            return _target != Vector3.zero;
        } 
    }
}