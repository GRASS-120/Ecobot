using System.Collections.Generic;
using GUI.Gameplay.Windows.View;
using GUI.UIFramework;
using Inventory;
using Inventory.UI;
using Player;
using R3;
using UnityEngine;

namespace GUI.Gameplay.Windows.Controller
{
    [Window(WindowType.Popup, "PlayerInventoryWindow")]
    public class InventoryWindowController : WindowController<InventoryWindowView>
    {
        public override string Id => "PlayerInventoryWindow";

        private MouseInventoryItemUI _mouseUI;
        private InventorySystem _quickMoveTarget;
        private InventorySystem _inventory;
        private List<InventorySlotUI> _slots = new List<InventorySlotUI>();
        private InventorySelectionService _inventorySelectionService;
        
        public void Init(
            InventorySystem inventorySystem, 
            InventorySelectionService inventorySelectionService,
            MouseInventoryItemUI mouseUI, 
            InventorySystem quickMoveTarget = null)
        {
            _inventory = inventorySystem;
            _inventorySelectionService = inventorySelectionService;
            _mouseUI = mouseUI;
            _quickMoveTarget = quickMoveTarget;
        }
        
        public override void OnOpen()
        {
            Clear();
            for (int i = 0; i < _inventory.InventorySize; i++)
            {
                var slot = View.CreateSlotVisual();
                
                slot.Init(_inventory, _inventorySelectionService, i, _mouseUI, _quickMoveTarget);
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
                .AddTo(Subs);
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