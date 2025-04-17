using System;
using Bot;
using Grid.Base;
using Player;
using UnityEngine;

namespace Grid.PathfindingSystem
{
    public class GridPathfindingSystem : MonoBehaviour
    {
        [Header("Entities")]
        [SerializeField] private BotBase _botBase;
        [SerializeField] private PlayerManager player;
        
        private GridBase<GridNode> _grid;
        private Vector3 _mousePosition;
        
        private void Start()
        {
            _grid = GetComponentInParent<GridMap>().Grid;
        }
        
        // todo: remake
        public void HandlePathfinding()
        {
            // _mousePosition = player.GetMouseRaycast().position;
            //
            // if (Input.GetMouseButtonDown(0)) {
            //     bot.SetTargetPosition(_mousePosition);
            // }
        }
    }
}