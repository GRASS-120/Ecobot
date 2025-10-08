using R3;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Inventory.UI
{
    public class InventorySlotUI : MonoBehaviour, IPointerClickHandler, 
        IBeginDragHandler, IEndDragHandler, IDragHandler, IDropHandler
    {
        [SerializeField] private Image image;
        [SerializeField] private TextMeshProUGUI count;
        [SerializeField] private Button button;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private GameObject selectionBorder; 

        private InventorySystem _inventory;
        private int _index;
        private MouseInventoryItemUI _mouseUI;
        private InventoryOperationsService _ops;
        private InventorySystem _quickMoveTarget;
        private InventorySelectionService _selection;

        public void Init(
            InventorySystem inventory, 
            InventorySelectionService selection,
            int index, 
            MouseInventoryItemUI mouseUI,
            InventorySystem quickMoveTarget = null)
        {
            _inventory = inventory;
            _index = index;
            _mouseUI = mouseUI;
            _ops = inventory.InventoryOperationsService;
            _quickMoveTarget = quickMoveTarget;
            _selection = selection;

            // СНАЧАЛА отключаем рамку у всех
            if (selectionBorder != null)
            {
                selectionBorder.SetActive(false);
            }

            // Подписываемся на изменение выбранного слота
            if (_selection != null)
            {
                _selection.Active.Subscribe(OnSelectionChanged).AddTo(this);
            }
    
            // Проверяем текущее состояние выделения (вдруг уже что-то выбрано)
            if (_selection != null && _selection.Active.Value.IsValid)
            {
                OnSelectionChanged(_selection.Active.Value);
            }
        }

        private void OnSelectionChanged(InventorySelectionService.InventorySelection sel)
        {
            // Проверяем, выбран ли этот слот
            bool isSelected = sel.IsValid && sel.Inventory == _inventory && sel.Index == _index;
            
            UpdateSelectionVisual(isSelected);
        }

        private void UpdateSelectionVisual(bool isSelected)
        {
            if (selectionBorder != null)
            {
                selectionBorder.SetActive(isSelected);
            }
        }

        public void Refresh()
        {
            var slot = _inventory.GetSlot(_index);
            
            if (slot.ItemData == null)
            {
                image.enabled = false;
                image.sprite = null;
                count.text = string.Empty;
            }
            else
            {
                image.enabled = true;
                image.sprite = slot.ItemData.icon;
                count.text = slot.StackSize > 1 ? slot.StackSize.ToString() : string.Empty;
            }

            // Обновляем выделение при рефреше
            if (_selection != null)
            {
                var active = _selection.Active.Value;
                bool isSelected = active.IsValid && active.Inventory == _inventory && active.Index == _index;
                UpdateSelectionVisual(isSelected);
            }
        }

        // ЛКМ/ПКМ/Shift+ЛКМ
        public void OnPointerClick(PointerEventData eventData)
        {
            bool shift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            bool alt = Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);

            // НОВАЯ ЛОГИКА: ЛКМ с Alt — выбираем слот (любой!)
            bool isSelectGesture = eventData.button == PointerEventData.InputButton.Left && alt;
            
            if (isSelectGesture && _selection != null)
            {
                var slot = _inventory.GetSlot(_index);
                
                // Если кликнули на уже выбранный слот — снимаем выделение
                var currentSelection = _selection.Active.Value;
                if (currentSelection.IsValid && 
                    currentSelection.Inventory == _inventory && 
                    currentSelection.Index == _index)
                {
                    _selection.Clear();
                }
                else
                {
                    // Иначе выбираем этот слот (даже если он пустой или не постройка)
                    _selection.Select(_inventory, _index);
                }
                return;
            }

            // Дальше — ваше текущее поведение ЛКМ/ПКМ/Shift+ЛКМ
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                if (shift && _quickMoveTarget != null)
                {
                    _ops.Move(_inventory, _index, _quickMoveTarget, FindAnyFreeOrMergeIndex(_quickMoveTarget), int.MaxValue);
                    return;
                }
                if (_mouseUI.IsEmpty())
                {
                    _mouseUI.PickUpAll(_inventory, _index);
                }
                else
                {
                    _mouseUI.PlaceAll(_inventory, _index);
                }
            }
            else if (eventData.button == PointerEventData.InputButton.Right)
            {
                if (_mouseUI.IsEmpty())
                {
                    _mouseUI.PickUpHalf(_inventory, _index);
                }
                else
                {
                    _mouseUI.PlaceOne(_inventory, _index);
                }
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (_mouseUI.IsEmpty())
            {
                _mouseUI.PickUpAll(_inventory, _index);
            }
            SetDraggingVisual(true);
        }

        public void OnDrag(PointerEventData eventData)
        {
            // Визуально мышь уже следует курсору, ничего делать не нужно.
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            SetDraggingVisual(false);
            
            if (_mouseUI.IsEmpty()) return;
            
            _mouseUI.ReturnBack(_inventory, _index);
        }

        public void OnDrop(PointerEventData eventData)
        {
            if (!_mouseUI.IsEmpty())
            {
                _mouseUI.PlaceAll(_inventory, _index);
            }
        }

        private void SetDraggingVisual(bool dragging)
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = dragging ? 0.6f : 1f;
            }
        }

        private int FindAnyFreeOrMergeIndex(InventorySystem target)
        {
            var src = _inventory.GetSlot(_index);
            if (src.ItemData == null) return _index;

            for (int i = 0; i < target.InventorySize; i++)
            {
                var t = target.GetSlot(i);
                if (t.ItemData == src.ItemData && t.CanAddInStack(1))
                    return i;
            }
            
            if (target.TryGetFreeSlotIndex(out int freeIndex))
                return freeIndex;

            return 0;
        }
    }
}
