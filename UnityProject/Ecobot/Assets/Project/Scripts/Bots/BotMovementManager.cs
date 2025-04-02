using System.Collections;
using UnityEngine;

namespace Bots
{
    public class BotMovementManager : MonoBehaviour
    {
        // private IEnumerator Move()
        // {
        //     if (_pathVectorList != null && _pathVectorList.Count > 0) {
        //         // _visualDebugger.DrawPath(_pathVectorList);
        //         Vector3 targetPosition = _pathVectorList[_currentPathIndex];  // берем вейпоинт
        //         // была проблема странная - бот всегда опускался на Oy = 0 => наполовину был под землей. единственное что помогло:
        //         targetPosition.y = -0.19f;
        //
        //         // передвижение по шагам (то есть с одинаковой скоростью)
        //         float step = moveSpeed * Time.deltaTime;
        //         transform.position = Vector3.MoveTowards(transform.position, targetPosition, step);;
        //         // проверка того, достиг ли робот вейпоинта
        //         if (Vector3.Distance(transform.position, targetPosition) < 0.01f) {
        //             _currentPathIndex++;  // след вейпоинт
        //             if (_currentPathIndex >= _pathVectorList.Count) {
        //                 StopMoving();
        //             }
        //         }
        //
        //         // поворот бота в сторону ходьбы
        //         Vector3 direction = (targetPosition - transform.position).normalized;
        //         if (direction != Vector3.zero) {
        //             Quaternion toRotation = Quaternion.LookRotation(direction, Vector3.up);
        //             // так как моделька не в сторону Oz (в unity по умолчанию так, просто модель экспортировали криво видимо),
        //             // а в Ox, то из-за этого криво работает перемещение - робот едет правым боком. причем если робота просто
        //             // через инспектор повернуть, то это не помогает. только так - через код
        //             // умножение кватерниона добавляет угол
        //             toRotation *= Quaternion.Euler(0, 90, 0);
        //             transform.rotation = Quaternion.Slerp(transform.rotation, toRotation, 0.15f);
        //         }
        //     }
        // }
        //
        // private void StopMoving() {
        //     _pathVectorList = null;
        // }
    }
}