using System;

namespace Bot.Command
{
    public abstract class BotCommand {
        public Action OnExecutionEnd;
        
        public abstract void Execute();

        public abstract bool CanBeExecuted();
    }
}
