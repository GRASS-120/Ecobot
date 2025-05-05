using GUI.UIFramework;
using UnityEngine;
using UnityEngine.UI;

namespace GUI.Programming.Windows
{
    public class ProgrammingOverlayView : OverlayView<ProgrammingOverlayViewModel>
    {
        [Header("UI Elements")]
        [SerializeField] private Button btnClose;
        
        private void OnEnable()
        {
            btnClose.onClick.AddListener(OnCloseBtn_Clicked);
        }
        
        private void OnDisable()
        {
            btnClose.onClick.RemoveListener(OnCloseBtn_Clicked);
        }
        
        private void OnCloseBtn_Clicked()
        {
            ViewModel.RequestCloseOverlay();
        }
    }
}