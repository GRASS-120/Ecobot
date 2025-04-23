using Game;
using GUI.Main;
using UnityEngine;
using UnityEngine.Serialization;

namespace GUI.UIFramework.Base
{
    public class UIManager : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private UIRootView uiRootView;
        
        [Header("UI Managers")]
        [SerializeField] private GameplayUIManager gameplayUIManager;
        
        public void Init()
        {
            gameplayUIManager.Init(uiRootView);
            // var a = new UIRootViewModel();
            // uiRootBinder.Bind(a);
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