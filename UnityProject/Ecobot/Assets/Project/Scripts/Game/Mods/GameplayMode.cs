namespace Game.Mods
{
    public class GameplayMode : GameMode
    {
        public GameplayMode(PlayerInputManager playerInputManager) : base(playerInputManager)
        {
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