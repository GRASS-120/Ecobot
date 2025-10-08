using System;
using Game;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;

namespace Player.InputManager
{
    public class PlayerInputManager : MonoBehaviour {
        public event Action OnToggleBuildMode;
        public event Action OnRotateBuilding;
        public event Action OnDemountBuilding;
        public event Action OnOpenInventory;
        public event Action OnInteract;
        public event Action OnAltInteract;

        public event Action OnHoldInteraction;
        public event Action OnHoldInteractCanceled;

        [SerializeField] private GameManager gameManager;
    
        private PlayerInputAction _inputActions;
    
        private void Awake()
        {
            _inputActions = new PlayerInputAction();
            _inputActions.GameplayMode.Enable();

            _inputActions.GameplayMode.ToggleBuildMode.performed += ToggleBuildMode_Callback;
            _inputActions.GameplayMode.Inventory.performed += OnOpenInventory_Callback;
            _inputActions.GameplayMode.Interact.performed += OnInteractPerformed_Callback;
            _inputActions.GameplayMode.Interact.canceled += OnInteractCanceled_Callback;
            _inputActions.GameplayMode.AltInteract.performed += OnAltInteractionPerformed_Callback;
            
            _inputActions.BuildingMode.RotateBuilding.performed += RotateBuilding_Callback;
            _inputActions.BuildingMode.DemountBuilding.performed += DemountBuilding_Callback;
        }

        // --- fabric? --- в зависимости от мода отрабатывает перегрузка 
        public void HandleGameplayMap(bool isActive)
        {
            if (isActive) _inputActions.GameplayMode.Enable();
            else _inputActions.GameplayMode.Disable();
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
        // --- fabric? ---

        private void DemountBuilding_Callback(InputAction.CallbackContext obj) {
            OnDemountBuilding?.Invoke();
        }

        private void RotateBuilding_Callback(InputAction.CallbackContext obj) {
            OnRotateBuilding?.Invoke();
        }

        private void OnOpenInventory_Callback(InputAction.CallbackContext obj)
        {
            OnOpenInventory?.Invoke();
        }

        private void ToggleBuildMode_Callback(InputAction.CallbackContext context) {
            OnToggleBuildMode?.Invoke();
        }
        
        private void OnAltInteractionPerformed_Callback(InputAction.CallbackContext context) {
            OnAltInteract?.Invoke();
        }
        
        private void OnInteractPerformed_Callback(InputAction.CallbackContext context)
        {
            switch (context.interaction)
            {
                case PressInteraction:
                    OnInteract?.Invoke();
                    break;
                case HoldInteraction:
                    OnHoldInteraction?.Invoke();
                    break;
            }
        }
        
        // TODO: пока что cancel срабатывает всегда... хз как это исправить
        // идея - можно на разные action раскидать зажатие и нажатие!
        private void OnInteractCanceled_Callback(InputAction.CallbackContext context)
        {
            if (context.interaction is HoldInteraction)
            {
                OnHoldInteractCanceled?.Invoke();
            }
        }

        public Vector2 GetMovementVectorNormalized() {
            Vector2 inputVector = _inputActions.GameplayMode.Movement.ReadValue<Vector2>();
            return inputVector.normalized;
        }

        public Vector2 GetMousePosition() {
            Vector2 inputVector = _inputActions.GameplayMode.MousePosition.ReadValue<Vector2>();
            return inputVector;
        }

        public float GetMouseScroll() {
            Vector2 inputVector = _inputActions.GameplayMode.MouseScroll.ReadValue<Vector2>();
            return inputVector.y;
        }
    }
}
