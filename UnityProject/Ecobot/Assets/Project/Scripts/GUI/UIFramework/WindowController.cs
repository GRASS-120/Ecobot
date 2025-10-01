using System;
using System.Reflection;
using R3;

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
        public WindowType WindowType { get; private set; }
        public bool IsOpen { get; private set; }
        
        protected T View { get; private set; }
        protected CompositeDisposable Subs { get; private set; } = new CompositeDisposable();
        
        private readonly Subject<IWindowController> _closeRequested = new();
        private readonly Subject<IWindowController> _openRequested = new();

        protected WindowController()
        {
            // как обращаться к атрибуту
            var attribute = GetType().GetCustomAttribute<WindowAttribute>();
            
            // todo: add prefab path
            if (attribute != null)
                WindowType = attribute.WindowType;
        }
        
        public void Bind(WindowView windowView)
        {
            View = (T)windowView;
        }
        
        public void Close()
        {
            OnClose();
            _closeRequested.OnNext(this);
            
            IsOpen = false;
            Subs?.Dispose();
            Subs = null;
        }
        
        public void Open()
        {
            Subs?.Dispose();
            Subs = new CompositeDisposable();
            
            OnOpen();
            
            IsOpen = true;
            _openRequested.OnNext(this);
        }
        
        public virtual void OnClose()
        {
        }
        
        public virtual void OnOpen()
        {
        }
        
        public virtual void Dispose() { }
    }
}