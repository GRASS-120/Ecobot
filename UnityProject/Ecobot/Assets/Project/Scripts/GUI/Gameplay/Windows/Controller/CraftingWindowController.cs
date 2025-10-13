using System.Collections.Generic;
using Grid.BuildingSystem.Buildings.Types.Workbench.UI;
using GUI.Gameplay.Windows.View;
using GUI.UIFramework;
using Inventory;
using Inventory.CraftingSystem;
using R3;

namespace GUI.Gameplay.Windows.Controller
{
    [Window(WindowType.Popup, "CraftingWindow")]
    public class CraftingWindowController : WindowController<CraftingWindowView>
    {
         public override string Id => "CraftingWindow";

        private CraftingSystem _craftingSystem;
        private InventorySystem _mainInventory;
        private InventorySystem _hotbarInventory;
        
        private readonly ReactiveProperty<CraftingRecipeData> _selectedRecipe = new ReactiveProperty<CraftingRecipeData>();
        private readonly List<CraftingRecipeItemView> _recipeItemViews = new List<CraftingRecipeItemView>();
        private readonly List<CraftingIngredientItemView> _ingredientItemViews = new List<CraftingIngredientItemView>();
        
        private float _craftHoldTime;
        private bool _isHolding;

        public ReadOnlyReactiveProperty<CraftingRecipeData> SelectedRecipe => _selectedRecipe;
        public CraftingSystem CraftingSystem => _craftingSystem;

        public void Init(CraftingSystem craftingSystem, InventorySystem mainInventory, InventorySystem hotbarInventory)
        {
            _craftingSystem = craftingSystem;
            _mainInventory = mainInventory;
            _hotbarInventory = hotbarInventory;
        }

        public override void OnOpen()
        {
            base.OnOpen();

            CreateRecipeList();
            SubscribeToEvents();
        }

        private void CreateRecipeList()
        {
            View.ClearRecipeList(_recipeItemViews);
            _recipeItemViews.Clear();

            var recipes = _craftingSystem.GetAvailableRecipes();

            foreach (var recipe in recipes)
            {
                var itemView = View.CreateRecipeItem();
                itemView.Init(recipe, this);
                _recipeItemViews.Add(itemView);
            }

            if (recipes.Count > 0)
            {
                SelectRecipe(recipes[0]);
            }
        }

        private void SubscribeToEvents()
        {
            _selectedRecipe
                .Subscribe(recipe =>
                {
                    if (recipe != null)
                    {
                        DisplayRecipeDetails(recipe);
                        UpdateAllVisuals();
                    }
                })
                .AddTo(Subs);

            _craftingSystem.OnCraftingSuccessful
                .Subscribe(_ => UpdateAllVisuals())
                .AddTo(Subs);

            View.CraftButton.OnClickAsObservable()
                .Subscribe(_ => OnCraftButtonClick())
                .AddTo(Subs);

            View.CloseButton.OnClickAsObservable()
                .Subscribe(_ => Close())
                .AddTo(Subs);
        }

        public void SelectRecipe(CraftingRecipeData recipe)
        {
            _selectedRecipe.Value = recipe;
        }

        private void DisplayRecipeDetails(CraftingRecipeData recipe)
        {
            View.UpdateResultDisplay(recipe);
            CreateIngredientList(recipe);
            UpdateDetailVisuals(recipe);
        }

        private void CreateIngredientList(CraftingRecipeData recipe)
        {
            View.ClearIngredientList(_ingredientItemViews);
            _ingredientItemViews.Clear();

            foreach (var ingredient in recipe.Ingredients)
            {
                var itemView = View.CreateIngredientItem();
                itemView.Init(ingredient, _craftingSystem);
                _ingredientItemViews.Add(itemView);
            }
        }

        private void UpdateDetailVisuals(CraftingRecipeData recipe)
        {
            bool canCraft = _craftingSystem.CanCraft(recipe);
            View.UpdateCraftButtonVisual(canCraft);

            foreach (var item in _ingredientItemViews)
            {
                item.UpdateVisuals();
            }
        }

        private void UpdateAllVisuals()
        {
            foreach (var item in _recipeItemViews)
            {
                item.UpdateVisuals(_craftingSystem);
            }

            if (_selectedRecipe.Value != null)
            {
                UpdateDetailVisuals(_selectedRecipe.Value);
            }
        }

        private void OnCraftButtonClick()
        {
            if (_selectedRecipe.Value == null) return;
    
            bool canCraft = _craftingSystem.CanCraft(_selectedRecipe.Value);
            if (!canCraft) return;

            _craftingSystem.TryCraft(_selectedRecipe.Value, _mainInventory, _hotbarInventory);
        }
    }
}