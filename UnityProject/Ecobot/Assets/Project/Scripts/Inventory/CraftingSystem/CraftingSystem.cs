using System.Collections.Generic;
using Inventory.Services;
using R3;
using UnityEngine;

namespace Inventory.CraftingSystem
{
    public class CraftingSystem
    {
        private readonly InventoryResourceCounterService _resourceCounter;
        private readonly List<CraftingRecipeData> _availableRecipes;
        private readonly IInventoryHolder _inventoryHolder;
        
        public Subject<CraftingRecipeData> OnCraftingSuccessful = new Subject<CraftingRecipeData>();
        public Subject<CraftingRecipeData> OnCraftingFailed = new Subject<CraftingRecipeData>();
        
        public CraftingSystem(
            InventoryResourceCounterService resourceCounter,
            List<CraftingRecipeData> availableRecipes,
            IInventoryHolder inventoryHolder)
        {
            _resourceCounter = resourceCounter;
            _availableRecipes = availableRecipes;
            _inventoryHolder = inventoryHolder; 
        }
        
        public bool CanCraft(CraftingRecipeData recipe)
        {
            if (recipe == null) return false;
            if (recipe.Ingredients == null || recipe.Ingredients.Count == 0) return false;
            
            foreach (var ingredient in recipe.Ingredients)
            {
                if (!_resourceCounter.HasResource(ingredient.item, ingredient.amount))
                {
                    return false;
                }
            }
            
            return true;
        }
        
        public List<CraftingRecipeData> GetAvailableRecipes() => _availableRecipes;
        
        public List<CraftingRecipeData> GetCraftableRecipes()
        {
            var craftable = new List<CraftingRecipeData>();
            
            foreach (var recipe in _availableRecipes)
            {
                if (CanCraft(recipe))
                {
                    craftable.Add(recipe);
                }
            }
            
            return craftable;
        }
        
        public bool TryCraft(CraftingRecipeData recipe, InventorySystem mainInventory, InventorySystem hotbarInventory)
        {
            if (!CanCraft(recipe))
            {
                OnCraftingFailed.OnNext(recipe);
                return false;
            }
            
            // удаляем ресурсы
            if (!TryConsumeResources(recipe, mainInventory, hotbarInventory))
            {
                OnCraftingFailed.OnNext(recipe);
                return false;
            }
            
            // добавляем результат
            if (!_inventoryHolder.TryAddToInventory(recipe.ResultItem, recipe.ResultAmount))
            {
                // если не удалось добавить результат - возвращаем ресурсы обратно
                ReturnResources(recipe, mainInventory, hotbarInventory);
                OnCraftingFailed.OnNext(recipe);
                return false;
            }
            
            OnCraftingSuccessful.OnNext(recipe);
            return true;
        }

        private bool TryConsumeResources(CraftingRecipeData recipe, InventorySystem mainInventory, InventorySystem hotbarInventory)
        {
            var consumedItems = new List<(InventorySlot slot, int amount)>();
            
            foreach (var ingredient in recipe.Ingredients)
            {
                int remainingAmount = ingredient.amount;
                
                // сначала пробуем взять из hotbar
                remainingAmount = ConsumeFromInventory(hotbarInventory, ingredient.item, remainingAmount, consumedItems);
                
                // если не хватило - берем из основного инвентаря
                if (remainingAmount > 0)
                {
                    remainingAmount = ConsumeFromInventory(mainInventory, ingredient.item, remainingAmount, consumedItems);
                }
                
                if (remainingAmount > 0)
                {
                    // не хватило ресурсов - откатываем изменения
                    RollbackConsumption(consumedItems, mainInventory, hotbarInventory);
                    return false;
                }
            }
            
            // уведомляем об изменениях слотов
            NotifyConsumedSlots(consumedItems, mainInventory, hotbarInventory);
            return true;
        }

        private int ConsumeFromInventory(InventorySystem inventory, InventoryItemData item, int amount, List<(InventorySlot slot, int amount)> consumedItems)
        {
            int remainingAmount = amount;
            
            foreach (var slot in inventory.InventorySlots)
            {
                if (slot.ItemData != item) continue;
                if (remainingAmount <= 0) break;
                
                int toConsume = Mathf.Min(remainingAmount, slot.StackSize);
                slot.RemoveFromStack(toConsume);
                
                consumedItems.Add((slot, toConsume));
                remainingAmount -= toConsume;
                
                if (slot.StackSize <= 0)
                {
                    slot.ClearSlot();
                }
            }
            
            return remainingAmount;
        }
        
        public bool TryConsume(List<RecipeIngredient> ingredients, InventorySystem mainInventory, InventorySystem hotbarInventory)
        {
            var consumedItems = new List<(InventorySlot slot, int amount)>();

            foreach (var ingredient in ingredients)
            {
                int remainingAmount = ingredient.amount;

                // сначала пробуем взять из hotbar
                remainingAmount = ConsumeFromInventory(hotbarInventory, ingredient.item, remainingAmount, consumedItems);

                // если не хватило - берем из основного инвентаря
                if (remainingAmount > 0)
                {
                    remainingAmount = ConsumeFromInventory(mainInventory, ingredient.item, remainingAmount, consumedItems);
                }

                if (remainingAmount > 0)
                {
                    // не хватило ресурсов - откатываем изменения
                    RollbackConsumption(consumedItems, mainInventory, hotbarInventory);
                    return false;
                }
            }

            // уведомляем об изменениях слотов
            NotifyConsumedSlots(consumedItems, mainInventory, hotbarInventory);
            return true;
        }

        private void RollbackConsumption(List<(InventorySlot slot, int amount)> consumedItems, InventorySystem mainInventory, InventorySystem hotbarInventory)
        {
            foreach (var consumed in consumedItems)
            {
                consumed.slot.AddToStack(consumed.amount);
            }
        }

        private void ReturnResources(CraftingRecipeData recipe, InventorySystem mainInventory, InventorySystem hotbarInventory)
        {
            foreach (var ingredient in recipe.Ingredients)
            {
                _inventoryHolder.TryAddToInventory(ingredient.item, ingredient.amount);
            }
        }

        private void NotifyConsumedSlots(List<(InventorySlot slot, int amount)> consumedItems, InventorySystem mainInventory, InventorySystem hotbarInventory)
        {
            foreach (var consumed in consumedItems)
            {
                int index = mainInventory.IndexOf(consumed.slot);
                if (index >= 0)
                {
                    mainInventory.NotifySlotChanged(index);
                }
                else
                {
                    index = hotbarInventory.IndexOf(consumed.slot);
                    if (index >= 0)
                    {
                        hotbarInventory.NotifySlotChanged(index);
                    }
                }
            }
        }
        
        public int GetResourceCount(InventoryItemData item)
        {
            return _resourceCounter.GetResourceCount(item);
        }
    }
}