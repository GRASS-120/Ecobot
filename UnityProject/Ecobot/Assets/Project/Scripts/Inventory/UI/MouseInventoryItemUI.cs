using System;
using System.Collections.Generic;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Inventory.UI
{
    public class MouseInventoryItemUI : MonoBehaviour
    {
        [SerializeField] private Image image;
        [SerializeField] private TextMeshProUGUI count;
        [SerializeField] private CanvasGroup canvasGroup;
        
        public InventorySystem MouseInventory => _mouseInv;
        
        private InventorySystem _mouseInv;
        private InventoryOperationsService _ops;
        private CompositeDisposable _disposables;

        public void Init(CompositeDisposable disposables)
        {
            _disposables = disposables;
            _mouseInv = new InventorySystem(1);
            _ops = _mouseInv.InventoryOperationsService;

            image.raycastTarget = false;
            SetVisible(false);

            _mouseInv.OnInventorySlotChanged
                .Subscribe(_ => Refresh())
                .AddTo(_disposables);
        }

        private void Update()
        {
            if (Mouse.current != null)
            {
                transform.position = Mouse.current.position.ReadValue();
            }
        }

        public bool IsEmpty()
        {
            var slot = _mouseInv.GetSlot(0);
            return slot.ItemData == null || slot.StackSize <= 0;
        }

        public void Refresh()
        {
            var slot = _mouseInv.GetSlot(0);
            
            if (slot.ItemData == null)
            {
                SetVisible(false);
                image.sprite = null;
                count.text = string.Empty;
            }
            else
            {
                SetVisible(true);
                image.sprite = slot.ItemData.icon;
                count.text = slot.StackSize > 1 ? slot.StackSize.ToString() : string.Empty;
            }
        }

        public void PickUpAll(InventorySystem fromInv, int fromIndex)
        {
            if (!IsEmpty()) return;
            
            var moved = _ops.Move(fromInv, fromIndex, _mouseInv, 0, int.MaxValue);
            if (moved > 0) Refresh();
        }

        public void PickUpHalf(InventorySystem fromInv, int fromIndex)
        {
            if (!IsEmpty()) return;
            var src = fromInv.GetSlot(fromIndex);
            if (src.ItemData == null || src.StackSize <= 1) return;
            int half = src.StackSize / 2;
            var moved = _ops.Split(fromInv, fromIndex, half, _mouseInv, 0);
            if (moved > 0) Refresh();
        }

        public void PlaceAll(InventorySystem toInv, int toIndex)
        {
            if (IsEmpty()) return;
            // Сначала пробуем Move (мердж в стак или в пустую ячейку)
            var moved = _ops.Move(_mouseInv, 0, toInv, toIndex, int.MaxValue);
            if (moved == 0)
            {
                // Если предметы разные и Move не сработал — пробуем Swap
                if (_ops.Swap(_mouseInv, 0, toInv, toIndex))
                {
                    Refresh();
                    return;
                }
            }
            Refresh();
        }

        public void PlaceOne(InventorySystem toInv, int toIndex)
        {
            if (IsEmpty()) return;
            
            var moved = _ops.Move(_mouseInv, 0, toInv, toIndex, 1);
            if (moved > 0) Refresh();
        }

        public void ReturnBack(InventorySystem toInv, int toIndex)
        {
            if (IsEmpty()) return;
            
            var moved = _ops.Move(_mouseInv, 0, toInv, toIndex, int.MaxValue);
            
            if (moved > 0) Refresh();
        }

        private void SetVisible(bool visible)
        {
            if (!canvasGroup) return;
            
            canvasGroup.alpha = visible ? 1f : 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }
}