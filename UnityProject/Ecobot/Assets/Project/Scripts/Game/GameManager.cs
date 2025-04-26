using System;
using FiniteStateMachine;
using Game.Mods;
using Game.Mods.Core;
using GUI.Core;
using GUI.Main;
using Player.InputManager;
using R3;
using UnityEngine;
using Utils;

namespace Game
{
    public class GameManager : MonoBehaviour {
        [SerializeField] private UIManager uiManager;
        [SerializeField] private PlayerInputManager inputManager;

        public ReactiveProperty<GameMode> CurrentMode;

        private StateMachine _stateMachine;
        public GameplayMode GameplayMode { get; private set; }
        public BuildingMode BuildingMode { get; private set; }
        public MenuMode MenuMode { get; private set; }
        public ProgrammingMode ProgrammingMode { get; private set; }

        private ToggleObject<GameMode> _gameplayOrBuilding;
        
        private void Awake()
        {
            _stateMachine = new StateMachine();
            GameplayMode = new GameplayMode(inputManager);
            BuildingMode = new BuildingMode(inputManager);
            MenuMode = new MenuMode(inputManager);
            ProgrammingMode = new ProgrammingMode(inputManager);
            
            _gameplayOrBuilding = new ToggleObject<GameMode>(GameplayMode, BuildingMode);
            
            At(GameplayMode, BuildingMode, new FuncPredicate(() => _gameplayOrBuilding.GetState() == BuildingMode));
            At(BuildingMode, GameplayMode, new FuncPredicate(() => _gameplayOrBuilding.GetState() == GameplayMode));
            
            CurrentMode = new ReactiveProperty<GameMode>(GameplayMode);
            _stateMachine.SetState(GameplayMode);
            
            uiManager.Init();
        }
    
        private void Start()
        {
            inputManager.OnToggleBuildMode += OnToggleBuildMode_Callback;
        }

        private void Update()
        {
            _stateMachine.Update();
        }

        private void FixedUpdate()
        {
            _stateMachine.FixedUpdate();
        }

        private void OnToggleBuildMode_Callback()
        {
            _gameplayOrBuilding.Toggle();
            SetCurrentMode(_gameplayOrBuilding.GetState());
        }

        private void SetCurrentMode(GameMode mode)
        {
            CurrentMode.Value = mode;
        }

        private void At(IState from, IState to, IPredicate condition)
        {
            _stateMachine.AddTransition(from, to, condition);
        }

        private void Any(IState to, IPredicate condition)
        {
            _stateMachine.AddAnyTransition(to, condition);
        }
    }
}