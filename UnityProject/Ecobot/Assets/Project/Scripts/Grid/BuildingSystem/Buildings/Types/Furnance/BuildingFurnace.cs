using System.Collections;
using System.Collections.Generic;
using Grid.BuildingSystem.Buildings.Base;
using Grid.BuildingSystem.Buildings.Reactors;
using Grid.BuildingSystem.Buildings.Types.WindTurbine;
using Grid.BuildingSystem.Buildings.Visual;
using Grid.BuildingSystem.PowerSystem;
using GUI.Gameplay.Windows.Controller;
using InteractionSystem;
using Inventory;
using R3;
using UnityEngine;

namespace Grid.BuildingSystem.Buildings.Types.Furnance
{
    public class BuildingFurnace : BuildingBase, IPowerNode, IPowerNodeInternalDisconnect, IInteractable, IPowerAnchorProvider
    {
        [SerializeField] private int consumedUnits = 1;
        [SerializeField] private Transform wireAnchor;
        [SerializeField] private PowerWireProximityReactor proximityReactor;
        [SerializeField] private BuildingFurnaceVisual visual;

        [SerializeField] private List<SmeltingRecipeData> recipes = new();

        // оставляем для совместимости: окно и логика могут опираться на это поле
        [SerializeField] private InventoryItemData coalItem;

        [Header("Whitelists (drag & drop items)")]
        [Tooltip("Если список НЕ пуст — только эти предметы разрешены как топливо.")]
        [SerializeField] private List<InventoryItemData> fuelWhitelist = new();
        [Tooltip("Если список НЕ пуст — только эти предметы разрешены как руда (input).")]
        [SerializeField] private List<InventoryItemData> oreWhitelist = new();

        private readonly List<IPowerNode> _inputs = new();
        private readonly List<IPowerNode> _outputs = new();
        private bool _isPowered;
        private SmeltingRecipeData _currentRecipe;
        private InventorySystem _furnaceInv = new InventorySystem(3);
        private const int ORE_INDEX = 0;
        private const int FUEL_INDEX = 1;
        private const int OUTPUT_INDEX = 2;
        private Coroutine _smeltRoutine;
        private float _progress01;

        // === ПУБЛИЧНЫЙ API (как ждёт окно) ===
        public List<SmeltingRecipeData> Recipes => recipes;
        public SmeltingRecipeData CurrentRecipe => _currentRecipe;
        public InventorySystem FurnaceInventory => _furnaceInv;
        public int OreIndex => ORE_INDEX;
        public int FuelIndex => FUEL_INDEX;
        public int OutputIndex => OUTPUT_INDEX;
        public InventoryItemData CoalItem => coalItem;
        public float Progress01 => _progress01;

        // UI events
        public Subject<Unit> OnSlotsChanged = new();
        public Subject<float> OnProgressChanged = new();
        public Subject<bool> OnPoweredChanged = new();
        public Subject<Unit> OnRecipeChanged = new();

        // Power node
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
            
            _progress01 = 0f;
            OnProgressChanged.OnNext(_progress01);
            
            _furnaceInv.OnInventorySlotChanged
                .Subscribe(changedSlot =>
                {
                    int idx = _furnaceInv.IndexOf(changedSlot);

                    // Если в слот руды положили предмет и рецепт не выбран — выбрать рецепт по руде
                    if (idx == ORE_INDEX && _currentRecipe == null)
                    {
                        TryAutoSelectRecipeFromOreSlot();
                    }

                    OnSlotsChanged.OnNext(Unit.Default);
                    EvaluateAndRun();
                })
                .AddTo(this);

            // Если при спавне руда уже лежит
            TryAutoSelectRecipeFromOreSlot();
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

            var wnd = _context.WindowManager.GetController<FurnaceWindowController>();
            if (wnd.IsOpen)
            {
                _context.WindowManager.CloseWindow<FurnaceWindowController>();
            }
            else
            {
                var invHolder = _context.PlayerManager.Inventory;
                _context.WindowManager.OpenWindow<FurnaceWindowController>(c =>
                {
                    c.Init(
                        this,
                        _context.MouseInventoryItemUI,
                        invHolder.InventorySelectionService,
                        quickMoveTarget: invHolder.MainInventory
                    );
                });
            }
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
        
        private void TryAutoSelectRecipeFromOreSlot()
        {
            var ore = _furnaceInv.GetSlot(ORE_INDEX);
            if (ore.ItemData == null) return;

            var match = recipes.Find(r => r != null && r.inputItem == ore.ItemData);
            if (match != null && match != _currentRecipe)
            {
                _currentRecipe = match;
                OnRecipeChanged.OnNext(Unit.Default);
            }
        }

        // ==== МЕТОДЫ, КОТОРЫЕ ЖДЁТ ОКНО ====

        // Вызывается из FurnaceWindowController.SelectRecipe(...)
        public void SelectRecipe(SmeltingRecipeData r)
        {
            if (r == _currentRecipe) return;
            _currentRecipe = r;
            OnRecipeChanged.OnNext(Unit.Default);
            EvaluateAndRun();
        }

        // Вызывается из FurnaceWindowController для подсветки рецептов
        public bool IsRecipePotentiallyAvailable(SmeltingRecipeData r)
        {
            if (r == null) return false;
            // Лёгкая проверка на пригодность (оставляем логически простой индикатор)
            // Можно усложнить по желанию.
            return true;
        }

