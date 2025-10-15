using System.Collections;
using Bot.Programming.Nodes.Slots;
using Grid.BuildingSystem.Buildings.Base;
using UnityEngine;

namespace Bot.Programming.Nodes.Concrete
{
    public class ProgNodeFindBuilding : ProgNodeAction
    {
        private string buildingTypeName;
        private ProgNodeDataSlot<BuildingBase> foundBuildingSlot;

        public ProgNodeFindBuilding(string buildingTypeName) : base("Find Building")
        {
            this.buildingTypeName = buildingTypeName;
            Description = $"Find nearest {buildingTypeName}";
            foundBuildingSlot = new ProgNodeDataSlot<BuildingBase>("Found Building", this);
            slots.Add(foundBuildingSlot);
        }

        public override IEnumerator Execute(BotBase bot, BotProgramExecutor executor)
        {
            Debug.Log($"[{NodeName}] 🔍 Searching for building: '{buildingTypeName}'");

            bool found = executor.SimulateFindBuilding(buildingTypeName, out BuildingBase building);

            if (found && building != null)
            {
                Vector3 pos = building.transform ? building.transform.position : Vector3.zero;
                Debug.Log($"[{NodeName}] ✅ Found building '{buildingTypeName}' at {pos}, object={building.gameObject.name}");
                foundBuildingSlot.Value = building;

                if (successSlot.ConnectedNode != null)
                {
                    Debug.Log($"[{NodeName}] → Executing success slot -> {successSlot.ConnectedNode.NodeName}");
                    yield return executor.ExecuteNode(successSlot.ConnectedNode);
                }
                else
                {
                    Debug.Log($"[{NodeName}] Success slot is not connected — stopping.");
                }
            }
            else
            {
                Debug.LogWarning($"[{NodeName}] ❌ Could not find building '{buildingTypeName}'");

                if (failureSlot.ConnectedNode != null)
                {
                    Debug.Log($"[{NodeName}] → Executing failure slot -> {failureSlot.ConnectedNode.NodeName}");
                    yield return executor.ExecuteNode(failureSlot.ConnectedNode);
                }
            }
        }
    }
}
