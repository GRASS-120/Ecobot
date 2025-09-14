using InteractionSystem;

namespace Inventory.LootSystem
{
    public interface ILootReceiver
    {
        public bool TryReceive(LootQuery loot);
    }
}