using GUI.UIFramework;
using Player;

namespace Grid.BuildingSystem
{
    public class BuildingContext
    {
        public WindowManager WindowManager { get; }
        public PlayerManager PlayerManager { get; }
        
        public BuildingContext(WindowManager windowManager, PlayerManager playerManager)
        {
            WindowManager = windowManager;
            PlayerManager = playerManager;
        }
    }
}