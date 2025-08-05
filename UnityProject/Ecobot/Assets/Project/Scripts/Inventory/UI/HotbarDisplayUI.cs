using System.Collections.Generic;
using R3;
using UnityEngine;

namespace Inventory.UI
{
    public class HotbarDisplayUI : InventoryDisplay
    {
        [SerializeField] private InventorySlotUI[] slotsUI;
        public void Init(InventorySystem hotbarInvSystem, CompositeDisposable disposables)
        {
            inventorySystem = hotbarInvSystem;
            
            if (inventorySystem != null)
            {
                // Подписка добавляется в CompositeDisposable контроллера
                inventorySystem.OnInventorySlotChanged.Subscribe(UpdateSlot).AddTo(disposables);
            }
            else
            {
                Debug.LogWarning($"Hotbar InventorySystem is null on {this.gameObject}");
            }
            
            ConnectSlots(inventorySystem);
        }
        
        public override void ConnectSlots(InventorySystem invToDisplay)
        {
            slotDict = new Dictionary<InventorySlotUI, InventorySlot>();

            if (invToDisplay == null)
            {
                Debug.LogError("InventorySystem for HotbarDisplay is null.");
                return;
            }

            if (slotsUI.Length != invToDisplay.InventorySize)
            {
                Debug.LogWarning($"Hotbar slots UI count ({slotsUI.Length}) out of sync with InventorySystem size ({invToDisplay.InventorySize}) on {this.gameObject}");
            }
            
            for (int i = 0; i < invToDisplay.InventorySize; i++)
            {
                // Убедимся, что индексы не выходят за пределы массива slotsUI
                if (i < slotsUI.Length)
                {
                    slotDict.Add(slotsUI[i], invToDisplay.InventorySlots[i]);
                    slotsUI[i].Init(invToDisplay.InventorySlots[i]);
                    slotsUI[i].UpdateSlotUI(); // Обновляем UI при инициализации
                }
                else
                {
                    Debug.LogError($"Not enough Hotbar UI slots assigned for inventory size {invToDisplay.InventorySize}. Missing slot for index {i}.");
                    break; 
                }
            }
        }
    }
}