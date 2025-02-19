using System.Collections.Generic;
using UnityEngine;

namespace Grid
{
    public class Pathfinding {
        private const int MOVE_STRAIGHT_COST = 10;
        private const int MOVE_DIAGONAL_COST = 14;

        private GridBase<GridNode> _grid;
        private List<GridNode> openList;    // ноды, которые считаем
        private List<GridNode> closedList;  // ноды, которые не считаем (либо прошли, либо isWalkable = false)

        public Pathfinding(GridBase<GridNode> grid) {
            this._grid = grid;
        }

        // получается, что путь состоит из точек Vector3. и юнит будет передвигаться от точки к точке
        public List<Vector3> FindPath(Vector3 startWorldPosition, Vector3 endWorldPosition) {
            var startGridPosition = _grid.GetGridPosition(startWorldPosition);
            var endGridPosition = _grid.GetGridPosition(endWorldPosition);

            List<GridNode> path = FindPath(startGridPosition, endGridPosition);
            if (path == null) {
                return null;
            } else {
                List<Vector3> vectorPath = new List<Vector3>();
                foreach (GridNode node in path) {
                    vectorPath.Add(new Vector3(node.cell.x, 0, node.cell.y) * _grid.cellSize);
                }
                return vectorPath;
            }
        }

        public List<GridNode> FindPath(Vector2Int startCell, Vector2Int endCell) {
            GridNode startNode = _grid.GetGridObject(startCell);
            GridNode endNode = _grid.GetGridObject(endCell);

            if (startNode == null || endNode == null) {
                return null;
            }
        
            openList = new List<GridNode>{ startNode };
            closedList = new List<GridNode>();

            // инициализация всех нод
            for (int x = 0; x < _grid.width; x++) {
                for (int y = 0; y < _grid.height; y++) {
                    GridNode node = _grid.GetGridObject(new Vector2Int(x, y));
                    node.gCost = int.MaxValue;
                    node.CalculateFCost();
                    node.cameFromNode = null;
                }
            }

            startNode.gCost = 0;
            startNode.hCost = CalculateDistanceCost(startNode, endNode);
            startNode.CalculateFCost();

            while (openList.Count > 0) {
                GridNode currentNode = GetLowestFCostNode(openList);

                // пришли в конец
                if (currentNode == endNode) {
                    return CalculatePath(endNode);
                }
                // теперь точку не учитываем в расчетах
                openList.Remove(currentNode);
                closedList.Add(currentNode);

                // перебираем соседей
                foreach (GridNode neighborNode in GetNeighborList(currentNode)) {
                    if (closedList.Contains(neighborNode)) continue;
                    if (!neighborNode.isWalkable) {
                        closedList.Add(neighborNode);
                        continue;
                    }
                    // то есть будем считать только те ноды, у которых g (расстояние от начала до нас) больше чем наше => они дальше от старта и ближе к концу
                    int checkGCost = currentNode.gCost + CalculateDistanceCost(currentNode, neighborNode);
                    if (checkGCost < neighborNode.gCost) {
                        neighborNode.cameFromNode = currentNode;
                        neighborNode.gCost = checkGCost;
                        neighborNode.hCost = CalculateDistanceCost(neighborNode, endNode);
                        neighborNode.CalculateFCost();

                        if (!openList.Contains(neighborNode)) {
                            openList.Add(neighborNode);
                        }
                    }
                }
            }

            // out of nodes in the open list => no way
            return null;
        }

        // чекаем клетки вокруг
        private List<GridNode> GetNeighborList(GridNode currentNode) {
            List<GridNode> neighborList = new List<GridNode>();
            var (x, y) = currentNode.cell;

            if (x - 1 >= 0) {
                // left
                neighborList.Add(_grid.GetGridObject(new Vector2Int(x - 1, y)));
                // left down
                if (y - 1 >= 0) neighborList.Add(_grid.GetGridObject(new Vector2Int(x - 1, y - 1)));
                // left up
                if (y + 1 < _grid.height) neighborList.Add(_grid.GetGridObject(new Vector2Int(x - 1, y + 1)));
            }
            if (x + 1 < _grid.width) {
                // right
                neighborList.Add(_grid.GetGridObject(new Vector2Int(x + 1, y)));
                // right down
                if (y - 1 >= 0) neighborList.Add(_grid.GetGridObject(new Vector2Int(x + 1, y - 1)));
                // right up
                if (y + 1 < _grid.height) neighborList.Add(_grid.GetGridObject(new Vector2Int(x + 1, y + 1)));
            }
            // down
            if (y - 1 >= 0) neighborList.Add(_grid.GetGridObject(new Vector2Int(x, y - 1)));
            // up
            if (y + 1 < _grid.height) neighborList.Add(_grid.GetGridObject(new Vector2Int(x, y + 1)));

            return neighborList;
        }

        // узнаем путь, используя предшествующую ноду
        private List<GridNode> CalculatePath(GridNode endNode) {
            List<GridNode> path = new List<GridNode> { endNode };
            GridNode currentNode = endNode;

            while (currentNode.cameFromNode != null) {
                path.Add(currentNode.cameFromNode);
                currentNode = currentNode.cameFromNode;
            }
            path.Reverse();
            return path;
        }

        // на словах тут сложно объяснить. если забудешь как работает - начерти и станет сразу понятно. по сути это просто формула 
        private int CalculateDistanceCost(GridNode a, GridNode b) {
            int xDistance = Mathf.Abs(a.cell.x - b.cell.x);  // расстояние по x
            int yDistance = Mathf.Abs(a.cell.y - b.cell.y);  // расстояние по y
            int remaining = Mathf.Abs(xDistance - yDistance);
            // суть в чем: сначала мы рассчитываем, сколько возможно пройти по диагонали (берем минимум, так как если больше, то пройдем лишнее)
            // а затем сколько нужно пройти по прямой. и вот как раз разница для этого и нужна
            // умножаем, так как дистанция = количество клеток
            return MOVE_DIAGONAL_COST * Mathf.Min(xDistance, yDistance) + MOVE_STRAIGHT_COST * remaining;
        }

        // простой поиск минимума
        private GridNode GetLowestFCostNode(List<GridNode> pathNodeList) {
            GridNode lowestFCostNode = pathNodeList[0];
            for (int i = 1; i < pathNodeList.Count; i++) {
                if (pathNodeList[i].fCost < lowestFCostNode.fCost) {
                    lowestFCostNode = pathNodeList[i];
                }
            }
            return lowestFCostNode;
        }
    }
}
