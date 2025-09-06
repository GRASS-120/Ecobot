using System;
using UnityEngine;

namespace Inventory
{
    [Serializable]
    public class InventorySlot
    {
        [SerializeField] private InventoryItemData itemData;
        [SerializeField] private int stackSize;

        public InventoryItemData ItemData => itemData;
        public int StackSize => stackSize;

        public InventorySlot(InventoryItemData source, int amount)
        {
            itemData = source;
            stackSize = amount;
        }

        public InventorySlot()
        {
            ClearSlot();
        }

        public void ClearSlot()
        {
            itemData = null;
            stackSize = 0;
        }
        
        public void UpdateSlot(InventoryItemData item, int amount)
        {
            itemData = item;
            stackSize = amount;
        }

        public bool CanAddInStack(int amountToAdd, out int amountRemaining)
        {
            if (itemData == null)
            {
                amountRemaining = 0;
                return false;
            }
            amountRemaining = itemData.maxStackValue - stackSize;
            return CanAddInStack(amountToAdd);
        }

        public bool CanAddInStack(int amountToAdd) =>
            itemData != null && (stackSize + amountToAdd) <= itemData.maxStackValue;

        public void AddToStack(int amount) => stackSize += amount;
        public void RemoveFromStack(int amount) => stackSize -= amount;

        // public bool TrySplitStack(out InventorySlot slittedStack)
        // {
        //     if (stackSize <= 1)
        //     {
        //         slittedStack = null;
        //         return false;
        //     }
        //
        //     int halfStack = stackSize / 2;
        //     RemoveFromStack(halfStack);
        //     slittedStack = new InventorySlot(itemData, halfStack);
        //     return true;
        // }
    }
}