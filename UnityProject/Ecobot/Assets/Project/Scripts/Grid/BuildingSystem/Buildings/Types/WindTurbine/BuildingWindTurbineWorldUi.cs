using System.Collections;
using Grid.BuildingSystem.Buildings.Base;
using R3;
using TMPro;
using UnityEngine;

namespace Grid.BuildingSystem.Buildings.Types.WindTurbine
{
    public class BuildingWindTurbineWorldUi : BuildingWorldUiBase
    {
        [Header("Texts")]
        [SerializeField] private TextMeshPro producedText;    
        [SerializeField] private TextMeshPro consumedText;    
        [SerializeField] private Transform textsRoot;

        [Header("Overload Icon")]
        [SerializeField] private SpriteRenderer overloadIcon;
        [SerializeField] private float blinkPhaseDuration = 1f; // 1 сек: затухание/проявление
        [SerializeField] private float minAlpha = 0.15f;
        [SerializeField] private float maxAlpha = 1f;

        private BuildingWindTurbine _turbine;
        private CompositeDisposable _subs;
        private Coroutine _blinkRoutine;

        public override void Init(BuildingBase building, BuildingContext context)
        {
            base.Init(building, context);
            _turbine = building as BuildingWindTurbine;

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
            StopBlink();
        }

        private void Refresh()
        {
            if (_turbine == null) return;

            bool got = _context.PowerGridService.TryGetNetworkState(
                _turbine, out bool powered, out bool overload, out int totalProd, out int totalCons);

            bool isBroken = _turbine.IsBroken;

            // Перегруз: показываем мигающую иконку, прячем весь блок текстов целиком
            if (got && overload)
            {
                if (textsRoot != null) textsRoot.gameObject.SetActive(false);
                SetOverloadVisible(true);
                StartBlink();
                return;
            }

            // Рабочее состояние: тексты n/m, иконку скрываем
            SetOverloadVisible(false);
            StopBlink();

            if (got && !isBroken && powered)
            {
                if (textsRoot != null) textsRoot.gameObject.SetActive(true);
                if (producedText != null) producedText.text = _turbine.ProducedUnits.ToString();
                if (consumedText != null) consumedText.text = totalCons.ToString();
            }
            else
            {
                if (textsRoot != null) textsRoot.gameObject.SetActive(false);
            }
        }

        private void SetOverloadVisible(bool v)
        {
            if (overloadIcon != null) overloadIcon.gameObject.SetActive(v);
        }

        private void StartBlink()
        {
            if (overloadIcon == null) return;
            if (_blinkRoutine != null) return;
            _blinkRoutine = StartCoroutine(BlinkRoutine());
        }

        private void StopBlink()
        {
            if (_blinkRoutine != null)
            {
                StopCoroutine(_blinkRoutine);
                _blinkRoutine = null;
            }
            if (overloadIcon != null)
            {
                var c = overloadIcon.color;
                c.a = maxAlpha;
                overloadIcon.color = c;
            }
        }

        private IEnumerator BlinkRoutine()
        {
            // цикл: 1 сек тухнет (max->min), 1 сек проявляется (min->max), 1 сек без изменений (max)
            float cycle = blinkPhaseDuration * 3f;

            while (true)
            {
                float t = (Time.time % cycle) / blinkPhaseDuration; // 0..3

                float alpha;
                if (t < 1f)
                {
                    // 0..1: затухание
                    alpha = Mathf.Lerp(maxAlpha, minAlpha, t);
                }
                else if (t < 2f)
                {
                    // 1..2: проявление
                    alpha = Mathf.Lerp(minAlpha, maxAlpha, t - 1f);
                }
                else
                {
                    // 2..3: пауза (без изменений)
                    alpha = maxAlpha;
                }

                if (overloadIcon != null)
                {
                    var c = overloadIcon.color;
                    c.a = alpha;
                    overloadIcon.color = c;
                }

                yield return null;
            }
        }
    }
}