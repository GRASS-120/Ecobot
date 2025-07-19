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
    public abstract class UIRootController : IDisposable
    {
        public ReadOnlyReactiveProperty<WindowController> OpenedOverlay => _openedOverlay;
        public IObservableCollection<WindowController> OpenedPopups => _openedPopups;
        
        private ReactiveProperty<WindowController> _openedOverlay = new();
        private ObservableList<WindowController> _openedPopups = new();
        
        // todo: при помощи этого словаря по идее можно реализовать cached - услово, те окна, что не показываются, можно
        // скрыть... хотя диспозить же не нужно, хм...
        private Dictionary<WindowController, IDisposable> _popupSubscriptions = new();
        
        public void OpenOverlay(WindowController overlay)
        {
            overlay.Close();
            
            _openedOverlay.Value?.Dispose();
            _openedOverlay.Value = overlay;
            
            overlay.Open();
        }

        public void OpenPopup(WindowController popup)
        {
            if (_openedPopups.Contains(popup))
            {
                Debug.LogError($"{popup} already opened");
                return;
            }

            var sub = popup.OnClose.Subscribe(ClosePopup);
            _popupSubscriptions[popup] = sub;
            
            _openedPopups.Add(popup);
            
            popup.Open();
        }

        public void ClosePopup(WindowController popup)
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
            _openedOverlay.Value?.Dispose();
        }
    }
}