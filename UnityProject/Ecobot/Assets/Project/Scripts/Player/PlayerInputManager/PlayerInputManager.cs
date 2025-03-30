using System;
using System.Collections;
using System.Collections.Generic;
using Game;
using UnityEngine;
using UnityEngine.InputSystem;

// сделать моды через классы. 
// gameplay - базовый класс. управление гироком. -> building - наследуется от gameplay, добавляет инпуты для управления строительством
// menu - азовый класс для интерфейса - меню и тп. блокирует gameplay -> programming - добавляет инпуты для visual programming

public class PlayerInputManager : MonoBehaviour {
    public event Action OnToggleBuildMode;
    public event Action OnRotateBuilding;
    public event Action OnDemountBuilding;

    [SerializeField] private GameManager gameManager;
    
    private PlayerInputAction _inputActions;
    public PlayerInputAction InputActions => _inputActions;
    
    // private GameManager.Mode _currentMode = GameManager.Mode.Default;

    private void Awake()
    {
        _inputActions = new PlayerInputAction();
        _inputActions.DefaultMode.Enable();

        _inputActions.DefaultMode.ToggleBuildMode.performed += ToggleBuildMode_Callback;
        
        _inputActions.BuildingMode.RotateBuilding.performed += RotateBuilding_Callback;
        _inputActions.BuildingMode.DemountBuilding.performed += DemountBuilding_Callback;
    }

    public void HandleGameplayMap(bool isActive)
    {
        if (isActive) _inputActions.DefaultMode.Enable();
        else _inputActions.DefaultMode.Disable();
    }
    
    public void HandleBuildingMap(bool isActive)
    {
        if (isActive) _inputActions.BuildingMode.Enable();
        else _inputActions.BuildingMode.Disable();
    }
    
    public void HandleMenuMap(bool isActive)
    {
        if (isActive) _inputActions.MenuMode.Enable();
        else _inputActions.MenuMode.Disable();
    }
    
    public void HandleProgrammingMap(bool isActive)
    {
        if (isActive) _inputActions.ProgrammingMode.Enable();
        else _inputActions.ProgrammingMode.Disable();
    }
    
    private void Start()
    {
        // gameManager.OnModeChanged += HandleCallbacks;
    }

    private void DemountBuilding_Callback(InputAction.CallbackContext obj) {
        OnDemountBuilding?.Invoke();
    }

    private void RotateBuilding_Callback(InputAction.CallbackContext obj) {
        OnRotateBuilding?.Invoke();
    }

    private void ToggleBuildMode_Callback(InputAction.CallbackContext context) {
        // toggle input
        OnToggleBuildMode?.Invoke();
    }

    public Vector2 GetMovementVectorNormalized() {
        Vector2 inputVector = _inputActions.DefaultMode.Movement.ReadValue<Vector2>();
        return inputVector.normalized;
    }

    public Vector2 GetMousePosition() {
        Vector2 inputVector = _inputActions.DefaultMode.MousePosition.ReadValue<Vector2>();
        return inputVector;
    }

    public float GetMouseScroll() {
        Vector2 inputVector = _inputActions.DefaultMode.MouseScroll.ReadValue<Vector2>();
        return inputVector.y;
    }
}
