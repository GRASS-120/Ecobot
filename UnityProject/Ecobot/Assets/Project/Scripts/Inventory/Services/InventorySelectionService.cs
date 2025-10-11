using R3;

namespace Inventory
{
    public class InventorySelectionService
    {
        public readonly ReactiveProperty<InventorySelection> Active = new ReactiveProperty<InventorySelection>(InventorySelection.Empty);
        
        public void Select(InventorySystem inventory, int index)
        {
            Active.Value = new InventorySelection(inventory, index);
        }
        
        public void Clear()
        {
            Active.Value = InventorySelection.Empty;
        }
        
        public readonly struct InventorySelection
        {
            public readonly InventorySystem Inventory;
            public readonly int Index;
            
            public bool IsValid => Inventory != null && Index >= 0;
            public InventoryItemData ItemData => IsValid ? Inventory.GetSlot(Index).ItemData : null;
            
            public InventorySelection(InventorySystem inv, int index)
            {
                Inventory = inv;
                Index = index;
            }
            
            public static InventorySelection Empty => new InventorySelection(null, -1);
        }
    }
}