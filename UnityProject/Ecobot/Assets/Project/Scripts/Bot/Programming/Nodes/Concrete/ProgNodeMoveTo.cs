using System.Collections;
using Bot.Programming.Nodes.Slots;
using Grid.BuildingSystem;
using Inventory;
using UnityEngine;

namespace Bot.Programming.Nodes.Concrete
{
    public class ProgNodeMoveTo : ProgNodeAction
    {
        private ProgNodeDataSlot<object> targetSlot;
        
        public ProgNodeMoveTo() : base("Move To")
        {
            Description = "Move to the specified target";
            targetSlot = new ProgNodeDataSlot<object>("Target", this);
            slots.Add(targetSlot);
        }
        
        public override IEnumerator Execute(BotBase bot, BotProgramExecutor executor)
        {
            Debug.Log($"[{NodeName}] Executing MoveTo node");
            
            object target = targetSlot.Value;
            Vector3 targetPosition = Vector3.zero;
            bool hasTarget = false;
            
            if (target != null)
            {
                // Получаем позицию в зависимости от типа объекта
                if (target is InventoryItemData item)
                {
                    // targetPosition = item.transform.position;
                    targetPosition = new Vector3(5F, 0F, 5F);

                    hasTarget = true;
                }
                else if (target is Building building)
                {
                    targetPosition = building.transform.position;
                    hasTarget = true;
                }
                else if (target is Transform transform)
                {
                    targetPosition = transform.position;
                    hasTarget = true;
                }
                else if (target is GameObject gameObject)
                {
                    targetPosition = gameObject.transform.position;
                    hasTarget = true;
                }
            }
            
            if (!hasTarget)
            {
                Debug.LogWarning($"[{NodeName}] No valid target found");
                
                if (failureSlot.ConnectedNode != null)
                {
                    yield return executor.ExecuteNode(failureSlot.ConnectedNode);
                }
                
                yield break;
            }
            
            
            Debug.Log($"[{NodeName}] Moving to position: {targetPosition}");
            // Создаем и выполняем команду движения
            var moveCommand = bot.CommandController.Fabric.CreateMoveCommand(targetPosition);
            bot.CommandController.AddCommand(moveCommand);
            moveCommand.Execute();
            
            // Симулируем движение для тестирования
            yield return new WaitForSeconds(0.5f);
            
            Debug.Log($"[{NodeName}] Reached position: {targetPosition}");
            
            if (successSlot.ConnectedNode != null)
            {
                yield return executor.ExecuteNode(successSlot.ConnectedNode);
            }
        }
    }
}