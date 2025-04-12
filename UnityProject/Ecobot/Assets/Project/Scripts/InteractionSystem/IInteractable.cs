using System;

namespace InteractionSystem
{
    public interface IInteractable
    {
        // public event Action<IInteractable> OnInteractionComplete;

        // + hold interaction
        public void StartInteraction(IInteractor interactor);
        public void StopInteraction(IInteractor interactor);
    }
}