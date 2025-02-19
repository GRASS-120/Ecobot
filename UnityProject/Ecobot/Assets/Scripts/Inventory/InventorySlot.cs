using UnityEngine;

namespace Inventory
{
    // это нужно, чтобы слот отображался в редакторе при том, что скрипт ни к чему не прикреплен
    [System.Serializable]
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
            stackSize = -1;
        }

        public void UpdateSlot(InventoryItemData item, int amount)
        {
            itemData = item;
            stackSize = amount;
        }

        public bool CanAddInStack(int amountToAdd, out int amountRemaining)
        {
            amountRemaining = itemData.maxStackValue - stackSize;
            return CanAddInStack(amountToAdd);
        }

        public bool CanAddInStack(int amountToAdd) => stackSize + amountToAdd <= itemData.maxStackValue;

        public void AddToStack(int amount) => stackSize += amount;

        public void RemoveFromStack(int amount) => stackSize -= amount;

        public void ReassignItem(InventorySlot slot)  // ! rename
        {
            if (itemData == slot.itemData) AddToStack(slot.stackSize);
            else  // rewrite slot
            {
                itemData = slot.itemData;
                stackSize = 0;
                AddToStack(slot.stackSize);
            }
        }

        public bool TrySplitStack(out InventorySlot slittedStack)
        {
            if (stackSize <= 1)
            {
                slittedStack = null;
                return false;
            }

            int halfStack = Mathf.RoundToInt(stackSize / 2);
            RemoveFromStack(halfStack);

            slittedStack = new InventorySlot(itemData, halfStack);
            return true;
        }
    }
}