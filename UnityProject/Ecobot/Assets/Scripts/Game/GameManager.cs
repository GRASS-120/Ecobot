using System;
using Game.Mods;
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
        // public event Action<Mode> OnModeChanged;

        [SerializeField] private PlayerInputManager inputManager;

        public ReactiveProperty<GameMode> CurrentMode; // todo: reactive property

        public enum Mode {
            Default,
            Building,
            Inventory,
            Interface,
            Programming,
            Menu
        }

        private void Awake()
        {
            CurrentMode = new ReactiveProperty<GameMode>();
        }
    
        private void Start()
        {
            inputManager.OnToggleBuildMode += OnToggleBuildMode_Callback;
        }
        
        // как переключать режимы?
        // private void HandleGameMode()

        private void OnToggleBuildMode_Callback(object sender, EventArgs e)
        {
            // _currentMode = _currentMode == Mode.Default ? Mode.Building : Mode.Default;
            // OnModeChanged?.Invoke(_currentMode);
        }

        public void ChangeMode(Mode mode)
        {
            // CurrentMode.Value = mode;
            // _currentMode = mode;
            // OnModeChanged?.Invoke(_currentMode);
        }
    }
}