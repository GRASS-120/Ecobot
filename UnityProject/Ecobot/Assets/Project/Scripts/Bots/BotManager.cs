using System;
using System.Collections.Generic;
using Grid;
using UnityEngine;

namespace Bots
{
    public class BotManager : MonoBehaviour
    {
        [Header("Entities")]
        [SerializeField] private GridMap gridMap;
        [SerializeField] private List<Bot> bots;

        private void Awake()
        {
            foreach (var bot in bots)
            {
                bot.Init(gridMap);
            }
        }
        
        private void Update()
        {
            foreach (var bot in bots)
            {
                bot.HandleMovement();  // TODO: -> MOVEMENT MANAGER 
            }
        }
    }
}
