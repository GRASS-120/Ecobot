using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GUI.Programming.Graph;
using GUI.Programming.Windows.Nodes;
using Bot.Programming.Nodes.Base;
using Bot.Programming.Nodes.Concrete;
using Bot.Programming.Nodes.Slots;
using Grid.BuildingSystem.Buildings.Base;
using UnityEngine;

namespace Bot.Programming
{
    /// <summary>
    /// Собирает логическую программу из снапшота графа.
    /// Поддерживаются: IdleStart, IdleEnd, FindOre, FindBuilding, MoveTo, Mine, Put.
    /// Поддерживаются Stream и Data соединения.
    /// </summary>
    public static class GraphSnapshotToProgram
    {
        public static ProgNodeBase Build(GraphSnapshotDTO snap, out List<string> errors)
        {
            errors = new List<string>();
            if (snap == null || snap.nodes == null || snap.nodes.Count == 0)
            {
                errors.Add("Snapshot is empty.");
                return null;
            }

            // 1) создаём все логические ноды
            var id2node = new Dictionary<int, ProgNodeBase>();
            GraphNodeDTO idleStartNode = null;

            foreach (var n in snap.nodes)
            {
                ProgNodeBase logic = CreateLogicNode(n, errors);
                if (logic == null) continue;

                id2node[n.id] = logic;
                if (n.type == NodeController.UINodeKind.IdleStart)
                    idleStartNode = n;
            }

            if (id2node.Count == 0)
            {
                errors.Add("No logical nodes were created.");
                return null;
            }

            // 2) подключаем DATA (чтобы MoveTo уже имел Target и т.п.)
            foreach (var e in snap.edges.Where(e => e.fromKind == PortKind.Data && e.toKind == PortKind.Data))
            {
                if (!id2node.TryGetValue(e.fromId, out var from)) continue;
                if (!id2node.TryGetValue(e.toId, out var to)) continue;
                TryConnectData(from, to, e, errors);
            }

            // 3) подключаем STREAM
            foreach (var e in snap.edges.Where(e => e.fromKind == PortKind.Stream && e.toKind == PortKind.Stream))
            {
                if (!id2node.TryGetValue(e.fromId, out var from)) continue;
                if (!id2node.TryGetValue(e.toId, out var to)) continue;
                TryConnectStream(from, to, e, errors);
            }

            // 4) выбираем корень (IdleStart или первая нода)
            if (idleStartNode != null && id2node.TryGetValue(idleStartNode.id, out var rootIdle))
                return rootIdle;

            return id2node.Values.FirstOrDefault();
        }

        private static string N(string id) => NormalizeSlotId(id);

        // ---------- создание логических нод ----------
        private static ProgNodeBase CreateLogicNode(GraphNodeDTO n, List<string> errors)
        {
            switch (n.type)
            {
                case NodeController.UINodeKind.IdleStart:
                    return new ProgNodeStateIdle();

                case NodeController.UINodeKind.IdleEnd:
                    return new ProgNodeStateIdle();

                case NodeController.UINodeKind.FindOre:
                {
                    var key = string.IsNullOrEmpty(n.dropdownTech) ? "IronData" : n.dropdownTech; // тип руды
                    return new ProgNodeFindOre(key);
                }

                case NodeController.UINodeKind.FindBuilding:
                {
                    var key = string.IsNullOrEmpty(n.dropdownTech) ? "Building_FurnanceData" : n.dropdownTech;
                    return new ProgNodeFindBuilding(key);
                }

                case NodeController.UINodeKind.MoveTo:
                    return new ProgNodeMoveTo();

                case NodeController.UINodeKind.Mine:
                {
                    // dropdownTech у «Mine» — строка «сколько добыть». По умолчанию «1».
                    var desiredText = string.IsNullOrEmpty(n.dropdownTech) ? "1" : n.dropdownTech;
                    return new ProgNodeMineOre(desiredText);
                }

                case NodeController.UINodeKind.Put:
                    return new ProgNodePut();

                default:
                    errors.Add($"Unsupported node type: {n.type}");
                    return null;
            }
        }

        // ---------- подключение DATA ----------
        private static void TryConnectData(ProgNodeBase from, ProgNodeBase to, GraphEdgeDTO e, List<string> errors)
        {
            string fromKey = NormalizeSlotId(e.fromSlotId);
            string toKey   = NormalizeSlotId(e.toSlotId);

            // целевой (приёмник) data-слот по имени
            var toAnyDataSlot = FindAnyDataSlotByName(to, toKey);
            if (toAnyDataSlot == null)
            {
                errors.Add($"DATA connect failed: target slot not found {to.NodeName}.{toKey}");
                return;
            }

            // 1) Ore
            var fromOre = FindDataOutByName<environment.Ore.Ore>(from, fromKey);
            if (fromOre != null && TryReflectConnect(toAnyDataSlot, fromOre)) return;

            // 2) Building
            var fromBld = FindDataOutByName<BuildingBase>(from, fromKey);
            if (fromBld != null && TryReflectConnect(toAnyDataSlot, fromBld)) return;

            // 3) InventoryItemData (для Put.Item и т.п.)
            var fromItem = FindDataOutByName<Inventory.InventoryItemData>(from, fromKey);
            if (fromItem != null && TryReflectConnect(toAnyDataSlot, fromItem)) return;

            // 4) Generic fallback
            var fromAny = FindAnyDataOutSlotByName(from, fromKey);
            if (fromAny != null && TryReflectConnect(toAnyDataSlot, fromAny)) return;

            errors.Add($"DATA connect failed: {from.NodeName}.{fromKey} -> {to.NodeName}.{toKey}");
        }

