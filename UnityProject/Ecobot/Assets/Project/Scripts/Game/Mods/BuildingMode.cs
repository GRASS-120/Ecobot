using UnityEngine;

namespace Game.Mods
{
    public class BuildingMode : GameplayMode
    {
        public BuildingMode(PlayerInputManager playerInputManager) : base(playerInputManager)
        {
        }

        public override void ActivateInputMap()
        {
            base.ActivateInputMap();
            PlayerInputManager.HandleBuildingMap(true);
        }

        public override void DisableInputMap()
        {
            base.DisableInputMap();
            PlayerInputManager.HandleBuildingMap(false);
        }
    }
}