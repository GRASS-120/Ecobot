using System.Collections.Generic;
using R3;

namespace Inventory.Services
{
    public class InventoryResourceCounterService
    {
        private readonly Dictionary<string, int> _resourceCounts = new Dictionary<string, int>();
        private readonly Dictionary<string, ReactiveProperty<int>> _resourceObservables = new Dictionary<string, ReactiveProperty<int>>();
        private readonly List<InventorySystem> _trackedInventories = new List<InventorySystem>(); 

        public void SubscribeToInventory(InventorySystem inventory)
        {
            _trackedInventories.Add(inventory); 
            inventory.OnInventorySlotChanged.Subscribe(slot => UpdateResourceCount()); 
            
            UpdateResourceCount(); 
        }
        
        private void UpdateResourceCount()
        {
            var newCounts = new Dictionary<string, int>(); 
            
            foreach (var inventory in _trackedInventories)
            {
                foreach (var slot in inventory.InventorySlots)
                {
                    if (slot.ItemData == null) continue;
                    
                    string itemId = slot.ItemData.ID; 
                    
                    if (newCounts.ContainsKey(itemId))
                    {
                        newCounts[itemId] += slot.StackSize;
                    }
                    else
                    {
                        newCounts[itemId] = slot.StackSize;
                    }
                }
            }
            
            foreach (var kvp in newCounts)
            {
                string itemId = kvp.Key; // <- изменить
                int newCount = kvp.Value;
                
                if (!_resourceCounts.ContainsKey(itemId) || _resourceCounts[itemId] != newCount)
                {
                    _resourceCounts[itemId] = newCount;
                    GetOrCreateObservable(itemId).Value = newCount;
                }
            }
            
            var itemsToRemove = new List<string>(); // <- изменить
            foreach (var kvp in _resourceCounts)
            {
                if (!newCounts.ContainsKey(kvp.Key))
                {
                    itemsToRemove.Add(kvp.Key);
                }
            }
            
            foreach (var itemId in itemsToRemove)
            {
                _resourceCounts[itemId] = 0;
                GetOrCreateObservable(itemId).Value = 0;
            }
        }

        private ReactiveProperty<int> GetOrCreateObservable(string itemId) // <- изменить
        {
            if (!_resourceObservables.ContainsKey(itemId))
            {
                _resourceObservables[itemId] = new ReactiveProperty<int>(0);
            }
            return _resourceObservables[itemId];
        }

        public int GetResourceCount(string itemId) // <- изменить
        {
            return _resourceCounts.ContainsKey(itemId) ? _resourceCounts[itemId] : 0;
        }

        public int GetResourceCount(InventoryItemData item)
        {
            return GetResourceCount(item.ID); // <- изменить
        }

        public bool HasResource(string itemId, int amount) // <- изменить
        {
            return GetResourceCount(itemId) >= amount;
        }

        public bool HasResource(InventoryItemData item, int amount)
        {
            return HasResource(item.ID, amount); // <- изменить
        }

        public ReadOnlyReactiveProperty<int> ObserveResource(string itemId) // <- изменить
        {
            return GetOrCreateObservable(itemId);
        }

        public ReadOnlyReactiveProperty<int> ObserveResource(InventoryItemData item)
        {
            return ObserveResource(item.ID); // <- изменить
        }
    }
}