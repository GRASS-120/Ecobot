using System.Collections.Generic;
using R3;
using UnityEngine;

namespace Inventory.UI
{
    public class HotbarUI : MonoBehaviour
    {
        [SerializeField] private InventorySlotUI slotPrefab;
        [SerializeField] private RectTransform slotsRoot;
        
        private InventorySystem _inventory;
        private List<InventorySlotUI> _slots = new List<InventorySlotUI>();

        public void Init(
            InventorySystem inventorySystem, 
            MouseInventoryItemUI mouseUI, 
            CompositeDisposable disposables,
            InventorySystem quickMoveTarget = null)
        {
            _inventory = inventorySystem;

            Clear();
            for (int i = 0; i < _inventory.InventorySize; i++)
            {
                var slot = Instantiate(slotPrefab, slotsRoot);
                slot.Init(_inventory, i, mouseUI, _inventory.InventoryOperationsService, disposables, quickMoveTarget);
                _slots.Add(slot);
            }

            RefreshAll();

            _inventory.OnInventorySlotChanged
                .Subscribe(changedSlot =>
                {
                    int idx = _inventory.IndexOf(changedSlot);
                    if (idx >= 0 && idx < _slots.Count)
                        _slots[idx].Refresh();
                })
                .AddTo(disposables);
            
            _inventory.NotifySlotChanged(0); 
        }

        private void RefreshAll()
        {
            for (int i = 0; i < _slots.Count; i++)
                _slots[i].Refresh();
        }

        private void Clear()
        {
            foreach (Transform child in slotsRoot)
                Destroy(child.gameObject);
            _slots.Clear();
        }
    }
}