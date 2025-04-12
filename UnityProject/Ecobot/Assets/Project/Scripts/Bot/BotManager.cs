using System.Collections.Generic;
using Grid;
using UnityEngine;

namespace Bot
{
    public class BotManager : MonoBehaviour
    {
        [Header("Entities")]
        [SerializeField] private GridMap gridMap;
        [SerializeField] private List<BotBase> bots;
        [SerializeField] private Transform target;

        private void Awake()
        {
            foreach (var bot in bots)
            {
                bot.Init(gridMap);
            }
        }

        // todo: tmp
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
