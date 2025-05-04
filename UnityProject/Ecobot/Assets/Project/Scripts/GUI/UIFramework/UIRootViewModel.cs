using System;
using System.Collections.Generic;
using System.Linq;
using ObservableCollections;
using R3;
using UnityEngine;

namespace GUI.UIFramework
{
    /// <summary>
    /// Базовая модель представления для корневого UI.
    /// Хранит текущие открытые окна, предоставляет методы для работы с ними
    /// </summary>
    public abstract class UIRootViewModel : IDisposable
    {
        public ReadOnlyReactiveProperty<WindowViewModel> OpenedScreen => _openedScreen;
        public IObservableCollection<WindowViewModel> OpenedPopups => _openedPopups;
        
        private ReactiveProperty<WindowViewModel> _openedScreen = new();
        private ObservableList<WindowViewModel> _openedPopups = new();
        
        // todo: при помощи этого словаря по идее можно реализовать cached - услово, те окна, что не показываются, можно
        // скрыть... хотя диспозить же не нужно, хм...
        private Dictionary<WindowViewModel, IDisposable> _popupSubscriptions = new();
        
        public void OpenOverlay(WindowViewModel overlay)
        {
            _openedScreen.Value?.Dispose();
            _openedScreen.Value = overlay;
        }

        public void OpenPopup(WindowViewModel popup)
        {
            if (_openedPopups.Contains(popup))
            {
                Debug.LogError($"{popup} already opened");
                return;
            }

            var sub = popup.CloseRequested.Subscribe(ClosePopup);
            _popupSubscriptions[popup] = sub;
            
            _openedPopups.Add(popup);
        }

        public void ClosePopup(WindowViewModel popup)
        {
            if (_openedPopups.Contains(popup))
            {
                popup.Dispose();
                _openedPopups.Remove(popup);
                
                var sub = _popupSubscriptions[popup];
                sub.Dispose();
                _popupSubscriptions.Remove(popup);
            }
        }

        public void ClosePopup(string popupId)
        {
            var popup = _openedPopups.FirstOrDefault(p => p.Id == popupId);
            ClosePopup(popup);
        }

        public void CloseAllPopups()
        {
            foreach (var openedPopup in _openedPopups)
            {
                ClosePopup(openedPopup);
            }
        }

        public void Dispose()
        {
            CloseAllPopups();
            _openedScreen.Value?.Dispose();
        }
    }
}