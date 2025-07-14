using UnityEngine;

namespace environment
{
    public class WindManager : MonoBehaviour
    {
        [Header("Настройки ветра")]
        [SerializeField] private float _windAngle = 0f;
        [SerializeField] private float _rotationSpeed = 50f;

        public float CurrentWindAngle => _windAngle;
        
        private void Update()
        {
            HandleInput();
            NormalizeAngle();
        }

        private void HandleInput()
        {
            if (Input.GetKey(KeyCode.LeftArrow))
                _windAngle -= _rotationSpeed * Time.deltaTime;

            if (Input.GetKey(KeyCode.RightArrow))
                _windAngle += _rotationSpeed * Time.deltaTime;
        }

        private void NormalizeAngle()
        {
            _windAngle = Mathf.Repeat(_windAngle, 360);
        }
        
        public void SetWindAngle(float newAngle)
        {
            _windAngle = newAngle;
            NormalizeAngle();
        }
    }
}