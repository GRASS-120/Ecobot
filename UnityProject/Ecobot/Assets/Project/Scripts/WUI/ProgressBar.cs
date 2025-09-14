using System;
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
        [SerializeField] private Image backgroundImage; // опционально
        [SerializeField] private Canvas canvas;
        
        // [Header("Optional Components")]
        // [SerializeField] private LookAtCamera lookAtCamera;
        
        [Header("Visual Settings")]
        [SerializeField] private Color fillColor = Color.green;
        [SerializeField] private Color backgroundColor = Color.gray;
        [SerializeField] private bool animateColor = false;
        [SerializeField] private Gradient colorGradient;
        
        private IDisposable _progressSubscription;
        private float _duration;
        private bool _isVisible = false;        
        private void Awake()
        {
            ValidateComponents();
            SetupVisuals();
            HideProgressBar();
        }
        
        private void ValidateComponents()
        {
            // if (lookAtCamera == null)
            //     lookAtCamera = GetComponent<LookAtCamera>();
            
            if (fillImage != null && fillImage.type != Image.Type.Filled)
            {
                Debug.LogWarning($"Fill Image на {gameObject.name} должен иметь тип 'Filled'");
                fillImage.type = Image.Type.Filled;
            }
        }
        
        private void SetupVisuals()
        {
            if (fillImage != null)
            {
                fillImage.color = fillColor;
                fillImage.fillAmount = 0f;
                fillImage.type = Image.Type.Filled;
                // fillImage.fillMethod = fillMethod;
                // fillImage.fillOrigin = fillOrigin;
                // fillImage.fillClockwise = clockwise;
            }
            
            if (backgroundImage != null)
            {
                backgroundImage.color = backgroundColor;
            }
        }
        
        public void Init(float duration)
        {
            _duration = duration;
            SetProgress(0f);
        }
        
        /// <summary>
        /// Показывает прогресс бар (вызывается один раз в начале добычи)
        /// </summary>
        public void ShowProgressBar()
        {
            if (_isVisible) return;
            
            _isVisible = true;
            canvas.enabled = true;
            SetProgress(0f);
            // lookAtCamera?.ForceUpdate();
            
            Debug.Log("Progress bar показан");
        }
        
        /// <summary>
        /// Скрывает прогресс бар (вызывается когда добыча полностью завершена)
        /// </summary>
        public void HideProgressBar()
        {
            _isVisible = false;
            _progressSubscription?.Dispose();
            canvas.enabled = false;
            
            Debug.Log("Progress bar скрыт");
        }
        
        /// <summary>
        /// Запускает прогресс для одной единицы добычи
        /// </summary>
        public void StartSingleProgress()
        {
            if (!_isVisible) return;
            
            SetProgress(0f);
            
            _progressSubscription?.Dispose();
            _progressSubscription = Observable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(0.016f))
                .TakeWhile(_ => fillImage != null && fillImage.fillAmount < 1f)
                .Subscribe(_ =>
                {
                    var newProgress = fillImage.fillAmount + (0.016f / _duration);
                    SetProgress(newProgress);
                })
                .AddTo(this);
                
            Debug.Log("Запущен прогресс единицы добычи");
        }
        
        /// <summary>
        /// Завершает прогресс одной единицы (сбрасывает до 0)
        /// </summary>
        public void CompleteSingleProgress()
        {
            _progressSubscription?.Dispose();
            SetProgress(0f); // Сбрасываем прогресс к 0 для следующей единицы
            
            Debug.Log("Завершен прогресс единицы добычи");
        }
        
        /// <summary>
        /// Обновляет прогресс напрямую (для внешнего управления)
        /// </summary>
        public void UpdateProgress(float progress)
        {
            if (_isVisible)
            {
                SetProgress(progress);
            }
        }
        
        private void SetProgress(float progress)
        {
            if (fillImage == null) return;
            
            progress = Mathf.Clamp01(progress);
            fillImage.fillAmount = progress;
            
            // Анимация цвета если включена
            if (animateColor && colorGradient != null)
            {
                fillImage.color = colorGradient.Evaluate(progress);
            }
        }
        
        public void SetFillMethod(Image.FillMethod method, int origin = 0, bool isClockwise = true)
        {
            if (fillImage != null)
            {
                fillImage.fillMethod = method;
                fillImage.fillOrigin = origin;
                fillImage.fillClockwise = isClockwise;
            }
        }
        
        public void SetColors(Color fill, Color background)
        {
            fillColor = fill;
            backgroundColor = background;
            
            if (fillImage != null)
                fillImage.color = fillColor;
                
            if (backgroundImage != null)
                backgroundImage.color = backgroundColor;
        }
        
        private void OnDestroy()
        {
            _progressSubscription?.Dispose();
        }
    }
}