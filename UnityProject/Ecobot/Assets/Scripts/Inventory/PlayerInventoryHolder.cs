using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Inventory
{
    public class PlayerInventoryHolder : InventoryHolder
    {
        public static Action<InventorySystem> OnPlayerBackpackDisplayRequested;

        [SerializeField] protected int secondaryInventorySize;
        [SerializeField] protected InventorySystem secondaryInventorySystem;

        public InventorySystem SecondaryInventorySystem => secondaryInventorySystem;

        protected override void Awake()
        {
            base.Awake();
            secondaryInventorySystem = new InventorySystem(secondaryInventorySize);
        }
        
        private void Update()
        {
            if (Keyboard.current.bKey.wasPressedThisFrame)
            {
                OnPlayerBackpackDisplayRequested?.Invoke(secondaryInventorySystem);
            }
        }

        public bool TryAddToInventory(InventoryItemData data, int amount)
        {
            if (primaryInventorySystem.TryAddToInventory(data, amount))
            {
                return true;
            }
            else if (secondaryInventorySystem.TryAddToInventory(data, amount))
            {
                return true;
            }
            return false;
        }
    }
}