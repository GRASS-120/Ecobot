using System;
using UnityEngine;
using UnityEngine.Events;

namespace Inventory
{
    interface IInventoryHolder 
    {
        // public int MainInventorySize { get; private set; }
        // public InventorySystem MainInventory { get; protected set; }

        public bool TryAddToInventory(InventoryItemData data, int amount);
    }
}