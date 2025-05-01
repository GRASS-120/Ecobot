using UnityEngine;

namespace GUI.UIFramework
{
    /// <summary>
    /// Базовый абстрактный класс для всех оконных представлений UI.
    /// Обеспечивает механизм привязки модели представления к окну и стандартное закрытие окна.
    /// </summary>
    /// <typeparam name="T">Тип модели представления, наследуемой от <see cref="WindowViewModel"/>.</typeparam>
    public abstract class WindowView<T> : MonoBehaviour, IWindowView where T : WindowViewModel
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