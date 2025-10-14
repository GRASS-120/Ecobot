using System.Collections;
using Bot.Programming.Nodes.Slots;
using Grid.BuildingSystem;
using Grid.BuildingSystem.Buildings;
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
            Description = $"Find nearest {buildingTypeName} building";
            foundBuildingSlot = new ProgNodeDataSlot<BuildingBase>("Found Building", this);
            slots.Add(foundBuildingSlot);
        }
        
        public override IEnumerator Execute(BotBase bot, BotProgramExecutor executor)
        {
            Debug.Log($"[{NodeName}] Looking for building: {buildingTypeName}");
            
            // Симуляция поиска здания
            bool found = executor.SimulateFindBuilding(buildingTypeName, out BuildingBase building);
            
            if (found && building != null)
            {
                // Логи для отладки позиции — это ключ
                Vector3 pos = building.transform != null ? building.transform.position : Vector3.zero;
                Debug.Log($"[{NodeName}] Found building '{buildingTypeName}' -> object: {building.GetType().Name}, position: {pos}");

                // Записываем сам объект в слот (передаём ссылку)
                foundBuildingSlot.Value = building;
                
                if (successSlot.ConnectedNode != null)
                {
                    yield return executor.ExecuteNode(successSlot.ConnectedNode);
                }
            }
            else
            {
                Debug.LogWarning($"[{NodeName}] Building not found or null: {buildingTypeName}");
                
                if (failureSlot.ConnectedNode != null)
                {
                    yield return executor.ExecuteNode(failureSlot.ConnectedNode);
                }
            }
        }
    }
}
