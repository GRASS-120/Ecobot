using FiniteStateMachine;
using Game.Mods;
using Game.Mods.Core;
using GUI.Core;
using Player.InputManager;
using R3;
using UnityEngine;
using Utils;

namespace Game
{
    public class GameManager : MonoBehaviour {
        [SerializeField] private UIManager uiManager;
        [SerializeField] private PlayerInputManager inputManager;

        public StateMachine FSM { get; private set; }
        public GameplayMode GameplayMode { get; private set; }
        public BuildingMode BuildingMode { get; private set; }
        public MenuMode MenuMode { get; private set; }
        public ProgrammingMode ProgrammingMode { get; private set; }

        private ToggleObject<GameMode> _gameplayOrBuilding;

        private void Awake()
        {
            GameplayMode = new GameplayMode(inputManager);
            BuildingMode = new BuildingMode(inputManager);
            MenuMode = new MenuMode(inputManager);
            ProgrammingMode = new ProgrammingMode(inputManager);

            _gameplayOrBuilding = new ToggleObject<GameMode>(GameplayMode, BuildingMode);

            FSM = new StateMachine();
            
            At(GameplayMode, BuildingMode, new FuncPredicate(() => _gameplayOrBuilding.GetState() == BuildingMode));
            At(BuildingMode, GameplayMode, new FuncPredicate(() => _gameplayOrBuilding.GetState() == GameplayMode));
            
            FSM.SetState(GameplayMode);

            uiManager.Init(this);
        }

        private void Start()
        {
            inputManager.OnToggleBuildMode += OnToggleBuildMode_Callback;

            FSM.CurrentState.Subscribe(OnCurrentStateChanged).AddTo(this); 
        }

        private void Update()
        {
            FSM.Update();
        }

        private void FixedUpdate()
        {
            FSM.FixedUpdate();
        }

        private void OnToggleBuildMode_Callback()
        {
            _gameplayOrBuilding.Toggle();
            FSM.SetState(_gameplayOrBuilding.GetState());
        }

        private void At(IState from, IState to, IPredicate condition)
        {
            FSM.AddTransition(from, to, condition);
        }

        private void Any(IState to, IPredicate condition)
        {
            FSM.AddAnyTransition(to, condition);
        }

        private void OnCurrentStateChanged(IState newState)
        {
            // Здесь вы можете реагировать на изменение состояния
            Debug.Log($"State changed to: {newState}");
            // Обновите UI или другие системы, которые зависят от текущего состояния
        }
    }
}