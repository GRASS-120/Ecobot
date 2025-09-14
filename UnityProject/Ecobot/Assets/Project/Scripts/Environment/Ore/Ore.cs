using System;
using System.Collections;
using FiniteStateMachine;
using InteractionSystem;
using Inventory;
using Inventory.LootSystem;
using Player;
using R3;
using Sirenix.OdinInspector;
using UnityEngine;

namespace environment.Ore
{
    public class Ore : MonoBehaviour, IInteractable, ILootProvider
    {
        public Observable<Unit> OnMiningStart => _onMiningStart;
        public Observable<Unit> OnMiningEnd => _onMiningEnd;
        public Observable<LootQuery> OnProvideLoot => _onProvideLoot;
        
        [Header("Settings")]
        [SerializeField] private OreData data;
        
        [Header("Debug")]
        [ReadOnly][SerializeField] private SerializableReactiveProperty<float> currentCapacity;
        
        private readonly Subject<LootQuery> _onProvideLoot = new();
        private readonly Subject<Unit> _onMiningStart = new();
        private readonly Subject<Unit> _onMiningEnd = new();
        private Coroutine _miningCoroutine;

        private void Awake()
        {
            currentCapacity = new SerializableReactiveProperty<float>(data.Capacity);
            
            currentCapacity.Subscribe(value => Debug.Log(value)).AddTo(this);
        }
        
        private IEnumerator StartMining()
        {
            Debug.Log("Starting Mining");
            _onMiningStart?.OnNext(Unit.Default);
            
            while (currentCapacity.Value >= 0)
            {
                yield return new WaitForSeconds(data.MiningTime);
                
                currentCapacity.Value--;
                
                _onProvideLoot.OnNext(new LootQuery(data.OreItem, 1));
            }
            
            _onMiningEnd.OnNext(Unit.Default);
            
            currentCapacity.Dispose();
            _onProvideLoot.OnCompleted();
        }

        private void StopMining()
        {
            Debug.Log("Stopping Mining");
            _onMiningEnd?.OnNext(Unit.Default);
            
            StopCoroutine(_miningCoroutine);
        }
        
        public IEnumerator HoldInteract(IInteractor interactor)
        {
            _miningCoroutine = StartCoroutine(StartMining());

            yield return new WaitWhile(() => interactor.IsHoldInteracting);
        }

        public void HoldInteractionCancel(IInteractor interactor)
        {
            StopMining();
        }
    }
}