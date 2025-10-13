using GUI.Gameplay.Windows.Controller;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Grid.BuildingSystem.Buildings.Types.Furnance.UI
{
    public class SmeltingRecipeItemView : MonoBehaviour
    {
        [SerializeField] private Image itemIcon;
        [SerializeField] private TextMeshProUGUI itemNameText;
        [SerializeField] private Button selectButton;
        [SerializeField] private Color normalTextColor = Color.white;
        [SerializeField] private Color unavailableTextColor = Color.gray;

        private SmeltingRecipeData _recipe;
        private FurnaceWindowController _controller;

        public void Init(SmeltingRecipeData recipe, FurnaceWindowController controller, bool isAvailable)
        {
            _recipe = recipe;
            _controller = controller;
            itemIcon.sprite = recipe.resultItem.icon;
            itemNameText.text = recipe.resultItem.displayName;
            selectButton.onClick.RemoveAllListeners();
            selectButton.onClick.AddListener(() => _controller.SelectRecipe(_recipe));
            UpdateVisuals(isAvailable);
        }

        public void UpdateVisuals(bool isAvailable)
        {
            itemNameText.color = isAvailable ? normalTextColor : unavailableTextColor;
        }
    }
}