using Player;
using R3;
using UnityEngine;

namespace Grid.BuildingSystem.PowerSystem
{
    [RequireComponent(typeof(PlayerTrigger))]
    public class PowerWireProximityReactor : MonoBehaviour
    {
        [SerializeField] private PlayerTrigger playerTrigger;

        private BuildingContext _context;
        private IPowerNode _node;
        private CompositeDisposable _subs;

        public void Init(BuildingContext context, IPowerNode node)
        {
            _context = context;
            _node = node;

            _subs?.Dispose();
            _subs = new CompositeDisposable();

            if (playerTrigger == null || _context?.PowerWireToolService == null || _node == null)
                return;

            playerTrigger.OnPlayerEntered
                .Subscribe(_ => _context.PowerWireToolService.RegisterProximity(_node))
                .AddTo(_subs);

            playerTrigger.OnPlayerExited
                .Subscribe(_ => _context.PowerWireToolService.UnregisterProximity(_node))
                .AddTo(_subs);
        }

        private void OnDisable()
        {
            _subs?.Dispose();
            _subs = null;

            if (_context?.PowerWireToolService != null && _node != null)
            {
                _context.PowerWireToolService.UnregisterProximity(_node);
            }
        }
    }
}