namespace Game.Mods
{
    public class MenuMode : GameMode
    {
        public MenuMode(PlayerInputManager playerInputManager) : base(playerInputManager)
        {
        }

        public override void ActivateInputMap()
        {
            PlayerInputManager.HandleMenuMap(true);
        }

        public override void DisableInputMap()
        {
            PlayerInputManager.HandleMenuMap(true);
        }
    }
}