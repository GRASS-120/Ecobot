using System.Collections.Generic;
using Grid.BuildingSystem.Buildings.Types.Furnance.UI;
using GUI.UIFramework;
using Inventory.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GUI.Gameplay.Windows.View
{
    public class FurnaceWindowView : PopupView
    {
        [Header("Recipe List")]
        [SerializeField] private Transform recipeListContainer;
        [SerializeField] private GameObject recipeItemPrefab;

        [Header("Power Icon")]
        [SerializeField] private Image electricityIcon;
        [SerializeField] private Color powerOnColor = new Color(1f, 0.9f, 0f);
        [SerializeField] private Color powerOffColor = Color.gray;
        
        [SerializeField] private Image resultIcon;
        [SerializeField] private TextMeshProUGUI resultNameText;
        [SerializeField] private TextMeshProUGUI resultAmountText;

        [SerializeField] private Image oreIcon;
        [SerializeField] private TextMeshProUGUI oreNameText;
        [SerializeField] private TextMeshProUGUI oreAmountText;
        
        [SerializeField] private Image progressFill;
        [SerializeField] private Button closeButton;

        [Header("Slots (Roots)")]
        [SerializeField] private InventorySlotUI oreSlot;
        [SerializeField] private InventorySlotUI fuelSlot;
        [SerializeField] private InventorySlotUI outputSlot;

        public Button CloseButton => closeButton;

        public SmeltingRecipeItemView CreateRecipeItem()
        {
            var go = Instantiate(recipeItemPrefab, recipeListContainer);
            return go.GetComponent<SmeltingRecipeItemView>();
        }

        public void ClearRecipeList(List<SmeltingRecipeItemView> items)
        {
            foreach (var item in items)
            {
                if (item != null) Destroy(item.gameObject);
            }
        }
        
        public void SetOreDisplay(Sprite icon, string name, int amount)
        {
            if (oreIcon != null)
            {
                oreIcon.sprite = icon;
                oreIcon.enabled = icon != null;
            }
            if (oreNameText != null)
            {
                oreNameText.text = string.IsNullOrEmpty(name) ? "" : name;
            }
            if (oreAmountText != null)
            {
                oreAmountText.text = amount > 0 ? $"x{amount}" : "";
            }
        }

        public void SetElectricity(bool on)
        {
            if (electricityIcon != null)
                electricityIcon.color = on ? powerOnColor : powerOffColor;
        }

        public void SetResultDisplay(Sprite icon, string name, int amount)
        {
            if (resultIcon != null) resultIcon.sprite = icon;
            if (resultNameText != null) resultNameText.text = string.IsNullOrEmpty(name) ? "" : name;
            if (resultAmountText != null) resultAmountText.text = amount > 0 ? $"x{amount}" : "x0";
        }

        public void SetProgress(float v01)
        {
            if (progressFill != null) progressFill.fillAmount = Mathf.Clamp01(v01);
        }

        public InventorySlotUI OreSlot => oreSlot;
        public InventorySlotUI FuelSlot => fuelSlot;
        public InventorySlotUI OutputSlot => outputSlot;
    }
}