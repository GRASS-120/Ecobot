using Game;
using Game.Mods;
using Grid.BuildingSystem.Buildings;
using Grid.BuildingSystem.Buildings.Base;
using R3;
using UnityEngine;
using UnityEngine.Serialization;
using Grid.Base; // --- CHANGED: нужен для доступа к GridNode (HasDynamicOccupant/BuildingBase)

namespace Grid.BuildingSystem.BuildingPreview
{
    public class BuildingPreview : MonoBehaviour
    {
        [Header("Entities")]
        [SerializeField] private GridBuildingSystem buildingSystem;

        private BuildingPreviewVisual _buildingPreviewVisual;
        private Vector3 _mousePosition;

        private ReactiveProperty<BuildingAssetData> _buildingItem;
        private ReactiveProperty<bool> _canBuildByCollision;

        public ReactiveProperty<bool> CanBuildByCollision => _canBuildByCollision;
        public ReactiveProperty<BuildingAssetData> BuildingItem => _buildingItem;
        public GridBuildingSystem BuildingSystem => buildingSystem;

        private void Awake()
        {
            _canBuildByCollision = new ReactiveProperty<bool>(true);
            _buildingItem = new ReactiveProperty<BuildingAssetData>();
            _buildingPreviewVisual = GetComponent<BuildingPreviewVisual>();
        }

        public void Init()
        {
            _mousePosition = buildingSystem.Player.GetMouseRaycast().position;
                
            buildingSystem.CurrentBuildingItem.Subscribe(OnBuildingChanged_Callback).AddTo(this);
            buildingSystem.OnBuildingPlaced += OnBuildingPlaced_Callback;
                
            buildingSystem.GameManager.GameplayMode.OnEnterEvent += OnEnterGameplayMode_Callback;
            
            _buildingPreviewVisual.Init();
        }
        
        public void ForceRefreshVisual()
        {
            if (_buildingItem.Value == null) return;
    
            // Обновляем позицию мыши
            _mousePosition = buildingSystem.Player.GetMouseRaycast().position;
    
            // --- CHANGED: пересчитываем И коллизию, и грид, чтобы визуал сразу подсветился корректно
            var targetPos = CalcTargetPosition();
            var planeSize = CalcVisualPlaneSize();

            // размер бокса для оверлапа
            _buildingItem.Value.GetSizesDependsOnDir(buildingSystem.Dir, out int w, out int h);
            var boxHalfExtents = new Vector3(
                buildingSystem.Grid.CellSize * w / 2f,
                1,
                buildingSystem.Grid.CellSize * h / 2f
            );

            bool canByCollision = _buildingPreviewVisual.Plane
                .GetComponent<BuildingPreviewPlane>()
                .CheckCollision(targetPos, boxHalfExtents);

            var cell = buildingSystem.Grid.GetGridPosition(targetPos);
            bool canByGrid = CalcCanBuildByGrid(cell); // ← учёт зданий и руды

            _canBuildByCollision.Value = canByCollision && canByGrid;
            // -------------------------------------------------------------

            // Принудительно обновляем визуал
            _buildingPreviewVisual.RefreshVisual(targetPos, planeSize);
            _buildingPreviewVisual.HandleVisual(_canBuildByCollision.Value);
        }
        
        private void LateUpdate()
        {
            UpdatePreviewPosition();
        }

        private void OnEnterGameplayMode_Callback()
        {
            _buildingPreviewVisual.DestroyPreview();
        }

        private void OnBuildingChanged_Callback(BuildingAssetData assetData)
        {
            _buildingItem.Value = assetData;
            
            if (_buildingItem.Value == null) return; 
            
            _buildingPreviewVisual.RefreshVisual(CalcTargetPosition(), CalcVisualPlaneSize());
            _buildingPreviewVisual.HandleVisual(_canBuildByCollision.Value);
        }
        
        private void OnBuildingPlaced_Callback(BuildingBase obj)
        {
            _buildingPreviewVisual.RefreshVisual(CalcTargetPosition(), CalcVisualPlaneSize());
        }

        private void UpdatePreviewPosition()
        {
            // если брать _mousePosition из building system, то она не будет обновляться вне режима строительства =>
            // при включении постройка будет перемещаться с прошлого положения в текущее резко
            _mousePosition = buildingSystem.Player.GetMouseRaycast().position;
            Vector3 targetPosition = CalcTargetPosition();
            Quaternion toRotation = _buildingItem.Value == null ? Quaternion.identity : Quaternion.Euler(0, _buildingItem.Value.GetRotationAngle(buildingSystem.Dir), 0);
            
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * 15f);
            transform.rotation = Quaternion.Lerp(transform.rotation, toRotation, Time.deltaTime * 15f);

            if (_buildingItem.Value == null) return;

            // размеры оверлап-бокса для коллизии
            _buildingItem.Value.GetSizesDependsOnDir(buildingSystem.Dir, out int w, out int h);  // если left/right, то h = w, w = h
            var halfExtents = new Vector3(
                buildingSystem.Grid.CellSize * w / 2f,
                1, 
                buildingSystem.Grid.CellSize * h / 2f
            );

            bool canByCollision = _buildingPreviewVisual
                .Plane
                .GetComponent<BuildingPreviewPlane>()
                .CheckCollision(targetPosition, halfExtents);

            // --- CHANGED: добавляем учет грида (здания + динамические оккупанты/руда)
            Vector2Int originCell = buildingSystem.Grid.GetGridPosition(targetPosition);
            bool canByGrid = CalcCanBuildByGrid(originCell);
            _canBuildByCollision.Value = canByCollision && canByGrid;
            // --------------------------------------------------------------------
        }

        private Vector3 CalcTargetPosition()
        {
            Vector2Int mouseGridPosition = buildingSystem.Grid.GetGridPosition(_mousePosition);
            return new Vector3(mouseGridPosition.x, 1f, mouseGridPosition.y);
        }

        private Vector3 CalcVisualPlaneSize()
        {
            return new Vector3(
                buildingSystem.Grid.CellSize * _buildingItem.Value.width / 10f,
                1, 
                buildingSystem.Grid.CellSize * _buildingItem.Value.height / 10f);
        }

        // --- CHANGED: единственное новое приватное вычисление, использует текущий грид и твои же типы.
        // Проверяет, что все клетки под сооружением пусты: нет здания и нет динамического оккупанта (руды).
        private bool CalcCanBuildByGrid(Vector2Int originCell)
        {
            if (_buildingItem.Value == null) return true;

            var grid = buildingSystem.Grid;
            if (grid == null) return true;

            Vector2Int[,] cells = _buildingItem.Value.GetAllGridPositions(originCell, buildingSystem.Dir);
            int w = cells.GetLength(0);
            int h = cells.GetLength(1);

            for (int i = 0; i < w; i++)
            {
                for (int j = 0; j < h; j++)
                {
                    var c = cells[i, j];
                    var node = grid.GetGridObject(c);
                    if (node == null) return false;

                    // нельзя строить, если здесь уже есть здание или динамический оккупант (руда и т.п.)
                    if (node.BuildingBase != null || node.HasDynamicOccupant)
                        return false;
                }
            }
            return true;
        }
    }
}
