using System.Collections;
using Bot.Programming.Nodes.Slots;
using Inventory;
using UnityEngine;

namespace Bot.Programming.Nodes.Concrete
{
    public class ProgNodeFindAndPick : ProgNodeAction
    {
        private string itemTypeName;
        private ProgNodeDataSlot<InventoryItemData> foundItemSlot;
        
        public ProgNodeFindAndPick(string itemTypeName) : base("Find and Pick")
        {
            this.itemTypeName = itemTypeName;
            Description = $"Find and pick up {itemTypeName} from the ground";
            foundItemSlot = new ProgNodeDataSlot<InventoryItemData>("Found Item", this);
            slots.Add(foundItemSlot);
        }
        
        public override IEnumerator Execute(BotBase bot, BotProgramExecutor executor)
        {
            Debug.Log($"[{NodeName}] Looking for item: {itemTypeName}");
            
            // Симуляция поиска предмета
            bool found = executor.SimulateFindItem(itemTypeName, out InventoryItemData item);
            
            if (found)
            {
                Debug.Log($"[{NodeName}] Found item: {item.displayName}");
                foundItemSlot.Value = item;
                
                if (successSlot.ConnectedNode != null)
                {
                    yield return executor.ExecuteNode(successSlot.ConnectedNode);
                }
            }
            else
            {
                Debug.Log($"[{NodeName}] Item not found: {itemTypeName}");
                
                if (failureSlot.ConnectedNode != null)
                {
                    yield return executor.ExecuteNode(failureSlot.ConnectedNode);
                }
            }
        }
    }
}