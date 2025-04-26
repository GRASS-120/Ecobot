using GUI.Gameplay;
using GUI.Main;
using GUI.Programming;
using GUI.UIFramework;
using UnityEngine;

namespace GUI.Core
{
    public class UIManager : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private UIRootView uiRootView;
        
        [Header("UI Managers")]
        [SerializeField] private GameplayUIManager gameplayUIManager;
        [SerializeField] private ProgrammingUIManager programmingUIManager;
        
        public void Init()
        {
            gameplayUIManager.Init(uiRootView);
            // programmingUIManager.Init(uiRootView);
        }
    }
}