namespace Inventory.LootSystem
{
    public class LootQuery
    {
        public LootQuery(InventoryItemData item, int amount)
        {
            _item = item;
            _amount = amount;
        }
        
        public InventoryItemData Item => _item;
        public int Amount => _amount;
        
        private InventoryItemData _item;
        private int _amount;

    }
}