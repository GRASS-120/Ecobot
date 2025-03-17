using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Cinemachine.Utility;
using Grid;
using UnityEngine;

// пока бот не может открывать инвентарь так как он открывается только для nteractable, а bot - entity.
public class Bot : MonoBehaviour {
    [Header("Entities")]
    [SerializeField] private GridMap _gridMap;

    [Header("Params")]
    [SerializeField] private float _moveSpeed = 15f;

    private Pathfinding _pathfinder;
    private int _currentPathIndex = 0;
    private List<Vector3> _pathVectorList;

    void Awake() {
        _pathfinder = new Pathfinding(_gridMap.Grid);
    }

    void Update() {
        HandleMovement();
    }
    
    // gizmos отображается только на сцене - в игре нет. то есть он используется для проверки до запуска
    void OnDrawGizmos() {
        // Gizmos.DrawSphere(transform.position, 5f);
    }

    private void HandleMovement() {
        if (_pathVectorList != null && _pathVectorList.Count > 0) {
            DrawPath();
            Vector3 targetPosition = _pathVectorList[_currentPathIndex];  // берем вейпоинт
            // была проблема странная - бот всегда опускался на Oy = 0 => наполовину был под землей. единственное что помогло:
            targetPosition.y = -0.19f;

            // передвижение по шагам (то есть с одинаковой скоростью)
            float step = _moveSpeed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, step);;
            // проверка того, достиг ли робот вейпоинта
            if (Vector3.Distance(transform.position, targetPosition) < 0.01f) {
                _currentPathIndex++;  // след вейпоинт
                if (_currentPathIndex >= _pathVectorList.Count) {
                    StopMoving();
                }
            }

            // поворот бота в сторону ходьбы
            Vector3 direction = (targetPosition - transform.position).normalized;
            if (direction != Vector3.zero) {
                Quaternion toRotation = Quaternion.LookRotation(direction, Vector3.up);
                // так как моделька не в сторону Oz (в unity по умолчанию так, просто модель экспортировали криво видимо),
                // а в Ox, то из-за этого криво работает перемещение - робот едет правым боком. причем если робота просто
                // через инспектор повернуть, то это не помогает. только так - через код
                // умножение кватерниона добавляет угол
                toRotation *= Quaternion.Euler(0, 90, 0);
                transform.rotation = Quaternion.Slerp(transform.rotation, toRotation, 0.15f);
            }
        }
    }

    private void DrawPath() {
        if (_pathVectorList != null) {
            for (int i = 0; i < _pathVectorList.Count - 1; i ++) {
                Vector3 point1 = new Vector3(_pathVectorList[i].x, 0.1f, _pathVectorList[i].z);
                Vector3 point2 = new Vector3(_pathVectorList[i+1].x, 0.1f, _pathVectorList[i+1].z);
                Debug.DrawLine(point1, point2, Color.red);
            }
        }
    }

    private void StopMoving() {
        _pathVectorList = null;
    }

    public void SetTargetPosition(Vector3 targetPosition) {
        _currentPathIndex = 0;
        _pathVectorList = _pathfinder.FindPath(transform.position, targetPosition);

        if (_pathVectorList != null && _pathVectorList.Count > 1) {
            _pathVectorList.RemoveAt(0);  // убираем начальную точку
        }
    }
}
