using System;
using System.Collections.Generic;
using Game;
using Game.Mods;
using Grid.BuildingSystem;
using Grid.PathfindingSystem;
using R3;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace Grid
{
    public class GridMap : MonoBehaviour {
        [Title("Components")]
        [SerializeField] private GameManager gameManager;
        
        [Title("Params")]
        public int width = 100;
        public int height = 100;
        public float cellSize = 1f;
        
        [Title("Grid Systems")]
        [SerializeField] private GridBuildingSystem buildingSystem;
        [SerializeField] private GridPathfindingSystem pathfindingSystem;
        
        public GridBase<GridNode> Grid => _grid;

        private GridBase<GridNode> _grid;
        private List<GridNode> _gridNodesWithBuilding;
        private GameMode _currentMode;

        private void Awake()
        {
            _grid = new GridBase<GridNode>(
                width, height, cellSize, Vector3.zero,
                (GridBase<GridNode> g, Vector2Int cell) => new GridNode(g, cell)
            );
            _gridNodesWithBuilding = new List<GridNode>();
        }
        
        private void Start()
        {
            gameManager.BuildingMode.OnUpdate += buildingSystem.HandleBuilding;
            gameManager.GameplayMode.OnUpdate += pathfindingSystem.HandlePathfinding;
        }
    }
}
