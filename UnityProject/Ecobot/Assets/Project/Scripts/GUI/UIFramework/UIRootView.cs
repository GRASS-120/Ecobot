using ObservableCollections;
using R3;
using UnityEngine;

namespace GUI.UIFramework
{
    /// <summary>
    /// Абстрактный класс для корневого представления пользовательского интерфейса.
    /// Размещается на Canvas и управляет диспетчеризацией экранов и всплывающих окон через <see cref="WindowsDispatcher"/>.
    /// Наследники ему нужны, так как, например, логика в игре и логика в меню может различаться
    /// </summary>
    public abstract class UIRootView : MonoBehaviour
    {
        [SerializeField] private WindowsDispatcher windowsDispatcher;
        
        // Подписки на все открытые окна
        private readonly CompositeDisposable _subscriptions = new();
        
        // Управляет отрисовкой окон
        public virtual void Dispatch(UIRootViewModel viewModel)
        {
            _subscriptions.Add(viewModel.OpenedScreen.Subscribe(newScreen =>
            {
                windowsDispatcher.OpenScreen(newScreen);
            }));
            
            // проверка попапов, которые могли уже быть добавлены до подписки
            foreach (var openedPopup in viewModel.OpenedPopups)
            {
                windowsDispatcher.OpenPopup(openedPopup);
            }
            
            _subscriptions.Add(viewModel.OpenedPopups.ObserveAdd().Subscribe(e =>
            {
                windowsDispatcher.OpenPopup(e.Value);
            }));
            
            _subscriptions.Add(viewModel.OpenedPopups.ObserveRemove().Subscribe(e =>
            {
                windowsDispatcher.ClosePopup(e.Value);
            }));
            
            OnDispatch();
        }
        
        // todo: такие функции переделать на Action или ReactiveCommand
        protected virtual void OnDispatch() {}

        private void OnDestroy()
        {
            _subscriptions.Dispose();
        }
    }
}