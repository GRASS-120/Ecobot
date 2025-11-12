using System.Collections.Generic;
using Grid.BuildingSystem;
using Grid.BuildingSystem.Buildings;
using Grid.BuildingSystem.Buildings.Base;
using UnityEngine;

namespace Grid.Base
{
    public class GridNode
    {
        public Vector2Int Cell;
        public int GCost;  // расстояние от стартовой ноды
        public int HCost;  // расстояние до конца
        public int FCost;  // g + h
        public bool IsWalkable;
        public BuildingBase BuildingBase;
        public GridNode CameFromNode;

        private readonly GridBase<GridNode> _grid;

        // --- Динамические «заниматели» клетки (руда и т.п.) ---
        //  Они не являются BuildingBase, но тоже должны блокировать строительство и ходьбу.
        private readonly HashSet<object> _dynamicOccupants = new();
        public bool HasDynamicOccupant => _dynamicOccupants.Count > 0;
        // -------------------------------------------------------

        public GridNode(GridBase<GridNode> grid, Vector2Int cell)
        {
            _grid = grid;
            BuildingBase = null;
            Cell = cell;
            IsWalkable = true;
        }

        public void CalculateFCost()
        {
            FCost = GCost + HCost;
        }

        /// <summary>
        /// Можно ли строить в этой клетке?
        /// </summary>
        public bool CanBuild()
        {
            // Запрещаем, если занято зданием или есть динамический оккупант (руда и т.п.)
            return BuildingBase == null && !HasDynamicOccupant;
        }

        public void SetBuilding(BuildingBase building)
        {
            BuildingBase = building;

            // Здание делает клетку непроходимой (важно для превью и pathfinding)
            if (IsWalkable)
                IsWalkable = false;

            _grid.TriggerGridObjectChanged(Cell);
        }

        public void ClearBuilding()
        {
            BuildingBase = null;

            // Если больше нет ни здания, ни динамических оккупантов — снова проходимо
            if (!HasDynamicOccupant && !IsWalkable)
                IsWalkable = true;

            _grid.TriggerGridObjectChanged(Cell);
        }

        // ======================= ВАЖНО: синхронизация IsWalkable с динамическими оккупантами =======================
        // Раньше при добавлении/удалении оккупанта мы НЕ трогали IsWalkable — из-за этого превью «не краснело» над рудой.
        // Теперь:
        //  - когда добавляется ПЕРВЫЙ оккупант → IsWalkable = false (как при здании),
        //  - когда удаляется ПОСЛЕДНИЙ оккупант и нет здания → IsWalkable = true.
        // ==========================================================================================================

        public void AddDynamicOccupant(object who)
        {
            if (who == null) return;

            // было ноль → станет один: переводим в непроходимую
            bool wasEmpty = _dynamicOccupants.Count == 0;
            if (_dynamicOccupants.Add(who))
            {
                if (wasEmpty)
                {
                    if (IsWalkable)
                        IsWalkable = false;
                }
                _grid.TriggerGridObjectChanged(Cell);
            }
        }

        public void RemoveDynamicOccupant(object who)
        {
            if (who == null) return;

            if (_dynamicOccupants.Remove(who))
            {
                // стало ноль и нет здания → проходимо обратно
                if (_dynamicOccupants.Count == 0 && BuildingBase == null)
                {
                    if (!IsWalkable)
                        IsWalkable = true;
                }
                _grid.TriggerGridObjectChanged(Cell);
            }
        }
        // ==========================================================================================================

        public override string ToString()
        {
            return $"({Cell.x}, {Cell.y} | Bld:{(BuildingBase ? BuildingBase.name : "null")} | Dyn:{_dynamicOccupants.Count})";
        }
    }
}
