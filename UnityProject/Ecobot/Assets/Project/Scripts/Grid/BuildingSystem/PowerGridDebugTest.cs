using System.Linq;
using Grid.BuildingSystem.Buildings;
using Grid.BuildingSystem.Buildings.Types.PowerPole;
using Grid.BuildingSystem.Buildings.Types.WindTurbine;
using Grid.BuildingSystem.PowerSystem;
using UnityEngine;

namespace Grid.BuildingSystem
{
    public class PowerGridDebugTest : MonoBehaviour
    {
        [SerializeField] private GridBuildingSystem gridBuildingSystem;
        [SerializeField] private PowerGridService powerGridService;

        [ContextMenu("PowerTest/Connect 1 Generator -> Pole -> 3 Furnaces")]
        private void ConnectBaseNetwork()
        {
            var gen = gridBuildingSystem.BuildingDatabase.GetAllOfType<BuildingWindTurbine>().FirstOrDefault();
            var pole = gridBuildingSystem.BuildingDatabase.GetAllOfType<BuildingPowerPole>().FirstOrDefault();
            var furnaces = gridBuildingSystem.BuildingDatabase.GetAllOfType<BuildingFurnace>().Take(3).ToList();

            if (gen == null || pole == null || furnaces.Count < 3)
            {
                Debug.LogWarning("Not enough nodes in scene: need 1 generator, 1 pole, 3 furnaces");
                return;
            }

            var ok1 = powerGridService.Connect(gen as IPowerNode, pole as IPowerNode);
            var ok2 = furnaces.All(f => powerGridService.Connect(pole as IPowerNode, f as IPowerNode));

            Debug.Log($"Connect results: gen->pole={ok1}, pole->3 furnaces={ok2}");

            foreach (var f in furnaces)
            {
                Debug.Log($"Furnace {f.name} powered={f.IsPowered}");
            }
        }

        [ContextMenu("PowerTest/Add 4th Furnace (expect trip)")]
        private void AddFourthAndTrip()
        {
            var pole = gridBuildingSystem.BuildingDatabase.GetAllOfType<BuildingPowerPole>().FirstOrDefault();
            var furnaces = gridBuildingSystem.BuildingDatabase.GetAllOfType<BuildingFurnace>().ToList();
            var gen = gridBuildingSystem.BuildingDatabase.GetAllOfType<BuildingWindTurbine>().FirstOrDefault();

            if (pole == null || gen == null || furnaces.Count < 4)
            {
                Debug.LogWarning("Need at least 1 pole, 1 generator and 4 furnaces placed");
                return;
            }

            var fourth = furnaces[3];
            var ok = powerGridService.Connect(pole as IPowerNode, fourth as IPowerNode);
            Debug.Log($"Connect pole -> 4th furnace: {ok}");

            // Ожидаем: трип — генераторы сломаны, печи обесточены
            Debug.Log($"Generator {gen.name} broken={gen.IsBroken}");
            foreach (var f in furnaces.Take(4))
            {
                Debug.Log($"Furnace {f.name} powered={f.IsPowered}");
            }

            if (!gen.IsBroken)
            {
                Debug.LogError("Expected generator to be broken after overload.");
            }
            if (furnaces.Take(4).Any(f => f.IsPowered))
            {
                Debug.LogError("Expected all furnaces to be unpowered after overload.");
            }
        }

        [ContextMenu("PowerTest/Repair All Generators")]
        private void RepairAllGenerators()
        {
            var gens = gridBuildingSystem.BuildingDatabase.GetAllOfType<BuildingWindTurbine>().ToList();
            if (gens.Count == 0)
            {
                Debug.LogWarning("No generators found");
                return;
            }

            foreach (var g in gens)
            {
                g.Repair();
            }

            Debug.Log("All generators repaired. If network is still overloaded, they will trip again on recompute.");
        }
    }
}