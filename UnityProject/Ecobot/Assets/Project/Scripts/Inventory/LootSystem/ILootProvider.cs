using R3;

namespace Inventory.LootSystem
{
    public interface ILootProvider
    {
        public Observable<LootQuery> OnProvideLoot { get; }
    }
}