namespace Game.Mods
{
    public class BuildingMode : GameMode
    {
        public BuildingMode(PlayerInputManager playerInputManager) : base(playerInputManager)
        {
        }

        public override void ActivateInputMap()
        {
            PlayerInputManager.HandleBuildingMap(true);
        }

        public override void DisableInputMap()
        {
            PlayerInputManager.HandleBuildingMap(false);
        }
    }
}