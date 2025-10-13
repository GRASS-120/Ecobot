using Grid.BuildingSystem.PowerSystem;
using Grid.BuildingSystem.PowerSystem.WireSystem;
using GUI.UIFramework;
using Inventory.UI;
using Player;

namespace Grid.BuildingSystem
{
    public class BuildingContext
    {
        public WindowManager WindowManager { get; }
        public PlayerManager PlayerManager { get; }
        public PowerGridService PowerGridService { get; }
        public PowerWireToolService PowerWireToolService { get; }
        public MouseInventoryItemUI MouseInventoryItemUI { get; }

        public BuildingContext(
            WindowManager windowManager,
            PlayerManager playerManager,
            PowerGridService powerGridService,
            PowerWireToolService powerWireToolService,
            MouseInventoryItemUI mouseInventoryItemUI)
        {
            WindowManager = windowManager;
            PlayerManager = playerManager;
            PowerGridService = powerGridService;
            PowerWireToolService = powerWireToolService;
            MouseInventoryItemUI = mouseInventoryItemUI;
        }
    }
}