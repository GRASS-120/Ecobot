using GUI.UIFramework;
using GUI.UIFramework.Base;
using UnityEngine;

namespace GUI.Main
{
    /// <summary>
    /// Менеджер всех окон, связанных с Gameplay. Создает ViewModel для всех окон
    /// </summary>
    public class GameplayUIManager : MonoBehaviour
    {
        private GameUIRootViewModel _rootViewModel;

        public void Init(UIRootView rootView)
        {
            _rootViewModel = new GameUIRootViewModel();
            rootView.Dispatch(_rootViewModel);
            
            // открываем по умолчанию
            OpenScreenGame();
        }
        
        public GameScreenViewModel OpenScreenGame()
        {
            var viewModel = new GameScreenViewModel(this);
           
            _rootViewModel.OpenScreen(viewModel);
            
            return viewModel;
        }
        
        public PopupAViewModel OpenPopupA()
        {
            var viewModel = new PopupAViewModel();
            
            _rootViewModel.OpenPopup(viewModel);
            
            return viewModel;
        }
        
        public PopupBViewModel OpenPopupB()
        {
            var viewModel = new PopupBViewModel();
            
            _rootViewModel.OpenPopup(viewModel);
            
            return viewModel;
        }
    }
}