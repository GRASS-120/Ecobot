using ObservableCollections;
using R3;
using UnityEngine;

namespace GUI.UIFramework.Base
{
    public class UIRootBinder : MonoBehaviour
    {
        private readonly CompositeDisposable _subscriptions = new();
        
        public void Bind(UIRootViewModel viewModel)
        {
            _subscriptions.Add(viewModel.OpenedScreen.Subscribe(newScreen =>
            {
                
            }));
            
            // проверка попапов, которые могли уже быть добавлены до подписки
            foreach (var openedPopup in viewModel.OpenedPopups)
            {
                
            }
            
            _subscriptions.Add(viewModel.OpenedPopups.ObserveAdd().Subscribe(e =>
            {
                
            }));
            
            _subscriptions.Add(viewModel.OpenedPopups.ObserveRemove().Subscribe(e =>
            {
                
            }));
        }
    }
}