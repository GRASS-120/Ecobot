using ObservableCollections;
using R3;
using UnityEngine;
using UnityEngine.Serialization;

namespace GUI.UIFramework.Base
{
    /// <summary>
    /// Класс, который накидываем на Canvas. Наследники ему нужны, так как
    /// например, логика в игре и логика в меню может различаться
    /// </summary>
    public abstract class UIRootView : MonoBehaviour
    {
        [SerializeField] private WindowsDispatcher windowsDispatcher;
        
        private readonly CompositeDisposable _subscriptions = new();
        
        public virtual void Bind(UIRootViewModel viewModel)
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
            
            OnBind();
        }
        
        // todo: такие функции переделать на Action или ReactiveCommand
        protected virtual void OnBind() {}

        private void OnDestroy()
        {
            _subscriptions.Dispose();
        }
    }
}