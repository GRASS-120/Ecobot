using System;
using System.Collections;
using FiniteStateMachine;
using InteractionSystem;
using Player;
using R3;
using Sirenix.OdinInspector;
using UnityEngine;

namespace environment.Ore
{
    public class Ore : MonoBehaviour, IInteractable
    {
        // нужно чтобы игрок мог взаимодействовать войдя в тригер, в область
        
        public event Action OnMiningStart;
        public event Action OnMiningEnd;
        
        [SerializeField] private OreData data;
        [ReadOnly][SerializeField] private SerializableReactiveProperty<float> currentCapacity;
        
        private Coroutine _miningCoroutine;
        private PlayerManager _player;

        private void Awake()
        {
            currentCapacity = new SerializableReactiveProperty<float>(data.Capacity);
            
            currentCapacity.Subscribe(value => Debug.Log(value));
        }
        
        private IEnumerator StartMining()
        {
            Debug.Log("Starting Mining");
            OnMiningStart?.Invoke();
            
            while (currentCapacity.Value >= 0)
            {
                yield return new WaitForSeconds(data.MiningTime);
                
                currentCapacity.Value--;  // нужно еще дабавлять шмотку в инвентарь
                
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
            // не работает
            // if (interactor is PlayerInteractor playerInteractor)
            // {
            //     _player = playerInteractor.Player;
            //     Debug.Log("player: " + _player);
            // }
            
            _miningCoroutine = StartCoroutine(StartMining());

            yield return new WaitWhile(() => interactor.IsHoldInteracting);
        }

        public void HoldInteractionCancel(IInteractor interactor)
        {
            StopMining();
        }
    }
}