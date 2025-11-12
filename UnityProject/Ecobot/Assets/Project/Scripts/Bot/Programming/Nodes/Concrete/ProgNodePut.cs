using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Bot;
using Bot.Programming.Nodes.Base;
using Bot.Programming.Nodes.Slots;
using Grid.BuildingSystem.Buildings.Types.Furnance; // BuildingFurnace
using InteractionSystem;
using Inventory;
using UnityEngine;

namespace Bot.Programming.Nodes.Concrete
{
    /// <summary>
    /// Put: переложить ВСЕ предметы из инвентаря бота в цель.
    /// Входы:
    ///   - stream
    ///   - data "Target" (object) — цель (склад/печь/и т.п.)
    /// Выходы:
    ///   - stream: success / fail
    /// </summary>
    public class ProgNodePut : ProgNodeAction
    {
        private readonly ProgNodeDataSlot<object> _targetSlot;

        private static readonly List<IPutHandler> s_handlers = new()
        {
            new FurnacePutHandler(), // специфичная
            new StoragePutHandler(), // дефолт: любой IInventoryHolder
        };

        public ProgNodePut() : base("Put")
        {
            Description = "Put ALL items from bot inventory into target (strategy-based).";
            _targetSlot = new ProgNodeDataSlot<object>("Target", this);
            slots.Add(_targetSlot);
        }

        public override IEnumerator Execute(BotBase bot, BotProgramExecutor executor)
        {
            if (bot == null || bot.InventorySystem == null || bot.Interactor == null)
            {
                Debug.LogWarning("[Put] Bot or required components are NULL → Fail");
                yield return ExecFail(executor);
                yield break;
            }

            object target = null;
            try { target = _targetSlot.Value; } catch { /* ignore */ }

            if (target == null)
            {
                Debug.LogWarning("[Put] Target is NULL → Fail");
                yield return ExecFail(executor);
                yield break;
            }

            var handler = FindHandler(target);
            if (handler == null)
            {
                Debug.LogWarning($"[Put] No handler for target '{target}' → Fail");
                yield return ExecFail(executor);
                yield break;
            }

            if (!handler.IsInRange(bot.Interactor, target))
            {
                Debug.Log("[Put] Target is out of range → Fail");
                yield return ExecFail(executor);
                yield break;
            }

            int moved = handler.PutAll(bot, target);
            Debug.Log($"[Put] Moved total = {moved}");

            if (moved > 0) yield return ExecSuccess(executor);
            else           yield return ExecFail(executor);
        }

        private static IPutHandler FindHandler(object target)
        {
            foreach (var h in s_handlers)
                if (h.CanHandle(target))
                    return h;
            return null;
        }

        private IEnumerator ExecSuccess(BotProgramExecutor executor)
        {
            if (successSlot?.ConnectedNode != null)
                yield return executor.ExecuteNode(successSlot.ConnectedNode);
        }

        private IEnumerator ExecFail(BotProgramExecutor executor)
        {
            if (failureSlot?.ConnectedNode != null)
                yield return executor.ExecuteNode(failureSlot.ConnectedNode);
        }
    }

    // ===== интерфейс стратегии =====
    public interface IPutHandler
    {
        bool CanHandle(object target);
        bool IsInRange(BotInteractor interactor, object target);
        int  PutAll(BotBase bot, object target);
    }

    // ===== Печь =====
    public class FurnacePutHandler : IPutHandler
    {
        public bool CanHandle(object target)
        {
            var c = ExtractComponent(target);
            return c != null && c.GetComponentInParent<BuildingFurnace>() != null;
        }

        public bool IsInRange(BotInteractor interactor, object target)
        {
            var c = ExtractComponent(target);
            return c != null && interactor != null && interactor.IsTargetInRange(c.transform);
        }

        public int PutAll(BotBase bot, object target)
        {
            var c = ExtractComponent(target);
            var furnace = c != null ? c.GetComponentInParent<BuildingFurnace>() : null;
            if (furnace == null || bot?.InventorySystem == null) return 0;

            var botInv = bot.InventorySystem;
            var furInv = furnace.FurnaceInventory;
            var ops    = botInv.InventoryOperationsService;

            int moved = 0;

            // Проходим по слотам бота и раскладываем:
            for (int i = 0; i < botInv.InventorySize; i++)
            {
                var src = botInv.GetSlot(i);
                if (src.ItemData == null || src.StackSize <= 0) continue;

                if (furnace.IsFuelItem(src.ItemData))
                {
                    moved += ops.Move(botInv, i, furInv, furnace.FuelIndex, int.MaxValue);
                }
                else if (furnace.IsOreItem(src.ItemData))
                {
                    moved += ops.Move(botInv, i, furInv, furnace.OreIndex, int.MaxValue);
                }
                // Остальные предметы игнорируем
            }

            return moved;
        }

        private static Component ExtractComponent(object obj)
        {
            if (obj is Component comp) return comp;
            if (obj is GameObject go)  return go.transform;
            return null;
        }
    }

    // ===== Склад/любой IInventoryHolder =====
    public class StoragePutHandler : IPutHandler
    {
        public bool CanHandle(object target)
        {
            var c = ExtractComponent(target);
            return c != null && c.GetComponentInParent<IInventoryHolder>() != null;
        }

        public bool IsInRange(BotInteractor interactor, object target)
        {
            var c = ExtractComponent(target);
            return c != null && interactor != null && interactor.IsTargetInRange(c.transform);
        }

        public int PutAll(BotBase bot, object target)
        {
            var c = ExtractComponent(target);
            var holder = c != null ? c.GetComponentInParent<IInventoryHolder>() : null;
            if (holder == null || bot?.InventorySystem == null) return 0;

            return MoveAllItems(bot.InventorySystem, holder);
        }

        private static Component ExtractComponent(object obj)
        {
            if (obj is Component comp) return comp;
            if (obj is GameObject go)  return go.transform;
            return null;
        }

        private static int MoveAllItems(InventorySystem fromInv, IInventoryHolder toHolder)
        {
            int moved = 0;

            for (int i = 0; i < fromInv.InventorySlots.Count; i++)
            {
                var s = fromInv.GetSlot(i);
                if (s.ItemData == null || s.StackSize <= 0) continue;

                moved += DrainSlotSafe(fromInv, i, toHolder, s.ItemData);
            }
            return moved;
        }

        private static int DrainSlotSafe(InventorySystem inv, int slotIndex, IInventoryHolder holder, InventoryItemData item)
        {
            int moved = 0;
            var slot = inv.GetSlot(slotIndex);

            while (slot.ItemData == item && slot.StackSize > 0)
            {
                int remaining = slot.StackSize;
                int granted = AddUpTo(holder, item, remaining);
                if (granted <= 0) break;

                slot.RemoveFromStack(granted);
                if (slot.StackSize <= 0) slot.ClearSlot();
                inv.NotifySlotChanged(slotIndex);

                moved += granted;
            }

            return moved;
        }

        private static int AddUpTo(IInventoryHolder holder, InventoryItemData item, int maxAmount)
        {
            int remaining = Mathf.Max(1, maxAmount);
            int moved = 0;

            int chunk = remaining;
            while (remaining > 0)
            {
                if (holder.TryAddToInventory(item, chunk))
                {
                    moved    += chunk;
                    remaining -= chunk;
                    chunk     = remaining;
                }
                else
                {
                    if (chunk == 1) break;
                    chunk = Mathf.Max(1, chunk / 2);
                }
            }

            return moved;
        }
    }
}
