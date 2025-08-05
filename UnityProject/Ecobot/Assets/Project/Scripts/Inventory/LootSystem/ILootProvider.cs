using Inventory;
using Inventory.LootSystem;
using R3;

namespace InteractionSystem
{
    public interface ILootProvider
    {
        public Observable<LootQuery> OnGiveLoot { get; }
    }
}