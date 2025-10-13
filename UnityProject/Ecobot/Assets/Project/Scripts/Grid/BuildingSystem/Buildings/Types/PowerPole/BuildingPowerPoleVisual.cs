using Grid.BuildingSystem.Buildings.Base;
using UnityEngine;

namespace Grid.BuildingSystem.Buildings.Visual
{
    public class BuildingPowerPoleVisual : BuildingVisualBase
    {
        [SerializeField] private ParticleSystem electricityVfx;

        public override void Init(BuildingBase building, BuildingContext context)
        {
            base.Init(building, context);
            UpdateVfx(false);

            if (electricityVfx != null)
            {
                var main = electricityVfx.main;
                main.playOnAwake = false;

                var emission = electricityVfx.emission;
                emission.enabled = false;

                if (electricityVfx.isPlaying)
                    electricityVfx.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }

        public override void SetPowered(bool isPowered)
        {
            UpdateVfx(isPowered);
        }

        private void UpdateVfx(bool on)
        {
            if (electricityVfx == null) return;

            var emission = electricityVfx.emission;

            if (on)
            {
                emission.enabled = true;
                if (!electricityVfx.isPlaying) electricityVfx.Play(true);
            }
            else
            {
                emission.enabled = false;
                if (electricityVfx.isPlaying)
                    electricityVfx.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }
    }
}