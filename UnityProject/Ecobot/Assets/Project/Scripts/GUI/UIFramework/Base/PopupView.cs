using UnityEngine;
using UnityEngine.UI;

namespace GUI.UIFramework.Base
{
    public abstract class PopupView<T> : WindowView<T> where T : WindowViewModel
    {
        [Header("Buttons")]
        [SerializeField] private Button btnClose;
        [SerializeField] private Button btnCloseAlt;

        protected virtual void OnEnable()
        {
            btnClose?.onClick.AddListener(OnCloseBtnClick);
            btnCloseAlt?.onClick.AddListener(OnCloseBtnClick);
        }
        
        protected virtual void OnDestroy()
        {
            btnClose?.onClick.RemoveListener(OnCloseBtnClick);
            btnCloseAlt?.onClick.RemoveListener(OnCloseBtnClick);
        }
        
        protected virtual void OnCloseBtnClick()
        {
            ViewModel.RequestClose();
        }
    }
}