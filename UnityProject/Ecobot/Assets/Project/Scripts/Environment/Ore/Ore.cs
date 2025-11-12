using System;
using System.Collections;
using System.Collections.Generic;
using InteractionSystem;
using Inventory;
using Inventory.LootSystem;
using R3;
using Sirenix.OdinInspector;
using UnityEngine;
using WUI;

namespace environment.Ore
{
    public class Ore : MonoBehaviour, IInteractable, ILootProvider
    {
        public Observable<Unit> OnMiningStart => _onMiningStart;
        public Observable<Unit> OnMiningEnd   => _onMiningEnd;

        // ⚠️ Только для UI/логов. amount = 0, чтобы внешние подписки не добавляли предмет второй раз.
        public Observable<LootQuery> OnProvideLoot => _onProvideLoot;

        [Header("Settings")]
        [SerializeField] private OreData data;

        [Header("UI")]
        [SerializeField] private ProgressBar progressBar;

        [Header("Debug")]
        [ReadOnly] [SerializeField] private SerializableReactiveProperty<float> currentCapacity;

        private readonly Subject<LootQuery> _onProvideLoot = new();
        private readonly Subject<Unit> _onMiningStart = new();
        private readonly Subject<Unit> _onMiningEnd   = new();

        // Очередь держателей (сохраняем порядок подключения)
        private readonly List<IInteractor> _holders = new();

        // Текущий «владелец» дропа — первый, кто начал держать
        private IInteractor _owner;

        private Coroutine _miningCoroutine;
        private bool _miningActive;

        private void Awake()
        {
            currentCapacity = new SerializableReactiveProperty<float>(data.Capacity);
            currentCapacity.Subscribe(v => Debug.Log(v)).AddTo(this);

            progressBar?.Init(data.MiningTime);
        }

        private IEnumerator MiningLoop()
        {
            if (!_miningActive)
            {
                _miningActive = true;
                _onMiningStart.OnNext(Unit.Default);
                progressBar?.ShowProgressBar();
            }

            while (currentCapacity.Value > 0 && _holders.Count > 0)
            {
                // ускоряем линейно от числа держателей
                int miners = Mathf.Max(1, _holders.Count);
                float tickTime = data.MiningTime / miners;

                progressBar?.StartSingleProgress();
                yield return new WaitForSeconds(tickTime);

                // если к концу тика никого не осталось — без лута
                if (_holders.Count == 0) break;

                // актуализируем владельца (если он отвалился)
                if (_owner == null || !_holders.Contains(_owner))
                {
                    _owner = _holders[0]; // передаём владение следующему в очереди
                }

                // уменьшаем ёмкость
                currentCapacity.Value--;

                // выдаём лут ТОЛЬКО владельцу
                GiveLootToOwner(_owner, data.OreItem, 1);

                // событие только для UI (amount=0)
                _onProvideLoot.OnNext(new LootQuery(data.OreItem, 0));

                progressBar?.CompleteSingleProgress();
            }

            if (currentCapacity.Value <= 0)
            {
                progressBar?.HideProgressBar();
                _onMiningEnd.OnNext(Unit.Default);

                currentCapacity.Dispose();
                _onProvideLoot.OnCompleted();
                
                Destroy(gameObject);

                _miningActive = false;
                _miningCoroutine = null;
                yield break;
            }

            // Пауза
            progressBar?.HideProgressBar();
            _onMiningEnd.OnNext(Unit.Default);

            _miningActive = false;
            _miningCoroutine = null;
        }

        private void EnsureMiningRunning()
        {
            if (_miningCoroutine == null && currentCapacity.Value > 0 && _holders.Count > 0)
                _miningCoroutine = StartCoroutine(MiningLoop());
        }

        private void StopMiningIfNoHolders()
        {
            if (_holders.Count == 0 && _miningCoroutine != null)
            {
                StopCoroutine(_miningCoroutine);
                _miningCoroutine = null;

                if (_miningActive)
                {
                    progressBar?.HideProgressBar();
                    _onMiningEnd.OnNext(Unit.Default);
                    _miningActive = false;
                }
            }
        }

        private static void GiveLootToOwner(IInteractor owner, InventoryItemData item, int amount)
        {
            if (owner is not Component comp) return;

            var invHolder = comp.GetComponentInParent<IInventoryHolder>();
            if (invHolder == null)
            {
                Debug.LogWarning("[Ore] Owner has no IInventoryHolder — loot is lost.");
                return;
            }

            if (!invHolder.TryAddToInventory(item, amount))
            {
                Debug.Log($"[Ore] Inventory full or rejected loot on '{comp.name}'.");
            }
        }

        // IInteractable
        public IEnumerator HoldInteract(IInteractor interactor)
        {
            if (interactor != null && !_holders.Contains(interactor))
            {
                _holders.Add(interactor);
                // если владельца ещё нет — назначаем первого пришедшего
                if (_owner == null) _owner = interactor;
            }

            EnsureMiningRunning();

            // ждём пока именно этот держатель «держит»
            yield return new WaitWhile(() => interactor != null && _holders.Contains(interactor));
        }

        public void HoldInteractionCancel(IInteractor interactor)
        {
            if (interactor != null)
            {
                int idx = _holders.IndexOf(interactor);
                if (idx >= 0) _holders.RemoveAt(idx);

                if (_owner == interactor)
                {
                    // владелец отпустил — если есть другие, передать первому в очереди
                    _owner = _holders.Count > 0 ? _holders[0] : null;
                }
            }

            StopMiningIfNoHolders();
        }
    }
}
