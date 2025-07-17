using GUI.UIFramework;
using Inventory.UI;
using UnityEngine;
using UnityEngine.UI;

namespace GUI.Gameplay.Windows
{
    public class GameplayOverlayView : OverlayView<GameplayOverlayViewModel>
    {
        [Header("Buttons")]
        [SerializeField] private InventoryUIController inventoryController;

        protected override void OnBind(GameplayOverlayViewModel model)
        {
            base.OnBind(model);
            
            // player.inv -> invCUI 
            // inventoryController
        }
    }
}