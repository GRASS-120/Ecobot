using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Inventory
{
    [System.Serializable]
    public class InventorySystem
    {
        public event EventHandler<OnInventorySlotChangedEventArgs> OnInventorySlotChanged;
        public class OnInventorySlotChangedEventArgs
        {
            public InventorySlot Slot;
        }
        
        [SerializeField] private List<InventorySlot> inventorySlots;

        private int _inventorySize;
        
        public List<InventorySlot> InventorySlots => inventorySlots;
        public int InventorySize => inventorySlots.Count;

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
            if (ContainsItem(item, out List<InventorySlot> slots))  // check whether item exists in inventory
            {
                foreach (var slot in slots)
                {
                    if (!slot.CanAddInStack(amount)) continue;
                    
                    slot.AddToStack(amount);
                    OnInventorySlotChanged?.Invoke(this, new OnInventorySlotChangedEventArgs { Slot = slot });
                    return true;
                }
            }
            
            if (HasFreeSlot(out InventorySlot freeSlot)) // gets the first available slot
            {
                //if (!freeSlot.CanAddInStack(amount)) return false;
                
                freeSlot.UpdateSlot(item, amount);
                OnInventorySlotChanged?.Invoke(this, new OnInventorySlotChangedEventArgs {Slot = freeSlot});
                return true;
            }

            return false;
        }

        public bool ContainsItem(InventoryItemData item, out List<InventorySlot> slots)
        {
            slots = inventorySlots.Where(i => i.ItemData == item).ToList();
            return slots != null;
        }

        public bool HasFreeSlot(out InventorySlot slot)
        {
            slot = inventorySlots.FirstOrDefault(i => i.ItemData == null);
            return slot != null;
        }
    }
}