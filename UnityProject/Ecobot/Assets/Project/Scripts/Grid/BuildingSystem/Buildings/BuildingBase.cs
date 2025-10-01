using GUI.UIFramework;
using InteractionSystem;
using UnityEngine;

namespace Grid.BuildingSystem.Buildings
{
    public class BuildingBase : MonoBehaviour 
    {
        protected BuildingAssetData _buildingAssetData;
        protected Vector2Int _origin;
        protected WindowManager _windowManager;
        protected BuildingAssetData.Dir _dir;

        public Vector2Int[,] AllGridPositions => _buildingAssetData.GetAllGridPositions(_origin, _dir);
        public BuildingAssetData BuildingAssetData => _buildingAssetData;
        public Vector2Int Origin => _origin;
        public BuildingAssetData.Dir Dir => _dir;

        public virtual void Init(
            BuildingAssetData data,
            Vector2Int origin,
            WindowManager windowManager,
            BuildingAssetData.Dir dir = BuildingAssetData.Dir.Down)
        {
            _buildingAssetData = data;
            _origin = origin;
            _dir = dir;
        }

        public void DestroySelf() {
            Destroy(gameObject);
        }
    }
}
