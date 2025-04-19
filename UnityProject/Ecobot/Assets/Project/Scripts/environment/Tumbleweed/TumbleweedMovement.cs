using UnityEngine;
namespace environment.Tumbleweed
{
    public class TumbleweedMovement : MonoBehaviour
    {
        [Header("Base Movement Settings")]
        [SerializeField] private float baseMovementSpeed = 3f;
        [SerializeField] private float baseRotationSpeed = 100f;
        [SerializeField] private float baseJumpAmplitude = 0.5f;
        [SerializeField] private float baseJumpFrequency = 10f;

        [Header("Randomization Settings")]
        [SerializeField] private float speedVariation = 1f;
        [SerializeField] private float rotationVariation = 30f;
        [SerializeField] private float amplitudeVariation = 0.2f;
        [SerializeField] private float frequencyVariation = 2f;

        private Vector3 _targetPosition;
        private bool _isMoving;
        private float _initialYPosition;
        private Vector3 _startPosition;
        private float _progress;
        
        // Randomized parameters for current movement
        private float _currentMovementSpeed;
        private float _currentRotationSpeed;
        private float _currentJumpAmplitude;
        private float _currentJumpFrequency;

        private void Awake()
        {
            // Initialize base values
            _currentMovementSpeed = baseMovementSpeed;
            _currentRotationSpeed = baseRotationSpeed;
            _currentJumpAmplitude = baseJumpAmplitude;
            _currentJumpFrequency = baseJumpFrequency;
        }

        public void StartMovement(Vector3 targetPosition, float windForce)
        {
            // Randomize parameters for this movement
            RandomizeParameters(windForce);
            
            _targetPosition = targetPosition;
            _isMoving = true;
            _startPosition = transform.position;
            _initialYPosition = _startPosition.y;
            _progress = 0f;
        }

        private void RandomizeParameters(float windForce)
        {
            _currentMovementSpeed = Mathf.Max(0.1f, 
                baseMovementSpeed + Random.Range(-speedVariation, speedVariation)) * windForce;
            
            _currentRotationSpeed = Mathf.Max(0f, 
                baseRotationSpeed + Random.Range(-rotationVariation, rotationVariation));
            
            _currentJumpAmplitude = Mathf.Max(0f, 
                baseJumpAmplitude + Random.Range(-amplitudeVariation, amplitudeVariation));
            
            _currentJumpFrequency = Mathf.Max(0.1f, 
                baseJumpFrequency + Random.Range(-frequencyVariation, frequencyVariation));
        }

        private void Update()
        {
            if (!_isMoving) return;

            // Обновляем прогресс движения (0-1)
            _progress += _currentMovementSpeed * Time.deltaTime / 
                       Vector3.Distance(_startPosition, _targetPosition);
            _progress = Mathf.Clamp01(_progress);

            // Базовое перемещение
            Vector3 newPosition = Vector3.Lerp(_startPosition, _targetPosition, _progress);

            // Добавляем подпрыгивание
            float yOffset = Mathf.Sin(_progress * Mathf.PI * _currentJumpFrequency) * _currentJumpAmplitude;
            newPosition = new Vector3(
                newPosition.x, 
                Mathf.Max(_initialYPosition, _initialYPosition + yOffset), 
                newPosition.z
            );

            transform.position = newPosition;

            // Вращение с рандомизированной скоростью
            transform.Rotate(Vector3.up, _currentRotationSpeed * Time.deltaTime);

            // Проверка достижения цели
            if (Vector3.Distance(
                new Vector3(transform.position.x, 0, transform.position.z), 
                new Vector3(_targetPosition.x, 0, _targetPosition.z)) < 0.1f)
            {
                _isMoving = false;
            }
        }

        public bool IsMoving => _isMoving;
    }
}