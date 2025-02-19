using System;
using UnityEngine;
using Utils;

namespace Grid.BuildingSystem
{
    [RequireComponent(typeof(BoxCollider))]
    
    public class Building : MonoBehaviour {
        private BuildingItem _buildingItem;
        private Vector2Int _origin;
        private BuildingItem.Dir _dir;

        public Vector2Int[,] AllGridPositions => _buildingItem.GetAllGridPositions(_origin, _dir);
        public BuildingItem.Dir LocalDir => _dir;
        
        private void Awake()
        {
            _dir = BuildingItem.Dir.Down;
            
            // что б сразу устанавливал слой нужный! хз оптимизировано или нет
            if (!Helper.Layers.IsLayersEqual(gameObject.layer, Const.BUILDING_LAYER))
            {
                Helper.Layers.SetLayer(transform, Const.BUILDING_LAYER);
            }
        }
        
        public static Building Create(Vector3 worldPosition, Vector2Int origin, BuildingItem.Dir dir, BuildingItem buildingItem) {        
            var buildingTransform = Instantiate(
                buildingItem.prefab,
                worldPosition,
                Quaternion.Euler(0, buildingItem.GetRotationAngle(dir), 0)
            );

            var building = buildingTransform.GetComponent<Building>();
            building._buildingItem = buildingItem;
            building._origin = origin;
            building._dir = dir;

            return building;
        }

        public void DestroySelf() {
            Destroy(gameObject);
        }
    }
}
