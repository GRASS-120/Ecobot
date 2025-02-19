using System;

public interface IInteractable
{
    public event Action<IInteractable> OnInteractionComplete;

    public void Interact(Interactor interactor, out bool success);

    public void EndInteraction();
}