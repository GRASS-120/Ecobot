using System;

namespace Bot.Programming.Nodes.Base
{
    public class ProgNodeStreamSlot
    {
        public ProgNodeBase ConnectedNode { get; set; }
        public int ConnectedSlotIndex { get; set; } = -1; 
        
        // public ProgNodeStreamSlot(string name, Type valueType)
        // {
        //     Name = name;
        //     ValueType = valueType;
        // }
        //
        // public bool CanConnect(ProgNodeDataSlot other)
        // {
        //     return ValueType.IsAssignableFrom(other.ValueType);
        // }
    }
}