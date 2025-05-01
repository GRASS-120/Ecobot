using System;
using GUI.UIFramework;
using UnityEngine;
using UnityEngine.UI;

namespace GUI.Main
{
    public class GamelayOverlayView : OverlayView<GameScreenViewModel>
    {
        [Header("Buttons")]
        [SerializeField] private Button btnPopupA;
        [SerializeField] private Button btnPopupB;
        [SerializeField] private Button btnGoToMainMenu;

        // override OnBind для реализации доп логики
        protected override void OnBind(GameScreenViewModel model)
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