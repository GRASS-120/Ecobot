namespace Bot.Programming.Nodes.Base
{
    public class ProgNodeStreamSlot : ProgNodeSlotBase
    {
        public ProgNodeStreamSlot(string slotName, ProgNodeBase owner) : base(slotName, owner) { }
        
        public override bool CanConnect(ProgNodeBase node)
        {
            // К потоковому слоту можно подключить любую ноду
            return true;
        }
    }
}