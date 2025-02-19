using System.Collections.Generic;
using UnityEngine;

namespace Grid.BuildingSystem.BuildingPreview
{
    // по итогу как работает смена визуала - OverlapBox! просто создаеться зона вокгру плоскости, и она
    // чекает кто в нее входит
    public class BuildingPreviewPlane : MonoBehaviour
    {
        private LayerMask _entityMask;
        private LayerMask _environmentMask;
        private LayerMask _interactableMask;
        private LayerMask _buildingMask;
        private List<LayerMask> _masks;
        private void Awake()
        {
            _entityMask = LayerMask.GetMask(Const.ENTITY_LAYER);
            _environmentMask = LayerMask.GetMask(Const.ENVIRONMENT_LAYER);
            _interactableMask = LayerMask.GetMask(Const.INTERACTABLE_LAYER);
            _buildingMask = LayerMask.GetMask(Const.BUILDING_LAYER);
            _masks = new List<LayerMask> {
                _entityMask,
                _environmentMask,
                _interactableMask,
                _buildingMask,
            };
        }

        public bool CheckCollision(Vector3 center, Vector3 size)
        {
            if (size == Vector3.zero) return false;

            var hits = Physics.OverlapBox(
                center,
                size, 
                Quaternion.identity
            );

            foreach (var hit in hits)
            {
                foreach (var mask in _masks)
                {
                    if ((mask & (1 << hit.gameObject.layer)) != 0)
                    {
                        return false;
                    };
                }
            }
            return true;
        }
    }
}