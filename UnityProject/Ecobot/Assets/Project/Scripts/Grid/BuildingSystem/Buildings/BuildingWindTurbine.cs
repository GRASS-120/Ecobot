using System.Collections.Generic;
using Grid.BuildingSystem.Buildings.Visual;
using Grid.BuildingSystem.PowerSystem;
using GUI.Gameplay.Windows.Controller;
using InteractionSystem;
using Inventory.CraftingSystem;
using UnityEngine;

namespace Grid.BuildingSystem.Buildings
{
    public class BuildingWindTurbine : BuildingBase, IPowerNode, IInteractable, IPowerAnchorProvider
    {
        [SerializeField] private int producedUnits = 3;
        [SerializeField] private Transform wireAnchor;
        [SerializeField] private List<RecipeIngredient> repairCost = new();
        [SerializeField] private PowerWireProximityReactor proximityReactor;
        [SerializeField] private BuildingWindTurbineVisual visual;
        
        private readonly List<IPowerNode> _inputs = new();
        private readonly List<IPowerNode> _outputs = new();
        private bool _isBroken;

        public PowerNodeType NodeType => PowerNodeType.Generator;
        public int ProducedUnits => _isBroken ? 0 : producedUnits;
        public int ConsumedUnits => 0;
        public int MaxInputs => 0;
        public int MaxOutputs => 1;
        public bool IsBroken => _isBroken;
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
        
        public bool CanAcceptInputFrom(IPowerNode from) => false;
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

            if (IsBroken)
            {
                OpenRepairPopup();
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
        
        private void OpenRepairPopup()
        {
            var inventoryHolder = _context.PlayerManager.Inventory;
            var crafting = inventoryHolder.CraftingSystem;

            _context.WindowManager.OpenWindow<PowerRepairPopupController>(controller =>
            {
                controller.Init(
                    craftingSystem: crafting,
                    cost: repairCost,
                    title: "Ремонт генератора",
                    tryConsume: () => TryConsumeForRepair(crafting),
                    onSuccess: () =>
                    {
                        Repair();
                    }
                );
            });
        }

        private bool TryConsumeForRepair(CraftingSystem crafting)
        {
            var inv = _context.PlayerManager.Inventory;
            return crafting.TryConsume(repairCost, inv.MainInventory, inv.HotbarInventorySystem);
        }
        
        public bool TryConnectInput(IPowerNode from) => false;

        public bool TryConnectOutput(IPowerNode to)
        {
            if (!CanProvideOutputTo(to)) return false;
            if (!to.CanAcceptInputFrom(this)) return false;

            _outputs.Add(to);
            (to as BuildingBase)?.BuildingAssetData.ToString(); // сохраним стиль обращений
            return (to.TryConnectInput(this));
        }

        public void Disconnect(IPowerNode other)
        {
            _outputs.Remove(other);
            (other as IPowerNodeInternalDisconnect)?.InternalRemoveInput(this);
        }

        public void OnPowerStateChanged(bool isPowered)
        {
            // генератору не нужно
        }

        public void MarkBroken()
        {
            if (_isBroken) return;
            _isBroken = true;
            visual?.OnBroken();
            _context.PowerGridService.NotifyNodeStateChanged(this);
        }

        public void Repair()
        {
            if (!_isBroken) return;
            _isBroken = false;
            visual?.OnRepaired();
            _context.PowerGridService.NotifyNodeStateChanged(this);
        }
    }
    
    // Вспомогательный внутренний интерфейс для симметричного удаления входа
    internal interface IPowerNodeInternalDisconnect
    {
        void InternalRemoveInput(IPowerNode from);
        void InternalRemoveOutput(IPowerNode to);
    }
}