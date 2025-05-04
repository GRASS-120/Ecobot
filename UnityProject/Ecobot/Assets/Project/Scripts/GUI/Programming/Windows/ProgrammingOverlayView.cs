using GUI.UIFramework;
using UnityEngine;
using UnityEngine.UI;

namespace GUI.Programming.Windows
{
    public class ProgrammingOverlayView : OverlayView<ProgrammingOverlayViewModel>
    {
        // [SerializeField] private Button btnClose;
        //
        // protected override void OnBind(ProgrammingOverlayViewModel model)
        // {
        //     base.OnBind(model);
        // }
        //
        // private void OnEnable()
        // {
        //     btnClose.onClick.AddListener(OnCloseBtn_Clicked);
        // }
        //
        // private void OnDisable()
        // {
        //     btnClose.onClick.RemoveListener(OnCloseBtn_Clicked);
        // }
        //
        // private void OnCloseBtn_Clicked()
        // {
        //     Debug.Log("OnCloseBtn_Clicked");
        //     ViewModel.RequestClose();
        // }
    }
}