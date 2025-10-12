using UnityEngine;

namespace Bot.Programming.Nodes.Base
{
    public abstract class ProgNodeSlotBase
    {
        public string SlotName { get; protected set; }
        public ProgNodeBase Owner { get; protected set; }
        public ProgNodeBase ConnectedNode { get; protected set; }
        
        public ProgNodeSlotBase(string slotName, ProgNodeBase owner)
        {
            SlotName = slotName;
            Owner = owner;
        }
        
        
        public void Connect(ProgNodeBase node)
        {
            if (CanConnect(node))
            {
                ConnectedNode = node;
            }
            else
            {
                Debug.LogWarning($"Cannot connect node {node.NodeName} to slot {SlotName}");
            }
        }
        
        public abstract bool CanConnect(ProgNodeBase node);
        
        
        
        public void Disconnect()
        {
            ConnectedNode = null;
        }
    }
}