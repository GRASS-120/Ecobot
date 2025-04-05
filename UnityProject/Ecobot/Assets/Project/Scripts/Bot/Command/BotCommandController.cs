using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Bot.Command
{
    // todo: затем скорее всего нужно будет переделать на очередь с приоритетом + в целом зарефакторить, там ясно будет
    public class BotCommandController : MonoBehaviour
    {
        private BotBase _bot;
        private List<BotCommand> _commands;
        public BotCommandFabric Fabric { get; private set;}

        public void Init(BotBase bot)
        {
            _bot = bot;
            _commands = new List<BotCommand>();
            Fabric = new BotCommandFabric(_bot);
        }
        
        public void AddCommand(BotCommand command)
        {
            _commands.Add(command);
        }

        public void Play()
        {
            foreach (var command in _commands.ToList())
            {
                bool success = command.CanBeExecuted();

                if (success)
                {
                    Debug.Log($"command {command} executed");
                    command.Execute();
                }
                
            }
        }
    }
}