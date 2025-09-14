using UnityEngine;

namespace WUI
{
    public class LookAtCamera : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private bool smoothRotation;
        [SerializeField] private float rotationSpeed = 5f;
        [SerializeField] private bool invertDirection;
        [SerializeField] private bool lockX;
        [SerializeField] private bool lockY;
        [SerializeField] private bool lockZ;
        
        private Transform _transform;
        private Camera _targetCamera;
        
        private void Awake()
        {
            _transform = transform;
            _targetCamera = Camera.main;
        }
        
        private void Update()
        {
            if (_targetCamera == null) return;
            
            LookAt();
        }
        
        private void LookAt()
        {
            Vector3 directionToCamera = invertDirection 
                ? _transform.position - _targetCamera.transform.position 
                : _targetCamera.transform.position - _transform.position;
            
            if (lockX) directionToCamera.x = 0;
            if (lockY) directionToCamera.y = 0;
            if (lockZ) directionToCamera.z = 0;
            
            if (directionToCamera == Vector3.zero) return;
            
            Quaternion targetRotation = Quaternion.LookRotation(directionToCamera);
            
            if (smoothRotation)
            {
                _transform.rotation = Quaternion.Slerp(
                    _transform.rotation, 
                    targetRotation, 
                    rotationSpeed * Time.deltaTime
                );
            }
            else
            {
                _transform.rotation = targetRotation;
            }
        }
        
        public void SetTargetCamera(Camera newCamera)
        {
            _targetCamera = newCamera;
        }
        
        /// <summary>
        /// Принудительно поворачивает к камере (полезно при включении объекта)
        /// </summary>
        public void ForceUpdate()
        {
            if (_targetCamera != null)
            {
                LookAt();
            }
        }
    }
}