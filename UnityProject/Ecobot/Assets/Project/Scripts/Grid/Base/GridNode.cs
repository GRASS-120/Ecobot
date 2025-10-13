using Grid.BuildingSystem;
using Grid.BuildingSystem.Buildings;
using Grid.BuildingSystem.Buildings.Base;
using UnityEngine;

//! наверное все же нужно разделить ноды: сделать общую ноду и от нее наследовать
namespace Grid.Base
{
    public class GridNode {
        public Vector2Int Cell;
        public int GCost;  // расстояние от стартовой ноды
        public int HCost;  // расстояние до конца
        public int FCost;  // g + h
        public bool IsWalkable;
        public BuildingBase BuildingBase;
        public GridNode CameFromNode;
        
        private readonly GridBase<GridNode> _grid;

        public GridNode(GridBase<GridNode> grid, Vector2Int cell) {
            _grid = grid;
            BuildingBase = null;
            Cell = cell;
            IsWalkable = true;
        }

        public void CalculateFCost() {
            FCost = GCost + HCost;
        }

        public bool CanBuild() {
            return BuildingBase == null;
        }

        public void SetBuilding(BuildingBase building) {
            BuildingBase = building;
            IsWalkable = false;
            
            _grid.TriggerGridObjectChanged(Cell);
        }

        public void ClearBuilding() {
            BuildingBase = null;
            
            _grid.TriggerGridObjectChanged(Cell);
        }

        public override string ToString() {
            return $"({Cell.x}, {Cell.y} | {BuildingBase})";
        }
    }
}
