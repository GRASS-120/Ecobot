using System;
using System.Collections.Generic;
using Game;
using Grid.Base;
using Grid.BuildingSystem.Buildings;
using Grid.BuildingSystem.Buildings.Base;
using Grid.BuildingSystem.PowerSystem;
using Grid.BuildingSystem.PowerSystem.WireSystem;
using GUI.Gameplay.Windows.Controller;
using Inventory;
using Player;
using R3;
using Sirenix.OdinInspector;
using Unity.VisualScripting;
using UnityEngine;
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
        [SerializeField] private BuildingListConfig buildingListConfig;
        [SerializeField] private PowerGridService powerGridService;
        [SerializeField] private PowerWireToolService powerWireToolService;

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
        private InventorySelectionService _inventorySelectionService;
        
        public GridBase<GridNode> Grid => _grid;
        public BuildingAssetData.Dir Dir => _dir;
        public GameManager GameManager => gameManager;
        public PlayerManager Player => player;
        public BuildingDatabase BuildingDatabase => _buildingDatabase;
        
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

        public void Init()
        {
            inputManager.OnRotateBuilding += OnRotateBuilding_Callback;
            inputManager.OnDemountBuilding += OnDemountBuilding_Callback;

            OnBuildingPlaced += OnBuildingPlaced_Callback;           
            _currentBuildingItem.Subscribe(CurrentBuildingItem_Callback).AddTo(this);

            gameManager.BuildingMode.OnEnterEvent += OnEnterBuildingMode_Callback;
            gameManager.BuildingMode.OnExitEvent += OnExitBuildingMode_Callback;
            
            _inventorySelectionService = player.Inventory.InventorySelectionService;
            
            Observable.NextFrame()
                .Subscribe(_ =>
                    _inventorySelectionService.Active.Subscribe(OnSelectionChanged).AddTo(this))
                .AddTo(this);
            
            buildingPreview.Init();
        }
        
        private void OnSelectionChanged(InventorySelectionService.InventorySelection sel)
        {
            // Если ничего не выбрано ИЛИ выбран НЕ билдинг — выходим из режима строительства
            if (!sel.IsValid || sel.ItemData is not BuildingAssetData buildingData)
            {
                ClearBuildingItem();
                gameManager.EnterGameplayMode();
                return;
            }
    
            _currentBuildingItem.Value = buildingData;
            gameManager.EnterBuildingMode();
        }
        
        private void OnEnterBuildingMode_Callback()
        {
            pointer.SetActive(true);
            gridVisualTiles.SetActive(true);
    
            // Принудительно обновляем визуал если предмет уже выбран
            if (_currentBuildingItem.Value != null && buildingPreview != null)
            {
                buildingPreview.ForceRefreshVisual();
            }
        }
        
        private void OnExitBuildingMode_Callback()
        {
            pointer.SetActive(false);
            gridVisualTiles.SetActive(false);
            
            // ClearBuildingItem();  
        }

        private void OnBuildingPlaced_Callback(BuildingBase obj)
        {
            _buildingDatabase.Append(obj);
            if (obj is IPowerNode node)
            {
                powerGridService.Register(node);
            }
        }
        
        private void ResetDir()
        {
            _dir = BuildingAssetData.Dir.Down;
        }

        private void CurrentBuildingItem_Callback(BuildingAssetData assetData)
        {
            ResetDir();
            // pointer.SetActive(false);
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

            if (building is IPowerNode node)
            {
                powerGridService.Unregister(node);
            }

            _buildingDatabase.Remove(building);
            building.DestroySelf();

            Vector2Int[,] gridPositionList = building.AllGridPositions;
            foreach (Vector2Int gridPosition in gridPositionList)
            {
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
            var building = Instantiate(
                buildingAssetData.prefab,
                worldPosition,
                Quaternion.Euler(0, buildingAssetData.GetRotationAngle(dir), 0)
            );
            
            var overlay = player.WindowManager.GetController<GameplayOverlayController>();
            var context = new BuildingContext(
                player.WindowManager,
                player,
                powerGridService,
                powerWireToolService,
                overlay?.MouseUI 
            );
            
            building.Init(buildingAssetData, origin, context, dir);

            return building;
        }
        
        public void HandleBuilding() {
            
            if (_currentBuildingItem.Value == null)
                return;
            
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
                    
                    ConsumeOneFromSelectionOrExit();
                } 
                
                _canBuildByGrid.Value = true;
            }
            
            // ResetDir();
        }
        
        private void ConsumeOneFromSelectionOrExit()
        {
            var sel = _inventorySelectionService.Active.Value;
            if (!sel.IsValid) return;
    
            var slot = sel.Inventory.GetSlot(sel.Index);
            if (slot.ItemData == null)
            {
                _inventorySelectionService.Clear();
                ClearBuildingItem();
                gameManager.EnterGameplayMode(); // ← ИСПОЛЬЗУЙ НОВЫЙ МЕТОД
                return;
            }
    
            // Списываем 1
            slot.RemoveFromStack(1);
    
            if (slot.StackSize <= 0)
            {
                slot.UpdateSlot(null, 0);
                sel.Inventory.NotifySlotChanged(sel.Index);
        
                // Очищаем выбор и выходим из режима
                _inventorySelectionService.Clear();
                ClearBuildingItem();
                gameManager.EnterGameplayMode(); // ← ИСПОЛЬЗУЙ НОВЫЙ МЕТОД
            }
            else
            {
                sel.Inventory.NotifySlotChanged(sel.Index);
                // Предмет ещё есть - остаёмся в режиме строительства
            }
        }
    }
}
