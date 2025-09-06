using System;
using System.Collections.Generic;
using System.Linq;
using R3;
using UnityEngine;

namespace Inventory
{
    [Serializable]
    public class InventorySystem
    {
        [SerializeField] private List<InventorySlot> inventorySlots;
        
        public List<InventorySlot> InventorySlots => inventorySlots;
        public int InventorySize => inventorySlots.Count;
        public InventoryOperationsService InventoryOperationsService => _operationService;
        public Subject<InventorySlot> OnInventorySlotChanged = new Subject<InventorySlot>();
        
        private int _inventorySize;
        private InventoryOperationsService _operationService;

        public InventorySystem(int size)
        {
            inventorySlots = new List<InventorySlot>(size);
            _operationService = new InventoryOperationsService();
            
            for (int i = 0; i < size; i++)
            {
                inventorySlots.Add(new InventorySlot());
            }
        }

        public bool TryAddToInventory(InventoryItemData item, int amount)
        {
            // сначала проверяем слоты с таким же предметом
            if (ContainsItem(item, out List<InventorySlot> matchedSlots)) 
            {
                foreach (var slot in matchedSlots)
                {
                    if (!slot.CanAddInStack(amount)) continue;
                    
                    slot.AddToStack(amount);
                    
                    NotifySlotChanged(IndexOf(slot));
                    
                    return true;
                }
            }
            
            // если стаки заполнены, то проверяем свободные слоты
            if (HasFreeSlot(out InventorySlot freeSlot)) 
            {
                freeSlot.UpdateSlot(item, amount);
                
                NotifySlotChanged(IndexOf(freeSlot));
                
                return true;
            }

            return false;
        }

        public bool ContainsItem(InventoryItemData item, out List<InventorySlot> matchedSlots)
        {
            matchedSlots = inventorySlots.Where(i => i.ItemData == item).ToList();
            return matchedSlots.Count > 0;
        }

        public InventorySlot GetSlot(int index) => inventorySlots[index];

        public void NotifySlotChanged(int index)
        {
            OnInventorySlotChanged.OnNext(inventorySlots[index]);
        }

        public bool TryGetFreeSlotIndex(out int index)
        {
            index = -1;
            for (int i = 0; i < inventorySlots.Count; i++)
            {
                if (inventorySlots[i].ItemData == null)
                {
                    index = i;
                    return true;
                }
            }
            return false;
        }

        public int IndexOf(InventorySlot slot) => inventorySlots.IndexOf(slot);

        public bool HasFreeSlot(out InventorySlot slot)
        {
            slot = inventorySlots.FirstOrDefault(i => i.ItemData == null);
            return slot != null;
        }
    }
}