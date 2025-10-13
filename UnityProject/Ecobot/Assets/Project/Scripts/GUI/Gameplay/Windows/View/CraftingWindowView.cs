using System.Collections.Generic;
using Grid.BuildingSystem.Buildings.Types.Workbench.UI;
using GUI.UIFramework;
using Inventory.CraftingSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GUI.Gameplay.Windows.View
{
    public class CraftingWindowView : WindowView
    {
        [Header("Recipe List")]
        [SerializeField] private Transform recipeListContainer;
        [SerializeField] private GameObject recipeItemPrefab;
        
        [Header("Recipe Detail")]
        [SerializeField] private Image resultIcon;
        [SerializeField] private TextMeshProUGUI resultNameText;
        [SerializeField] private TextMeshProUGUI resultDescriptionText;
        [SerializeField] private Transform ingredientsContainer;
        [SerializeField] private GameObject ingredientItemPrefab;
        [SerializeField] private Button craftButton; 
        [SerializeField] private Image craftButtonImage; 
        [SerializeField] private TextMeshProUGUI resultAmountText; 

        [Header("Window Controls")]
        [SerializeField] private Button closeButton; 
        
        [Header("Colors")]
        [SerializeField] private Color normalButtonColor = Color.white;
        [SerializeField] private Color disabledButtonColor = Color.gray;

        public Button CraftButton => craftButton; 
        public Button CloseButton => closeButton; 
        
        public CraftingRecipeItemView CreateRecipeItem()
        {
            var itemGO = Instantiate(recipeItemPrefab, recipeListContainer);
            return itemGO.GetComponent<CraftingRecipeItemView>();
        }

        public void ClearRecipeList(List<CraftingRecipeItemView> items)
        {
            foreach (var item in items)
            {
                if (item != null)
                    Destroy(item.gameObject);
            }
        }

        public CraftingIngredientItemView CreateIngredientItem()
        {
            var itemGO = Instantiate(ingredientItemPrefab, ingredientsContainer);
            return itemGO.GetComponent<CraftingIngredientItemView>();
        }

        public void ClearIngredientList(List<CraftingIngredientItemView> items)
        {
            foreach (var item in items)
            {
                if (item != null)
                    Destroy(item.gameObject);
            }
        }

        public void UpdateResultDisplay(CraftingRecipeData recipe)
        {
            resultIcon.sprite = recipe.ResultItem.icon;
            resultNameText.text = recipe.ResultItem.displayName;
            resultDescriptionText.text = recipe.ResultItem.description;
            resultAmountText.text = $"x{recipe.ResultAmount}"; 
        }

        public void UpdateCraftButtonVisual(bool canCraft)
        {
            craftButtonImage.color = canCraft ? normalButtonColor : disabledButtonColor;
            craftButton.interactable = canCraft;
        }
    }
}