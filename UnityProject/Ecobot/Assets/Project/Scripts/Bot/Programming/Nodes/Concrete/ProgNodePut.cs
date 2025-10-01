using System.Collections;
using Bot.Programming.Nodes.Slots;
using Grid.BuildingSystem;
using Grid.BuildingSystem.Buildings;
using Inventory;
using UnityEngine;

namespace Bot.Programming.Nodes.Concrete
{
    public class ProgNodePut : ProgNodeAction
    {
        private ProgNodeDataSlot<InventoryItemData> itemSlot;
        private ProgNodeDataSlot<BuildingBase> buildingSlot;
        
        public ProgNodePut() : base("Put")
        {
            Description = "Put the specified item into the specified building";
            itemSlot = new ProgNodeDataSlot<InventoryItemData>("Item", this);
            buildingSlot = new ProgNodeDataSlot<BuildingBase>("Building", this);
            slots.Add(itemSlot);
            slots.Add(buildingSlot);
        }
        
        public override IEnumerator Execute(BotBase bot, BotProgramExecutor executor)
        {
            if (itemSlot.ConnectedNode == null || buildingSlot.ConnectedNode == null)
            {
                Debug.LogWarning($"[{NodeName}] Item or building not specified");
                
                if (failureSlot.ConnectedNode != null)
                {
                    yield return executor.ExecuteNode(failureSlot.ConnectedNode);
                }
                
                yield break;
            }
            
            InventoryItemData item = itemSlot.Value;
            BuildingBase building = buildingSlot.Value;
            
            Debug.Log($"[{NodeName}] Putting item {item.displayName} into building");
            
            // Симуляция размещения предмета
            bool success = executor.SimulatePutItem(item, building);
            
            if (success)
            {
                Debug.Log($"[{NodeName}] Successfully put item into building");
                
                if (successSlot.ConnectedNode != null)
                {
                    yield return executor.ExecuteNode(successSlot.ConnectedNode);
                }
            }
            else
            {
                Debug.Log($"[{NodeName}] Failed to put item into building");
                
                if (failureSlot.ConnectedNode != null)
                {
                    yield return executor.ExecuteNode(failureSlot.ConnectedNode);
                }
            }
        }
    }
}