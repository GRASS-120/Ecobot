using System;
using GUI.UIFramework.Base;
using UnityEngine;
using UnityEngine.UI;

namespace GUI.Main
{
    public class ScreenGameBinder : WindowBinder<ScreenGameViewModel>
    {
        [Header("Buttons")]
        [SerializeField] private Button btnPopupA;
        [SerializeField] private Button btnPopupB;
        [SerializeField] private Button btnGoToMainMenu;

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
            ViewModel.RequestOpenPopupA();
        }
        
        private void OnGoToMainMenuBtn_Clicked()
        {
            ViewModel.RequestGoToMainMenu();
        }
    }
}