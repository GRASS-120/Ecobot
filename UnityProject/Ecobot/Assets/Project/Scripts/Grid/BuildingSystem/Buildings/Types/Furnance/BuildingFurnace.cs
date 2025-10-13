using System.Collections.Generic;
using Grid.BuildingSystem.Buildings.Base;
using Grid.BuildingSystem.Buildings.Reactors;
using Grid.BuildingSystem.Buildings.Types.WindTurbine;
using Grid.BuildingSystem.Buildings.Visual;
using Grid.BuildingSystem.PowerSystem;
using InteractionSystem;
using UnityEngine;

namespace Grid.BuildingSystem.Buildings
{
    public class BuildingFurnace : BuildingBase, IPowerNode, IPowerNodeInternalDisconnect, IInteractable, IPowerAnchorProvider
    {
        [SerializeField] private int consumedUnits = 1;
        [SerializeField] private Transform wireAnchor;
        [SerializeField] private PowerWireProximityReactor proximityReactor;
        [SerializeField] private BuildingFurnaceVisual visual;

        private readonly List<IPowerNode> _inputs = new();
        private readonly List<IPowerNode> _outputs = new();
        private bool _isPowered;

        public bool IsPowered => _isPowered;
        public PowerNodeType NodeType => PowerNodeType.Consumer;
        public int ProducedUnits => 0;
        public int ConsumedUnits => consumedUnits;
        public int MaxInputs => 1;
        public int MaxOutputs => 0;
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
            visual?.Init(this, _context);
            proximityReactor?.Init(_context, this);
        }
        
        public void Interact(IInteractor interactor)
        {
            var tool = _context.PowerWireToolService;
            if (tool == null) return;

            if (tool.IsActive)
            {
                tool.HandleInteract(this);
            }
            // если режим не активен — ничего, у печи нет выходов
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
                var from = Inputs.Count > 0 ? Inputs[0] : null;
                if (from != null)
                {
                    _context.PowerGridService.Disconnect(from, this);
                }
            }
        }
        
        public bool CanAcceptInputFrom(IPowerNode from)
        {
            if (_inputs.Count >= MaxInputs) return false;
            return from.NodeType == PowerNodeType.Generator || from.NodeType == PowerNodeType.Pole;
        }

        public bool CanProvideOutputTo(IPowerNode to) => false;

        public bool TryConnectInput(IPowerNode from)
        {
            if (!CanAcceptInputFrom(from)) return false;
            _inputs.Add(from);
            return true;
        }

        public bool TryConnectOutput(IPowerNode to) => false;

        public void Disconnect(IPowerNode other)
        {
            _inputs.Remove(other);
            (other as IPowerNodeInternalDisconnect)?.InternalRemoveOutput(this);
        }

        public void OnPowerStateChanged(bool isPowered)
        {
            _isPowered = isPowered;
            visual?.SetPowered(isPowered);
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