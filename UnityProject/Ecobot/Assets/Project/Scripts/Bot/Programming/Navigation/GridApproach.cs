using System.Collections.Generic;
using Grid.Base;
using Grid.BuildingSystem;
using UnityEngine;

namespace Bot.Programming.Navigation
{
    /// <summary>
    /// Универсальная грид-утилита для поиска ближайшей свободной клетки вокруг «футпринта».
    /// </summary>
    public static class GridApproach
    {
        public static GridBase<GridNode> GetGrid()
        {
            var gbs = Object.FindObjectOfType<GridBuildingSystem>();
            var map = gbs ? gbs.GetComponentInParent<GridMap>() : null;
            return map ? map.Grid : null;
        }

        /// <summary>
        /// Ищет ближайшую свободную клетку вокруг множества занятых клеток.
        /// Сначала 4-соседние, если занято — расширение кольцами (Манхэттен).
        /// </summary>
        public static bool TryFindApproach(Vector3 botWorldPos, IEnumerable<Vector2Int> occupiedCells, out Vector3 worldPos)
        {
            worldPos = default;
            var grid = GetGrid(); if (grid == null) return false;

            var occ = new HashSet<Vector2Int>(occupiedCells);
            if (occ.Count == 0) return false;

            var dirs = new[] { new Vector2Int(1, 0), new(-1, 0), new(0, 1), new(0, -1) };

            bool Free(Vector2Int c)
            {
                if (c.x < 0 || c.y < 0 || c.x >= grid.Width || c.y >= grid.Height) return false;
                var n = grid.GetGridObject(c);
                return n != null
                    && n.IsWalkable
                    && n.BuildingBase == null
                    && !n.HasDynamicOccupant; // ← NEW: учитываем руду/другие оккупанты
            }

            Vector3 botXZ = new(botWorldPos.x, 0f, botWorldPos.z);

            // 1) соседи футпринта
            var nbr = new HashSet<Vector2Int>();
            foreach (var o in occ) foreach (var d in dirs) nbr.Add(o + d);

            bool found = false; Vector2Int best = default; float bestSqr = float.PositiveInfinity;
            foreach (var c in nbr)
            {
                if (!Free(c)) continue;
                var w = grid.GetWorldPosition(c); w.y = 0f;
                float sq = (botXZ - w).sqrMagnitude;
                if (sq < bestSqr) { bestSqr = sq; best = c; found = true; }
            }

            // 2) кольца, если всё занято
            if (!found)
            {
                var perimeter = new HashSet<Vector2Int>();
                foreach (var o in occ) foreach (var d in dirs) if (!occ.Contains(o + d)) perimeter.Add(o + d);

                int maxR = Mathf.CeilToInt(Mathf.Max(grid.Width, grid.Height));
                float localBest = float.PositiveInfinity; Vector2Int bestCell = default;

                for (int r = 1; r <= maxR; r++)
                {
                    bool any = false;
                    foreach (var p in perimeter)
                    {
                        for (int dx = -r; dx <= r; dx++)
                        {
                            int dy = r - Mathf.Abs(dx);
                            var c1 = new Vector2Int(p.x + dx, p.y + dy);
                            var c2 = new Vector2Int(p.x + dx, p.y - dy);

                            if (Free(c1))
                            {
                                var w1 = grid.GetWorldPosition(c1); w1.y = 0f;
                                float s1 = (botXZ - w1).sqrMagnitude;
                                if (s1 < localBest) { localBest = s1; bestCell = c1; any = true; }
                            }
                            if (dy != 0 && Free(c2))
                            {
                                var w2 = grid.GetWorldPosition(c2); w2.y = 0f;
                                float s2 = (botXZ - w2).sqrMagnitude;
                                if (s2 < localBest) { localBest = s2; bestCell = c2; any = true; }
                            }
                        }
                    }

                    if (any) { best = bestCell; found = true; break; }
                }
            }

            if (!found) return false;
            worldPos = grid.GetWorldPosition(best); worldPos.y = 0f;
            return true;
        }
    }
}
