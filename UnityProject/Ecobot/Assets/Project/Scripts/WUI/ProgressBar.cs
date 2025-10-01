using System;
using System.Collections;
using System.Threading;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace WUI
{
    public class ProgressBar : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private Image fillImage;
        [SerializeField] private Image backgroundImage; 
        [SerializeField] private Canvas canvas;
        
        [Header("Visual Settings")]
        [SerializeField] private bool animateColor = false;
        [SerializeField] private Gradient colorGradient;
        
        private Coroutine _progressCoroutine;
        private float _duration;
        private bool _isVisible;

        public void Init(float duration)
        {
            _duration = Mathf.Max(0.0001f, duration);
            SetProgress(0f);
            HideProgressBar();
        }

        public void ShowProgressBar()
        {
            _isVisible = true;
            if (canvas != null) canvas.enabled = true;
            SetProgress(0f);
        }

        public void HideProgressBar()
        {
            _isVisible = false;
            if (_progressCoroutine != null)
            {
                StopCoroutine(_progressCoroutine);
                _progressCoroutine = null;
            }
            if (canvas != null) canvas.enabled = false;
        }

        public void StartSingleProgress()
        {
            if (!_isVisible) ShowProgressBar();

            if (_progressCoroutine != null)
            {
                StopCoroutine(_progressCoroutine);
                _progressCoroutine = null;
            }
            _progressCoroutine = StartCoroutine(RunProgress());
        }

        public void CompleteSingleProgress()
        {
            if (_progressCoroutine != null)
            {
                StopCoroutine(_progressCoroutine);
                _progressCoroutine = null;
            }
            
            SetProgress(0f);
        }

        private IEnumerator RunProgress()
        {
            SetProgress(0f);
            float t = 0f;
            while (t < _duration)
            {
                t += Time.deltaTime; 
                SetProgress(t / _duration);
                yield return null;
            }
            SetProgress(1f);
            _progressCoroutine = null;
        }

        private void SetProgress(float progress)
        {
            if (fillImage == null) return;
            progress = Mathf.Clamp01(progress);
            fillImage.type = Image.Type.Filled;
            fillImage.fillAmount = progress;

            if (animateColor && colorGradient != null)
                fillImage.color = colorGradient.Evaluate(progress);
        }
    }
}