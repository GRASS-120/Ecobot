using System;
using R3;

namespace GUI.UIFramework
{
    /// <summary>
    /// Абстрактная базовая модель представления для окон UI.
    /// </summary>
    public abstract class WindowController : IDisposable
    {
        public Observable<WindowController> OnClose => _closeRequested;
        public Observable<WindowController> OnOpen => _openRequested;
        
        // Id == название префаба, который кладем в Resources!
        public abstract string Id { get; } 
        
        // protected 
        
        private readonly Subject<WindowController> _closeRequested = new();
        private readonly Subject<WindowController> _openRequested = new();

        public void Load(IWindowView windowView)
        {
            
        }
        
        public void Close()
        {
            _closeRequested.OnNext(this);
        }
        
        public void Open()
        {
            _openRequested.OnNext(this);
        }
        
        public virtual void Dispose() { }
    }
}