using System.Collections.Generic;
using System.Linq;
using Grid.BuildingSystem.Buildings.Base;
using Player;
using UnityEngine;

namespace Grid.BuildingSystem.PowerSystem
{
    public class PowerWireToolService : MonoBehaviour
    {
        [SerializeField] private PowerGridService powerGridService;
        [SerializeField] private PlayerManager playerManager;
        [SerializeField] private PowerNodeHighlighter highlighter;
        [SerializeField] private float hoverRadius = 1.5f;
        [SerializeField] private Vector3 playerPreviewOffset = new Vector3(0, 1.2f, 0);
        
        private IPowerNode _source;
        private bool _isActive;
        private readonly HashSet<IPowerNode> _nearbyNodes = new();
        public bool HoverIsValid { get; private set; }

        public bool IsActive => _isActive;
        public IPowerNode Source => _source;
        public IPowerNode CurrentHover { get; private set; }

        private void Update()
        {
            if (!_isActive || _source == null || powerGridService == null || playerManager == null)
            {
                CurrentHover = null;
                HoverIsValid = false;
                return;
            }

            var playerPos = playerManager.transform.position + playerPreviewOffset;

            // Берём кандидатов: приоритет — узлы в зоне триггера, иначе — по радиусу
            IPowerNode best = null;
            float bestDist = float.MaxValue;

            // Кандидаты из зоны близости (триггера)
            foreach (var node in _nearbyNodes)
            {
                if (node == null || node == _source) continue;

                var toPos = ((node as IPowerAnchorProvider)?.WireAnchor?.position) 
                            ?? (node as BuildingBase)?.transform.position 
                            ?? Vector3.zero;
                var dist = Vector3.Distance(playerPos, toPos);
                if (dist < bestDist)
                {
                    bestDist = dist;
                    best = node;
                }
            }

            // Если нет кандидатов из зоны — ищем по радиусу
            if (best == null)
            {
                foreach (var node in powerGridService.Nodes)
                {
                    if (node == null || node == _source) continue;

                    var toPos = ((node as IPowerAnchorProvider)?.WireAnchor?.position) 
                                ?? (node as BuildingBase)?.transform.position 
                                ?? Vector3.zero;
                    var dist = Vector3.Distance(playerPos, toPos);
                    if (dist < hoverRadius && dist < bestDist)
                    {
                        bestDist = dist;
                        best = node;
                    }
                }
            }

            CurrentHover = best;

            if (CurrentHover != null)
            {
                var forwardValid = _source.CanProvideOutputTo(CurrentHover) && CurrentHover.CanAcceptInputFrom(_source);
                var reverseValid = CurrentHover.CanProvideOutputTo(_source) && _source.CanAcceptInputFrom(CurrentHover);
                HoverIsValid = forwardValid || reverseValid;
            }
            else
            {
                HoverIsValid = false;
            }

            if (highlighter != null)
            {
                highlighter.RefreshPreview();
            }
        }
        
        public void Begin(IPowerNode source)
        {
            // Не позволяем входить в режим, если у источника нет свободных выходов
            if (source == null || source.Outputs.Count >= source.MaxOutputs)
                return;

            _source = source;
            _isActive = true;
            if (highlighter != null)
            {
                highlighter.StartPreview(this, playerManager);
            }
        }

        public void HandleInteract(IPowerNode target)
        {
            if (!_isActive || target == null)
            {
                Begin(target);
                return;
            }

            if (_source == null)
            {
                Begin(target);
                return;
            }

            if (_source == target)
            {
                Cancel();
                return;
            }

            var targetIsConsumer = target.NodeType == PowerNodeType.Consumer;
            var targetIsPole = target.NodeType == PowerNodeType.Pole;
            var targetIsGenerator = target.NodeType == PowerNodeType.Generator;

            var ok = powerGridService.Connect(_source, target);
            if (ok)
            {
                var noFreeOutputs = _source.Outputs.Count >= _source.MaxOutputs;

                // Выходим из режима: потребитель, столб, генератор или закончились порты
                if (targetIsConsumer || targetIsPole || targetIsGenerator || noFreeOutputs)
                {
                    Cancel();
                }
            }
            else
            {
                // Если кликнули по генератору — тоже выходим (соединение недопустимо)
                if (targetIsGenerator)
                {
                    Cancel();
                }
                // иначе остаёмся в режиме
            }
        }

        public void HandleAltInteract(IPowerNode node)
        {
            if (_isActive)
            {
                if (_source != null && node != null)
                {
                    if (_source.Outputs.Contains(node) || node.Inputs.Contains(_source))
                    {
                        powerGridService.Disconnect(_source, node);
                    }
                    else
                    {
                        Cancel();
                    }
                }
                else
                {
                    Cancel();
                }
                return;
            }

            // Режим не активен: удаляем последний выход узла (подходит для генератора/столба)
            var last = node?.Outputs.LastOrDefault();
            if (last != null)
            {
                powerGridService.Disconnect(node, last);
            }
        }

        public void Cancel()
        {
            _source = null;
            _isActive = false;
            CurrentHover = null;
            if (highlighter != null)
            {
                highlighter.StopPreview();
            }
        }
        
        public void RegisterProximity(IPowerNode node)
        {
            if (node == null) return;
            _nearbyNodes.Add(node);
        }

        public void UnregisterProximity(IPowerNode node)
        {
            if (node == null) return;
            _nearbyNodes.Remove(node);
        }
    }
}