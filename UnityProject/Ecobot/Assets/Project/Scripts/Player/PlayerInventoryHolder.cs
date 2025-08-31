using InteractionSystem;
using Inventory;
using R3;
using UnityEngine;

namespace Player
{
    public class PlayerInventoryHolder : MonoBehaviour, IInventoryHolder
    {
        [Header("Main Inventory")]
        [Min(1)]
        [SerializeField] protected int mainInventorySize = 20;

        [Header("Hot Bar Inventory")]
        [Min(1)]
        [SerializeField] private int hotbarInventorySize = 10;

        private InventorySystem _mainInventorySystem;
        private InventorySystem _hotbarInventorySystem;

        public int MainInventorySize => mainInventorySize;
        public InventorySystem MainInventory => _mainInventorySystem;
        public InventorySystem HotbarInventorySystem => _hotbarInventorySystem;

        private void Awake()
        {
            // _hotbarInventorySystem = new InventorySystem(hotbarInventorySize);
            // _mainInventorySystem = new InventorySystem(mainInventorySize);
        }

        public void Init(PlayerManager player)
        {
            _hotbarInventorySystem = new InventorySystem(hotbarInventorySize);
            _mainInventorySystem = new InventorySystem(mainInventorySize);
            
            Debug.Log($"[Holder Awake] hotbar={_hotbarInventorySystem.GetHashCode()} main={_mainInventorySystem.GetHashCode()}");
            
            player.Input.OnOpenInventory += HandleInventory;
        }

        private void HandleInventory()
        {
            // load panel
        }

        public bool TryAddToInventory(InventoryItemData data, int amount)
        {
            if (_hotbarInventorySystem.TryAddToInventory(data, amount)) return true;
            if (_mainInventorySystem.TryAddToInventory(data, amount)) return true;
            return false;
        }

        public void HandleLoot(ILootProvider lootProvider)
        {
            lootProvider.OnGiveLoot
                .Subscribe(loot => TryAddToInventory(loot.Item, loot.Amount))
                .AddTo(this);
        }
    }
}