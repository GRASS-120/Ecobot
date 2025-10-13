using System.Collections.Generic;
using Grid.BuildingSystem.Buildings;
using Grid.BuildingSystem.Buildings.Types.Furnance;
using Grid.BuildingSystem.Buildings.Types.Furnance.UI;
using GUI.Gameplay.Windows.View;
using GUI.UIFramework;
using Inventory;
using Inventory.UI;
using R3;

namespace GUI.Gameplay.Windows.Controller
{
    [Window(WindowType.Popup, "FurnaceWindow")]
    public class FurnaceWindowController : WindowController<FurnaceWindowView>
    {
        public override string Id => "FurnaceWindow";

        private BuildingFurnace _furnace;
        private MouseInventoryItemUI _mouseUI;
        private InventorySelectionService _selection;
        private InventorySystem _quickMoveTarget;

        private readonly List<(SmeltingRecipeItemView view, SmeltingRecipeData recipe)> _recipeItems = new();

        public void Init(
            BuildingFurnace furnace,
            MouseInventoryItemUI mouseUI,
            InventorySelectionService selection,
            InventorySystem quickMoveTarget)
        {
            _furnace = furnace;
            _mouseUI = mouseUI;
            _selection = selection;
            _quickMoveTarget = quickMoveTarget;
        }

        public override void OnOpen()
        {
            base.OnOpen();

            BuildRecipeList();
            BindSlots();           // <-- вместо динамического AddComponent
            RefreshAll();

            _furnace.OnSlotsChanged
                .Subscribe(_ =>
                {
                    RefreshSlots();
                    RefreshRecipesAvailability();
                })
                .AddTo(Subs);

            _furnace.OnProgressChanged
                .Subscribe(p => View.SetProgress(p))
                .AddTo(Subs);

            _furnace.OnPoweredChanged
                .Subscribe(on => View.SetElectricity(on))
                .AddTo(Subs);

            _furnace.OnRecipeChanged
                .Subscribe(_ =>
                {
                    RefreshResultDisplay();
                    RefreshRecipesAvailability();
                    BindSlots();   // <-- обновить валидаторы под новый рецепт
                })
                .AddTo(Subs);

            View.CloseButton.OnClickAsObservable()
                .Subscribe(_ => Close())
                .AddTo(Subs);
        }

        public override void OnClose()
        {
            base.OnClose();
            ClearRecipeList();
        }

        public void SelectRecipe(SmeltingRecipeData recipe)
        {
            _furnace.SelectRecipe(recipe);
            RefreshResultDisplay();
            RefreshRecipesAvailability();
        }
        
        private void BindSlots()
        {
            var inv = _furnace.FurnaceInventory;

            View.OreSlot?.Init(
                inv,
                _selection,
                _furnace.OreIndex,
                _mouseUI,
                quickMoveTarget: null
            );

            View.FuelSlot?.Init(
                inv,
                _selection,
                _furnace.FuelIndex,
                _mouseUI,
                quickMoveTarget: null
            );

            View.OutputSlot?.Init(
                inv,
                _selection,
                _furnace.OutputIndex,
                _mouseUI,
                quickMoveTarget: null
            );

            RefreshSlots();
        }

        private void BuildRecipeList()
        {
            ClearRecipeList();

            var list = _furnace.Recipes;
            if (list != null)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    var r = list[i];
                    if (r == null || r.inputItem == null || r.resultItem == null) continue;

                    var item = View.CreateRecipeItem();
                    bool available = _furnace.IsRecipePotentiallyAvailable(r);
                    item.Init(r, this, available);
                    _recipeItems.Add((item, r));
                }
            }

            if (_furnace.CurrentRecipe == null && _recipeItems.Count > 0)
            {
                var ore = _furnace.FurnaceInventory.GetSlot(_furnace.OreIndex).ItemData;
                SmeltingRecipeData match = null;

                if (ore != null)
                {
                    for (int i = 0; i < _recipeItems.Count; i++)
                    {
                        if (_recipeItems[i].recipe.inputItem == ore)
                        {
                            match = _recipeItems[i].recipe;
                            break;
                        }
                    }
                }

                SelectRecipe(match ?? _recipeItems[0].recipe);
            }
        }

        private void ClearRecipeList()
        {
            var views = new List<SmeltingRecipeItemView>(_recipeItems.Count);
            foreach (var (view, _) in _recipeItems) views.Add(view);
            View.ClearRecipeList(views);
            _recipeItems.Clear();
        }

        private void RefreshRecipesAvailability()
        {
            foreach (var (view, recipe) in _recipeItems)
            {
                bool available = _furnace.IsRecipePotentiallyAvailable(recipe);
                view.UpdateVisuals(available);
            }
        }

        private void RefreshSlots()
        {
            View.OreSlot?.Refresh();
            View.FuelSlot?.Refresh();
            View.OutputSlot?.Refresh();
        }

        private void RefreshResultDisplay()
        {
            var r = _furnace.CurrentRecipe;
            if (r != null)
            {
                View.SetResultDisplay(r.resultItem.icon, r.resultItem.displayName, r.resultAmount);
            }
            else
            {
                View.SetResultDisplay(null, "", 0);
            }
        }

        private void RefreshAll()
        {
            View.SetElectricity(_furnace.IsPowered);
            View.SetProgress(_furnace.Progress01);
            RefreshResultDisplay();
            RefreshSlots();
            RefreshRecipesAvailability();
        }
    }
}