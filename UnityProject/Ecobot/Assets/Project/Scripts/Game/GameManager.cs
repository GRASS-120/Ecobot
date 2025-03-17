using System;
using Game.Mods;
// using Game.Mods;
using R3;
using UnityEngine;

namespace Game
{
    // пока схема такая:
    // в game manager запускается update для каждого режима
    // система долна определиться, когда ей работать - (добавить default mode, который работает всегда)
    // у мода есть ActionMode - иввент вызывается каждый update. на этот ивент должна подписаться система своей главной handle функцией
    // (имеет смысл сделать разные ивенты для разных функций - start и тп)
    
    public class GameManager : MonoBehaviour {
        [SerializeField] private PlayerInputManager inputManager;

        public ReactiveProperty<GameMode> CurrentMode; // todo: reactive property

        private void Awake()
        {
            // перенести в словарь (как со спелами)
            CurrentMode = new ReactiveProperty<GameMode>(new GameplayMode(inputManager));
        }
    
        private void Start()
        {
            CurrentMode.Subscribe(SetCurrentMode);
            inputManager.OnToggleBuildMode += OnToggleBuildMode_Callback;
        }
        
        // как переключать режимы?
        // private void HandleGameMode()

        private void OnToggleBuildMode_Callback(object sender, EventArgs e)
        {
            CurrentMode.Value = CurrentMode.Value is GameplayMode ? new BuildingMode(inputManager) : new GameplayMode(inputManager);
            // CurrentMode.Value = CurrentMode.Value is  Mode.Default ? Mode.Building : Mode.Default;
            // OnModeChanged?.Invoke(_currentMode);
        }

        private void SetCurrentMode(GameMode mode)
        {
            CurrentMode.Value.DisableInputMap();
            CurrentMode.Value = mode;
            CurrentMode.Value.ActivateInputMap();
        }

        // public void ChangeMode(Mode mode)
        // {
        //     // CurrentMode.Value = mode;
        //     // _currentMode = mode;
        //     // OnModeChanged?.Invoke(_currentMode);
        // }
    }
}