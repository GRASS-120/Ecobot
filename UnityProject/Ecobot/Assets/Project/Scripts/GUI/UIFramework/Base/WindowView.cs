using UnityEngine;

namespace GUI.UIFramework.Base
{
    public abstract class WindowView<T> : MonoBehaviour, IWindowView where T : WindowViewModel
    {
        protected T ViewModel;

        public void Bind(WindowViewModel viewModel)
        {
            Debug.Log("biba");
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