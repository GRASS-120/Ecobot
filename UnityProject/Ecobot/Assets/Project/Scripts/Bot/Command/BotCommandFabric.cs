using Bot.Command.Types;
using UnityEngine;

namespace Bot.Command
{
    public class BotCommandFabric
    {
        private readonly BotBase _bot;
        
        public BotCommandFabric(BotBase bot)
        {
            _bot = bot;
        }

        public BotCommand CreateMoveCommand(Vector3 target)
        {
            return new BotCommandMove(_bot.MovementController, target);
        }
    }
}
