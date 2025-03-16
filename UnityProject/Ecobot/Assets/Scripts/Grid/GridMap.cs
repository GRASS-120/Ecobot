using System;
using System.Collections.Generic;
using Game;
using Game.Mods;
using Grid.BuildingSystem;
using Grid.PathfindingSystem;
using R3;
using UnityEngine;
using UnityEngine.Serialization;

namespace Grid
{
    public class GridMap : MonoBehaviour {
        [Header("Entities")]
        [SerializeField] private GameManager gameManager;
        [Header("Params")]
        public int width = 100;
        public int height = 100;
        public float cellSize = 1f;

        private GridBase<GridNode> _grid;
        private List<GridNode> _gridNodesWithBuilding;
        private GridBuildingSystem _gridBuildingSystem;
        private GameMode _currentMode;
        public GridBase<GridNode> Grid => _grid;

        private void Awake()
        {
            _grid = new GridBase<GridNode>(
                width, height, cellSize, Vector3.zero,
                (GridBase<GridNode> g, Vector2Int cell) => new GridNode(g, cell)
            );
            _gridNodesWithBuilding = new List<GridNode>();
        }
        
        // роблема в том, что не понятно к какой постройке принадлежит клетка...

        private void Start()
        {
            // _currentMode = gameManager.CurrentMode.Value;
            _grid.OnGridObjectChanged += OnGridObjectChanged;
            _gridBuildingSystem = GetComponentInChildren<GridBuildingSystem>();
            
            gameManager.CurrentMode.Subscribe(OnModeChanged_Callback);
        }

        private void Update()
        {
            if (_currentMode == null) return;
            
            if (_currentMode is BuildingMode)
            {
                _gridBuildingSystem.HandleBuilding();
            }
            // todo: !!!
            // switch (_currentMode) {
            //     default:
            //     case GameManager.Mode.Interface:
            //     case GameManager.Mode.Programming:
            //     case GameManager.Mode.Menu:
            //     case GameManager.Mode.Inventory:
            //     case GameManager.Mode.Default: {
            //         // pathfindingSystem.gameObject.SetActive(true);
            //         break;
            //     }
            //     case GameManager.Mode.Building: {
            //         // pathfindingSystem.gameObject.SetActive(false);
            //         _gridBuildingSystem.HandleBuilding();
            //         break;
            //     }
            // }
        }
        
        private void OnGridObjectChanged(Vector2Int obj)   // скорее нужно обрабатывать событие установки постройки
        {
            // Debug.Log(obj);
        }

        private void OnModeChanged_Callback(GameMode currentMode)
        {
            _currentMode = currentMode;
        }
    }
}
