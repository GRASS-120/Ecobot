using System;
using System.Collections;
using FiniteStateMachine;

namespace InteractionSystem
{
    public interface IInteractable
    {
        public virtual void Interact(IInteractor interactor) {}

        public virtual IEnumerator HoldInteract(IInteractor interactor)
        {
            yield return null;
        }

        public virtual void HoldInteractionCancel(IInteractor interactor) {}
    }
}