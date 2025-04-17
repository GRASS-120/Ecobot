using UnityEngine;

namespace GUI.UIFramework.Base
{
    public abstract class WindowBinder<T> : MonoBehaviour, IWindowBinder where T : WindowViewModel
    {
        protected T ViewModel;

        public void Bind(WindowViewModel viewModel)
        {
            ViewModel = (T)viewModel;

            OnBind(ViewModel);
        }

        public virtual void Close()
        {
            // Сначала уничтожаем, затем делаем анимации на закрытие
            Destroy(gameObject);
        }
        
        protected virtual void OnBind(T viewModel) { }
    }
}