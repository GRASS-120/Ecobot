using Player;
using R3;
using UnityEngine;

namespace Grid.BuildingSystem.Buildings.Reactors
{
    public class WorldUiProximityReactor : MonoBehaviour
    {
        [SerializeField] private PlayerTrigger playerTrigger;
        [SerializeField] private GameObject[] uiRoots;

        private CompositeDisposable _subs;

        public void Init(BuildingContext context)
        {
            _subs?.Dispose();
            _subs = new CompositeDisposable();

            if (playerTrigger == null) return;

            playerTrigger.OnPlayerEntered
                .Subscribe(_ => SetActive(true))
                .AddTo(_subs);

            playerTrigger.OnPlayerExited
                .Subscribe(_ => SetActive(false))
                .AddTo(_subs);

            SetActive(false);
        }

        private void OnDisable()
        {
            _subs?.Dispose();
            _subs = null;
            SetActive(false);
        }

        private void SetActive(bool value)
        {
            if (uiRoots == null) return;
            foreach (var go in uiRoots)
            {
                if (go != null) go.SetActive(value);
            }
        }
    }
}