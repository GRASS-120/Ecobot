using System.Collections;
using Bot.Programming.Nodes.Base;
using UnityEngine;

namespace Bot.Programming.Nodes
{
    public class ProgNodeInput : ProgNodeBase
    {
        public ProgNodeInput(string name) : base(name)
        {
        }

        public override void Init()
        {
            base.Init();
            NodeName = "Start";
        
            OutputDataSlots.Add(new ProgNodeDataSlot("Flow", typeof(void)));
        }
    
        // public override IEnumerator Execute(ProgNodeExecutionContext context)
        // {
        //     if (NextNode != null)
        //     {
        //         yield return context.Bot.StartCoroutine(NextNode.Execute(context));
        //     }
        //     else
        //     {
        //         Debug.LogError($"{this}: нет следующей ноды");
        //     }
        //         
        // }
    }
}