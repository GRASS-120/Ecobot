using System;
using R3;

namespace GUI.UIFramework
{
    /// <summary>
    /// Абстрактная базовая модель представления для окон UI.
    /// </summary>
    public abstract class WindowViewModel : IDisposable
    {
        // Id == название префаба, который кладем в Resources!
        public abstract string Id { get; } 
        
        public Observable<WindowViewModel> CloseRequested => _closeRequested;
        
        private readonly Subject<WindowViewModel> _closeRequested = new();

        public void RequestClose()
        {
            _closeRequested.OnNext(this);
        }
        
        public virtual void Dispose() { }
    }
}