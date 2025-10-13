using UnityEngine;

namespace Grid.BuildingSystem.Buildings.Visual
{
    public class BuildingFurnaceVisual : BuildingVisualBase
    {
        [SerializeField] private ParticleSystem smokeVfx;

        public override void Init(BuildingBase building, BuildingContext context)
        {
            base.Init(building, context);
            UpdateVfx(false);
        }

        public override void SetPowered(bool isPowered)
        {
            UpdateVfx(isPowered);
        }

        private void UpdateVfx(bool on)
        {
            if (smokeVfx == null) return;

            var emission = smokeVfx.emission;
            emission.enabled = on;

            if (on)
            {
                if (!smokeVfx.isPlaying) smokeVfx.Play(true);
            }
            else
            {
                if (smokeVfx.isPlaying) smokeVfx.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }
        }
    }
}