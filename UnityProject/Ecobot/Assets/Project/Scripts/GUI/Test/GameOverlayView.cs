using System;
using GUI.UIFramework;
using UnityEngine;
using UnityEngine.UI;

namespace GUI.Main
{
    public class GameOverlayView : OverlayView<GameScreenViewModel>
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
            btnPopupB.onClick.AddListener(OnPopupBBtn_Clicked);
            btnGoToMainMenu.onClick.AddListener(OnGoToMainMenuBtn_Clicked);
        }
        
        private void OnDisable()
        {
            btnPopupA.onClick.RemoveListener(OnPopupABtn_Clicked);
            btnPopupB.onClick.RemoveListener(OnPopupBBtn_Clicked);
            btnGoToMainMenu.onClick.RemoveListener(OnGoToMainMenuBtn_Clicked);
        }

        private void OnPopupABtn_Clicked()
        {
            ViewModel.RequestOpenPopupA();
        }
        
        private void OnPopupBBtn_Clicked()
        {
            ViewModel.RequestOpenPopupB();
        }
        
        private void OnGoToMainMenuBtn_Clicked()
        {
            ViewModel.RequestGoToMainMenu();
        }
    }
}