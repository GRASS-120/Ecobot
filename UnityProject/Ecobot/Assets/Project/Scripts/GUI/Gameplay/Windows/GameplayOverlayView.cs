using GUI.UIFramework;
using UnityEngine;
using UnityEngine.UI;

namespace GUI.Gameplay.Windows
{
    public class GameplayOverlayView : OverlayView<GameplayOverlayViewModel>
    {
        [Header("Buttons")]
        [SerializeField] private Button btnPopupA;
        [SerializeField] private Button btnPopupB;
        [SerializeField] private Button btnGoToMainMenu;

        protected override void OnBind(GameplayOverlayViewModel model)
        {
            base.OnBind(model);
        }
        
        private void OnEnable()
        {
            btnPopupA.onClick.AddListener(OnPopupABtn_Clicked);
        }
        
        private void OnDisable()
        {
            btnPopupA.onClick.RemoveListener(OnPopupABtn_Clicked);
        }

        private void OnPopupABtn_Clicked()
        {
            ViewModel.RequestOpenPopupA();
        }
    }
}