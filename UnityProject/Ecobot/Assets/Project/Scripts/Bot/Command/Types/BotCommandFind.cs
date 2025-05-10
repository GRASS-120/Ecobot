using UnityEngine;

namespace Bot.Command.Types
{
    public class BotCommandFind<T> : BotCommand
    {
        private T _target;
        private readonly BotMovementController _mover;
        
        public BotCommandFind(BotMovementController mover, T target)
        {
            _mover = mover;
            _target = target;
        }
        
        public override void Execute()
        {
            // _mover.StartCoroutine(_mover.Move(_target));
        }

        public override bool CanBeExecuted() {
            return _target != null;
        } 
    }
}