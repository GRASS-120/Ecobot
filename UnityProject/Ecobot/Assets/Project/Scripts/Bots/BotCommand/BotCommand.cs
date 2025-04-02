namespace Bots.BotCommand
{
    public abstract class BotCommand {
        public abstract void Execute();

        public abstract void CanBeExecuted();
    }
}
