using System.Collections.Generic;
using Grid.BuildingSystem.PowerSystem;
using InteractionSystem;
using UnityEngine;

namespace Grid.BuildingSystem.Buildings
{
    public class BuildingPowerPole : BuildingBase, IPowerNode, IPowerNodeInternalDisconnect, IInteractable, IPowerAnchorProvider
    {
        [SerializeField] private Transform wireAnchor;
        [SerializeField] private PowerWireProximityReactor proximityReactor;

        private readonly List<IPowerNode> _inputs = new();
        private readonly List<IPowerNode> _outputs = new();

        public PowerNodeType NodeType => PowerNodeType.Pole;
        public int ProducedUnits => 0;
        public int ConsumedUnits => 0;
        public int MaxInputs => 1;
        public int MaxOutputs => 3;
        public bool IsBroken => false;
        public BuildingBase Building => this;

        public IReadOnlyList<IPowerNode> Inputs => _inputs;
        public IReadOnlyList<IPowerNode> Outputs => _outputs;
        public Transform WireAnchor => wireAnchor != null ? wireAnchor : transform;
        
        public override void Init(
            BuildingAssetData data,
            Vector2Int origin,
            BuildingContext context, 
            BuildingAssetData.Dir dir = BuildingAssetData.Dir.Down)
        {
            base.Init(data, origin, context, dir); 
            proximityReactor?.Init(_context, this);
        }
        
        public bool CanAcceptInputFrom(IPowerNode from)
        {
            if (_inputs.Count >= MaxInputs) return false;
            return from.NodeType == PowerNodeType.Generator || from.NodeType == PowerNodeType.Pole;
        }

        public bool CanProvideOutputTo(IPowerNode to)
        {
            if (_outputs.Count >= MaxOutputs) return false;
            return to.NodeType == PowerNodeType.Pole || to.NodeType == PowerNodeType.Consumer;
        }
        
        public void Interact(IInteractor interactor)
        {
            var tool = _context.PowerWireToolService;
            if (tool == null) return;

            if (tool.IsActive)
            {
                tool.HandleInteract(this);
                return;
            }

            // Нет свободных выходов — не заходим в режим проводов
            if (Outputs.Count >= MaxOutputs)
                return;

            tool.Begin(this);
        }

        public void AltInteract(IInteractor interactor)
        {
            var tool = _context.PowerWireToolService;
            if (tool == null) return;

            if (tool.IsActive)
            {
                tool.HandleAltInteract(this);
            }
            else
            {
                var last = Outputs.Count > 0 ? Outputs[Outputs.Count - 1] : null;
                if (last != null)
                {
                    _context.PowerGridService.Disconnect(this, last);
                }
            }
        }

        public bool TryConnectInput(IPowerNode from)
        {
            if (!CanAcceptInputFrom(from)) return false;
            _inputs.Add(from);
            return true;
        }

        public bool TryConnectOutput(IPowerNode to)
        {
            if (!CanProvideOutputTo(to)) return false;
            if (!to.CanAcceptInputFrom(this)) return false;

            _outputs.Add(to);
            return to.TryConnectInput(this);
        }

        public void Disconnect(IPowerNode other)
        {
            _outputs.Remove(other);
            _inputs.Remove(other);

            (other as IPowerNodeInternalDisconnect)?.InternalRemoveInput(this);
            (other as IPowerNodeInternalDisconnect)?.InternalRemoveOutput(this);
        }

        public void OnPowerStateChanged(bool isPowered)
        {
            // столбу не нужно
        }

        public void MarkBroken() { }
        public void Repair() { }

        void IPowerNodeInternalDisconnect.InternalRemoveInput(IPowerNode from)
        {
            _inputs.Remove(from);
        }

        void IPowerNodeInternalDisconnect.InternalRemoveOutput(IPowerNode to)
        {
            _outputs.Remove(to);
        }
    }
}