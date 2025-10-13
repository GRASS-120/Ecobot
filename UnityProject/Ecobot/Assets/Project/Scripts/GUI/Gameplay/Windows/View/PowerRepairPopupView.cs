using System.Collections.Generic;
using Grid.BuildingSystem.Buildings.Types.Workbench.UI;
using GUI.UIFramework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GUI.Gameplay.Windows.View
{
    public class PowerRepairPopupView : PopupView
    {
        [Header("UI")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private Transform ingredientsContainer;
        [SerializeField] private GameObject ingredientItemPrefab;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button repairButton;

        public Button CloseButton => closeButton;
        public Button RepairButton => repairButton;

        public void SetTitle(string text)
        {
            if (titleText != null) titleText.text = text;
        }

        public CraftingIngredientItemView CreateIngredientItem()
        {
            var go = Instantiate(ingredientItemPrefab, ingredientsContainer);
            return go.GetComponent<CraftingIngredientItemView>();
        }

        public void ClearIngredientList(List<CraftingIngredientItemView> items)
        {
            foreach (var item in items)
            {
                if (item != null) Destroy(item.gameObject);
            }
        }

        public void SetRepairButtonInteractable(bool value)
        {
            if (repairButton != null) repairButton.interactable = value;
        }
    }
}