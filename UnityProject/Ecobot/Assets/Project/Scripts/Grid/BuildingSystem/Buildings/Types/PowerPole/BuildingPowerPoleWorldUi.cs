using Grid.BuildingSystem.Buildings.Base;
using R3;
using UnityEngine;

namespace Grid.BuildingSystem.Buildings.Types.PowerPole
{
    public class BuildingPowerPoleWorldUi : BuildingWorldUiBase
    {
        [Header("Icons (3 slots)")]
        [SerializeField] private SpriteRenderer[] slotIcons; // size = 3

        [Header("Colors")]
        [SerializeField] private Color noPower_NoConnection = new Color(0.2f, 0.2f, 0.2f); // темносерый
        [SerializeField] private Color noPower_Connected = new Color(0.6f, 0.6f, 0.6f);     // светлосерый
        [SerializeField] private Color power_NoConnection = new Color(0.5f, 0.4f, 0.0f);    // темножёлтый
        [SerializeField] private Color power_Connected = new Color(1.0f, 0.9f, 0.0f);       // жёлтый

        private BuildingPowerPole _pole;
        private CompositeDisposable _subs;

        public override void Init(BuildingBase building, BuildingContext context)
        {
            base.Init(building, context);
            _pole = building as BuildingPowerPole;

            _subs?.Dispose();
            _subs = new CompositeDisposable();

            if (_context?.PowerGridService != null)
            {
                _context.PowerGridService.Changed
                    .Subscribe(_ => Refresh())
                    .AddTo(_subs);
            }

            Refresh();
        }

        private void OnDisable()
        {
            _subs?.Dispose();
            _subs = null;
        }

        public void SetPowered(bool isPowered)
        {
            // вызов из BuildingPowerPole.OnPowerStateChanged — можно просто перерисовать
            Refresh();
        }

        private void Refresh()
        {
            if (_pole == null || slotIcons == null || slotIcons.Length == 0) return;

            bool hasState = _context.PowerGridService.TryGetNetworkState(
                _pole, out bool powered, out _, out _, out _);

            int connectedCount = Mathf.Clamp(_pole.Outputs.Count, 0, slotIcons.Length);

            for (int i = 0; i < slotIcons.Length; i++)
            {
                bool isConnected = i < connectedCount;
                slotIcons[i].color = powered
                    ? (isConnected ? power_Connected : power_NoConnection)
                    : (isConnected ? noPower_Connected : noPower_NoConnection);
            }
        }
    }
}