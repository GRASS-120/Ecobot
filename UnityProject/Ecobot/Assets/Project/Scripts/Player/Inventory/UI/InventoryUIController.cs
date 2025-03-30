using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = System.Random;

namespace Inventory.UI
{
    public class InventoryUIController : MonoBehaviour
    {
        public DynamicInventoryDisplayUI storagePanel;
        public DynamicInventoryDisplayUI playerBackpackPanel;

        private void Awake()
        {
            storagePanel.gameObject.SetActive(false);
            playerBackpackPanel.gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            InventoryHolder.OnDynamicInventoryDisplayRequested += DisplayInventory;
            PlayerInventoryHolder.OnPlayerBackpackDisplayRequested += DisplayPlayerBackpack;
        }

        private void OnDisable()
        {
            InventoryHolder.OnDynamicInventoryDisplayRequested -= DisplayInventory;
            PlayerInventoryHolder.OnPlayerBackpackDisplayRequested -= DisplayPlayerBackpack;

        }

        private void Update()
        {
            if (storagePanel.gameObject.activeInHierarchy && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                storagePanel.gameObject.SetActive(false);
            }
            
            if (playerBackpackPanel.gameObject.activeInHierarchy && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                playerBackpackPanel.gameObject.SetActive(false);
            }
        }

        private void DisplayInventory(InventorySystem inventoryToDisplay)
        {
            storagePanel.gameObject.SetActive(true);
            storagePanel.RefreshDynamicInventory(inventoryToDisplay);
        }

        private void DisplayPlayerBackpack(InventorySystem inventoryToDisplay)
        {
            playerBackpackPanel.gameObject.SetActive(true);
            playerBackpackPanel.RefreshDynamicInventory(inventoryToDisplay);
        }
    }
}