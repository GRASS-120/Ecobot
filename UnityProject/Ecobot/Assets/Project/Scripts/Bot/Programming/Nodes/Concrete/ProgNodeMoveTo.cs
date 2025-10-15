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
            Debug.Log($"[{NodeName}] ▶️ Starting execution");

            object target = null;
            try
            {
                target = targetSlot.Value;
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[{NodeName}] Exception reading targetSlot.Value: {ex}");
            }

            if (target == null)
            {
                Debug.LogWarning($"[{NodeName}] ❌ Target is NULL");
            }
            else
            {
                Debug.Log($"[{NodeName}] Target type: {target.GetType().Name}");
            }

            Vector3 targetPos = Vector3.zero;
            bool hasTarget = false;

            if (target != null)
            {
                switch (target)
                {
                    case BuildingBase building:
                        if (building.transform)
                        {
                            targetPos = building.transform.position;
                            hasTarget = true;
                            Debug.Log($"[{NodeName}] Target is BuildingBase '{building.name}' -> {targetPos}");
                        }
                        break;

                    case GameObject go:
                        targetPos = go.transform.position;
                        hasTarget = true;
                        Debug.Log($"[{NodeName}] Target is GameObject '{go.name}' -> {targetPos}");
                        break;

                    case Transform t:
                        targetPos = t.position;
                        hasTarget = true;
                        Debug.Log($"[{NodeName}] Target is Transform -> {targetPos}");
                        break;

                    case InventoryItemData item:
                        targetPos = executor.GetItemPosition(item);
                        hasTarget = true;
                        Debug.Log($"[{NodeName}] Target is InventoryItemData '{item.displayName}' -> {targetPos}");
                        break;

                    case Vector3 vec:
                        targetPos = vec;
                        hasTarget = true;
                        Debug.Log($"[{NodeName}] Target is Vector3 -> {targetPos}");
                        break;

                    default:
                        Debug.LogWarning($"[{NodeName}] Unknown target type: {target.GetType().Name}");
                        break;
                }
            }

            if (!hasTarget)
            {
                Debug.LogWarning($"[{NodeName}] ❌ No valid target found for MoveTo");
                if (failureSlot.ConnectedNode != null)
                    yield return executor.ExecuteNode(failureSlot.ConnectedNode);
                yield break;
            }

            Debug.Log($"[{NodeName}] Moving to {targetPos}...");

            if (bot == null)
            {
                Debug.LogWarning($"[{NodeName}] Bot reference is null");
                yield break;
            }

            if (bot.CommandController == null || bot.CommandController.Fabric == null)
            {
                Debug.LogWarning($"[{NodeName}] CommandController or Fabric is null — cannot move");
                yield break;
            }

            var moveCmd = bot.CommandController.Fabric.CreateMoveCommand(targetPos);
            if (moveCmd == null)
            {
                Debug.LogWarning($"[{NodeName}] ❌ CreateMoveCommand returned NULL");
                yield break;
            }

            Debug.Log($"[{NodeName}] Command created, adding and executing...");
            bot.CommandController.AddCommand(moveCmd);
            moveCmd.Execute();

            // Debug движение
            float timeout = 12f;
            float elapsed = 0f;
            float stopDistance = 1f;
            float logTimer = 0.5f;

            while (true)
            {
                if (bot == null) yield break;
                Vector3 botPos = bot.transform.position;
                float distXZ = Vector2.Distance(new Vector2(botPos.x, botPos.z), new Vector2(targetPos.x, targetPos.z));

                elapsed += Time.deltaTime;
                logTimer -= Time.deltaTime;
                if (logTimer <= 0f)
                {
                    Debug.Log($"[{NodeName}] ⏱ {elapsed:F1}s | BotPos={botPos} | Target={targetPos} | DistXZ={distXZ:F2}");
                    logTimer = 0.5f;
                }

                if (distXZ <= stopDistance)
                {
                    Debug.Log($"[{NodeName}] ✅ Reached target! Distance={distXZ:F2}");
                    break;
                }

                if (elapsed > timeout)
                {
                    Debug.LogWarning($"[{NodeName}] ⏱ Timeout after {elapsed:F1}s (Dist={distXZ:F2})");
                    break;
                }

                yield return null;
            }

            if (successSlot.ConnectedNode != null)
            {
                Debug.Log($"[{NodeName}] → Executing success slot -> {successSlot.ConnectedNode.NodeName}");
                yield return executor.ExecuteNode(successSlot.ConnectedNode);
            }
            else
            {
                Debug.Log($"[{NodeName}] Success slot not connected.");
            }
        }
    }
}
