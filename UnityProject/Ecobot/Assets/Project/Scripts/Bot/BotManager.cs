using System.Collections.Generic;
using Game;
using Grid;
using Grid.Base;
using GUI.Programming;
using GUI.UIFramework;
using UnityEngine;

namespace Bot
{
    public class BotManager : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private GridMap gridMap;
        [SerializeField] private List<BotBase> bots;
        [SerializeField] private Transform target;
        [SerializeField] private GameManager gameManager;
        [SerializeField] private WindowManager windowManager;

        private void Awake()
        {
            foreach (var bot in bots)
            {
                bot.Init(gridMap, gameManager, windowManager);
            }
        }

        private void Start()
        {
            foreach (var bot in bots)
            {
                var c = bot.CommandController.Fabric.CreateMoveCommand(target.position);
                bot.CommandController.AddCommand(c);
            }
        }
    }
}
