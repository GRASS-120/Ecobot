using System.Collections;
using Bot.Programming.Nodes.Base;

namespace Bot.Programming.Nodes
{
    public class ProgNodeVar<T> : ProgNodeBase
    {
        public T Value { get; set; }
        
        public ProgNodeVar(string nodeName, T initialValue)
        {
            NodeName = nodeName;
            Value = initialValue;
            Description = $"Variable of type {typeof(T).Name}";
        }
        
        public override IEnumerator Execute(BotBase bot, BotProgramExecutor executor)
        {
            // Переменные не выполняются сами по себе, они просто хранят значение
            yield break;
        }
    }
}