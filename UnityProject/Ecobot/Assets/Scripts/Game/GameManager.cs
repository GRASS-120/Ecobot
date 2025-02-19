using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {
    public event Action<Mode> OnModeChanged;

    [SerializeField] private PlayerInputManager inputManager;

    public Mode CurrentMode => _currentMode;

    public enum Mode {
        Default,
        Building,
        Inventory,
        Interface,
        Programming,
        Menu
    }

    private Mode _currentMode = Mode.Default;

    private void Start()
    {
        inputManager.OnToggleBuildMode += OnToggleBuildMode_Callback;
    }

    private void OnToggleBuildMode_Callback(object sender, EventArgs e)
    {
        _currentMode = _currentMode == Mode.Default ? Mode.Building : Mode.Default;
        OnModeChanged?.Invoke(_currentMode);
    }

    public void ChangeMode(Mode mode)
    {
        _currentMode = mode;
        OnModeChanged?.Invoke(_currentMode);
    }
}