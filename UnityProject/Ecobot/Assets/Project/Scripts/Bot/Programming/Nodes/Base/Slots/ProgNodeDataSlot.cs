using System;

namespace Bot.Programming.Nodes.Base
{
    public class ProgNodeDataSlot
    {
        public string Name { get; set; }
        public Type ValueType { get; set; }
        public object Value { get; set; }
        public ProgNodeBase ConnectedNode { get; set; }
        public int ConnectedSlotIndex { get; set; } = -1;
    
        public ProgNodeDataSlot(string name, Type valueType)
        {
            Name = name;
            ValueType = valueType;
        }
    
        public bool CanConnect(ProgNodeDataSlot other)
        {
            return ValueType.IsAssignableFrom(other.ValueType);
        }
    }
}