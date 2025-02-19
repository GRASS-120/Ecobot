using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class CameraScript : MonoBehaviour {
    [Header("Entities")]
    [SerializeField] private PlayerInputManager inputManager;
    [SerializeField] private CinemachineVirtualCamera virtualCamera;

    [Header("Params")]
    [SerializeField] private float zoomSpeed = 3f;
    [SerializeField] private float targetFOVMin = 30f;
    [SerializeField] private float targetFOVMax = 80f;

    private float targetFOV;

    void Awake() {
        targetFOV = virtualCamera.m_Lens.FieldOfView;
    }

    void Update() {
        HandleCameraZoom();
    }

    public void HandleCameraZoom() {
        float mouseScrollValue = inputManager.GetMouseScroll();  // wheel down -> (-), wheel up -> (+)
        float mouseScrollSign = Mathf.Sign(mouseScrollValue); 
        float zoomIncrement = 10f;

        if (mouseScrollValue != 0) {  // no wheel movement -> 0
            targetFOV -= mouseScrollSign * zoomIncrement;  // mouseScrollDelta = -1 / 0 / 1. -=, так как inputManager.GetMouseScroll() такой
            targetFOV = Mathf.Clamp(targetFOV, targetFOVMin, targetFOVMax);
        }

        virtualCamera.m_Lens.FieldOfView = Mathf.Lerp(virtualCamera.m_Lens.FieldOfView, targetFOV, Time.deltaTime * zoomSpeed);
    }
}
