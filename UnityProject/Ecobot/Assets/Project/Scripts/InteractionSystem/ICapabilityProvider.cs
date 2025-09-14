namespace InteractionSystem
{
    public interface ICapabilityProvider
    {
        public void RegisterCapability<T>(T capability) where T : class;
        public bool TryGetCapability<T>(out T capability) where T : class;
    }
}