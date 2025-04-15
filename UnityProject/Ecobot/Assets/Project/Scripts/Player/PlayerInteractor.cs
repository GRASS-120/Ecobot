using InteractionSystem;
using Player.InputManager;
using Sirenix.OdinInspector;
using UnityEngine;

// пока через инпуты, но потом нужно сделать так, чтобы и роботы наверное могли nteract делать... но как?

namespace Player
{
    public class PlayerInteractor : MonoBehaviour, IInteractor
    {
        [Title("Components")]
        [SerializeField] private Transform interactorSource;
        
        [Title("Interaction Params")]
        [SerializeField] private float interactionRange = 10f;
        
        public Transform InteractorSource { get; set; }
        
        private PlayerManager _player;
        private PlayerInputManager _input;
        
        private void Awake()
        {
            _player = GetComponent<PlayerManager>();
            _input = _player.Input;
            InteractorSource = interactorSource;
            
            _input.OnInteract += HandleInteractions;
        }
        
        public void HandleInteractions()
        {
            var r = new Ray(InteractorSource.position, InteractorSource.forward);
            if (Physics.Raycast(r, out RaycastHit hit, interactionRange))
            {
                if (hit.collider.gameObject.TryGetComponent(out IInteractable interactable))
                {
                    interactable.StartInteraction(this);
                }
            }
        }

        // private void StartInteraction(IInteractable interactable)
        // {
        //     // interactable.Interact(this, out bool success);
        //     IsInteracting = true;
        // }
        //
        // void EndInteraction()
        // {
        //     IsInteracting = false;
        // }
    }
}