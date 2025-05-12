namespace Bot.Programming.Nodes.Slots
{
    public class DataSlotAdapter<TSource, TTarget>
    {
        private ProgNodeDataSlot<TSource> sourceSlot;
        private System.Func<TSource, TTarget> converter;
    
        public DataSlotAdapter(ProgNodeDataSlot<TSource> sourceSlot, System.Func<TSource, TTarget> converter)
        {
            this.sourceSlot = sourceSlot;
            this.converter = converter;
        }
    
        public TTarget GetValue()
        {
            TSource sourceValue = sourceSlot.Value;
            return converter(sourceValue);
        }
    }
}