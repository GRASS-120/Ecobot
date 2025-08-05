using System;
using InteractionSystem;
using Inventory.LootSystem;
using Player;
using R3;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace Inventory
{
    public class PlayerInventoryHolder : MonoBehaviour, IInventoryHolder
    {
        // public event Action OnMainInventoryDisplayRequested;
        // public event Action OnDynamicInventoryDisplayRequested; 

        [Header("Main Inventory")]
        [SerializeField] protected int mainInventorySize;
        [SerializeField] protected InventorySystem mainInventorySystem;
        
        [Header("Hot Bar Inventory")]
        [SerializeField] private int hotbarInventorySize;
        
        private InventorySystem _hotbarInventorySystem;
        
        public int MainInventorySize => mainInventorySize;
        public InventorySystem MainInventory => mainInventorySystem;
        
        private int _inventorySize;
        private InventorySystem _mainInventory;

        public InventorySystem HotbarInventorySystem => _hotbarInventorySystem;

        private void Awake()
        {
            _hotbarInventorySystem = new InventorySystem(hotbarInventorySize);
            mainInventorySystem = new InventorySystem(mainInventorySize);
        }
        
        public void Init(PlayerManager player)
        {
            player.Input.OnOpenInventory += HandleInventory;
        }

        private void HandleInventory()
        {
            // load panel
        }

        public bool TryAddToInventory(InventoryItemData data, int amount)
        {
            if (_hotbarInventorySystem.TryAddToInventory(data, amount))
            {
                return true;
            }
            
            if (mainInventorySystem.TryAddToInventory(data, amount))
            {
                return true;
            }
            
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