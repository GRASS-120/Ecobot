using System;
using System.Collections;
using System.Collections.Generic;
using InteractionSystem;
using Inventory;
using Inventory.LootSystem;
using R3;
using UnityEngine;

namespace Bot
{
    /// <summary>
    /// Интерактор бота:
    /// - проверяет дистанцию / ищет интеракт
    /// - может "зажать" взаимодействие (IsHoldInteracting)
    /// - подписывается на ILootProvider и кладёт в инвентарь
    /// - сам пытается найти инвентарь у родителя (BotBase) в Awake
    /// </summary>
    public class BotInteractor : MonoBehaviour, IInteractor
    {
        [Header("Source / Origin")]
        [SerializeField] private Transform interactorSource;

        [Header("Interaction Params")]
        [SerializeField] private float interactionRange = 2.0f;
        [SerializeField] private float interactSphereRadius = 0.6f;
        [SerializeField] private LayerMask interactionMask = ~0;

        // кто даёт позицию / forward
        public Transform InteractorSource
        {
            get => interactorSource;
            set => interactorSource = value;
        }

        // нужен для Ore.HoldInteract(...)
        public bool IsHoldInteracting { get; set; }

        public float InteractionRange => interactionRange;

        // сюда мы складываем "куда класть лут"
        private IInventoryHolder _inventoryHolder;

        private readonly CompositeDisposable _lootSubs = new();
        private readonly HashSet<ILootProvider> _observedProviders = new();

        private void Awake()
        {
            // 1. если в инспекторе забыли вызвать Init(...) — пробуем сами
            if (_inventoryHolder == null)
            {
                var holder = GetComponentInParent<IInventoryHolder>();
                if (holder != null)
                {
                    _inventoryHolder = holder;
                    Debug.Log("[BotInteractor] Awake: auto-linked inventory holder from parent.");
                }
            }

            // 2. источник
            if (interactorSource == null)
                interactorSource = transform;
        }

        /// <summary>
        /// Нормальный явный Init — если ты вызываешь его из BotBase.Init(...)
        /// </summary>
        public void Init(IInventoryHolder inventoryHolder)
        {
            _inventoryHolder = inventoryHolder;

            if (interactorSource == null)
                interactorSource = transform;

            Debug.Log("[BotInteractor] Init. Inventory holder = " +
                      (_inventoryHolder != null ? _inventoryHolder.ToString() : "NULL"));
        }

        /// <summary>
        /// Можно дозадать инвентарь позже (мы будем звать из ноды)
        /// </summary>
        public void EnsureInventory(IInventoryHolder holder)
        {
            if (_inventoryHolder == null && holder != null)
            {
                _inventoryHolder = holder;
                Debug.Log("[BotInteractor] EnsureInventory: linked holder later.");
            }
        }

        public bool IsTargetInRange(Transform target)
        {
            if (target == null) return false;
            Vector3 origin = interactorSource != null ? interactorSource.position : transform.position;
            float dist = Vector3.Distance(origin, target.position);
            return dist <= interactionRange;
        }

        public bool TryFindBestInteractable(out IInteractable target)
        {
            target = null;

            Vector3 origin = interactorSource != null ? interactorSource.position : transform.position;
            Vector3 dir    = interactorSource != null ? interactorSource.forward  : transform.forward;

            var hits = Physics.SphereCastAll(
                origin,
                interactSphereRadius,
                dir,
                interactionRange,
                interactionMask,
                QueryTriggerInteraction.Ignore
            );

            if (hits == null || hits.Length == 0)
                return false;

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

        /// <summary>
        /// Подписываемся на провайдера лута (руда и т.п.)
        /// </summary>
        public void SubscribeTo(ILootProvider provider)
        {
            if (provider == null) return;
            if (_observedProviders.Contains(provider)) return;

            _observedProviders.Add(provider);

            provider.OnProvideLoot
                .Subscribe(loot =>
                {
                    // если к этому моменту инвентарь всё ещё null — попробуем ещё раз достать из родителя
                    if (_inventoryHolder == null)
                    {
                        var lateHolder = GetComponentInParent<IInventoryHolder>();
                        if (lateHolder != null)
                        {
                            _inventoryHolder = lateHolder;
                            Debug.Log("[BotInteractor] SubscribeTo: late-bound inventory holder from parent.");
                        }
                    }

                    if (_inventoryHolder != null)
                    {
                        _inventoryHolder.TryAddToInventory(loot.Item, loot.Amount);
                    }
                    else
                    {
                        // вот эта строка и была в логе — теперь она должна пропасть
                        Debug.LogWarning("[BotInteractor] inventory is NULL, loot will be lost.");
                    }
                })
                .AddTo(_lootSubs);
        }

        // IInteractor ── боту самому это особо не надо
        public IEnumerator HoldInteract(IInteractor interactor)
        {
            yield break;
        }

        public void HoldInteractionCancel(IInteractor interactor)
        {
            IsHoldInteracting = false;
        }
    }
}
