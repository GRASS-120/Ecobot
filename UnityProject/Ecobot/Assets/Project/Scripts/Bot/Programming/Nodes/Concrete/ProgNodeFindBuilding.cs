using System.Collections;
using System.Linq;
using Bot.Programming.Nodes.Slots;
using Grid.BuildingSystem;
using Grid.BuildingSystem.Buildings.Base;
using UnityEngine;

namespace Bot.Programming.Nodes.Concrete
{
    public class ProgNodeFindBuilding : ProgNodeAction
    {
        private readonly string buildingTypeName;
        private readonly ProgNodeDataSlot<BuildingBase> foundBuildingSlot;
        
        // Добавляем минимальные задержки
        private const float FoundDelaySec = 0.1f;
        private const float FailDelaySec = 0.1f;

        public ProgNodeFindBuilding(string buildingTypeName) : base("Find Building")
        {
            this.buildingTypeName = buildingTypeName;
            Description = $"Find nearest building of type '{buildingTypeName}'";
            foundBuildingSlot = new ProgNodeDataSlot<BuildingBase>("Found Building", this);
            slots.Add(foundBuildingSlot);
        }

        public override IEnumerator Execute(BotBase bot, BotProgramExecutor executor)
        {
            Debug.Log($"[{NodeName}] 🔍 Searching for nearest '{buildingTypeName}'...");

            if (bot == null)
            {
                Debug.LogWarning($"[{NodeName}] ❌ Bot reference is null!");
                yield break;
            }

            GridBuildingSystem gridSystem = GameObject.FindObjectOfType<GridBuildingSystem>();
            if (gridSystem == null)
            {
                Debug.LogWarning($"[{NodeName}] ❌ GridBuildingSystem not found in scene!");
                yield break;
            }

            var database = gridSystem.BuildingDatabase;
            if (database == null)
            {
                Debug.LogWarning($"[{NodeName}] ❌ BuildingDatabase is null!");
                yield break;
            }

            var allBuildings = database.EnumerateAll();
            if (allBuildings == null)
            {
                Debug.LogWarning($"[{NodeName}] ❌ No buildings found in database!");
                yield break;
            }

            var matchingBuildings = allBuildings
                .Where(b => b != null && b.BuildingAssetData != null && b.BuildingAssetData.name == buildingTypeName)
                .ToList();

            if (matchingBuildings.Count == 0)
            {
                Debug.Log($"[{NodeName}] ❌ No buildings of type '{buildingTypeName}' found!");
                if (FailDelaySec > 0f) yield return new WaitForSeconds(FailDelaySec);
                if (failureSlot.ConnectedNode != null)
                    yield return executor.ExecuteNode(failureSlot.ConnectedNode);
                yield break;
            }

            Vector3 botPos = bot.transform.position;
            BuildingBase closest = null;
            float closestDist = float.MaxValue;

            foreach (var b in matchingBuildings)
            {
                float dist = Vector3.Distance(botPos, b.transform.position);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closest = b;
                }
            }

            if (closest == null)
            {
                Debug.Log($"[{NodeName}] ❌ Could not find a valid closest building of type '{buildingTypeName}'.");
                yield break;
            }

            foundBuildingSlot.Value = closest;

            Debug.Log($"[{NodeName}] ✅ Found '{buildingTypeName}' at {closest.transform.position} | Distance = {closestDist:F1}");

            // 🔸 Короткая пауза, чтобы не спамил если стоит рядом
            if (FoundDelaySec > 0f)
                yield return new WaitForSeconds(FoundDelaySec);

            if (successSlot.ConnectedNode != null)
                yield return executor.ExecuteNode(successSlot.ConnectedNode);
        }
    }
}
