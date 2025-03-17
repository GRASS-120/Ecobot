using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ! blend tree
// 1. возможно нужно будет еще добавить переходные состояние для других направлений (не только из idle вперед)
// 2. разделить на несколько blend tree

// ? как сделать поворот головы и части туловища?
// ? как сделать,чтобы ступни в idle перемещались при повороте?  

// как можно решить проблему с поворотом?
// 1. сделать второй параметр - положение мыши. разделить жкран условно на 4 сектора и по жтому принципу двигать
// (то есть четверти совпадают с самим графиком анимации)
// 2. 


public class PlayerAnimator : MonoBehaviour {
    [Header("Params")]
    [SerializeField] private float acceleration = 2f; 
    [SerializeField] private float deceleration = 5f; 

    private Player _player;
    private Animator _animator;
    private float _velocityZ = 0f;
    private float _velocityX = 0f;
    private float _maxVelocity = 2f;
    private int _velocityZHash;
    private int _velocityXHash;

    void Awake() {
        _player = GetComponentInParent<Player>();
        _animator = GetComponent<Animator>();
        _velocityZHash = Animator.StringToHash("Velocity Z");
        _velocityXHash = Animator.StringToHash("Velocity X");
    }

    void Start() {
    }

    void Update() {
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
