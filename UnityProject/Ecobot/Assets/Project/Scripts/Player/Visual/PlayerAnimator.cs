using UnityEngine;

namespace Player.Visual
{
    public class PlayerAnimator : MonoBehaviour {
        [Header("Params")]
        [SerializeField] private float acceleration = 2f; 
        [SerializeField] private float deceleration = 5f; 

        private PlayerManager _player;
        private Animator _animator;
        private float _velocityZ = 0f;
        private float _velocityX = 0f;
        private float _maxVelocity = 2f;
        private int _velocityZHash;
        private int _velocityXHash;

        private void Awake() {
            _player = GetComponentInParent<PlayerManager>();
            _animator = GetComponent<Animator>();
            _velocityZHash = Animator.StringToHash("Velocity Z");
            _velocityXHash = Animator.StringToHash("Velocity X");
        }

        private void Update() {
            HandleAnimationMovement();
        }

        private void HandleAnimationMovement() {
            bool isMoving = _player.GetMoveDir() != Vector3.zero;

            ChangeVelocity(isMoving);
            LockVelocity(isMoving);

            _animator.SetFloat(_velocityZHash, _velocityZ);
            _animator.SetFloat(_velocityXHash, _velocityX);
        }

        private void ChangeVelocity(bool isMoving) {
            if (isMoving && _velocityZ < _maxVelocity) {
                _velocityZ += Time.deltaTime * acceleration;
            }
            if (!isMoving && _velocityZ > 0f) {
                _velocityZ -= Time.deltaTime * deceleration;
            }
        }

        private void LockVelocity(bool isMoving) {
            if (!isMoving && _velocityZ != 0f && _velocityZ > -0.05f && _velocityZ < 0.05f) {
                _velocityZ = 0f;
            }
        }
    }
}
