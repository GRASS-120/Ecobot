using Cinemachine;
using UnityEngine;

namespace PlayerCamera
{
    public class CameraScript : MonoBehaviour {
        [Header("Entities")]
        [SerializeField] private PlayerInputManager inputManager;
        [SerializeField] private CinemachineVirtualCamera virtualCamera;
        [Header("Params")]
        [SerializeField] private float zoomSpeed = 3f;
        [SerializeField] private float targetFOVMin = 30f;
        [SerializeField] private float targetFOVMax = 80f;

        private float _targetFOV;

        private void Awake() {
            _targetFOV = virtualCamera.m_Lens.FieldOfView;
        }

        private void Update() {
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
