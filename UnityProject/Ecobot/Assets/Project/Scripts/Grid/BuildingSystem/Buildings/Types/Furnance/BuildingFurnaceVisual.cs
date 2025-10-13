using System.Collections;
using System.Collections.Generic;
using Grid.BuildingSystem.Buildings.Base;
using UnityEngine;

namespace Grid.BuildingSystem.Buildings.Visual
{
    public class BuildingFurnaceVisual : BuildingVisualBase
    {
        [SerializeField] private ParticleSystem smokeVfx;
        [SerializeField] private float fadeInDuration = 0.8f;

        private float _baseRate;
        private Coroutine _fadeRoutine;
        
        public override void Init(BuildingBase building, BuildingContext context)
        {
            base.Init(building, context);
            if (smokeVfx != null)
            {
                var main = smokeVfx.main;
                main.playOnAwake = false;

                var emission = smokeVfx.emission;
                _baseRate = emission.rateOverTime.constant; // запоминаем целевой дебит
                SetEmissionRate(0f);
                emission.enabled = false;
                if (smokeVfx.isPlaying) smokeVfx.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }

        public override void SetPowered(bool isPowered)
        {
            if (smokeVfx == null) return;

            if (_fadeRoutine != null)
            {
                StopCoroutine(_fadeRoutine);
                _fadeRoutine = null;
            }

            var emission = smokeVfx.emission;

            if (isPowered)
            {
                // плавный fade-in
                emission.enabled = true;
                if (!smokeVfx.isPlaying) smokeVfx.Play(true);
                _fadeRoutine = StartCoroutine(FadeEmission(0f, _baseRate, fadeInDuration));
            }
            else
            {
                // естественный fade-out за счёт lifeTime частиц
                emission.enabled = false;
                if (smokeVfx.isPlaying)
                    smokeVfx.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                SetEmissionRate(0f);
            }
        }
        
        private void SetEmissionRate(float value)
        {
            var emission = smokeVfx.emission;
            var curve = emission.rateOverTime;
            curve.mode = ParticleSystemCurveMode.Constant;
            curve.constant = Mathf.Max(0f, value);
            emission.rateOverTime = curve;
        }

        private IEnumerator FadeEmission(float from, float to, float duration)
        {
            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                float k = Mathf.Clamp01(t / duration);
                SetEmissionRate(Mathf.Lerp(from, to, k));
                yield return null;
            }
            SetEmissionRate(to);
            _fadeRoutine = null;
        }
    }
}