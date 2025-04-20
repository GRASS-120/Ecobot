using Game;
using GUI.Main;
using UnityEngine;
using UnityEngine.Serialization;

namespace GUI.UIFramework.Base
{
    /// <summary>
    /// Знает, как создавать viewModel
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private UIRootView uiRootView;
        [SerializeField] private GameManager gameManager;
        
        [Header("UI Managers")]
        [SerializeField] private GameplayUIManager gameplayUIManager;

        // public UIRootBinder UIRootBinder => uiRootBinder;
        
        public void Init()
        {
            // var a = new UIRootViewModel();
            // uiRootBinder.Bind(a);
            gameplayUIManager.Init(uiRootView);
            // Запрашиваем рутовую вью модель и пихаем ее в баиндер, который создали
            // var uiSceneRootViewModel = new UI;
            // uiSceneRootBinder.Bind(uiSceneRootViewModel);
            //
            // // можно открывать окошки
            // var uiManager = viewsContainer.Resolve<GameplayUIManager>();
            // uiManager.OpenScreenGameplay();
        }
    }
}