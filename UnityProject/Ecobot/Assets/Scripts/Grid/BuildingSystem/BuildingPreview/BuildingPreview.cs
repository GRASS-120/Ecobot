using R3;
using UnityEngine;

namespace Grid.BuildingSystem.BuildingPreview
{
    public class BuildingPreview : MonoBehaviour
    {
        [Header("Entities")]
        [SerializeField] private GridBuildingSystem gridBuildingSystem;
        [SerializeField] private GameManager gameManager;
        [SerializeField] private Player player;

        private BuildingPreviewVisual _buildingPreviewVisual;
        private Vector3 _mousePosition;

        private ReactiveProperty<BuildingItem> _buildingItem;
        private ReactiveProperty<bool> _canBuildByCollision;

        public ReactiveProperty<bool> CanBuildByCollision => _canBuildByCollision;
        public ReactiveProperty<BuildingItem> BuildingItem => _buildingItem;

        private void Awake()
        {
            _canBuildByCollision = new ReactiveProperty<bool>(true);
            _buildingItem = new ReactiveProperty<BuildingItem>();
            _buildingPreviewVisual = GetComponent<BuildingPreviewVisual>();
        }

        private void Start()
        {
            // пофиксил тем, что переместил из Awake в Start...
            _mousePosition = player.GetMouseRaycast().position;

            gridBuildingSystem.CurrentBuildingItem.Subscribe(OnBuildingChanged_Callback).AddTo(this);
            gridBuildingSystem.OnBuildingPlaced += OnBuildingPlaced_Callback;
            gameManager.OnModeChanged += OnModeChanged_Callback;
        }
        
        private void LateUpdate()
        {
            UpdatePreviewPosition();
        }

        private void OnModeChanged_Callback(GameManager.Mode currentMode)
        {
            _buildingPreviewVisual.HandleModeChanging(currentMode);
        }

        private void OnBuildingChanged_Callback(BuildingItem item)
        {
            _buildingItem.Value = item;
            
            if (_buildingItem.Value == null) return; 
            
            _buildingPreviewVisual.RefreshVisual(CalcTargetPosition(), CalcVisualPlaneSize());
            _buildingPreviewVisual.HandleVisual(_canBuildByCollision.Value);
        }
        
        private void OnBuildingPlaced_Callback(Building obj)
        {
            _buildingPreviewVisual.RefreshVisual(CalcTargetPosition(), CalcVisualPlaneSize());
            _canBuildByCollision.Value = false;
        }

        private void UpdatePreviewPosition()
        {
            // если брать _mousePosition из building system, то она не будет обновляться вне режима строительства =>
            // при включении постройка будет перемещаться с прошлого положения в текущее резко
            _mousePosition = player.GetMouseRaycast().position;
            Vector3 targetPosition = CalcTargetPosition();
            Quaternion toRotation = _buildingItem.Value == null ? Quaternion.identity : Quaternion.Euler(0, _buildingItem.Value.GetRotationAngle(gridBuildingSystem.Dir), 0);
            
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * 15f);
            transform.rotation = Quaternion.Lerp(transform.rotation, toRotation, Time.deltaTime * 15f);

            if (_buildingItem.Value == null) return;

            
            _buildingItem.Value.GetSizesDependsOnDir(gridBuildingSystem.Dir, out int w, out int h);  // если left/right, то h = w, w = h
            
            var size = new Vector3(
                gridBuildingSystem.Grid.cellSize * w / 2f,
                1, 
                gridBuildingSystem.Grid.cellSize * h / 2f);
            _canBuildByCollision.Value = _buildingPreviewVisual.Plane.GetComponent<BuildingPreviewPlane>().CheckCollision(targetPosition, size);
        }

        private Vector3 CalcTargetPosition()
        {
            Vector2Int mouseGridPosition = gridBuildingSystem.Grid.GetGridPosition(_mousePosition);
            return new Vector3(mouseGridPosition.x, 1f, mouseGridPosition.y);
        }

        private Vector3 CalcVisualPlaneSize()
        {
            return new Vector3(
                gridBuildingSystem.Grid.cellSize * _buildingItem.Value.width / 10f,
                1, 
                gridBuildingSystem.Grid.cellSize * _buildingItem.Value.height / 10f);
        }
    }
}