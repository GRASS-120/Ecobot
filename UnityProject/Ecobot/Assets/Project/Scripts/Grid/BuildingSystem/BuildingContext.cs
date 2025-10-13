using Grid.BuildingSystem.PowerSystem;
using GUI.UIFramework;
using Player;

namespace Grid.BuildingSystem
{
    public class BuildingContext
    {
        public WindowManager WindowManager { get; }
        public PlayerManager PlayerManager { get; }
        public PowerGridService PowerGridService { get; }
        public PowerWireToolService PowerWireToolService { get; }
        
        public BuildingContext(
            WindowManager windowManager,
            PlayerManager playerManager,
            PowerGridService powerGridService,
            PowerWireToolService powerWireToolService)
        {
            WindowManager = windowManager;
            PlayerManager = playerManager;
            PowerGridService = powerGridService;
            PowerWireToolService = powerWireToolService;
        }
    }
}