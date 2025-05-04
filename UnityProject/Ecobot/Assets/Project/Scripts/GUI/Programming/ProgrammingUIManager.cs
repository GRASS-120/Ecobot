using GUI.Core;
using GUI.Gameplay;
using GUI.Programming.Windows;
using GUI.UIFramework;
using UnityEngine;

namespace GUI.Programming
{
    public class ProgrammingUIManager : MonoBehaviour, IUIManager
    {
        private ProgrammingUIRootViewModel _rootViewModel;

        public void Init(UIRootView rootView)
        {
            _rootViewModel = new ProgrammingUIRootViewModel();
            rootView.Dispatch(_rootViewModel);
            
            // открываем по умолчанию
            // OpenProgrammingOverlay();
        }

        public void OpenOverlay()
        {
            var viewModel = new ProgrammingOverlayViewModel(this);
            
            _rootViewModel.OpenOverlay(viewModel);
        }

        // private ProgrammingOverlayViewModel OpenProgrammingOverlay()
        // {
        //     var viewModel = new ProgrammingOverlayViewModel(this);
        //     
        //     _rootViewModel.OpenOverlay(viewModel);
        //  
        //     return viewModel;
        // }
    }
}