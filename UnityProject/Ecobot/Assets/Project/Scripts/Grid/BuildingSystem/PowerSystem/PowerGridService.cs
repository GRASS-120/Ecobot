using System.Collections.Generic;
using System.Linq;
using R3;
using UnityEngine;

namespace Grid.BuildingSystem.PowerSystem
{
    public class PowerGridService : MonoBehaviour
    {
        public IEnumerable<IPowerNode> Nodes => _nodes;
        
        private readonly HashSet<IPowerNode> _nodes = new();
        private bool _isRecomputing;

        private readonly Subject<Unit> _changed = new();
        public Observable<Unit> Changed => _changed;

        public void Register(IPowerNode node)
        {
            if (node == null || _nodes.Contains(node)) return;
            _nodes.Add(node);
            Recompute();
        }

        public void Unregister(IPowerNode node)
        {
            if (node == null || !_nodes.Contains(node)) return;

            // разрываем все связи с узлом
            foreach (var other in node.Inputs.ToList())
                other.Disconnect(node);
            foreach (var other in node.Outputs.ToList())
                node.Disconnect(other);

            _nodes.Remove(node);
            Recompute();
        }

        public bool Connect(IPowerNode a, IPowerNode b)
        {
            if (a == null || b == null) return false;
            if (!_nodes.Contains(a) || !_nodes.Contains(b)) return false;
            if (a == b) return false;

            // Пытаемся направить связь из "источника" в "приёмник"
            if (a.CanProvideOutputTo(b) && b.CanAcceptInputFrom(a) && a.TryConnectOutput(b))
            {
                Recompute();
                return true;
            }

            if (b.CanProvideOutputTo(a) && a.CanAcceptInputFrom(b) && b.TryConnectOutput(a))
            {
                Recompute();
                return true;
            }

            return false;
        }

        public void Disconnect(IPowerNode a, IPowerNode b)
        {
            if (a == null || b == null) return;
            a.Disconnect(b);
            b.Disconnect(a);
            Recompute();
        }

        public void NotifyNodeStateChanged(IPowerNode node)
        {
            if (node == null || !_nodes.Contains(node)) return;
            Recompute();
        }

        private void Recompute()
        {
            if (_isRecomputing) return;
            _isRecomputing = true;

            var components = BuildComponents();

            foreach (var net in components)
            {
                net.TotalProduction = net.Nodes
                    .Where(n => n.NodeType == PowerNodeType.Generator && !n.IsBroken)
                    .Sum(n => n.ProducedUnits);

                net.TotalConsumption = net.Nodes
                    .Where(n => n.NodeType == PowerNodeType.Consumer)
                    .Sum(n => n.ConsumedUnits);

                bool anyGeneratorBroken = net.Nodes.Any(n => n.NodeType == PowerNodeType.Generator && n.IsBroken);
                bool overload = net.TotalConsumption > net.TotalProduction;

                // Если перегруз и генераторы ещё не сломаны — "трипаем" все генераторы в сети
                if (overload && !anyGeneratorBroken)
                {
                    foreach (var g in net.Nodes.Where(n => n.NodeType == PowerNodeType.Generator))
                        g.MarkBroken();

                    // После трипа мощность сети = 0, питание отключено
                    anyGeneratorBroken = true;
                }

                bool powered = !anyGeneratorBroken && !overload;

                // Сообщаем потребителям состояние питания
                foreach (var c in net.Nodes.Where(n => n.NodeType == PowerNodeType.Consumer))
                    c.OnPowerStateChanged(powered);
            }

            _changed.OnNext(Unit.Default);
            _isRecomputing = false;
        }

        private List<PowerNetwork> BuildComponents()
        {
            var result = new List<PowerNetwork>();
            var visited = new HashSet<IPowerNode>();
            int id = 1;

            foreach (var node in _nodes)
            {
                if (visited.Contains(node)) continue;

                var stack = new Stack<IPowerNode>();
                var net = new PowerNetwork(id++);
                stack.Push(node);
                visited.Add(node);

                while (stack.Count > 0)
                {
                    var cur = stack.Pop();
                    net.Nodes.Add(cur);

                    foreach (var nb in GetNeighbors(cur))
                    {
                        if (visited.Add(nb))
                            stack.Push(nb);
                    }
                }

                result.Add(net);
            }

            return result;
        }

        private IEnumerable<IPowerNode> GetNeighbors(IPowerNode node)
        {
            // Связность неориентированная: входы + выходы
            foreach (var i in node.Inputs) yield return i;
            foreach (var o in node.Outputs) yield return o;
        }
    }
}