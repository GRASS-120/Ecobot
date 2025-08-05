using InteractionSystem;

namespace Inventory.LootSystem
{
    public interface ILooter
    {
        public float LootRadius { get; }
        
        public void HandleLoot(ILootProvider lootProvider);
    }
}