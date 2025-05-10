using System.Collections;
using Bot.Programming.Nodes.Base;

namespace Bot.Programming.Nodes
{
    public class ProgNodeVar<T> : ProgNodeBase
    {
        private T _value;

        public ProgNodeVar(string name, T value) : base(name)
        {
            _value = value;
        }

        public override void Init()
        {
            base.Init();
            NodeName = $"Variable ({typeof(T).Name})";
        
            // Добавляем выходной слот для значения
            OutputDataSlots.Add(new ProgNodeDataSlot("Value", typeof(T)));
        }
    
        // public override IEnumerator Execute(ProgNodeExecutionContext context, BotProgrammingController controller)
        // {
        //     // Устанавливаем значение в выходной слот
        //     // OutputDataSlots[0].Value = _value;
        //     yield break;
        // }
    }
}