using System.Collections.Generic;
using Bot.Programming.Navigation;
using Grid.Base; // GridBase, GridNode
using UnityEngine;

namespace environment.Ore
{
    /// <summary>
    /// Заниматель клеток для руды.
    /// Режимы:
    /// - ManualRectangle: прямоугольник size x pivot (как раньше)
    /// - ColliderBounds: авто-футпринт по Bounds коллайдера/рендерера
    /// Регистрирует себя в GridNode как динамический оккупант, чтобы нельзя было строить на руде
    /// и чтобы подъезд искался вокруг.
    /// </summary>
    [DisallowMultipleComponent]
    public class OreGridOccupant : MonoBehaviour
    {
        public enum FootprintMode
        {
            ColliderBounds,
            ManualRectangle
        }

        [Header("Mode")]
        [SerializeField] private FootprintMode mode = FootprintMode.ColliderBounds;

        [Header("Manual Rectangle (used if mode = ManualRectangle)")]
        [SerializeField] private Vector2Int size = new(1, 1);   // ширина x высота (в клетках)
        [SerializeField] private Vector2Int pivot = new(0, 0);  // смещение внутри прямоугольника

        [Header("Collider Bounds (used if mode = ColliderBounds)")]
        [Tooltip("Если пусто — возьмём любой Collider на объекте/детях, иначе попытаемся взять Renderer.bounds")]
        [SerializeField] private Collider sourceCollider;
        [Tooltip("Доп. буфер вокруг bounds в метрах, если нужно расширить/сузить футпринт")]
        [SerializeField] private float boundsPadding = 0.0f;

        // доступ к гриду
        private GridBase<GridNode> Grid => GridApproach.GetGrid();

        // кеш зарегистрированных клеток
        private readonly List<Vector2Int> _registered = new();
        private Vector2Int _lastOrigin;       // для ManualRectangle
        private Bounds _lastBoundsWorld;      // для ColliderBounds
        private bool _registeredOnce;

        private void Reset()
        {
            // если на префабе есть коллайдер — попробуем авто-выбрать его
            if (sourceCollider == null)
                sourceCollider = GetComponentInChildren<Collider>();
        }

        private void OnEnable()  => TryRegister();
        private void OnDisable() => Unregister();

        private void LateUpdate()
        {
            var g = Grid; if (g == null) return;

            if (mode == FootprintMode.ManualRectangle)
            {
                var cur = g.GetGridPosition(transform.position);
                if (!_registeredOnce || cur != _lastOrigin)
                    TryRegister();
            }
            else // ColliderBounds
            {
                var curBounds = GetWorldBounds();
                // если bounds изменились (позиция/поворот/скейл) — перерегистрируем
                if (!_registeredOnce || !ApproximatelyEqual(_lastBoundsWorld, curBounds))
                    TryRegister();
            }
        }

        private void TryRegister()
        {
            var g = Grid; if (g == null) return;

            if (_registered.Count > 0) Unregister();
            _registered.Clear();

            foreach (var c in EnumerateCells())
            {
                _registered.Add(c);
                var node = g.GetGridObject(c);
                node?.AddDynamicOccupant(this); // помечаем клетку занятой рудой
            }

            if (mode == FootprintMode.ManualRectangle)
                _lastOrigin = g.GetGridPosition(transform.position);
            else
                _lastBoundsWorld = GetWorldBounds();

            _registeredOnce = true;
        }

        private void Unregister()
        {
            var g = Grid; if (g == null) return;

            foreach (var c in _registered)
            {
                var node = g.GetGridObject(c);
                node?.RemoveDynamicOccupant(this);
            }
            _registered.Clear();
        }

        /// <summary>Клетки, занимаемые этой жилой.</summary>
        public IEnumerable<Vector2Int> EnumerateCells()
        {
            var g = Grid; if (g == null) yield break;

            if (mode == FootprintMode.ManualRectangle)
            {
                var origin = g.GetGridPosition(transform.position);
                var start  = new Vector2Int(origin.x - pivot.x, origin.y - pivot.y);

                int sx = Mathf.Max(1, size.x);
                int sy = Mathf.Max(1, size.y);

                for (int dx = 0; dx < sx; dx++)
                for (int dy = 0; dy < sy; dy++)
                    yield return new Vector2Int(start.x + dx, start.y + dy);
            }
            else // ColliderBounds
            {
                Bounds b = GetWorldBounds();
                if (b.size == Vector3.zero) yield break;

                // прогоняем по диапазону клеток, которые пересекают AABB коллайдера
                var minCell = g.GetGridPosition(new Vector3(b.min.x, 0f, b.min.z));
                var maxCell = g.GetGridPosition(new Vector3(b.max.x, 0f, b.max.z));

                // нормализуем (на случай отрицательных координат)
                var x0 = Mathf.Min(minCell.x, maxCell.x);
                var x1 = Mathf.Max(minCell.x, maxCell.x);
                var y0 = Mathf.Min(minCell.y, maxCell.y);
                var y1 = Mathf.Max(minCell.y, maxCell.y);

                // точная проверка пересечения тайла с AABB (не обязательно, но точнее)
                for (int x = x0; x <= x1; x++)
                {
                    for (int y = y0; y <= y1; y++)
                    {
                        // world AABB клетки
                        Vector3 cellMin = g.GetWorldPosition(new Vector2Int(x, y));
                        Vector3 cellMax = cellMin + new Vector3(g.CellSize, 0f, g.CellSize);
                        var cellBounds = new Bounds();
                        cellBounds.SetMinMax(
                            new Vector3(cellMin.x, b.center.y, cellMin.z),
                            new Vector3(cellMax.x, b.center.y, cellMax.z)
                        );

                        if (cellBounds.Intersects(b))
                            yield return new Vector2Int(x, y);
                    }
                }
            }
        }

        private Bounds GetWorldBounds()
        {
            // 1) предпочитаем Collider.bounds
            if (sourceCollider == null)
                sourceCollider = GetComponentInChildren<Collider>();

            Bounds b;
            if (sourceCollider != null)
            {
                b = sourceCollider.bounds;
            }
            else
            {
                // 2) fallback: Renderer.bounds
                var r = GetComponentInChildren<Renderer>();
                if (r != null) b = r.bounds;
                else return new Bounds(transform.position, Vector3.zero);
            }

            if (boundsPadding != 0f)
            {
                b.Expand(boundsPadding * 2f);
            }

            // сплющим по Y (работаем в XZ)
            b.min = new Vector3(b.min.x, 0f, b.min.z);
            b.max = new Vector3(b.max.x, 0f, b.max.z);
            return b;
        }

        private static bool ApproximatelyEqual(Bounds a, Bounds b)
        {
            const float EPS = 1e-3f;
            return Vector3.SqrMagnitude(a.center - b.center) < EPS
                && Vector3.SqrMagnitude(a.size   - b.size)   < EPS;
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            var g = Grid; if (g == null) return;

            Gizmos.matrix = Matrix4x4.identity;

            // визуал футпринта
            Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.35f);
            foreach (var c in EnumerateCells())
            {
                var w  = g.GetWorldPosition(c);
                var sz = Vector3.one * g.CellSize;
                Gizmos.DrawCube(new Vector3(w.x + sz.x * 0.5f, 0.03f, w.z + sz.z * 0.5f), sz * 0.98f);
            }

            // визуал bounds в режиме коллайдера
            if (mode == FootprintMode.ColliderBounds)
            {
                var b = GetWorldBounds();
                Gizmos.color = new Color(1f, 0.6f, 0.2f, 0.35f);
                Gizmos.DrawCube(b.center, b.size);
            }
        }
#endif
    }
}
