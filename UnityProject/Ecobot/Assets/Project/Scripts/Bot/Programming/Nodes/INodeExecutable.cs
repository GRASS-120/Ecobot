using System.Collections;
using Bot.Programming.Nodes.Base;

namespace Bot.Programming.Nodes
{
    public interface INodeExecutable
    {
        public IEnumerator Execute(ProgNodeExecutionContext context, BotProgrammingController controller);
    }
}