using System;
using System.Collections;
using InteractionSystem;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;
using PlayerInputManager = Player.InputManager.PlayerInputManager;

namespace Player
{
    public class PlayerInteractor : MonoBehaviour, IInteractor
    {
        [Header("Components")]
        [SerializeField] private Transform interactorSource;
        
        [Header("Interaction Params")]
        [SerializeField] private float interactionRange = 20f;
        
        public Transform InteractorSource { get; set; }
        public bool IsHoldInteracting { get; set; }

        private PlayerInputManager _input;
        
        private IInteractable _currentInteractable;
        
        public void Init(PlayerManager player)
        {
            _input = player.Input;
            InteractorSource = interactorSource;
            
            _input.OnInteract += HandleInteraction;
            _input.OnHoldInteraction += HandleHoldInteraction;
            _input.OnHoldInteractCanceled += HandleHoldInteractionCanceled;
        }
        
        public void HandleInteraction()
        {
            Debug.Log("HandleInteraction");
            
            var r = new Ray(InteractorSource.position, InteractorSource.forward);
            if (Physics.Raycast(r, out RaycastHit hit, interactionRange))
            {
                if (hit.collider.gameObject.TryGetComponent(out IInteractable interactable))
                {
                    interactable.Interact(this);
                }
            }
        }

        public void HandleHoldInteraction()
        {
            var r = new Ray(InteractorSource.position, InteractorSource.forward);
            if (Physics.Raycast(r, out RaycastHit hit, interactionRange))
            {
                if (hit.collider.gameObject.TryGetComponent(out IInteractable interactable))
                {
                    _currentInteractable = interactable;
                    IsHoldInteracting = true;
                    
                    StartCoroutine(interactable.HoldInteract(this));
                }
            }
        }

        public void HandleHoldInteractionCanceled()
        {
            if (_currentInteractable == null) return;
            
            Debug.Log("HandleHoldInteractionCanceled");
            
            StopCoroutine(_currentInteractable.HoldInteract(this));
            
            _currentInteractable.HoldInteractionCancel(this);
            
            IsHoldInteracting = true;
            _currentInteractable = null;
        }
    }
}