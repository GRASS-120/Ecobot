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
        [SerializeField] private float interactSphereRadius = 0.6f;
        [SerializeField] private LayerMask interactionMask = ~0;
        
        public Transform InteractorSource { get; set; }
        public bool IsHoldInteracting { get; set; }

        private PlayerInputManager _input;
        private IInteractable _currentInteractable;
        private ILootReceiver _lootReceiver;     
        private readonly CompositeDisposable _lootSubscription = new ();
        private Coroutine _holdRoutine;
        
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
            if (TryFindBestInteractable(out var interactable))
            {
                interactable.Interact(this);
            }
        }

        public void HandleAltInteraction()
        {
            if (TryFindBestInteractable(out var interactable))
            {
                interactable.AltInteract(this);
            }
        }

        public void HandleHoldInteraction()
        {
            if (IsHoldInteracting) return;

            if (TryFindBestInteractable(out var interactable))
            {
                _currentInteractable = interactable;
                IsHoldInteracting = true;

                if (interactable is ILootProvider provider)
                {
                    _lootSubscription.Clear();
                    provider.OnProvideLoot.Subscribe(loot =>
                    {
                        _lootReceiver.TryReceive(loot);
                    }).AddTo(_lootSubscription);
                }

                _holdRoutine = StartCoroutine(interactable.HoldInteract(this));
            }
        }

        public void HandleHoldInteractionCanceled()
        {
            if (_currentInteractable == null) return;

            if (_holdRoutine != null)
            {
                StopCoroutine(_holdRoutine);
                _holdRoutine = null;
            }

            _currentInteractable.HoldInteractionCancel(this);

            IsHoldInteracting = false;
            _currentInteractable = null;
            _lootSubscription.Clear();
        }
        
        private bool TryFindBestInteractable(out IInteractable target)
        {
            target = null;

            var origin = InteractorSource != null ? InteractorSource.position : transform.position;
            var dir = InteractorSource != null ? InteractorSource.forward : transform.forward;

            var hits = Physics.SphereCastAll(
                origin,
                interactSphereRadius,
                dir,
                interactionRange,
                interactionMask,
                QueryTriggerInteraction.Ignore
            );

            if (hits == null || hits.Length == 0) return false;

            Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

            for (int i = 0; i < hits.Length; i++)
            {
                var col = hits[i].collider;
                if (col == null) continue;

                if (col.TryGetComponent(out IInteractable direct))
                {
                    target = direct;
                    return true;
                }

                var parent = col.GetComponentInParent<IInteractable>();
                if (parent != null)
                {
                    target = parent;
                    return true;
                }
            }

            return false;
        }
    }
}