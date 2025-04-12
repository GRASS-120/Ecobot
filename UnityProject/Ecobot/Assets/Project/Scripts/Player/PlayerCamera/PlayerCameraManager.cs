using Cinemachine;
using Player.InputManager;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace Player.PlayerCamera
{
    public class PlayerCameraManager : MonoBehaviour {
        [Title("Components")]
        [SerializeField] private PlayerInputManager inputManager;
        [SerializeField] private Camera mainCamera;
        [SerializeField] private CinemachineVirtualCamera virtualCamera;
        
        [Title("Params")]
        [SerializeField] private float zoomSpeed = 3f;
        [SerializeField] private float targetFOVMin = 30f;
        [SerializeField] private float targetFOVMax = 80f;
        
        public Camera MainCamera => mainCamera;
        public CinemachineVirtualCamera VirtualCamera => virtualCamera;

        private float _targetFOV;

        private void Awake() {
            _targetFOV = virtualCamera.m_Lens.FieldOfView;
        }

        public void HandleCamera()
        {
            HandleCameraZoom();
        }

        private void HandleCameraZoom() {
            float mouseScrollValue = inputManager.GetMouseScroll();  // wheel down -> (-), wheel up -> (+)
            float mouseScrollSign = Mathf.Sign(mouseScrollValue); 
            float zoomIncrement = 10f;

            if (mouseScrollValue != 0) {  // no wheel movement -> 0
                _targetFOV -= mouseScrollSign * zoomIncrement;  // mouseScrollDelta = -1 / 0 / 1. -=, так как inputManager.GetMouseScroll() такой
                _targetFOV = Mathf.Clamp(_targetFOV, targetFOVMin, targetFOVMax);
            }

            virtualCamera.m_Lens.FieldOfView = Mathf.Lerp(virtualCamera.m_Lens.FieldOfView, _targetFOV, Time.deltaTime * zoomSpeed);
        }
    }
}
