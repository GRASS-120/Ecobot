using UnityEngine;

namespace environment
{
    public class WildStalker : MonoBehaviour
    {
        [Header("Настройки следования")]
        [SerializeField] private Transform playerTransform;   // Цель (игрок)
        [SerializeField] private Vector3 positionOffset;      // Отступ по осям (X, Y, Z)
        [SerializeField] private float followSpeed = 5f;      // Скорость следования

        private Vector3 _targetPosition;

        void Update()
        {
            if (playerTransform == null) return;

            // Вычисляем целевую позицию с отступом
            _targetPosition = playerTransform.position + positionOffset;

            // Плавное перемещение к цели
            transform.position = Vector3.Lerp(
                transform.position,
                _targetPosition,
                followSpeed * Time.deltaTime
            );
        }
    }
}