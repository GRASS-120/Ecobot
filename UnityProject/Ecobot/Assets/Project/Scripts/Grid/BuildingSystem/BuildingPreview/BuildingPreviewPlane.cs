using System.Collections.Generic;
using Player;
using UnityEngine;

namespace Grid.BuildingSystem.BuildingPreview
{
    // по итогу как работает смена визуала - OverlapBox! просто создаеться зона вокгру плоскости, и она
    // чекает кто в нее входит
    public class BuildingPreviewPlane : MonoBehaviour
    {
        private int _combinedMask;
        
        private void Awake()
        {
            var entityMask = LayerMask.GetMask(Const.ENTITY_LAYER);
            var environmentMask = LayerMask.GetMask(Const.ENVIRONMENT_LAYER);
            var interactableMask = LayerMask.GetMask(Const.INTERACTABLE_LAYER);
            var buildingMask = LayerMask.GetMask(Const.BUILDING_LAYER);

            // Объединяем маски в один битсет
            _combinedMask = entityMask | environmentMask | interactableMask | buildingMask;
        }

        public bool CheckCollision(Vector3 center, Vector3 size)
        {
            if (size == Vector3.zero) return false;

            // Собираем только нужные слои и игнорируем триггеры
            var hits = Physics.OverlapBox(
                center,
                size,
                Quaternion.identity,
                _combinedMask,
                QueryTriggerInteraction.Ignore
            );

            foreach (var hit in hits)
            {
                if (hit == null) continue;

                // Игнорируем собственное превью (его коллайдеры)
                if (hit.transform != null && transform != null && hit.transform.root == transform.root)
                    continue;

                // Игнорируем игрока, чтобы он не блокировал строительство
                if (hit.GetComponentInParent<PlayerManager>() != null)
                    continue;

                // Любой другой подходящий по маске коллайдер — блокирует
                return false;
            }
            return true;
        }
    }
}