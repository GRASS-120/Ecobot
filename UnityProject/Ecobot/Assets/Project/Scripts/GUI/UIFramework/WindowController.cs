using System;
using R3;
using UnityEditor.PackageManager.UI;

namespace GUI.UIFramework
{
    /// <summary>
    /// Абстрактная базовая модель представления для окон UI.
    /// </summary>
    public abstract class WindowController<T> : IWindowController where T : WindowView
    {
        public Observable<IWindowController> OnCloseEvent => _closeRequested;
        public Observable<IWindowController> OnOpenEvent => _openRequested;
        
        // Id == название префаба, который кладем в Resources!
        public abstract string Id { get; } 
        
        protected T View { get; private set; }
        
        private readonly Subject<IWindowController> _closeRequested = new();
        private readonly Subject<IWindowController> _openRequested = new();

        public void Bind(WindowView windowView)
        {
            View = (T)windowView;
        }
        
        public void Close()
        {
            OnClose();
            _closeRequested.OnNext(this);
        }
        
        public void Open()
        {
            OnOpen();
            _openRequested.OnNext(this);
        }
        
        public virtual void OnClose()
        {
            _closeRequested.OnNext(this);
        }
        
        public virtual void OnOpen()
        {
            _openRequested.OnNext(this);
        }
        
        public virtual void Dispose() { }
    }
}