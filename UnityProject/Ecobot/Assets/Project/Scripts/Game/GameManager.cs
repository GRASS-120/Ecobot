using FiniteStateMachine;
using Game.Mods;
using Game.Mods.Core;
using Grid.BuildingSystem;
using GUI.Core;
using GUI.Gameplay;
using Player;
using Player.InputManager;
using R3;
using UnityEngine;
using UnityEngine.Serialization;
using Utils;

namespace Game
{
    public class GameManager : MonoBehaviour
    {
        [Header("Components")] 
        [SerializeField] private PlayerManager player;
        [SerializeField] private GameUIRootView uiRootView;
        [SerializeField] private PlayerInputManager inputManager;
        [SerializeField] private GridBuildingSystem _gridBuildingSystem;

        public StateMachine FSM { get; private set; }
        public GameplayMode GameplayMode { get; private set; }
        public BuildingMode BuildingMode { get; private set; }
        public MenuMode MenuMode { get; private set; }
        public ProgrammingMode ProgrammingMode { get; private set; }

        private ToggleObject<GameMode> _gameplayOrBuilding;

        // todo: условия не работают для FSM здесь... почему-то. В целом то и не нужно оно здесь. Но вопрос: зачем тогда переходы?
        
        private void Awake()
        {
            FSM = new StateMachine();
            
            GameplayMode = new GameplayMode(inputManager);
            BuildingMode = new BuildingMode(inputManager);
            MenuMode = new MenuMode(inputManager);
            ProgrammingMode = new ProgrammingMode(inputManager);
            
            _gameplayOrBuilding = new ToggleObject<GameMode>(GameplayMode, BuildingMode);
            
            At(GameplayMode, BuildingMode, new FuncPredicate(() => _gameplayOrBuilding.GetState() == BuildingMode));
            At(BuildingMode, GameplayMode, new FuncPredicate(() => _gameplayOrBuilding.GetState() == GameplayMode));
            At(GameplayMode, ProgrammingMode, new FuncPredicate(() => _gameplayOrBuilding.GetState() == ProgrammingMode));
            At(ProgrammingMode, GameplayMode, new FuncPredicate(() => _gameplayOrBuilding.GetState() == ProgrammingMode));

            player.Init();
            GameplayMode.OnUpdate += player.ManualUpdate;
            BuildingMode.OnUpdate += player.ManualUpdate;
            
            uiRootView.Init(this, player);
            
            _gridBuildingSystem.Init();
        }

        private void Start()
        {
            FSM.SetState(GameplayMode);
            
            inputManager.OnToggleBuildMode += OnToggleBuildMode_Callback;
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
        
        // ДОБАВЬ ЭТИ МЕТОДЫ:
        public void EnterBuildingMode()
        {
            if (FSM.CurrentState != BuildingMode)
            {
                // Синхронизируем ToggleObject
                if (_gameplayOrBuilding.GetState() != BuildingMode)
                {
                    _gameplayOrBuilding.Toggle();
                }
                FSM.SetState(BuildingMode);
            }
        }

        public void EnterGameplayMode()
        {
            if (FSM.CurrentState != GameplayMode)
            {
                // Синхронизируем ToggleObject
                if (_gameplayOrBuilding.GetState() != GameplayMode)
                {
                    _gameplayOrBuilding.Toggle();
                }
                FSM.SetState(GameplayMode);
            }
        }

        private void At(IState from, IState to, IPredicate condition)
        {
            FSM.AddTransition(from, to, condition);
        }

        private void Any(IState to, IPredicate condition)
        {
            FSM.AddAnyTransition(to, condition);
        }
    }
}