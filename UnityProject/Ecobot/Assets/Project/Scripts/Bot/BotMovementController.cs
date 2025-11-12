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
        private GridBase<GridNode> _grid;
        private Pathfinder _pathfinder;
        private BotMovementVisualDebugger _movementVisualDebugger;
        private int _currentPathIndex;
        private List<Vector3> _pathVectorList;

        // --- Новые поля для защиты от наложения корутин ---
        // Инкрементируем при каждой новой запуске движения.
        private int _movementSession = 0;

        public void Init(BotBase bot, GridMap gridMap)
        {
            _bot = bot;
            _grid = gridMap.Grid;
            _pathfinder = new Pathfinder(gridMap.Grid);
            _movementVisualDebugger = GetComponent<BotMovementVisualDebugger>();
        }

        /// <summary>
        /// Запустить движение к точке. Эта публичная функция безопасно отменит/инвалидирует
        /// предыдущие корутины движения и запустит новую корутину.
        /// </summary>
        public Coroutine StartMove(Vector3 target)
        {
            // увеличиваем сессию — все старые корутины увидят расхождение и завершатся
            _movementSession++;
            // Запускаем корутину; если внешняя система вызывает Move(...) как IEnumerator,
            // она может использовать StartMove(...) чтобы получить корутину.
            return StartCoroutine(MoveRoutine(target, _movementSession));
        }

        /// <summary>
        /// Если внешняя сторона предпочитает напрямую запускать IEnumerator Move(...),
        /// можно оставить совместимый метод — он просто вызывает StartMove и возвращает IEnumerator.
        /// Но чаще системы запускают StartCoroutine(movementController.Move(target));
        /// поэтому оставляем публичный IEnumerator Move для обратной совместимости:
        /// </summary>
        public IEnumerator Move(Vector3 target)
        {
            // Увеличиваем сессию и запускаем внутреннюю реализацию.
            _movementSession++;
            yield return MoveRoutine(target, _movementSession);
        }

        // Внутренняя реализация корутины: принимает sessionId и в цикле проверяет, не устарела ли она.
        private IEnumerator MoveRoutine(Vector3 target, int sessionId)
        {
            SetTargetPosition(target);

            if (_pathVectorList == null || _pathVectorList.Count == 0)
                yield break;

            // основной цикл движения
            while (_pathVectorList != null && _pathVectorList.Count > 0)
            {
                // если сессия изменилась — прерываем (это гарантирует, что только самая последняя
                // запущенная корутина продолжит движение)
                if (sessionId != _movementSession)
                {
                    // Debug.Log($"[BotMovement] Movement session {sessionId} cancelled (current {_movementSession})");
                    yield break;
                }

                _movementVisualDebugger?.DrawPath(_pathVectorList);
                Vector3 targetPosition = _pathVectorList[_currentPathIndex];
                targetPosition.y = -0.19f;

                float step = moveSpeed * Time.deltaTime;
                _bot.transform.position = Vector3.MoveTowards(_bot.transform.position, targetPosition, step);

                if (Vector3.Distance(_bot.transform.position, targetPosition) < 0.01f)
                {
                    _currentPathIndex++;
                    if (_currentPathIndex >= _pathVectorList.Count)
                    {
                        // При корректном достижении точки — завершаем движение.
                        StopMovingInternal(); // очистка локальных данных
                        yield break;
                    }
                }

                Vector3 direction = (targetPosition - _bot.transform.position).normalized;
                if (direction != Vector3.zero)
                {
                    Quaternion toRotation = Quaternion.LookRotation(direction, Vector3.up);
                    toRotation *= Quaternion.Euler(0, 90, 0);
                    _bot.transform.rotation = Quaternion.Slerp(_bot.transform.rotation, toRotation, 0.15f);
                }

                yield return null;
            }
        }

        // Внешний метод остановки: инвалидирует сессию и очищает путь.
        public void StopMoving()
        {
            // Инвалидируем текущую сессию — все запущенные корутины завершатся при следующей итерации.
            _movementSession++;
            StopMovingInternal();
        }

        // Внутреннее обнуление данных пути
        private void StopMovingInternal()
        {
            _pathVectorList = null;
            _currentPathIndex = 0;
        }

        private void SetTargetPosition(Vector3 targetPosition)
        {
            _currentPathIndex = 0;
            _pathVectorList = _pathfinder.FindPath(transform.position, targetPosition);

            if (_pathVectorList != null && _pathVectorList.Count > 1)
            {
                _pathVectorList.RemoveAt(0); // убираем начальную точку
            }
        }
    }
}
