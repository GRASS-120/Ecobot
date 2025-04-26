using Game.Mods.Core;
using Player.InputManager;
using UnityEngine;

namespace Game.Mods
{
    public class GameplayMode : GameMode
    {
        public GameplayMode(PlayerInputManager playerInputManager) : base(playerInputManager)
        {
        }
        
        public override void OnEnter()
        {
            base.OnEnter();
            ActivateInputMap();
        }

        public override void OnExit()
        {
            base.OnExit();
            DisableInputMap();
        }

        public override void ActivateInputMap()
        {
            PlayerInputManager.HandleGameplayMap(true);
        }

        public override void DisableInputMap()
        {
            PlayerInputManager.HandleGameplayMap(false);
        }
    }
}