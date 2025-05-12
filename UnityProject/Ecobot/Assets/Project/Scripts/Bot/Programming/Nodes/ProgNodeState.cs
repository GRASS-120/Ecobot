using System.Collections;
using Bot.Programming.Nodes.Base;
using UnityEngine;

namespace Bot.Programming.Nodes
{
    public abstract class ProgNodeState : ProgNodeBase
    {
        protected ProgNodeStreamSlot nextSlot;
        
        public ProgNodeState(string nodeName)
        {
            NodeName = nodeName;
            nextSlot = new ProgNodeStreamSlot("Next", this);
            slots.Add(nextSlot);
        }
    }
}