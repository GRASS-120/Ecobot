using System;
using R3;

namespace GUI.UIFramework.Base
{
    public abstract class WindowViewModel : IDisposable
    {
        public Observable<WindowViewModel> CloseRequested => _closeRequested;
        public abstract string Id { get; } 
        
        private readonly Subject<WindowViewModel> _closeRequested = new();

        public void RequestClose()
        {
            _closeRequested.OnNext(this);
        }
        
        public virtual void Dispose()
        {
            // TODO release managed resources here
        }
    }
}