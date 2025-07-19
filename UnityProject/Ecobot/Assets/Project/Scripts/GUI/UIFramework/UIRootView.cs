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
        public readonly Subject<Unit> OnDispatch = new Subject<Unit>();
        
        [SerializeField] private WindowsDispatcher windowsDispatcher;
        
        // Подписки на все открытые окна
        private readonly CompositeDisposable _disposable = new();
        
        // Управляет отрисовкой окон
        public virtual void Dispatch(UIRootController controller)
        {
            _disposable.Add(controller.OpenedOverlay.Subscribe(newScreen =>
            {
                windowsDispatcher.OpenOverlay(newScreen);
            }));
            
            // проверка попапов, которые могли уже быть добавлены до подписки
            foreach (var openedPopup in controller.OpenedPopups)
            {
                windowsDispatcher.OpenPopup(openedPopup);
            }
            
            _disposable.Add(controller.OpenedPopups.ObserveAdd().Subscribe(e =>
            {
                windowsDispatcher.OpenPopup(e.Value);
            }));
            
            _disposable.Add(controller.OpenedPopups.ObserveRemove().Subscribe(e =>
            {
                windowsDispatcher.ClosePopup(e.Value);
            }));
            
            OnDispatch.OnNext(Unit.Default);
        }

        private void OnDestroy()
        {
            _disposable.Dispose();
        }
    }
}