        // ===== Whitelists helpers =====
        public bool IsFuelItem(InventoryItemData item)
        {
            if (fuelWhitelist != null && fuelWhitelist.Count > 0)
                return item != null && fuelWhitelist.Contains(item);

            if (coalItem != null) return item == coalItem;
            return item != null;
        }

        public bool IsOreItem(InventoryItemData item)
        {
            if (oreWhitelist != null && oreWhitelist.Count > 0)
                return item != null && oreWhitelist.Contains(item);

            if (item == null) return false;
            foreach (var r in recipes)
                if (r != null && r.inputItem == item) return true;
            return false;
        }

        // ===== ЛОГИКА ПЛАВКИ =====

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

        private void EvaluateAndRun()
        {
            if (_smeltRoutine != null) return;
            if (!_isPowered) return;

            if (_currentRecipe == null)
            {
                TryAutoSelectRecipeFromOreSlot();
                if (_currentRecipe == null) return;
            }

            if (!HasResourcesForOneOutput(_currentRecipe) || !CanAcceptOutput(_currentRecipe))
            {
                var prev = _currentRecipe;
                TryAutoSelectRecipeFromOreSlot();
                if (_currentRecipe != prev && _currentRecipe != null)
                {
                    if (!HasResourcesForOneOutput(_currentRecipe) || !CanAcceptOutput(_currentRecipe))
                        return;
                }
                else
                {
                    return;
                }
            }

            _smeltRoutine = StartCoroutine(SmeltRoutine());
        }

        private bool HasResourcesForOneOutput(SmeltingRecipeData r)
        {
            var ore = _furnaceInv.GetSlot(ORE_INDEX);
            if (ore.ItemData != r.inputItem) return false;
            if (ore.StackSize < r.inputAmountPerOutput) return false;

            if (r.fuelPerOutput > 0)
            {
                var fuel = _furnaceInv.GetSlot(FUEL_INDEX);
                if (!IsFuelItem(fuel.ItemData)) return false;
                if (fuel.StackSize < r.fuelPerOutput) return false;
            }
            return true;
        }

        private bool CanAcceptOutput(SmeltingRecipeData r)
        {
            var outSlot = _furnaceInv.GetSlot(OUTPUT_INDEX);
            if (outSlot.ItemData == null) return true;
            if (outSlot.ItemData != r.resultItem) return false;
            return outSlot.CanAddInStack(r.resultAmount);
        }

        private IEnumerator SmeltRoutine()
        {
            _progress01 = 0f;
            OnProgressChanged.OnNext(_progress01);

            float time = _currentRecipe.smeltTimeSeconds;
            float t = 0f;

            visual?.SetSmelting(true);

            while (t < time)
            {
                if (!_isPowered)
                {
                    visual?.SetSmelting(false);
                    yield return null;
                    continue;
                }
                if (!HasResourcesForOneOutput(_currentRecipe) || !CanAcceptOutput(_currentRecipe))
                {
                    visual?.SetSmelting(false);
                    yield return null;
                    continue;
                }

                if (t == 0f) visual?.SetSmelting(true);
                t += UnityEngine.Time.deltaTime;
                _progress01 = Mathf.Clamp01(t / time);
                OnProgressChanged.OnNext(_progress01);
                yield return null;
            }

            if (!_isPowered || !HasResourcesForOneOutput(_currentRecipe) || !CanAcceptOutput(_currentRecipe))
            {
                visual?.SetSmelting(false);
                _progress01 = 0f;
                OnProgressChanged.OnNext(_progress01);
                _smeltRoutine = null;
                EvaluateAndRun();
                yield break;
            }

            // списываем ресурсы
            _furnaceInv.GetSlot(ORE_INDEX).RemoveFromStack(_currentRecipe.inputAmountPerOutput);
            if (_furnaceInv.GetSlot(ORE_INDEX).StackSize <= 0) _furnaceInv.GetSlot(ORE_INDEX).ClearSlot();

            if (_currentRecipe.fuelPerOutput > 0) // <-- фикс: r -> _currentRecipe
            {
                _furnaceInv.GetSlot(FUEL_INDEX).RemoveFromStack(_currentRecipe.fuelPerOutput);
                if (_furnaceInv.GetSlot(FUEL_INDEX).StackSize <= 0) _furnaceInv.GetSlot(FUEL_INDEX).ClearSlot();
            }

            OnSlotsChanged.OnNext(Unit.Default);
            _furnaceInv.NotifySlotChanged(ORE_INDEX);
            _furnaceInv.NotifySlotChanged(FUEL_INDEX);

            if (_furnaceInv.GetSlot(OUTPUT_INDEX).ItemData == null)
                _furnaceInv.GetSlot(OUTPUT_INDEX).UpdateSlot(_currentRecipe.resultItem, 0);

            if (_furnaceInv.GetSlot(OUTPUT_INDEX).ItemData == _currentRecipe.resultItem)
            {
                _furnaceInv.GetSlot(OUTPUT_INDEX).AddToStack(_currentRecipe.resultAmount);
            }

            OnSlotsChanged.OnNext(Unit.Default);
            _furnaceInv.NotifySlotChanged(OUTPUT_INDEX);

            visual?.SetSmelting(false);

            _progress01 = 0f;
            OnProgressChanged.OnNext(_progress01);

            _smeltRoutine = null;
            EvaluateAndRun();
        }

        public void OnPowerStateChanged(bool isPowered)
        {
            _isPowered = isPowered;
            OnPoweredChanged.OnNext(isPowered);
            EvaluateAndRun();
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
