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


public class GuidePlayerAnimInputSystem : MonoBehaviour {
    [Header("Params")]
    [SerializeField] private float acceleration = 2f; // 1
    [SerializeField] private float deceleration = 10f; // 5

    private Player _player;
    private Animator _animator;
    private float _velocityZ = 0f;
    private float _velocityX = 0f;
    private int _velocityZHash;
    private int _velocityXHash;

    private float _maxVelocity = 2f;


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
        Debug.Log($"Z:{_velocityZ}, X: {_velocityX}");
    }

    private void HandleAnimationMovement() {
        bool forward = _player.GetMoveDir().z > 0;
        bool left = _player.GetMoveDir().x < 0;
        bool bottom = _player.GetMoveDir().z < 0;
        bool right = _player.GetMoveDir().x > 0;

        ChangeVelocity(forward, left, bottom, right);
        LockVelocity(forward, left, bottom, right);

        _animator.SetFloat(_velocityZHash, _velocityZ);
        _animator.SetFloat(_velocityXHash, _velocityX);
    }

    private void ChangeVelocity(bool f, bool l, bool b, bool r) {
        // +++ двигаемся
        if (f && _velocityZ < _maxVelocity) {
            _velocityZ += Time.deltaTime * acceleration;
        }

        if (b && _velocityZ > -_maxVelocity) {
            _velocityZ -= Time.deltaTime * acceleration;
        }

        if (l && _velocityX > -_maxVelocity) {
            _velocityX -= Time.deltaTime * acceleration;
        }

        if (r && _velocityX < _maxVelocity) {
            _velocityX += Time.deltaTime * acceleration;
        }

        // --- останавливаемся
        if (!f && _velocityZ > 0f) {
            _velocityZ -= Time.deltaTime * deceleration;
        }

        if (!b && _velocityZ < 0f) {
            _velocityZ += Time.deltaTime * deceleration;
        }

        if (!l && _velocityX < 0f) {
            _velocityX += Time.deltaTime * deceleration;
        }

        if (!r && _velocityX > 0f) {
            _velocityX -= Time.deltaTime * deceleration;
        }
    }

    private void LockVelocity(bool f, bool l, bool b, bool r) {
        if (!f && !b && _velocityZ != 0f && _velocityZ > -0.03f && _velocityZ < 0.03f) {
            _velocityZ = 0f;
        }
        if (!l && !r && _velocityX != 0f && _velocityX > -0.03f && _velocityX < 0.03f) {
            _velocityX = 0f;
        }
    }
}
