using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Inventory.UI
{
    public class DynamicInventoryDisplayUI : InventoryDisplay
    {
        [SerializeField] protected InventorySlotUI slotUITemplate;
        
        protected override void Start()
        {
            base.Start();
        }

        public void RefreshDynamicInventory(InventorySystem invSystem)
        {
            ClearSlots();
            inventorySystem = invSystem;
            if (inventorySystem != null) inventorySystem.OnInventorySlotChanged += UpdateSlot;
            ConnectSlots(invSystem);
        }

        // ! refactor -> object pooling (get from pool, not create)
        public override void ConnectSlots(InventorySystem invToDisplay)
        {
            slotDict = new Dictionary<InventorySlotUI, InventorySlot>();

            if (invToDisplay == null) return;

            for (int i = 0; i < invToDisplay.InventorySize; i++)
            {
                var slotUI = Instantiate(slotUITemplate, transform);
                slotUI.gameObject.SetActive(true);
                slotDict.Add(slotUI, invToDisplay.InventorySlots[i]);
                slotUI.Init(invToDisplay.InventorySlots[i]);
                slotUI.UpdateSlotUI();
            }
        }

        // ! refactor -> object pooling (return to pool, not destroy)
        public void ClearSlots()
        {
            foreach (var item in transform.Cast<Transform>())
            {
                Destroy(item.gameObject);
            }

            slotDict?.Clear();
        }

        private void OnDisable()
        {
            if (inventorySystem != null) inventorySystem.OnInventorySlotChanged -= UpdateSlot;
        }
    }
}