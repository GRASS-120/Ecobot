using System;
using System.Collections.Generic;
using Game;
using Game.Mods;
using Grid.Base;
using Player;
using R3;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.InputSystem;
using PlayerInputManager = Player.InputManager.PlayerInputManager;

namespace Grid.BuildingSystem
{
    public class GridBuildingSystem : SerializedMonoBehaviour
    {
        public event Action<Building> OnBuildingPlaced;
        public event Action OnBuildingPositionChanged;

        [Title("Components")]
        [SerializeField] private PlayerInputManager inputManager;
        [SerializeField] private PlayerManager player;
        [SerializeField] private GameManager gameManager;
        [SerializeField] private BuildingPreview.BuildingPreview buildingPreview;
        [SerializeField] private BuildingListSO buildingListSO;
        
        [Title("Visual")]
        [SerializeField] private GameObject pointer;
        [SerializeField] private GameObject gridVisualTiles;
        
        [Title("Data")][ReadOnly]
        [SerializeField] private Dictionary<BuildingSO, List<Building>> buildings;

        private ReactiveProperty<BuildingSO> _currentBuildingItem;
        private ReactiveProperty<bool> _canBuildByGrid;
        private BuildingDatabase _buildingDatabase;
        private GridBase<GridNode> _grid;
        private BuildingSO.Dir _dir;
        private Building _currentBuilding;
        private Vector3 _mousePosition;
        private LayerMask _groundMask;
        private List<BuildingSO> _buildingTypeList;

        public GridBase<GridNode> Grid => _grid;
        public BuildingSO.Dir Dir => _dir;
        public GameManager GameManager => gameManager;
        public PlayerManager Player => player;
        
        public ReadOnlyReactiveProperty<BuildingSO> CurrentBuildingItem => _currentBuildingItem;

        private void Awake()
        {
            _grid = GetComponentInParent<GridMap>().Grid;
            _canBuildByGrid = new ReactiveProperty<bool>(true);
            _currentBuildingItem = new ReactiveProperty<BuildingSO>();
            
            _buildingTypeList = buildingListSO.buildings;
            _buildingDatabase = new BuildingDatabase(buildingListSO);
            buildings = _buildingDatabase.BuildingsData;
        }

        private void Start()
        {
            inputManager.OnRotateBuilding += OnRotateBuilding_Callback;
            inputManager.OnDemountBuilding += OnDemountBuilding_Callback;

            OnBuildingPlaced += OnBuildingPlaced_Callback;           
            _currentBuildingItem.Subscribe(CurrentBuildingItem_Callback).AddTo(this);

            // gameManager.GameplayMode.OnEnterEvent += () => { Debug.Log("Game play entered"); };
            // gameManager.GameplayMode.OnExitEvent += () => { Debug.Log("Game play exited"); };

            gameManager.BuildingMode.OnEnterEvent += OnEnterBuildingMode_Callback;
            gameManager.BuildingMode.OnExitEvent += OnExitBuildingMode_Callback;
        }
        
        private void OnEnterBuildingMode_Callback()
        {
            pointer.SetActive(true);
            gridVisualTiles.SetActive(true);
            
            ClearBuildingItem();  
        }
        
        private void OnExitBuildingMode_Callback()
        {
            pointer.SetActive(false);
            gridVisualTiles.SetActive(false);
            
            ClearBuildingItem();  
        }

        private void OnBuildingPlaced_Callback(Building obj)
        {
            _buildingDatabase.Append(obj);
        }
        
        private void ResetDir()
        {
            _dir = BuildingSO.Dir.Down;
        }

        private void CurrentBuildingItem_Callback(BuildingSO so)
        {
            ResetDir();
            pointer.SetActive(false);
        }

        private void ClearBuildingItem()
        {
            _currentBuildingItem.Value = null;
        }

        private void OnDemountBuilding_Callback()
        {
            GridNode gridNode = _grid.GetGridObject(_mousePosition);
            Building building = gridNode.Building;

            if (building == null) return;
            
            _buildingDatabase.Remove(building);
            building.DestroySelf();
            
            Vector2Int[,] gridPositionList = building.AllGridPositions;
            foreach (Vector2Int gridPosition in gridPositionList) {
                _grid.GetGridObject(gridPosition).ClearBuilding();
            }
        }

        private void OnRotateBuilding_Callback()
        {
            _dir = BuildingSO.GetNextDir(_dir);
        }
        
        public void HandleBuilding() {
            
            if (_mousePosition != player.GetMouseRaycast().position)
            {
                OnBuildingPositionChanged?.Invoke();
                _mousePosition = player.GetMouseRaycast().position;
            }
            
            Vector2Int mouseGridPosition = _grid.GetGridPosition(_mousePosition);
            Vector3 pointerPosition = _grid.GetWorldPosition(mouseGridPosition);
            pointerPosition.y = 0.1f;
            pointer.transform.position = pointerPosition;

            if (Input.GetMouseButtonDown(1)) {
                Vector2Int[,] gridPositionMatrix = _currentBuildingItem.Value.GetAllGridPositions(mouseGridPosition, _dir);
            
                foreach (Vector2Int gridPosition in gridPositionMatrix) {
                    if (!_grid.GetGridObject(gridPosition).CanBuild()) {
                        _canBuildByGrid.Value = false;
                        break;
                    }
                }
                
                if (_canBuildByGrid.Value && buildingPreview.CanBuildByCollision.Value) {
                    Vector3 buildingWorldPosition = _grid.GetWorldPosition(mouseGridPosition);
                    Building building = Building.Create(buildingWorldPosition, mouseGridPosition, _dir, _currentBuildingItem.Value);

                    foreach (Vector2Int gridPosition in gridPositionMatrix) {
                        _grid.GetGridObject(gridPosition).SetBuilding(building);
                    }
                    
                    ResetDir();
                    OnBuildingPlaced?.Invoke(building);
                } 
                
                _canBuildByGrid.Value = true;
            }

            // ! remake
            bool hasBuildingChanged = false;

            if (Input.GetKeyDown(KeyCode.Alpha1)) {_currentBuildingItem.Value = _buildingTypeList[0];
                hasBuildingChanged = true;}
            if (Input.GetKeyDown(KeyCode.Alpha2)) {_currentBuildingItem.Value = _buildingTypeList[1];
                hasBuildingChanged = true;}
            if (Input.GetKeyDown(KeyCode.Alpha3)) {_currentBuildingItem.Value = _buildingTypeList[2];
                hasBuildingChanged = true;}
            if (Input.GetKeyDown(KeyCode.Alpha4)) {_currentBuildingItem.Value = _buildingTypeList[3];
                hasBuildingChanged = true;}
            if (Input.GetKeyDown(KeyCode.Alpha5)) {_currentBuildingItem.Value = _buildingTypeList[4];
                hasBuildingChanged = true;}
            if (Input.GetKeyDown(KeyCode.Alpha6)) {_currentBuildingItem.Value = _buildingTypeList[5];
                hasBuildingChanged = true;}
            if (Input.GetKeyDown(KeyCode.Alpha7)) {_currentBuildingItem.Value = _buildingTypeList[6];
                hasBuildingChanged = true;}
            if (Input.GetKeyDown(KeyCode.Alpha8)) {_currentBuildingItem.Value = _buildingTypeList[7];
                hasBuildingChanged = true;}

            if (!hasBuildingChanged) return;

            ResetDir();
        }
    }
}
