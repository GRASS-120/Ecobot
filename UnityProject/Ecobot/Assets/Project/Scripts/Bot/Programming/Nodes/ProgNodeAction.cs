using Bot.Programming.Nodes.Base;

namespace Bot.Programming.Nodes
{
    public abstract class ProgNodeAction : ProgNodeBase
    {
        protected ProgNodeStreamSlot successSlot;
        protected ProgNodeStreamSlot failureSlot;
        
        public ProgNodeAction(string nodeName)
        {
            NodeName = nodeName;
            successSlot = new ProgNodeStreamSlot("Success", this);
            failureSlot = new ProgNodeStreamSlot("Failure", this);
            slots.Add(successSlot);
            slots.Add(failureSlot);
        }
    }
}