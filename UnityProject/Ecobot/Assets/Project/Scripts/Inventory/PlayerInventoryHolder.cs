using System;
using Player;
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
        [SerializeField] protected InventorySystem hotbarInventorySystem;

        public int MainInventorySize => mainInventorySize;
        public InventorySystem MainInventory => mainInventorySystem;
        
        private int _inventorySize;
        private InventorySystem _mainInventory;

        public InventorySystem HotbarInventorySystem => hotbarInventorySystem;
        
        public void Init(PlayerManager player)
        {
            hotbarInventorySystem = new InventorySystem(hotbarInventorySize);
            mainInventorySystem = new InventorySystem(mainInventorySize);

            player.Input.OnOpenInventory += HandleInventory;
        }

        private void HandleInventory()
        {
            // load panel
        }

        public bool TryAddToInventory(InventoryItemData data, int amount)
        {
            if (hotbarInventorySystem.TryAddToInventory(data, amount))
            {
                return true;
            }
            
            if (mainInventorySystem.TryAddToInventory(data, amount))
            {
                return true;
            }
            
            return false;
        }
    }
}