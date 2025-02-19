using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour {
    [Header("Entities")]
    [SerializeField] private PlayerInputManager inputManager;

    [Header("Params")]
    [SerializeField] private float moveSpeed = 5f;

    private CharacterController _characterController;
    private Camera _mainCamera;
    private LayerMask _groundMask;
    private Vector3 _aimDir;
    private Vector3 _moveDir;
    private bool _isWalking;

    private void Awake() {
        _characterController = GetComponent<CharacterController>();
        _mainCamera = Camera.main;
        _groundMask = LayerMask.GetMask(Const.GROUND_LAYER);
    }

    private void Update() {
        HandleMovement();
        HandleRotation();
    }

    private void HandleMovement() {
        Vector2 inputDir = inputManager.GetMovementVectorNormalized();
        _moveDir = new Vector3(inputDir.x, 0, inputDir.y);

        _isWalking = _moveDir != Vector3.zero;

        _characterController.Move(_moveDir * moveSpeed * Time.deltaTime);
    }

    private void HandleRotation()
    {
        if (_moveDir == Vector3.zero) return;
        
        Quaternion toRotation = Quaternion.LookRotation(_moveDir);
        transform.rotation = Quaternion.Slerp(transform.rotation, toRotation, 0.15f);
    }

    public (bool success, Vector3 position) GetMouseRaycast()
    {
        Vector2 mousePosition = inputManager.GetMousePosition();
        Ray ray = _mainCamera.ScreenPointToRay(mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hitInfo, Mathf.Infinity, _groundMask)) {
            return (success: true, position: hitInfo.point);
        } else {
            return (success: false, position: Vector3.zero);
        }
    }

    public bool IsWalking()
    {
        return _isWalking;
    }

    public Vector3 GetMoveDir()
    {
        return _moveDir;
    }

    public Vector3 GetAimDir()
    {
        return _aimDir;
    }
}
