using System;
using System.Collections.Generic;
using Grid.BuildingSystem;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Inventory.UI
{
    public abstract class InventoryDisplay : MonoBehaviour
    {
        [SerializeField] private GameManager gameManager;
        [SerializeField] private MouseInventoryItem mouseInventoryItem;
        
        protected InventorySystem inventorySystem;
        protected Dictionary<InventorySlotUI, InventorySlot> slotDict;

        public InventorySystem InventorySystem => inventorySystem;
        public Dictionary<InventorySlotUI, InventorySlot> SlotDict => slotDict;

        protected virtual void Start()
        {
            
        }

        public abstract void ConnectSlots(InventorySystem invToDisplay);

        protected virtual void UpdateSlot(object sender, InventorySystem.OnInventorySlotChangedEventArgs e)
        {
            foreach (var (key, value) in slotDict)
            {
                if (value == e.Slot)  // logic slot
                {
                    key.UpdateSlotUI(e.Slot);  // ui slot
                }
            }
        }

        public void SlotClicked(InventorySlotUI clickedSlotUI)
        {
            bool isShiftPressed = Keyboard.current.leftShiftKey.isPressed;  // ! refactor

            // clicked slot has an item + mouse does not have any item
            if (clickedSlotUI.AssignedSlot.ItemData != null && mouseInventoryItem.assignedSlot.ItemData == null)
            {
                // if player holding shift => split the stack
                if (isShiftPressed && clickedSlotUI.AssignedSlot.TrySplitStack(out InventorySlot halfStackSlot))
                {
                    mouseInventoryItem.UpdateMouseSlot(halfStackSlot);
                    clickedSlotUI.UpdateSlotUI();
                    return;
                }
                // if not => just pick up item
                else
                {
                    mouseInventoryItem.UpdateMouseSlot(clickedSlotUI.AssignedSlot);
                    clickedSlotUI.ClearSlot();
                    return;
                }
            }
            
            // clicked slot does not have an item + mouse have an item => place mouse item into the empty slot
            if (clickedSlotUI.AssignedSlot.ItemData == null && mouseInventoryItem.assignedSlot.ItemData != null)
            {
                clickedSlotUI.AssignedSlot.ReassignItem(mouseInventoryItem.assignedSlot);
                clickedSlotUI.UpdateSlotUI();
                
                mouseInventoryItem.ClearSlot();
                return;
            }

            // both slots have an item + decide what to do...
            if (clickedSlotUI.AssignedSlot.ItemData != null && mouseInventoryItem.assignedSlot.ItemData != null)
            {
                bool isSameItem = clickedSlotUI.AssignedSlot.ItemData == mouseInventoryItem.assignedSlot.ItemData;

                // are both item the same => combine them
                if (isSameItem && clickedSlotUI.AssignedSlot.CanAddInStack(mouseInventoryItem.assignedSlot.StackSize))
                {
                    clickedSlotUI.AssignedSlot.ReassignItem(mouseInventoryItem.assignedSlot);
                    clickedSlotUI.UpdateSlotUI();
                    
                    mouseInventoryItem.ClearSlot();
                }
                // slot stack size + mouse stack size > slot max stack size => take remaining from mouse
                else if (isSameItem &&
                         !clickedSlotUI.AssignedSlot.CanAddInStack(mouseInventoryItem.assignedSlot.StackSize,
                             out int remaining))
                {
                    if (remaining < 1) SwapSlots(clickedSlotUI);  // stack is full => swap
                    else // slot is not max => take remaining
                    {
                        int remainingInMouse = mouseInventoryItem.assignedSlot.StackSize - remaining;
                        clickedSlotUI.AssignedSlot.AddToStack(remaining);
                        clickedSlotUI.UpdateSlotUI();

                        InventorySlot newMouseItem = new InventorySlot(mouseInventoryItem.assignedSlot.ItemData, remainingInMouse);
                        mouseInventoryItem.ClearSlot();
                        mouseInventoryItem.UpdateMouseSlot(newMouseItem);
                    } 
                }
                // different items => swap the items
                else if (clickedSlotUI.AssignedSlot.ItemData != mouseInventoryItem.assignedSlot.ItemData)
                {
                    SwapSlots(clickedSlotUI);
                }
            }
        }

        private void SwapSlots(InventorySlotUI clickedSlotUI)
        {
            var tmpSlot =
                new InventorySlot(mouseInventoryItem.assignedSlot.ItemData, mouseInventoryItem.assignedSlot.StackSize);
            
            // ? почему не используем slot.UpdateSlot? -> мб суть в том, что не только itemData обновляется, но и 
            // еще другие штуки делаются (ui и тп)
            mouseInventoryItem.ClearSlot();
            mouseInventoryItem.UpdateMouseSlot(clickedSlotUI.AssignedSlot);
            
            clickedSlotUI.ClearSlot();
            clickedSlotUI.AssignedSlot.ReassignItem(tmpSlot);
            clickedSlotUI.UpdateSlotUI();
        }
    }
}
