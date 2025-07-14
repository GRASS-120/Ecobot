using System.Collections;
using UnityEngine;

namespace InteractionSystem
{
    public interface IInteractor
    {
        public Transform InteractorSource { get; set; }
        public bool IsHoldInteracting { get; set; }

        public virtual void HandleInteraction() {}
        public virtual void HandleHoldInteraction() {}
        public virtual void HandleHoldInteractionCanceled() {}
    }
}