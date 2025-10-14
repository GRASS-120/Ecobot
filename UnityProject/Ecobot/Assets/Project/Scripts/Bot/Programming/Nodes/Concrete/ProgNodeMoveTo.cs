using System.Collections;
using Bot.Programming.Nodes.Slots;
using Grid.BuildingSystem.Buildings.Base;
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
                switch (target)
                {
                    case Transform transform:
                        targetPosition = transform.position;
                        hasTarget = true;
                        break;
                    
                    case GameObject go:
                        targetPosition = go.transform.position;
                        hasTarget = true;
                        break;
                    
                    case BuildingBase building:
                        targetPosition = building.transform.position;
                        hasTarget = true;
                        break;

                    case Vector3 vec:
                        targetPosition = vec;
                        hasTarget = true;
                        break;

                    case InventoryItemData item:
                        targetPosition = executor.GetItemPosition(item);
                        hasTarget = true;
                        break;

                    default:
                        Debug.LogWarning($"[{NodeName}] Unknown target type: {target.GetType().Name}");
                        break;
                }
            }

            if (!hasTarget)
            {
                Debug.LogWarning($"[{NodeName}] ❌ No valid target found for movement");
                if (failureSlot.ConnectedNode != null)
                    yield return executor.ExecuteNode(failureSlot.ConnectedNode);
                yield break;
            }

            Debug.Log($"[{NodeName}] Moving to position: {targetPosition}");
            
            // Создаем и запускаем команду движения
            var moveCommand = bot.CommandController.Fabric.CreateMoveCommand(targetPosition);
            bot.CommandController.AddCommand(moveCommand);
            moveCommand.Execute();

            // ✅ Ожидаем прибытия
            float timeout = 10f;
            float elapsed = 0f;
            float stopDistance = 0.15f;

            while (Vector3.Distance(bot.transform.position, targetPosition) > stopDistance)
            {
                elapsed += Time.deltaTime;

                if (elapsed > timeout)
                {
                    Debug.LogWarning($"[{NodeName}] ⏱ Timeout while moving to target!");
                    if (failureSlot.ConnectedNode != null)
                        yield return executor.ExecuteNode(failureSlot.ConnectedNode);
                    yield break;
                }

                yield return null;
            }

            Debug.Log($"[{NodeName}] ✅ Reached position: {targetPosition}");

            // Выполняем следующую ноду
            if (successSlot.ConnectedNode != null)
                yield return executor.ExecuteNode(successSlot.ConnectedNode);
        }
    }
}
