using Grid.BuildingSystem;
using UnityEngine;

//! наверное все же нужно разделить ноды: сделать общую ноду и от нее наследовать
namespace Grid
{
    public class GridNode {
        public Vector2Int cell;
        public int gCost;  // расстояние от стартовой ноды
        public int hCost;  // расстояние до конца
        public int fCost;  // g + h
        public bool isWalkable;
        public Building building;

        private GridBase<GridNode> _grid;

        public GridNode(GridBase<GridNode> grid, Vector2Int cell) {
            _grid = grid;
            building = null;
            this.cell = cell;
            isWalkable = true;
        }

        public GridNode cameFromNode;

        public void CalculateFCost() {
            fCost = gCost + hCost;
        }

        public bool CanBuild() {
            return building == null;
        }

        public void SetBuilding(Building building) {
            this.building = building;
            isWalkable = false;
            _grid.TriggerGridObjectChanged(cell);
        }

        public void ClearBuilding() {
            building = null;
            _grid.TriggerGridObjectChanged(cell);
        }

        public override string ToString() {
            return $"({cell.x}, {cell.y} | {building})";
        }
    }
}
