using System;

namespace Game.Mods
{
    public abstract class GameMode
    {
        public Action ModeAction;
        protected PlayerInputManager PlayerInputManager;

        protected GameMode(PlayerInputManager playerInputManager)
        {
            PlayerInputManager = playerInputManager;
        }

        public void Update()
        {
            ModeAction?.Invoke();
        }

        public abstract void ActivateInputMap();
        public abstract void DisableInputMap();
    }
}