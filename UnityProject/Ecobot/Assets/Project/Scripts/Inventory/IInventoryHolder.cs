using System;
using UnityEngine;
using UnityEngine.Events;

namespace Inventory
{
    interface IInventoryHolder 
    {
        public bool TryAddToInventory(InventoryItemData data, int amount);
    }
}