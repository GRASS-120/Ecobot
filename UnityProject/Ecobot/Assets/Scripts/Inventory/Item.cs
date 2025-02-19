using System;
using UnityEngine;

namespace Inventory
{
    public class Item : MonoBehaviour
    {
        public float pickUpRadius = 1f;
        public InventoryItemData itemData;

        private SphereCollider _collider;

        private void Awake()
        {
            _collider = GetComponent<SphereCollider>();
            _collider.isTrigger = true;
            _collider.radius = pickUpRadius;
        }

        private void OnTriggerEnter(Collider other)  // ! remake
        {
            var inventory = other.GetComponent<PlayerInventoryHolder>();
            if (!inventory) return;

            if (inventory.TryAddToInventory(itemData, 1))
            {
                Destroy(this.gameObject);
            }
        }
    }
}