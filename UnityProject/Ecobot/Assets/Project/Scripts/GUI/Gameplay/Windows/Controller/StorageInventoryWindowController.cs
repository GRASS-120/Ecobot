using System.Collections.Generic;
using GUI.Gameplay.Windows.View;
using GUI.UIFramework;
using Inventory;
using Inventory.UI;
using R3;
using UnityEngine;

namespace GUI.Gameplay.Windows.Controller
{
    [Window(WindowType.Popup, "StorageInventoryWindow")]
    public class StorageInventoryWindowController : WindowController<InventoryWindowView>
    {
        public override string Id => "StorageInventoryWindow";
        
        private MouseInventoryItemUI _mouseUI;
        private InventorySystem _quickMoveTarget;
        private InventorySystem _currentStorage;
        private List<InventorySlotUI> _slots = new List<InventorySlotUI>();
        private InventorySelectionService _inventorySelectionService;

        public void Init(
            InventorySelectionService inventorySelectionService,
            MouseInventoryItemUI mouseUI,
            InventorySystem quickMoveTarget = null)
        {
            _inventorySelectionService = inventorySelectionService;
            _mouseUI = mouseUI;
            _quickMoveTarget = quickMoveTarget;
        }

        public void SetStorage(InventorySystem storageInventory)
        {
            _currentStorage = storageInventory;
        }

        public override void OnOpen()
        {
            if (_currentStorage == null)
            {
                Debug.LogError("StorageInventoryWindow opened without storage set!");
                return;
            }

            Clear();
            
            for (int i = 0; i < _currentStorage.InventorySize; i++)
            {
                var slot = View.CreateSlotVisual();
                slot.Init(_currentStorage, _inventorySelectionService, i, _mouseUI, _quickMoveTarget);
                _slots.Add(slot);
            }
            
            RefreshAll();
            
            _currentStorage.OnInventorySlotChanged
                .Subscribe(changedSlot =>
                {
                    int idx = _currentStorage.IndexOf(changedSlot);
                    if (idx >= 0 && idx < _slots.Count)
                        _slots[idx].Refresh();
                })
                .AddTo(Subs);
        }

        public override void OnClose()
        {
            base.OnClose();
            _currentStorage = null; 
        }

        private void RefreshAll()
        {
            for (int i = 0; i < _slots.Count; i++)
                _slots[i].Refresh();
        }

        private void Clear()
        {
            View.ClearVisual();
            _slots.Clear();
        }
    }
}