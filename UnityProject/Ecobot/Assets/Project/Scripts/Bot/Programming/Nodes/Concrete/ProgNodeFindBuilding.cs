using System.Collections;
using Bot.Programming.Nodes.Slots;
using Grid.BuildingSystem;
using UnityEngine;

namespace Bot.Programming.Nodes.Concrete
{
    public class ProgNodeFindBuilding : ProgNodeAction
    {
        private string buildingTypeName;
        private ProgNodeDataSlot<Building> foundBuildingSlot;
        
        public ProgNodeFindBuilding(string buildingTypeName) : base("Find Building")
        {
            this.buildingTypeName = buildingTypeName;
            Description = $"Find nearest {buildingTypeName} building";
            foundBuildingSlot = new ProgNodeDataSlot<Building>("Found Building", this);
            slots.Add(foundBuildingSlot);
        }
        
        public override IEnumerator Execute(BotBase bot, BotProgramExecutor executor)
        {
            Debug.Log($"[{NodeName}] Looking for building: {buildingTypeName}");
            
            // Симуляция поиска здания
            bool found = executor.SimulateFindBuilding(buildingTypeName, out Building building);
            
            if (found)
            {
                Debug.Log($"[{NodeName}] Found building of type: {buildingTypeName}");
                foundBuildingSlot.Value = building;
                
                if (successSlot.ConnectedNode != null)
                {
                    yield return executor.ExecuteNode(successSlot.ConnectedNode);
                }
            }
            else
            {
                Debug.Log($"[{NodeName}] Building not found: {buildingTypeName}");
                
                if (failureSlot.ConnectedNode != null)
                {
                    yield return executor.ExecuteNode(failureSlot.ConnectedNode);
                }
            }
        }
    }
}