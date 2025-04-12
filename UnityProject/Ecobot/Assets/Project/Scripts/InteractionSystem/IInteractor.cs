using UnityEngine;

namespace InteractionSystem
{
    public interface IInteractor
    {
        public Transform InteractorSource { get; set; }

        public void HandleInteractions();
    }
}