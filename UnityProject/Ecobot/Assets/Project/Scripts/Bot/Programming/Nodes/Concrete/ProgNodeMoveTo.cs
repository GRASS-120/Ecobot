using System.Collections;
using System.Collections.Generic;
using Bot;
using Bot.Programming.Navigation;          // IApproachPointProvider
using Bot.Programming.Nodes.Base;          // ProgNodeAction
using Bot.Programming;                     // BotProgramExecutor
using Bot.Programming.Nodes.Slots;
using Grid.Base;                           // GridMap
using Grid.BuildingSystem;                 // GridBuildingSystem
using UnityEngine;

namespace Bot.Programming.Nodes.Concrete
{
    /// <summary>
    /// MoveTo: цель сообщает точку подъезда через IApproachPointProvider.
    /// Без кулдауна/анти-спама: нода идемпотентно выдаёт команду и ждёт прибытия.
    /// При отсутствии прогресса в окне стагнации — выход по Fail (если подключён).
    /// </summary>
    public class ProgNodeMoveTo : ProgNodeAction
    {
        private readonly ProgNodeDataSlot<object> targetSlot;

        // Тюнинги ожидания
        private const float TimeoutSeconds      = 15f;  // жёсткий лимит ожидания
        private const float StagnationWindowSec = 0.6f; // окно "нет прогресса"
        private const float ProgressEps         = 0.10f; // насколько должна уменьшаться дистанция, чтобы считать прогрессом

        public ProgNodeMoveTo() : base("Move To")
        {
            Description = "Move to the specified target via IApproachPointProvider";
            targetSlot = new ProgNodeDataSlot<object>("Target", this);
            slots.Add(targetSlot);
        }

        public override IEnumerator Execute(BotBase bot, BotProgramExecutor executor)
        {
            if (bot == null)
            {
                Debug.LogWarning($"[{NodeName}] Bot is null");
                yield break;
            }

            // 1) достаём цель из data-слота
            object target = null;
            try { target = targetSlot.Value; } catch { /* ignore */ }

            if (target == null)
            {
                Debug.LogWarning($"[{NodeName}] Target is NULL");
                yield break;
            }

            // 2) резолвим точку подъезда через IApproachPointProvider
            if (!TryResolveTargetPoint(bot, target, out var targetPos))
                yield break;

            // 3) радиус прибытия
            float stopDistance = ResolveStopDistance();

            // если уже на месте — готово
            if (XZDistance(bot.transform.position, targetPos) <= stopDistance)
            {
                Debug.Log($"[{NodeName}] ✅ Already in range (≤ {stopDistance:F2})");
                if (successSlot?.ConnectedNode != null)
                    yield return executor.ExecuteNode(successSlot.ConnectedNode);
                yield break;
            }

            // 4) создаём команду движения (идемпотентно)
            if (bot.CommandController == null || bot.CommandController.Fabric == null)
            {
                Debug.LogWarning($"[{NodeName}] Missing CommandController/Fabric");
                yield break;
            }

            var moveCmd = bot.CommandController.Fabric.CreateMoveCommand(targetPos);
            if (moveCmd == null)
            {
                Debug.LogWarning($"[{NodeName}] CreateMoveCommand returned NULL");
                yield break;
            }

            bot.CommandController.AddCommand(moveCmd);
            moveCmd.Execute();

            // 5) ждём прибытия / либо фиксируем стагнацию/таймаут
            bool arrived = false;
            bool stuck   = false;

            float elapsed          = 0f;
            float stagnationTimer  = 0f;
            float lastDist         = XZDistance(bot.transform.position, targetPos);

            while (true)
            {
                if (bot == null) break;

                float dist = XZDistance(bot.transform.position, targetPos);

                // прибытие
                if (dist <= stopDistance)
                {
                    Debug.Log($"[{NodeName}] ✅ Arrived (≤ {stopDistance:F2})");
                    arrived = true;
                    break;
                }

                // прогресс?
                if (lastDist - dist < ProgressEps)
                {
                    stagnationTimer += Time.deltaTime;
                    if (stagnationTimer >= StagnationWindowSec)
                    {
                        Debug.LogWarning($"[{NodeName}] 🛑 No progress for {StagnationWindowSec:F1}s");
                        stuck = true;
                        break;
                    }
                }
                else
                {
                    stagnationTimer = 0f;
                    lastDist = dist;
                }

                // таймаут
                elapsed += Time.deltaTime;
                if (elapsed > TimeoutSeconds)
                {
                    Debug.LogWarning($"[{NodeName}] ⏱ Timeout {elapsed:F1}s");
                    stuck = true;
                    break;
                }

                yield return null;
            }

            // 6) маршрутизация по выходам
            if (arrived)
            {
                if (successSlot?.ConnectedNode != null)
                    yield return executor.ExecuteNode(successSlot.ConnectedNode);
            }
            else if (stuck)
            {
                if (failureSlot?.ConnectedNode != null)
                    yield return executor.ExecuteNode(failureSlot.ConnectedNode);
            }
        }

        // ---------- provider ----------

        private bool TryResolveTargetPoint(BotBase bot, object target, out Vector3 point)
        {
            point = default;

            IApproachPointProvider provider = null;
            GameObject targetGO = null;

            if (target is Component comp)
            {
                targetGO = comp.gameObject;
                provider = comp.GetComponent<IApproachPointProvider>();
            }
            else if (target is GameObject go)
            {
                targetGO = go;
                provider = go.GetComponent<IApproachPointProvider>();
            }
            else
            {
                Debug.LogWarning(
                    $"[{NodeName}] ❌ Target type '{target.GetType().Name}' не поддерживается без провайдера " +
                    "(нужен Component/GameObject с IApproachPointProvider).");
                return false;
            }

            if (provider == null)
            {
                var name = targetGO != null ? $"'{targetGO.name}'" : "(no GameObject)";
                Debug.LogWarning($"[{NodeName}] ❌ На цели {name} нет IApproachPointProvider. Добавь BuildingApproachProvider / OreApproachProvider.");
                return false;
            }

            if (!provider.TryGetApproachPoint(bot.transform.position, out point))
            {
                var name = targetGO != null ? $"'{targetGO.name}'" : "(no GameObject)";
                Debug.LogWarning($"[{NodeName}] ❌ Провайдер на {name} не смог вернуть точку подъезда.");
                return false;
            }

            Debug.Log($"[{NodeName}] 🔌 Provider '{provider.GetType().Name}' → point={point}");
            return true;
        }

        // ---------- helpers ----------

        private float ResolveStopDistance()
        {
            float stop = 1f; // дефолт
            var gbs = GameObject.FindObjectOfType<GridBuildingSystem>();
            var map = gbs ? gbs.GetComponentInParent<GridMap>() : null;
            if (map != null && map.Grid != null)
                stop = Mathf.Max(0.35f, map.Grid.CellSize * 0.45f);
            return stop;
        }

        private static float XZDistance(Vector3 a, Vector3 b)
            => Vector2.Distance(new Vector2(a.x, a.z), new Vector2(b.x, b.z));
    }
}
