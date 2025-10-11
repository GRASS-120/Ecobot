using System;

namespace Inventory.CraftingSystem
{
    [Serializable]
    public struct RecipeIngredient
    {
        public InventoryItemData item;
        public int amount;
        
        public RecipeIngredient(InventoryItemData item, int amount)
        {
            this.item = item;
            this.amount = amount;
        }
    }
}