using R3;
using UnityEngine;

namespace Player
{
    [RequireComponent(typeof(Collider))]
    public class PlayerTrigger : MonoBehaviour
    {
        public readonly Subject<PlayerManager> OnPlayerEntered = new();
        public readonly Subject<PlayerManager> OnPlayerExited = new();

        private void OnTriggerEnter(Collider other)
        {
            var player = other.GetComponent<PlayerManager>();
            if (player != null)
            {
                OnPlayerEntered.OnNext(player);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            var player = other.GetComponent<PlayerManager>();
            if (player != null)
            {
                OnPlayerExited.OnNext(player);
            }
        }
    }
}