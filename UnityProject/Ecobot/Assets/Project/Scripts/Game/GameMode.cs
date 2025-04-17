using System;
using FiniteStateMachine;
using Player.InputManager;

namespace Game
{
    public abstract class GameMode : BaseState
    {
        // пока будет через события. если же будет важен порядок вызова,
        // то нужно переделать на функции (то есть вручную прокидывать)
        // ЛИБО!
        // можно попробовать события R3
        
        public Action OnUpdate;
        public Action OnFixedUpdate;
        public Action OnEnterEvent;
        public Action OnExitEvent;
        
        protected readonly PlayerInputManager PlayerInputManager;

        public override void OnEnter()
        {
            OnEnterEvent?.Invoke();
        }

        public override void Update()
        {
            OnUpdate?.Invoke();
        }

        public override void FixedUpdate()
        {
            OnFixedUpdate?.Invoke();
        }

        public override void OnExit()
        {
            OnExitEvent?.Invoke();
        }

        protected GameMode(PlayerInputManager playerInputManager)
        {
            PlayerInputManager = playerInputManager;
        }

        public abstract void ActivateInputMap();
        public abstract void DisableInputMap();
    }
}