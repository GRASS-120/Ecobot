using System;
using System.Collections.Generic;
using Grid.BuildingSystem.Buildings.Types.Workbench.UI;
using GUI.Gameplay.Windows.View;
using GUI.UIFramework;
using Inventory.CraftingSystem;
using R3;

namespace GUI.Gameplay.Windows.Controller
{
    [Window(WindowType.Popup, "PowerRepairPopup")]
    public class PowerRepairPopupController : WindowController<PowerRepairPopupView>
    {
        public override string Id => "PowerRepairPopup";

        private CraftingSystem _craftingSystem;
        private List<RecipeIngredient> _cost;
        private string _title;
        private Func<bool> _tryConsume;
        private Action _onSuccess;

        private readonly List<CraftingIngredientItemView> _items = new();

        public void Init(
            CraftingSystem craftingSystem,
            List<RecipeIngredient> cost,
            string title,
            Func<bool> tryConsume,
            Action onSuccess)
        {
            _craftingSystem = craftingSystem;
            _cost = cost;
            _title = title;
            _tryConsume = tryConsume;
            _onSuccess = onSuccess;
        }

        public override void OnOpen()
        {
            base.OnOpen();

            View.SetTitle(_title);
            BuildList();
            UpdateVisuals();

            View.CloseButton.OnClickAsObservable()
                .Subscribe(_ => Close())
                .AddTo(Subs);

            View.RepairButton.OnClickAsObservable()
                .Subscribe(_ => OnRepairClick())
                .AddTo(Subs);
        }

        public override void OnClose()
        {
            base.OnClose();
            View.ClearIngredientList(_items);
            _items.Clear();
        }

        private void BuildList()
        {
            View.ClearIngredientList(_items);
            _items.Clear();

            foreach (var ing in _cost)
            {
                var item = View.CreateIngredientItem();
                item.Init(ing, _craftingSystem);
                _items.Add(item);
            }
        }

        private void UpdateVisuals()
        {
            foreach (var it in _items) it.UpdateVisuals();

            bool canRepair = true;
            foreach (var ing in _cost)
            {
                if (_craftingSystem.GetResourceCount(ing.item) < ing.amount)
                {
                    canRepair = false;
                    break;
                }
            }
            View.SetRepairButtonInteractable(canRepair);
        }

        private void OnRepairClick()
        {
            // Доп. проверка
            foreach (var ing in _cost)
            {
                if (_craftingSystem.GetResourceCount(ing.item) < ing.amount)
                {
                    UpdateVisuals();
                    return;
                }
            }

            // Пытаемся списать ресурсы
            if (_tryConsume != null && _tryConsume.Invoke())
            {
                _onSuccess?.Invoke();
                Close();
            }
            else
            {
                // Не удалось списать — обновим визуал (вдруг инвентарь изменился)
                UpdateVisuals();
            }
        }
    }
}