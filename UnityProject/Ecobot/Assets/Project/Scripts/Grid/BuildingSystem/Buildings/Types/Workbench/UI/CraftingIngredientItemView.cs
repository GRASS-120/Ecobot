using Inventory.CraftingSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Grid.BuildingSystem.Buildings.Types.Workbench.UI
{
    public class CraftingIngredientItemView : MonoBehaviour
    {
        [SerializeField] private Image itemIcon;
        [SerializeField] private TextMeshProUGUI amountText;
        [SerializeField] private Image borderImage;
        [SerializeField] private Color normalBorderColor = Color.white;
        [SerializeField] private Color missingBorderColor = Color.red;

        private RecipeIngredient _ingredient;
        private CraftingSystem _craftingSystem;

        public void Init(RecipeIngredient ingredient, CraftingSystem craftingSystem)
        {
            _ingredient = ingredient;
            _craftingSystem = craftingSystem;

            itemIcon.sprite = ingredient.item.icon;
            
            UpdateVisuals();
        }

        public void UpdateVisuals()
        {
            int currentAmount = _craftingSystem.GetResourceCount(_ingredient.item);
            amountText.text = $"{currentAmount}/{_ingredient.amount}";

            bool hasEnough = currentAmount >= _ingredient.amount;
            borderImage.color = hasEnough ? normalBorderColor : missingBorderColor;
        }
    }
}