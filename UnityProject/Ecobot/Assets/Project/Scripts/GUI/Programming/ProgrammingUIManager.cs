using GUI.Core;
using GUI.Gameplay;
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
            // OpenScreenGame();
        }

        public void OpenProgrammingScreen()
        {
            // var viewModel = new Prog(this);
            //
            // _rootViewModel.OpenScreen(viewModel);
            //
            // return viewModel;
        }
    }
}