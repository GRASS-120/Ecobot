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

        private InventorySystem _inventory;
        private int _index;
        private MouseInventoryItemUI _mouseUI;
        private InventoryOperationsService _ops;
        private InventorySystem _quickMoveTarget;
        private CompositeDisposable _disposables;

        public void Init(
            InventorySystem inventory, 
            int index, 
            MouseInventoryItemUI mouseUI,
            InventoryOperationsService ops,
            CompositeDisposable disposables,
            InventorySystem quickMoveTarget = null)
        {
            _inventory = inventory;
            _index = index;
            _mouseUI = mouseUI;
            _ops = ops;
            _disposables = disposables;
            _quickMoveTarget = quickMoveTarget;

            // Refresh();
        }

        public void Refresh()
        {
            var slot = _inventory.GetSlot(_index);
            
            Debug.Log($"[SlotUI] inv={_inventory.GetHashCode()} idx={_index} item={(slot.ItemData ? slot.ItemData.name : "null")} cnt={slot.StackSize}");

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
        }

        // ЛКМ/ПКМ/Shift+ЛКМ
        public void OnPointerClick(PointerEventData eventData)
        {
            bool shift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

            if (eventData.button == PointerEventData.InputButton.Left)
            {
                if (shift && _quickMoveTarget != null)
                {
                    // Быстро переложить весь стак в целевой инвентарь (если возможно)
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

        // Drag & Drop
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
            // Если бросили не на слот (мимо), возвращаем назад в исходный слот
            if (_mouseUI.IsEmpty()) return; // уже куда-то положили
            // Если курсор над UI, но не над слотом — просто вернем назад
            _mouseUI.ReturnBack(_inventory, _index);
        }

        public void OnDrop(PointerEventData eventData)
        {
            // Когда на нас что-то дропнули — пытаемся положить из мыши в этот слот
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

        // Вспомогательно: ищем индекс для quick move — сначала мердж, потом пустой слот
        private int FindAnyFreeOrMergeIndex(InventorySystem target)
        {
            var src = _inventory.GetSlot(_index);
            if (src.ItemData == null) return _index;

            // 1) Сначала пробуем найти стак той же номенклатуры с местом
            for (int i = 0; i < target.InventorySize; i++)
            {
                var t = target.GetSlot(i);
                if (t.ItemData == src.ItemData && t.CanAddInStack(1))
                    return i;
            }
            // 2) Потом пустой
            if (target.TryGetFreeSlotIndex(out int freeIndex))
                return freeIndex;

            // Если не нашли — вернем текущий, Move просто не выполнится
            return 0;
        }
    }
}
