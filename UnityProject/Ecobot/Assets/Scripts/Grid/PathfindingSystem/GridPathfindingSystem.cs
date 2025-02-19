using System;
using UnityEngine;

namespace Grid.PathfindingSystem
{
    public class GridPathfindingSystem : MonoBehaviour
    {
        [Header("Entities")]
        [SerializeField] private Bot bot;
        [SerializeField] private Player player;
        
        private GridBase<GridNode> _grid;
        private Vector3 _mousePosition;

        private void Start()
        {
            _grid = GetComponentInParent<GridMap>().Grid;
        }

        private void Update()
        {
            HandlePathfinding();
        }

        public void HandlePathfinding()
        {
            _mousePosition = player.GetMouseRaycast().position;

            if (Input.GetMouseButtonDown(0)) {
                bot.SetTargetPosition(_mousePosition);
            }
        }
    }
}