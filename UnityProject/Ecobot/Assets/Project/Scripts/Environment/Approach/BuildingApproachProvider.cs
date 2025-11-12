// Assets/Project/Scripts/Environment/Approach/BuildingApproachProvider.cs
using System.Collections.Generic;
using Bot.Programming.Navigation;
using Grid.BuildingSystem.Buildings.Base;
using UnityEngine;

namespace Environment.Approach
{
    /// <summary>
    /// Провайдер точки подъезда для любых построек на BuildingBase.
    /// Берёт их клеточный футпринт и ищет ближайшую свободную клетку по гриду.
    /// </summary>
    [RequireComponent(typeof(BuildingBase))]
    [DisallowMultipleComponent]
    public class BuildingApproachProvider : MonoBehaviour, IApproachPointProvider
    {
        private BuildingBase _building;

        private void Awake() => _building = GetComponent<BuildingBase>();

        public bool TryGetApproachPoint(Vector3 botWorldPosition, out Vector3 worldPos)
        {
            worldPos = default;
            if (_building == null) return false;

            var fp = _building.AllGridPositions; // Vector2Int[,]
            if (fp == null || fp.Length == 0) return false;

            var list = new List<Vector2Int>();
            int w = fp.GetLength(0), h = fp.GetLength(1);
            for (int i = 0; i < w; i++)
            for (int j = 0; j < h; j++)
                list.Add(fp[i, j]);

            return GridApproach.TryFindApproach(botWorldPosition, list, out worldPos);
        }
    }
}