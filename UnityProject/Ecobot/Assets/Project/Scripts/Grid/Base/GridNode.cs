using Grid.BuildingSystem;
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
        public Building Building;
        public GridNode CameFromNode;
        
        private readonly GridBase<GridNode> _grid;

        public GridNode(GridBase<GridNode> grid, Vector2Int cell) {
            _grid = grid;
            Building = null;
            Cell = cell;
            IsWalkable = true;
        }

        public void CalculateFCost() {
            FCost = GCost + HCost;
        }

        public bool CanBuild() {
            return Building == null;
        }

        public void SetBuilding(Building building) {
            Building = building;
            IsWalkable = false;
            
            _grid.TriggerGridObjectChanged(Cell);
        }

        public void ClearBuilding() {
            Building = null;
            
            _grid.TriggerGridObjectChanged(Cell);
        }

        public override string ToString() {
            return $"({Cell.x}, {Cell.y} | {Building})";
        }
    }
}
