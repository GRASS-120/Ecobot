using System.Collections.Generic;
using UnityEngine;

namespace Inventory.UI
{
    public class InventoryDisplayUI : InventoryDisplay
    {
        [SerializeField] private InventoryHolder inventoryHolder;
        [SerializeField] private InventorySlotUI[] slotsUI;
        protected override void Start()
        {
            base.Start();

            if (inventoryHolder != null)
            {
                inventorySystem = inventoryHolder.PrimaryInventorySystem;
                inventorySystem.OnInventorySlotChanged += UpdateSlot;  // ???? почему добавили? (при добавлении рюкзака)
            }
            else Debug.LogWarning($"No inv assigned to {this.gameObject}");
            
            ConnectSlots(inventorySystem);
        }
        
        // ! если нужно будет спавинть UI, то нужно добавить object pooling 
        public override void ConnectSlots(InventorySystem invToDisplay)
        {
            slotDict = new Dictionary<InventorySlotUI, InventorySlot>();

            if (slotsUI.Length != inventorySystem.InventorySize) Debug.Log($"inv slots out of sync on {this.gameObject}");
            
            for (int i = 0; i < inventorySystem.InventorySize; i++)
            {
                slotDict.Add(slotsUI[i], inventorySystem.InventorySlots[i]);
                slotsUI[i].Init(inventorySystem.InventorySlots[i]);
            }
        }
    }
}