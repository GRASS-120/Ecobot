using Player.InputManager;
using Player.PlayerCamera;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Player
{
    public class PlayerManager : MonoBehaviour {
        [Title("Components")]
        [SerializeField] private PlayerInputManager inputManager;
        [SerializeField] private PlayerCameraManager cameraManager;

        [Title("Params")]
        [SerializeField] private float moveSpeed = 5f;

        private CharacterController _characterController;
        private Camera _mainCamera;
        private LayerMask _groundMask;
        private Vector3 _aimDir;
        private Vector3 _moveDir;
        private bool _isWalking;
        
        public PlayerInputManager Input => inputManager;

        private void Awake() {
            _characterController = GetComponent<CharacterController>();
            _mainCamera = cameraManager.MainCamera; 
            _groundMask = LayerMask.GetMask(Const.GROUND_LAYER);
        }

        private void Update() {
            HandleMovement();
            HandleRotation();
            cameraManager.HandleCamera();
        }

        private void HandleMovement() {
            var inputDir = inputManager.GetMovementVectorNormalized();
            _moveDir = new Vector3(inputDir.x, 0, inputDir.y);

            _isWalking = _moveDir != Vector3.zero;

            _characterController.Move(_moveDir * (moveSpeed * Time.deltaTime));
        }

        private void HandleRotation()
        {
            if (_moveDir == Vector3.zero) return;
        
            var toRotation = Quaternion.LookRotation(_moveDir);
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

        public Vector3 GetMoveDir()
        {
            return _moveDir;
        }

        public Vector3 GetAimDir()
        {
            return _aimDir;
        }
    }
}
