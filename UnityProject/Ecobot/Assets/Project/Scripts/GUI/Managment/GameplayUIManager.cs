using GUI.UIFramework.Base;
using UnityEngine;

namespace GUI.Main
{
    // менеджер всех окон в game
    public class GameplayUIManager : MonoBehaviour
    {
        private GameUIRootViewModel _rootViewModel;

        public void Init(UIRootView rootView)
        {
            _rootViewModel = new GameUIRootViewModel();
            rootView.Bind(_rootViewModel);
            
            OpenScreenGame();
        }
        
        public GameScreenViewModel OpenScreenGame()
        {
            var viewModel = new GameScreenViewModel(this);
            // var rootUI = UIGameRootViewModel
            Debug.Log("Open Screen Game " + viewModel);
            _rootViewModel.OpenScreen(viewModel);
            
            return viewModel;
        }
        
        public PopupAViewModel OpenPopupA()
        {
            var viewModel = new PopupAViewModel();
            // var rootUI = UIGameRootViewModel
            
            _rootViewModel.OpenPopup(viewModel);
            
            return viewModel;
        }
        
        public PopupBViewModel OpenPopupB()
        {
            var viewModel = new PopupBViewModel();
            // var rootUI = UIGameRootViewModel
            
            _rootViewModel.OpenPopup(viewModel);
            
            return viewModel;
        }
    }
}