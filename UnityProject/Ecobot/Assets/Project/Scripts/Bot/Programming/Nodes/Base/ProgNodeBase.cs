using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Bot.Programming.Nodes.Base
{
    public abstract class ProgNodeBase
    {
        protected string NodeName;
        
        public List<ProgNodeDataSlot> InputDataSlots { get; protected set; } = new ();
        public List<ProgNodeDataSlot> OutputDataSlots { get; protected set; } = new ();
        public ProgNodeStreamSlot InputStreamSlot { get; protected set; } = new ();
        public ProgNodeStreamSlot OutputStreamSlot { get; protected set; } = new ();
        
        public ProgNodeBase NextNode { get; protected set; }
        
        public ProgNodeBase(string name)
        {
            NodeName = name;
        }
        
        public virtual void Init() { }
    
        // public abstract IEnumerator Execute(ProgNodeExecutionContext context, BotProgrammingController controller);
    
        public virtual T GetOutputValue<T>(int slotIndex)
        {
            if (slotIndex >= 0 && slotIndex < OutputDataSlots.Count)
            {
                if (OutputDataSlots[slotIndex] is T value)
                    return value;
            }
            return default;
        }
    
        public virtual void SetInputValue(int slotIndex, ProgNodeDataSlot value)
        {
            if (slotIndex >= 0 && slotIndex < InputDataSlots.Count)
            {
                InputDataSlots[slotIndex] = value;
            }
        }
    }
}