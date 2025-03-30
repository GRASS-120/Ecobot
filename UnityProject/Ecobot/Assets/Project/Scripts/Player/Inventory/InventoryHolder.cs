using System;
using UnityEngine;
using UnityEngine.Events;

namespace Inventory
{
    public class InventoryHolder : MonoBehaviour  // переделать в интерфейс? абстрактный класс?
    {
        public static Action<InventorySystem> OnDynamicInventoryDisplayRequested;  // ! remake -> на event Action<>

        [SerializeField] private int inventorySize;
        [SerializeField] protected InventorySystem primaryInventorySystem;

        public InventorySystem PrimaryInventorySystem => primaryInventorySystem;

        protected virtual void Awake()
        {
            primaryInventorySystem = new InventorySystem(inventorySize);
        }
    }
}