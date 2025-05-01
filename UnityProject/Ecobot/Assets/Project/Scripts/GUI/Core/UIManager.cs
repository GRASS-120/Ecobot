using System.Collections.Generic;
using Game;
using Game.Mods.Core;
using GUI.Gameplay;
using GUI.Main;
using GUI.Programming;
using GUI.UIFramework;
using R3;
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
        
        private GameManager _gameManager;
        
        // concrete active manager depends on game mode
        // и все? -> в целом можно использовать UIManager для переброса инфы между манагерами + проброс в них
        // данных
        
        public void Init(GameManager gameManager)
        {
            _gameManager = gameManager;

            gameplayUIManager.Init(uiRootView);
            programmingUIManager.Init(uiRootView);
        }
    }
}