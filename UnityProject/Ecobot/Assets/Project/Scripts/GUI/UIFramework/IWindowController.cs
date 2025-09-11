using System;
using R3;
using UnityEngine;

namespace GUI.UIFramework
{
    public interface IWindowController : IDisposable
    {
        public Observable<IWindowController> OnCloseEvent { get; }
        public Observable<IWindowController> OnOpenEvent { get; }
        
        public string Id { get; } 
        public WindowType WindowType { get; }
        
        public void Bind(WindowView windowView){}
        public void Close(){}
        public void Open(){}
        public void OnOpen(){}
        public void OnClose(){}
    }
}