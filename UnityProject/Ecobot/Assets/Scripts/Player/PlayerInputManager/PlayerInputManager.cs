using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputManager : MonoBehaviour {
    public event EventHandler OnToggleBuildMode;
    public event EventHandler OnRotateBuilding;
    public event EventHandler OnDemountBuilding;

    [SerializeField] private GameManager gameManager;
    
    private PlayerInputAction _inputActions;
    private GameManager.Mode _currentMode = GameManager.Mode.Default;

    private void Awake()
    {
        _inputActions = new PlayerInputAction();
        _inputActions.DefaultMode.Enable();

        _inputActions.DefaultMode.ToggleBuildMode.performed += ToggleBuildModeOnperoformed;
        
        _inputActions.BuildingMode.RotateBuilding.performed += RotateBuildingOnperformed;
        _inputActions.BuildingMode.DemountBuilding.performed += DemountBuildingOnperformed;
    }

    private void Start()
    {
        gameManager.OnModeChanged += HandleCallbacks;
    }

    // ? переписать на паттерн state? либо похуй? все равно будет немного карт (+ не всегда одна карта исключает другю, пример - Default и Building
    private void HandleCallbacks(GameManager.Mode currentMode)
    {
        _currentMode = currentMode;

        switch (_currentMode)
        {
            default:
            case GameManager.Mode.Default:
                _inputActions.DefaultMode.Enable();
                
                _inputActions.InterfaceMode.Disable();
                _inputActions.BuildingMode.Disable();
                _inputActions.ProgrammingMode.Disable();
                _inputActions.MenuMode.Disable();
                break;
            case GameManager.Mode.Building:
                _inputActions.BuildingMode.Enable();
                
                _inputActions.InterfaceMode.Disable();
                _inputActions.ProgrammingMode.Disable();
                _inputActions.MenuMode.Disable();
                break;
            case GameManager.Mode.Inventory:
            case GameManager.Mode.Interface:
                _inputActions.InterfaceMode.Enable();
                
                _inputActions.BuildingMode.Disable();
                _inputActions.ProgrammingMode.Disable();
                _inputActions.MenuMode.Disable();
                break;
            case GameManager.Mode.Programming:
                _inputActions.ProgrammingMode.Enable();
                
                _inputActions.DefaultMode.Disable();
                _inputActions.BuildingMode.Disable();
                break;
            case GameManager.Mode.Menu:
                _inputActions.MenuMode.Enable();
                
                
                _inputActions.InterfaceMode.Disable();
                _inputActions.ProgrammingMode.Disable();
                _inputActions.DefaultMode.Disable();
                _inputActions.BuildingMode.Disable();
                break;
        }
    }

    private void DemountBuildingOnperformed(InputAction.CallbackContext obj) {
        OnDemountBuilding?.Invoke(this, EventArgs.Empty);
    }

    private void RotateBuildingOnperformed(InputAction.CallbackContext obj) {
        OnRotateBuilding?.Invoke(this, EventArgs.Empty);
    }

    private void ToggleBuildModeOnperoformed(InputAction.CallbackContext context) {
        OnToggleBuildMode?.Invoke(this, EventArgs.Empty);
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
