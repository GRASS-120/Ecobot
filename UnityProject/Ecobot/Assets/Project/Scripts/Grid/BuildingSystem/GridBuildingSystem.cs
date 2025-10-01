using System;
using System.Collections.Generic;
using Game;
using Game.Mods;
using Grid.Base;
using Grid.BuildingSystem.Buildings;
using Player;
using R3;
using Sirenix.OdinInspector;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.InputSystem;
using PlayerInputManager = Player.InputManager.PlayerInputManager;

namespace Grid.BuildingSystem
{
    public class GridBuildingSystem : SerializedMonoBehaviour
    {
        public event Action<BuildingBase> OnBuildingPlaced;
        public event Action OnBuildingPositionChanged;

        [Title("Components")]
        [SerializeField] private PlayerInputManager inputManager;
        [SerializeField] private PlayerManager player;
        [SerializeField] private GameManager gameManager;
        [SerializeField] private BuildingPreview.BuildingPreview buildingPreview;
        [FormerlySerializedAs("buildingListSO")] [SerializeField] private BuildingListConfig buildingListConfig;
        
        [Title("Visual")]
        [SerializeField] private GameObject pointer;
        [SerializeField] private GameObject gridVisualTiles;
        
        [Title("Data")][ReadOnly]
        [SerializeField] private Dictionary<BuildingAssetData, List<BuildingBase>> buildings;

        private ReactiveProperty<BuildingAssetData> _currentBuildingItem;
        private ReactiveProperty<bool> _canBuildByGrid;
        private BuildingDatabase _buildingDatabase;
        private GridBase<GridNode> _grid;
        private BuildingAssetData.Dir _dir;
        private BuildingBase _currentBuildingBase;
        private Vector3 _mousePosition;
        private LayerMask _groundMask;
        private List<BuildingAssetData> _buildingTypeList;

        public GridBase<GridNode> Grid => _grid;
        public BuildingAssetData.Dir Dir => _dir;
        public GameManager GameManager => gameManager;
        public PlayerManager Player => player;
        
        public ReadOnlyReactiveProperty<BuildingAssetData> CurrentBuildingItem => _currentBuildingItem;

        private void Awake()
        {
            _grid = GetComponentInParent<GridMap>().Grid;
            _canBuildByGrid = new ReactiveProperty<bool>(true);
            _currentBuildingItem = new ReactiveProperty<BuildingAssetData>();
            
            _buildingTypeList = buildingListConfig.buildings;
            _buildingDatabase = new BuildingDatabase(buildingListConfig);
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

        private void OnBuildingPlaced_Callback(BuildingBase obj)
        {
            _buildingDatabase.Append(obj);
        }
        
        private void ResetDir()
        {
            _dir = BuildingAssetData.Dir.Down;
        }

        private void CurrentBuildingItem_Callback(BuildingAssetData assetData)
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
            BuildingBase building = gridNode.BuildingBase;

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
            _dir = BuildingAssetData.GetNextDir(_dir);
        }

        private BuildingBase CreateBuilding(
            Vector3 worldPosition,
            Vector2Int origin,
            BuildingAssetData.Dir dir,
            BuildingAssetData buildingAssetData) 
        {        
            var buildingTransform = Instantiate(
                buildingAssetData.prefab,
                worldPosition,
                Quaternion.Euler(0, buildingAssetData.GetRotationAngle(dir), 0)
            );

            var building = buildingTransform.AddComponent<BuildingBase>();
            building.Init(buildingAssetData, origin, player.WindowManager, dir);

            return building;
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
                    BuildingBase building = CreateBuilding(buildingWorldPosition, mouseGridPosition, _dir, _currentBuildingItem.Value);

                    foreach (Vector2Int gridPosition in gridPositionMatrix) {
                        _grid.GetGridObject(gridPosition).SetBuilding(building);
                    }
                    
                    ResetDir();
                    OnBuildingPlaced?.Invoke(building);
                } 
                
                _canBuildByGrid.Value = true;
            }

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
