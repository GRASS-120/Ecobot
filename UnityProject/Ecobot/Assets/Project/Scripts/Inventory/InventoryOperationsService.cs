using UnityEngine;

namespace Inventory
{
    public class InventoryOperationsService
    {
        // Перенести amount единиц из fromInv/fromIndex -> toInv/toIndex
        // Возвращает сколько реально перенесено
        public int Move(InventorySystem fromInv, int fromIndex, InventorySystem toInv, int toIndex, int amount)
        {
            if (fromInv == null || toInv == null) return 0;
            if (amount <= 0) return 0;

            var fromSlot = fromInv.GetSlot(fromIndex);
            var toSlot   = toInv.GetSlot(toIndex);

            if (fromSlot.ItemData == null) return 0;

            // Пустая ячейка назначения
            if (toSlot.ItemData == null)
            {
                int moveAmount = Mathf.Min(amount, fromSlot.StackSize);
                toSlot.UpdateSlot(fromSlot.ItemData, moveAmount);
                fromSlot.RemoveFromStack(moveAmount);
                if (fromSlot.StackSize <= 0) fromSlot.ClearSlot();

                Debug.Log("Move successful empty");
                toInv.NotifySlotChanged(toIndex);
                fromInv.NotifySlotChanged(fromIndex);
                return moveAmount;
            }

            // Та же номенклатура -> пытаемся доложить до капа
            if (toSlot.ItemData == fromSlot.ItemData)
            {
                int capacity = toSlot.ItemData.maxStackValue - toSlot.StackSize;
                if (capacity <= 0) return 0;

                int moveAmount = Mathf.Min(amount, fromSlot.StackSize, capacity);
                toSlot.AddToStack(moveAmount);
                fromSlot.RemoveFromStack(moveAmount);
                if (fromSlot.StackSize <= 0) fromSlot.ClearSlot();

                Debug.Log("Move successful stack");
                toInv.NotifySlotChanged(toIndex);
                fromInv.NotifySlotChanged(fromIndex);
                return moveAmount;
            }

            // Разные предметы -> Move не выполняем (для этого есть Swap)
            return 0;
        }

        // Обмен содержимым ячеек (если нельзя смержить)
        public bool Swap(InventorySystem invA, int indexA, InventorySystem invB, int indexB)
        {
            if (invA == null || invB == null) return false;

            var a = invA.GetSlot(indexA);
            var b = invB.GetSlot(indexB);

            // Если можно смержить — лучше это делать через Move в UI-логике, Swap оставляем для разнотипных
            if (a.ItemData == b.ItemData) return false;

            var aData = a.ItemData; var aCount = a.StackSize;
            var bData = b.ItemData; var bCount = b.StackSize;

            if (bData == null)
            {
                b.UpdateSlot(aData, aCount);
                a.ClearSlot();
            }
            else if (aData == null)
            {
                a.UpdateSlot(bData, bCount);
                b.ClearSlot();
            }
            else
            {
                a.UpdateSlot(bData, bCount);
                b.UpdateSlot(aData, aCount);
            }

            Debug.Log("Swap successful");
            invA.NotifySlotChanged(indexA);
            invB.NotifySlotChanged(indexB);
            return true;
        }

        // Разделить стэк: amount из inv/index перенести в toInv/toIndex
        // Возвращает сколько реально перенесено
        public int Split(InventorySystem inv, int index, int amount, InventorySystem toInv, int toIndex)
        {
            var src = inv.GetSlot(index);
            if (src.ItemData == null || src.StackSize <= 1) return 0;

            amount = Mathf.Clamp(amount, 1, src.StackSize - 1);
            return Move(inv, index, toInv, toIndex, amount);
        }
    }
}