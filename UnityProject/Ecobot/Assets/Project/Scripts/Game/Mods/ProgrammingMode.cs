using Game.Mods.Core;
using GUI.Core;
using Player.InputManager;

namespace Game.Mods
{
    public class ProgrammingMode : GameMode
    {
        public ProgrammingMode(PlayerInputManager playerInputManager) : base(playerInputManager)
        {
        }

        public override void ActivateInputMap()
        {
            PlayerInputManager.HandleProgrammingMap(true);
        }

        public override void DisableInputMap()
        {
            PlayerInputManager.HandleProgrammingMap(false);
        }
    }
}