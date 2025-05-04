using Game.Mods;
using Game.Mods.Core;
using GUI.Core;
using GUI.Gameplay.Windows;
using GUI.Main;
using GUI.UIFramework;
using UnityEngine;

namespace GUI.Gameplay
{
    /// <summary>
    /// Менеджер всех окон, связанных с Gameplay. Создает ViewModel для всех окон
    /// </summary>
    public class GameplayUIManager : MonoBehaviour, IUIManager
    {
        private GameUIRootViewModel _rootViewModel;

        public void Init(UIRootView rootView)
        {
            _rootViewModel = new GameUIRootViewModel();
            rootView.Dispatch(_rootViewModel);
            
            // OpenGameplayOverlay();
        }

        public void OpenOverlay()
        {
            var viewModel = new GameplayOverlayViewModel(this);
           
            _rootViewModel.OpenOverlay(viewModel);
        }

        // private GameplayOverlayViewModel OpenGameplayOverlay()
        // {
        //     // инвентарь, hud и тп
        //     
        //     var viewModel = new GameplayOverlayViewModel(this);
        //    
        //     _rootViewModel.OpenOverlay(viewModel);
        //     
        //     return viewModel;
        // }
        
        public PopupAViewModel OpenPopupA()
        {
            var viewModel = new PopupAViewModel();
            
            _rootViewModel.OpenPopup(viewModel);
            
            return viewModel;
        }

        // public GameMode GameMode => new BuildingMode();
    }
}