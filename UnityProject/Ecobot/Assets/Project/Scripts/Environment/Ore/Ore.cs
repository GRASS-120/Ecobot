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
        public event Action OnMiningStart;
        public event Action OnMiningEnd;
        public Observable<LootQuery> OnGiveLoot => _onGiveLoot;
        
        [SerializeField] private OreData data;
        [ReadOnly][SerializeField] private SerializableReactiveProperty<float> currentCapacity;
        
        private Subject<LootQuery> _onGiveLoot = new Subject<LootQuery>();
        private Coroutine _miningCoroutine;

        private void Awake()
        {
            currentCapacity = new SerializableReactiveProperty<float>(data.Capacity);
            
            currentCapacity.Subscribe(value => Debug.Log(value));
        }
        
        private IEnumerator StartMining()
        {
            Debug.Log("Starting Mining");
            OnMiningStart?.Invoke();

            var loot = new LootQuery(data.OreItem, 1);
            
            while (currentCapacity.Value >= 0)
            {
                yield return new WaitForSeconds(data.MiningTime);
                
                currentCapacity.Value--;
                
                _onGiveLoot.OnNext(loot);
                
                yield return null;
            }
            
            currentCapacity.Dispose();
        }

        private void StopMining()
        {
            Debug.Log("Stopping Mining");
            OnMiningEnd?.Invoke();
            
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