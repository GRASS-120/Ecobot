using UnityEngine;

namespace environment
{
    public class WildStalker : MonoBehaviour
    {
        [Header("Основные настройки")]
        [SerializeField] private Transform playerTransform;
        // Целевой объект для следования
        [SerializeField] private float followSpeed = 5f;       
        // Скорость перемещения
        [SerializeField] private float radius = 3f;            
        // Радиус орбиты
        
        [Header("Поворот объекта")]
        [SerializeField] private Vector3 rotationOffset;       
        // Ручная настройка угла (в градусах)
        
        [Header("Ссылка на менеджер")]
        [SerializeField] private WindManager windManager;       
        // Контроллер угла орбиты

        private void Update()
        {
            if (!ValidateComponents()) return;
            UpdatePositionAndRotation();
        }

        private bool ValidateComponents()
        {
            if (playerTransform == null || windManager == null)
            {
                Debug.LogError("Не назначены Player или WindManager!");
                return false;
            }
            return true;
        }

        private void UpdatePositionAndRotation()
        {
            // Рассчет позиции на орбите
            float angle = windManager.CurrentWindAngle * Mathf.Deg2Rad;
            Vector3 orbitPosition = new Vector3(
                Mathf.Cos(angle) * radius,
                0,
                Mathf.Sin(angle) * radius
            );
            
            // Плавное перемещение
            transform.position = Vector3.Lerp(
                transform.position,
                playerTransform.position + orbitPosition,
                followSpeed * Time.deltaTime
            );

            // Поворот объекта
            transform.rotation = Quaternion.Euler(rotationOffset) * 
                               Quaternion.LookRotation(
                                   playerTransform.position - transform.position
                               );
        }
    }
}