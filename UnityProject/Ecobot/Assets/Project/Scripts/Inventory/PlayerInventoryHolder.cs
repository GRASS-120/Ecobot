using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace Inventory
{
    public class PlayerInventoryHolder : MonoBehaviour, IInventoryHolder
    {
        // todo: убрать нахуй статик
        public static Action<InventorySystem> OnPlayerBackpackDisplayRequested;
        public static Action<InventorySystem> OnDynamicInventoryDisplayRequested;  // ! remake -> на event Action<>

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
        
        private void Awake()
        {
            hotbarInventorySystem = new InventorySystem(hotbarInventorySize);
            mainInventorySystem = new InventorySystem(mainInventorySize);
        }
        
        private void Update()
        {
            if (Keyboard.current.bKey.wasPressedThisFrame)
            {
                OnPlayerBackpackDisplayRequested?.Invoke(mainInventorySystem);
            }
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