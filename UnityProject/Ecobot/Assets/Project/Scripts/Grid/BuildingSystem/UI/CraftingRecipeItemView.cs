using GUI.Gameplay.Windows.Controller;
using Inventory.CraftingSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Grid.BuildingSystem.UI
{
    public class CraftingRecipeItemView : MonoBehaviour
    {
        [SerializeField] private Image itemIcon;
        [SerializeField] private TextMeshProUGUI itemNameText;
        [SerializeField] private Button selectButton;
        [SerializeField] private Color normalTextColor = Color.white;
        [SerializeField] private Color unavailableTextColor = Color.gray;

        private CraftingRecipeData _recipe;
        private CraftingWindowController _controller;

        public void Init(CraftingRecipeData recipe, CraftingWindowController controller)
        {
            _recipe = recipe;
            _controller = controller;

            itemIcon.sprite = recipe.ResultItem.icon;
            itemNameText.text = recipe.ResultItem.displayName;

            selectButton.onClick.AddListener(() => _controller.SelectRecipe(_recipe));

            UpdateVisuals(controller.CraftingSystem);
        }

        public void UpdateVisuals(CraftingSystem craftingSystem)
        {
            bool canCraft = craftingSystem.CanCraft(_recipe);
            itemNameText.color = canCraft ? normalTextColor : unavailableTextColor;
        }
    }
}