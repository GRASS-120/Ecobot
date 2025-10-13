using UnityEngine;

namespace Grid.BuildingSystem.Buildings.Visual
{
    public class BuildingWindTurbineVisual : BuildingVisualBase
    {
        [SerializeField] private Transform rotor;
        [SerializeField] private float maxSpinSpeed = 360f;
        [SerializeField] private float spinRampDuration = 2f;

        private float _currentSpinSpeed;
        private Coroutine _spinRoutine;

        public override void Init(BuildingBase building, BuildingContext context)
        {
            base.Init(building, context);
            _currentSpinSpeed = 0f;
            SpinTo(maxSpinSpeed, spinRampDuration);
        }

        public override void OnBroken()
        {
            SpinTo(0f, spinRampDuration);
        }

        public override void OnRepaired()
        {
            SpinTo(maxSpinSpeed, spinRampDuration);
        }

        private void Update()
        {
            if (rotor == null) return;
            if (_currentSpinSpeed <= 0f) return;
            rotor.Rotate(Vector3.up, _currentSpinSpeed * Time.deltaTime, Space.Self);
        }

        private void SpinTo(float targetSpeed, float duration)
        {
            if (_spinRoutine != null) StopCoroutine(_spinRoutine);
            _spinRoutine = StartCoroutine(SpinToRoutine(targetSpeed, duration));
        }

        private System.Collections.IEnumerator SpinToRoutine(float target, float duration)
        {
            float start = _currentSpinSpeed;
            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                float k = Mathf.Clamp01(t / duration);
                _currentSpinSpeed = Mathf.Lerp(start, target, k);
                yield return null;
            }
            _currentSpinSpeed = target;
            _spinRoutine = null;
        }
    }
}