using UnityEngine;
using DG.Tweening;

namespace environment.Tumbleweed
{
    public class TumbleweedMovement : MonoBehaviour
    {
        [Header("Base Movement Settings")]
        [SerializeField] private float baseMovementSpeed = 3f;
        [SerializeField] private float baseRotationSpeed = 100f;
        [SerializeField] private float baseJumpAmplitude = 0.5f;
        [SerializeField] private float baseJumpFrequency = 2f;

        [Header("Randomization Settings")]
        [SerializeField] private float speedVariation = 1f;
        [SerializeField] private float rotationVariation = 30f;
        [SerializeField] private float amplitudeVariation = 0.2f;
        [SerializeField] private float frequencyVariation = 1f;

        private Vector3 _targetPosition;
        private float _initialYPosition;
        private Tween _moveTween;
        private Tween _rotateTween;
        private Tween _jumpTween;
        private float _calculatedLifetime;
        
        public bool IsMoving { get; private set; }
        public System.Action OnMovementComplete;

        private void OnDestroy()
        {
            _moveTween?.Kill();
            _rotateTween?.Kill();
            _jumpTween?.Kill();
        }

        public void StartMovement(Vector3 targetPosition, float windForce)
        {
            IsMoving = true;
            RandomizeParameters(windForce);
            _targetPosition = targetPosition;
            _initialYPosition = transform.position.y;

            SetupMovement();
            SetupRandomRotation();
            SetupJumpAnimation();
        }

        public void FullReset()
        {
            StopMovement();
            transform.rotation = Quaternion.identity;
            _initialYPosition = transform.position.y;
        }

        // Модифицируем метод остановки
        public void StopMovement()
        {
            _moveTween?.Kill();
            _rotateTween?.Kill();
            _jumpTween?.Kill();
            IsMoving = false;
        
            // Сбрасываем параметры анимации
            DOTween.Kill(transform);
            transform.DOKill();
        }

        private void RandomizeParameters(float windForce)
        {
            baseMovementSpeed = Mathf.Max(0.1f, 
                baseMovementSpeed + Random.Range(-speedVariation, speedVariation)) * windForce;

            baseRotationSpeed = Mathf.Max(10f, 
                baseRotationSpeed + Random.Range(-rotationVariation, rotationVariation));

            baseJumpAmplitude = Mathf.Max(0.1f, 
                baseJumpAmplitude + Random.Range(-amplitudeVariation, amplitudeVariation));

            baseJumpFrequency = Mathf.Max(0.5f, 
                baseJumpFrequency + Random.Range(-frequencyVariation, frequencyVariation));
        }

        private void SetupMovement()
        {
            float distance = Vector3.Distance(transform.position, _targetPosition);
            _calculatedLifetime = Mathf.Max(0.1f, distance / baseMovementSpeed);

            _moveTween = transform.DOMove(_targetPosition, _calculatedLifetime)
                .SetEase(Ease.Linear)
                .OnUpdate(() => {
                    // Защита от NaN значений
                    if (float.IsNaN(_initialYPosition)) 
                        _initialYPosition = transform.position.y;
                
                    MaintainYPosition();
                })
                .OnComplete(() => {
                    IsMoving = false;
                    OnMovementComplete?.Invoke();
                })
                .OnKill(() => IsMoving = false); // Защита при принудительной остановке
        }

        private void MaintainYPosition()
        {
            transform.position = new Vector3(
                transform.position.x,
                _initialYPosition,
                transform.position.z
            );
        }

        private void SetupRandomRotation()
        {
            int rotationDirection = Random.Range(0, 2) * 2 - 1;
            
            _rotateTween = transform.DORotate(
                new Vector3(0, 360 * rotationDirection, 0),
                1f / (baseRotationSpeed/360f), 
                RotateMode.FastBeyond360
            ).SetLoops(-1, LoopType.Restart)
             .SetEase(Ease.Linear);
        }

        private void SetupJumpAnimation()
        {
            _jumpTween = transform.DOMoveY(
                _initialYPosition + baseJumpAmplitude, 
                1f / baseJumpFrequency
            ).SetLoops(-1, LoopType.Yoyo)
             .SetEase(Ease.InOutSine);
        }
    }
}
