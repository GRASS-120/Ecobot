using System;
using UnityEngine;
using Utils;

namespace Grid.BuildingSystem
{
    [RequireComponent(typeof(BoxCollider))]
    public class Building : MonoBehaviour {
        private BuildingSO _buildingSO;
        private Vector2Int _origin;
        private BuildingSO.Dir _dir;

        public Vector2Int[,] AllGridPositions => _buildingSO.GetAllGridPositions(_origin, _dir);
        public BuildingSO.Dir LocalDir => _dir;
        public BuildingSO BuildingSO => _buildingSO;
        
        private void Awake()
        {
            _dir = BuildingSO.Dir.Down;
            
            // что б сразу устанавливал слой нужный! хз оптимизировано или нет
            if (!Helper.Layers.IsLayersEqual(gameObject.layer, Const.BUILDING_LAYER))
            {
                Helper.Layers.SetLayer(transform, Const.BUILDING_LAYER);
            }
        }
        
        public static Building Create(Vector3 worldPosition, Vector2Int origin, BuildingSO.Dir dir, BuildingSO buildingSo) {        
            var buildingTransform = Instantiate(
                buildingSo.prefab,
                worldPosition,
                Quaternion.Euler(0, buildingSo.GetRotationAngle(dir), 0)
            );

            var building = buildingTransform.GetComponent<Building>();
            building._buildingSO = buildingSo;
            building._origin = origin;
            building._dir = dir;

            return building;
        }

        public void DestroySelf() {
            Destroy(gameObject);
        }
    }
}