        private static string NormalizeSlotId(string id)
        {
            if (string.IsNullOrEmpty(id)) return id;
            string s = id;
            s = TrimSuffix(s, "_Input_Data");
            s = TrimSuffix(s, "_Output_Data");
            s = TrimSuffix(s, "_Input");
            s = TrimSuffix(s, "_Output");
            s = TrimSuffix(s, "_Data");
            s = s.Replace('_', ' ').Trim();
            return s;
        }

        private static string TrimSuffix(string s, string suffix)
        {
            if (s.EndsWith(suffix, StringComparison.Ordinal))
                return s.Substring(0, s.Length - suffix.Length);
            return s;
        }

        private static ProgNodeDataSlot<T> FindDataOutByName<T>(ProgNodeBase node, string name)
        {
            if (string.IsNullOrEmpty(name)) return null;
            foreach (var s in node.Slots)
                if (s is ProgNodeDataSlot<T> d && string.Equals(d.SlotName, name, StringComparison.Ordinal))
                    return d;
            return null;
        }

        private static ProgNodeSlotBase FindAnyDataOutSlotByName(ProgNodeBase node, string name)
        {
            if (string.IsNullOrEmpty(name)) return null;
            foreach (var s in node.Slots)
                if (IsDataSlot(s) && string.Equals(s.SlotName, name, StringComparison.Ordinal))
                    return s;
            return null;
        }

        private static ProgNodeSlotBase FindAnyDataSlotByName(ProgNodeBase node, string name)
        {
            if (string.IsNullOrEmpty(name)) return null;
            foreach (var s in node.Slots)
                if (IsDataSlot(s) && string.Equals(s.SlotName, name, StringComparison.Ordinal))
                    return s;
            return null;
        }

        // ---------- подключение STREAM ----------
        private static ProgNodeSlotBase FindStreamSlotByName(ProgNodeBase node, string slotName)
        {
            return node.Slots.FirstOrDefault(s => !IsDataSlot(s) &&
                                                  string.Equals(s.SlotName, slotName, StringComparison.Ordinal));
        }

        private static void TryConnectStream(ProgNodeBase from, ProgNodeBase to, GraphEdgeDTO e, List<string> errors)
        {
            var fromKey = N(e.fromSlotId); // ожидаем "Next"/"Success"/"Fail"/...
            var toKey   = N(e.toSlotId);   // обычно "In" у приемника

            var outSlot = FindStreamSlotByName(from, fromKey)
                          ?? FindStreamSlotByName(from, "Next");
            var inSlot  = FindStreamSlotByName(to,   toKey)
                          ?? FindStreamSlotByName(to,   "In");

            if (outSlot != null && inSlot != null)
            {
                outSlot.Connect(to);
                Debug.Log($"[Builder] Connected stream by name: {from.NodeName}.{outSlot.SlotName} → {to.NodeName}.{inSlot.SlotName}");
                return;
            }

            var outs = from.Slots.Where(s => !IsDataSlot(s)).ToList();
            if (outs.Count == 0)
            {
                errors.Add($"No stream outputs on {from.NodeName}");
                return;
            }

            var anyOut = outs.FirstOrDefault(s => s.ConnectedNode == null) ?? outs.Last();
            anyOut.Connect(to);
            Debug.Log($"[Builder] Connected stream (fallback): {from.NodeName} → {to.NodeName}");
        }

        // ---------- вспомогательные методы ----------
        private static bool IsDataSlot(ProgNodeSlotBase slot)
        {
            var t = slot.GetType();
            return t.IsGenericType && t.GetGenericTypeDefinition() == typeof(ProgNodeDataSlot<>);
        }

        /// <summary>
        /// Типобезопасно вызывает target.ConnectToDataSlot&lt;S&gt;(source, null) через reflection,
        /// где target = ProgNodeDataSlot&lt;T&gt;, source = ProgNodeDataSlot&lt;S&gt;.
        /// </summary>
        private static bool TryReflectConnect(ProgNodeSlotBase targetDataSlot, ProgNodeSlotBase sourceDataSlot)
        {
            try
            {
                var targetType = targetDataSlot.GetType(); // ProgNodeDataSlot<T>
                var sourceType = sourceDataSlot.GetType(); // ProgNodeDataSlot<S>
                if (!targetType.IsGenericType || !sourceType.IsGenericType) return false;


                var sourceArg = sourceType.GetGenericArguments()[0]; // S
                var method = targetType.GetMethod("ConnectToDataSlot");
                if (method == null) return false;
                if (method.IsGenericMethodDefinition)
                    method = method.MakeGenericMethod(sourceArg);

                method.Invoke(targetDataSlot, new object[] { sourceDataSlot, null });
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[Builder] Reflect connect failed: {ex.Message}");
                return false;
            }
        }
    }
}
