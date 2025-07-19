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
        public Subject<InventorySlot> OnInventorySlotChanged = new Subject<InventorySlot>();
        
        [SerializeField] private List<InventorySlot> inventorySlots;
        
        public List<InventorySlot> InventorySlots => inventorySlots;
        public int InventorySize => inventorySlots.Count;
        
        private int _inventorySize;

        public InventorySystem(int size)
        {
            inventorySlots = new List<InventorySlot>(size);

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
                    
                    OnInventorySlotChanged.OnNext(slot);
                    return true;
                }
            }
            
            // если стаки заполнены, то проверяем свободные слоты
            if (HasFreeSlot(out InventorySlot freeSlot)) 
            {
                freeSlot.UpdateSlot(item, amount);
                OnInventorySlotChanged.OnNext(freeSlot);
                
                return true;
            }

            return false;
        }

        public bool ContainsItem(InventoryItemData item, out List<InventorySlot> matchedSlots)
        {
            matchedSlots = inventorySlots.Where(i => i.ItemData == item).ToList();
            return matchedSlots != null;
        }

        public bool HasFreeSlot(out InventorySlot slot)
        {
            slot = inventorySlots.FirstOrDefault(i => i.ItemData == null);
            return slot != null;
        }
    }
}