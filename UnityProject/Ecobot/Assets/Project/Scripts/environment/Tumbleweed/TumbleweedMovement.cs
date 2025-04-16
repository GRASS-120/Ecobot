// TumbleweedMovement.cs
using UnityEngine;

namespace environment.Tumbleweed
{
    public class TumbleweedMovement : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float movementSpeed = 3f;
        [SerializeField] private float rotationSpeed = 100f;

        private Vector3 _targetPosition;
        private bool _isMoving;

        public void StartMovement(Vector3 targetPosition)
        {
            _targetPosition = targetPosition;
            _isMoving = true;
        }
        
        

        private void Update()
        {
            if (!_isMoving) return;

            // Плавное перемещение
            transform.position = Vector3.MoveTowards(
                transform.position,
                _targetPosition,
                movementSpeed * Time.deltaTime
            );

            // Вращение объекта
            transform.Rotate(Vector3.right * (rotationSpeed * Time.deltaTime));

            // Проверка достижения цели
            if (Vector3.Distance(transform.position, _targetPosition) < 0.1f)
            {
                _isMoving = false;
            }
        }

        public bool IsMoving => _isMoving;
    }
}