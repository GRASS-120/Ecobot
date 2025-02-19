using System;
using System.Collections.Generic;
using R3;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.InputSystem;

namespace Grid.BuildingSystem
{
    public class GridBuildingSystem : MonoBehaviour
    {
        public event Action<Building> OnBuildingPlaced;
        public event Action OnBuildingPositionChanged;
        // public 
        // public static event Action OnBuildingChanged;

        [Header("Entities")]
        [SerializeField] private PlayerInputManager inputManager;
        [SerializeField] private Player player;
        [SerializeField] private GameManager gameManager;
        [SerializeField] private BuildingPreview.BuildingPreview buildingPreview;
        [SerializeField] private List<BuildingItem> buildingTypeList;

        [Header("Visual")]
        [SerializeField] private GameObject pointer;
        [SerializeField] private GameObject gridVisualTiles;

        private ReactiveProperty<BuildingItem> _currentBuildingItem;
        private BuildingItem.Dir _dir;
        private Building _currentBuilding;
        private GridBase<GridNode> _grid;
        private Vector3 _mousePosition;
        private LayerMask _groundMask;
        private ReactiveProperty<bool> _canBuildByGrid;

        public GridBase<GridNode> Grid => _grid;
        public BuildingItem.Dir Dir => _dir;
        public ReactiveProperty<bool> CanBuildByGrid => _canBuildByGrid;

        public ReadOnlyReactiveProperty<BuildingItem> CurrentBuildingItem => _currentBuildingItem;

        private void Awake()
        {
            _grid = GetComponentInParent<GridMap>().Grid;
            _canBuildByGrid = new ReactiveProperty<bool>(true);
            _currentBuildingItem = new ReactiveProperty<BuildingItem>();
        }

        private void Start()
        {
            inputManager.OnRotateBuilding += OnRotateBuilding_Callback;
            inputManager.OnDemountBuilding += OnDemountBuilding_Callback;
            
            gameManager.OnModeChanged += OnModeChanged_Callback;

            _currentBuildingItem.Subscribe(CurrentBuildingItem_Callback).AddTo(this);
        }

        private void OnModeChanged_Callback(GameManager.Mode currentMode)
        {
            // handle visual
            bool isBuildingMode = currentMode == GameManager.Mode.Building;
            pointer.SetActive(isBuildingMode);
            gridVisualTiles.SetActive(isBuildingMode);
            
            ClearBuildingItem();  
        }
        private void ResetDir()
        {
            _dir = BuildingItem.Dir.Down;
        }

        private void CurrentBuildingItem_Callback(BuildingItem item)
        {
            ResetDir();
            pointer.SetActive(false);
        }

        private void ClearBuildingItem()
        {
            _currentBuildingItem.Value = null;
        }

        private void OnDemountBuilding_Callback(object sender, EventArgs e)
        {
            GridNode gridNode = _grid.GetGridObject(_mousePosition);
            Building building = gridNode.building;

            if (building == null) return;
            
            building.DestroySelf();
            
            Vector2Int[,] gridPositionList = building.AllGridPositions;
            foreach (Vector2Int gridPosition in gridPositionList) {
                _grid.GetGridObject(gridPosition).ClearBuilding();
            }
        }

        private void OnRotateBuilding_Callback(object sender, EventArgs e)
        {
            _dir = BuildingItem.GetNextDir(_dir);
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
            
                // _canBuildByGrid = true;
                foreach (Vector2Int gridPosition in gridPositionMatrix) {
                    if (!_grid.GetGridObject(gridPosition).CanBuild()) {
                        _canBuildByGrid.Value = false;
                        break;
                    }
                }
                
                // Debug.Log(_canBuildByGrid + " " + buildingPreview.CanBuildByCollision);
            
                if (_canBuildByGrid.Value && buildingPreview.CanBuildByCollision.Value) {
                    Vector3 buildingWorldPosition = _grid.GetWorldPosition(mouseGridPosition);
                    Building building = Building.Create(buildingWorldPosition, mouseGridPosition, _dir, _currentBuildingItem.Value);

                    foreach (Vector2Int gridPosition in gridPositionMatrix) {
                        _grid.GetGridObject(gridPosition).SetBuilding(building);
                        // Instantiate(pointer, _grid.GetWorldPosition(gridPosition), Quaternion.identity);
                    }
                    // Instantiate(center, _grid.GetWorldPosition(mouseGridPosition), Quaternion.identity);
                    
                    ResetDir();
                    OnBuildingPlaced?.Invoke(building);
                } 
                
                _canBuildByGrid.Value = true;
            }

            // ! remake
            bool hasBuildingChanged = false;

            if (Input.GetKeyDown(KeyCode.Alpha1)) {_currentBuildingItem.Value = buildingTypeList[0];
                hasBuildingChanged = true;}
            if (Input.GetKeyDown(KeyCode.Alpha2)) {_currentBuildingItem.Value = buildingTypeList[1];
                hasBuildingChanged = true;}
            if (Input.GetKeyDown(KeyCode.Alpha3)) {_currentBuildingItem.Value = buildingTypeList[2];
                hasBuildingChanged = true;}
            if (Input.GetKeyDown(KeyCode.Alpha4)) {_currentBuildingItem.Value = buildingTypeList[3];
                hasBuildingChanged = true;}
            if (Input.GetKeyDown(KeyCode.Alpha5)) {_currentBuildingItem.Value = buildingTypeList[4];
                hasBuildingChanged = true;}
            if (Input.GetKeyDown(KeyCode.Alpha6)) {_currentBuildingItem.Value = buildingTypeList[5];
                hasBuildingChanged = true;}
            if (Input.GetKeyDown(KeyCode.Alpha7)) {_currentBuildingItem.Value = buildingTypeList[6];
                hasBuildingChanged = true;}
            if (Input.GetKeyDown(KeyCode.Alpha8)) {_currentBuildingItem.Value = buildingTypeList[7];
                hasBuildingChanged = true;}

            if (!hasBuildingChanged) return;

            ResetDir();
        }
    }
}
