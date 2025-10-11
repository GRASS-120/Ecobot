using System.Collections.Generic;
using UnityEngine;

namespace Inventory.CraftingSystem
{
    [CreateAssetMenu(menuName = "Project/Crafting Recipe")]
    public class CraftingRecipeData : ScriptableObject
    {
        [SerializeField] private InventoryItemData resultItem;
        [SerializeField] private int resultAmount = 1;
        [SerializeField] private List<RecipeIngredient> ingredients;
        
        public InventoryItemData ResultItem => resultItem;
        public int ResultAmount => resultAmount;
        public List<RecipeIngredient> Ingredients => ingredients;
    }
}