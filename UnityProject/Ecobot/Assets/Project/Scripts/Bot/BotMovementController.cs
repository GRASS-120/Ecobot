using System.Collections;
using System.Collections.Generic;
using Grid;
using Grid.Base;
using Grid.PathfindingSystem;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Bot
{
    public class BotMovementController : MonoBehaviour
    {
        [Header("Params")]
        [SerializeField] private float moveSpeed = 5f;

        private BotBase _bot;
        private Pathfinder _pathfinder;
        private BotMovementVisualDebugger _movementVisualDebugger;
        private int _currentPathIndex = 0;
        private List<Vector3> _pathVectorList;

        public void Init(BotBase bot, GridMap gridMap)
        {
            _bot = bot;
            _pathfinder = new Pathfinder(gridMap.Grid);
            _movementVisualDebugger = GetComponent<BotMovementVisualDebugger>();
        }
        
        public IEnumerator Move(Vector3 target)
        {
            SetTargetPosition(target);
            
            if (_pathVectorList == null || _pathVectorList.Count == 0) yield break;

            while (_pathVectorList != null && _pathVectorList.Count > 0)
            {
                _movementVisualDebugger.DrawPath(_pathVectorList);
                Vector3 targetPosition = _pathVectorList[_currentPathIndex];  // берем вейпоинт
                // была проблема странная - бот всегда опускался на Oy = 0 => наполовину был под землей. единственное что помогло:
                targetPosition.y = -0.19f;

                // передвижение по шагам (то есть с одинаковой скоростью)
                float step = moveSpeed * Time.deltaTime;
                _bot.transform.position = Vector3.MoveTowards(_bot.transform.position, targetPosition, step);;
                // проверка того, достиг ли робот вейпоинта
                if (Vector3.Distance(_bot.transform.position, targetPosition) < 0.01f) {
                    _currentPathIndex++;  // след вейпоинт
                    if (_currentPathIndex >= _pathVectorList.Count) {
                        StopMoving();
                        yield break;
                    }
                }

                // поворот бота в сторону ходьбы
                Vector3 direction = (targetPosition - _bot.transform.position).normalized;
                if (direction != Vector3.zero) {
                    Quaternion toRotation = Quaternion.LookRotation(direction, Vector3.up);
                    
                    // так как моделька не в сторону Oz (в unity по умолчанию так, просто модель экспортировали криво видимо),
                    // а в Ox, то из-за этого криво работает перемещение - робот едет правым боком. причем если робота просто
                    // через инспектор повернуть, то это не помогает. только так - через код
                    // умножение кватерниона добавляет угол
                    toRotation *= Quaternion.Euler(0, 90, 0);
                    _bot.transform.rotation = Quaternion.Slerp(_bot.transform.rotation, toRotation, 0.15f);
                }
                
                yield return null;
            }
        }
        
        private void StopMoving() {
            _pathVectorList = null;
        }
        
        private void SetTargetPosition(Vector3 targetPosition) {
            _currentPathIndex = 0;
            _pathVectorList = _pathfinder.FindPath(transform.position, targetPosition);
        
            if (_pathVectorList != null && _pathVectorList.Count > 1) {
                _pathVectorList.RemoveAt(0);  // убираем начальную точку
            }
        }
    }
}