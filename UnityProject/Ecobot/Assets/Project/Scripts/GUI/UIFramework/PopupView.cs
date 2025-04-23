using GUI.UIFramework.Base;
using UnityEngine;
using UnityEngine.UI;

namespace GUI.UIFramework
{
    /// <summary>
    /// Абстрактный класс для всплывающих окон, отображающих небольшие элементы интерфейса поверх основного экрана.
    /// </summary>
    /// <typeparam name="T">Конкретный ViewModel (унаследован от <see cref="WindowViewModel"/>).</typeparam>
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