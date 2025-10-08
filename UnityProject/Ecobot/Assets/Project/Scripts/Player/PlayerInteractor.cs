using System;
using System.Collections;
using System.Collections.Generic;
using InteractionSystem;
using Inventory.LootSystem;
using R3;
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
        private ILootReceiver _lootReceiver;     
        private readonly CompositeDisposable _lootSubscription = new (); 
        
        public void Init(PlayerManager player, ILootReceiver lootReceiver)
        {
            _input = player.Input;
            _lootReceiver = lootReceiver;
            InteractorSource = interactorSource;
            
            _input.OnInteract += HandleInteraction;
            _input.OnAltInteract += HandleAltInteraction;
            _input.OnHoldInteraction += HandleHoldInteraction;
            _input.OnHoldInteractCanceled += HandleHoldInteractionCanceled;
        }
        
        public void HandleInteraction()
        {
            var r = new Ray(InteractorSource.position, InteractorSource.forward);
            if (Physics.Raycast(r, out RaycastHit hit, interactionRange))
            {
                if (hit.collider.gameObject.TryGetComponent(out IInteractable interactable))
                {
                    interactable.Interact(this);
                }
            }
        }
        
        public void HandleAltInteraction()
        {
            var r = new Ray(InteractorSource.position, InteractorSource.forward);
            if (Physics.Raycast(r, out RaycastHit hit, interactionRange))
            {
                if (hit.collider.gameObject.TryGetComponent(out IInteractable interactable))
                {
                    interactable.AltInteract(this);
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
                    
                    // Если цель производит лут — подпишемся и складываем в lootReceiver (если он есть)
                    if (interactable is ILootProvider provider)
                    {
                        _lootSubscription.Clear();
                        provider.OnProvideLoot.Subscribe(loot =>
                        {
                            _lootReceiver.TryReceive(loot);
                        }).AddTo(_lootSubscription);
                    }
                    
                    StartCoroutine(interactable.HoldInteract(this));
                }
            }
        }

        public void HandleHoldInteractionCanceled()
        {
            if (_currentInteractable == null) return;
            
            StopCoroutine(_currentInteractable.HoldInteract(this));
            
            _currentInteractable.HoldInteractionCancel(this);
            
            IsHoldInteracting = true;
            _currentInteractable = null;
        }
    }
}