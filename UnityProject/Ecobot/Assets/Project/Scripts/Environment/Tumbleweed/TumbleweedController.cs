using UnityEngine;
using Grid;
using System.Collections;
using System.Collections.Generic;
using Grid.Base;

namespace environment.Tumbleweed
{
    public class TumbleweedController : MonoBehaviour
    {
        [Header("Base Settings")]
        [SerializeField] private GameObject tumbleweedPrefab;
        [SerializeField] private float fadeTime = 1f;
        [SerializeField] private int maxTumbleweeds = 5;

        [Header("Wind Settings")]
        [SerializeField][Range(0f, 360f)] private float windDirection = 45f;
        [SerializeField][Range(0.5f, 2f)] private float windForce = 1f;

        [Header("References")]
        [SerializeField] private GridMap gridMap;

        private GridBase<GridNode> _grid;
        private List<TumbleweedData> _tumbleweeds = new List<TumbleweedData>();
        private List<Vector2Int> _edgeCells = new List<Vector2Int>();
        private float _modelHeight;
        private float _lastWindDirection;
        private Coroutine _spawnSystemCoroutine;

        private enum GridEdge { North, East, South, West }
        private GridEdge _startEdge;
        private GridEdge _endEdge;

        [System.Serializable]
        private class TumbleweedData
        {
            public GameObject instance;
            public TumbleweedMovement movement;
            public Renderer renderer;
            public Coroutine lifeCycleCoroutine;
            public bool isActive;
        }

        private void Start()
        {
            InitializeGrid();
            CacheEdgeCells();
            DetermineWindEdges();
            InitializePool();
            _spawnSystemCoroutine = StartCoroutine(SpawnSystem());
        }

        private void Update()
        {
            if (!Mathf.Approximately(_lastWindDirection, windDirection))
            {
                _lastWindDirection = windDirection;
                OnWindDirectionChanged();
            }
        }

        private void OnWindDirectionChanged()
        {
            DetermineWindEdges();
            
            foreach (var data in _tumbleweeds)
            {
                if (data.isActive)
                {
                    if (data.lifeCycleCoroutine != null)
                    {
                        StopCoroutine(data.lifeCycleCoroutine);
                        data.lifeCycleCoroutine = null;
                    }
                    
                    if (data.movement != null)
                    {
                        data.movement.StopMovement();
                    }
                    
                    if (data.instance != null)
                    {
                        data.instance.SetActive(false);
                    }
                    data.isActive = false;
                }
            }
            
            if (_spawnSystemCoroutine != null)
            {
                StopCoroutine(_spawnSystemCoroutine);
            }
            _spawnSystemCoroutine = StartCoroutine(SpawnSystem());
        }

        private void InitializePool()
        {
            for (int i = 0; i < maxTumbleweeds; i++)
            {
                CreateTumbleweedInPool();
            }
        }

        private void CreateTumbleweedInPool()
        {
            GameObject obj = Instantiate(tumbleweedPrefab);
            obj.SetActive(false);
    
            // Фиксируем позицию для всех экземпляров
            obj.transform.position = tumbleweedPrefab.transform.position;
            obj.transform.rotation = tumbleweedPrefab.transform.rotation;
    
            var data = new TumbleweedData
            {
                instance = obj,
                movement = obj.GetComponent<TumbleweedMovement>(),
                renderer = obj.GetComponent<Renderer>(),
                isActive = false
            };
    
            _tumbleweeds.Add(data);
        }

        private IEnumerator SpawnSystem()
        {
            while (true)
            {
                foreach (var data in _tumbleweeds)
                {
                    if (!data.isActive && data.instance != null)
                    {
                        data.isActive = true;
                        if (data.lifeCycleCoroutine != null)
                            StopCoroutine(data.lifeCycleCoroutine);
                        data.lifeCycleCoroutine = StartCoroutine(TumbleweedLifeCycle(data));
                        yield return new WaitForSeconds(Random.Range(0.5f, 2f));
                    }
                }
                yield return new WaitForSeconds(1f);
            }
        }

        private IEnumerator TumbleweedLifeCycle(TumbleweedData data)
        {
            GameObject tumbleweed = data.instance;
            tumbleweed.SetActive(true);
    
            // Фиксируем стартовую позицию на уровне префаба
            Vector3 initialPosition = tumbleweed.transform.position;
            Quaternion initialRotation = tumbleweed.transform.rotation;

            while (true)
            {
                // Жесткий сброс позиции и вращения
                tumbleweed.transform.SetPositionAndRotation(initialPosition, initialRotation);
                
                if (TryGetNewPosition(true, out Vector3 startPos, data))
                    tumbleweed.transform.position = startPos;

                yield return FadeEffect(data, 0f, 1f, fadeTime);

                if (TryGetNewPosition(false, out Vector3 targetPos, data))
                {
                    data.movement.FullReset(); // Сброс перед началом движения
                    data.movement.OnMovementComplete = null;
                    data.movement.OnMovementComplete += () => {
                        data.isActive = false;
                        data.movement.FullReset(); // Дополнительный сброс после завершения
                    };
                    data.movement.StartMovement(targetPos, windForce);
                    yield return new WaitUntil(() => !data.movement.IsMoving);
                }

                yield return FadeEffect(data, 1f, 0f, fadeTime);

                tumbleweed.SetActive(false);
                
                tumbleweed.transform.position = initialPosition;
                tumbleweed.transform.rotation = Quaternion.identity;
                
                yield return new WaitForSeconds(1f);
            }
        }

        private IEnumerator FadeEffect(TumbleweedData data, float startAlpha, float endAlpha, float duration)
        {
            if (data.renderer == null) yield break;

            Material mat = data.renderer.material;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                if (mat == null) yield break;
                mat.color = new Color(mat.color.r, mat.color.g, mat.color.b, 
                    Mathf.Lerp(startAlpha, endAlpha, elapsed/duration));
                elapsed += Time.deltaTime;
                yield return null;
            }

            if (mat != null)
                mat.color = new Color(mat.color.r, mat.color.g, mat.color.b, endAlpha);
        }

        private void InitializeGrid()
        {
            if (gridMap == null) return;
            _grid = gridMap.Grid;
        }

        private void CacheEdgeCells()
        {
            _edgeCells.Clear();
            for (int x = 0; x < _grid.Width; x++)
            {
                for (int y = 0; y < _grid.Height; y++)
                {
                    bool isEdge = x == 0 || x == _grid.Width-1 || y == 0 || y == _grid.Height-1;
                    GridNode node = _grid.GetGridObject(new Vector2Int(x, y));
                    if (isEdge && node != null && node.IsWalkable)
                        _edgeCells.Add(new Vector2Int(x, y));
                }
            }
        }

        private void DetermineWindEdges()
        {
            _startEdge = GetStartEdge(windDirection);
            _endEdge = GetOppositeEdge(_startEdge);
        }

        private GridEdge GetStartEdge(float angle)
        {
            angle = (angle % 360 + 360) % 360;
            if (angle >= 315f || angle < 45f) return GridEdge.North;
            if (angle >= 45f && angle < 135f) return GridEdge.East;
            if (angle >= 135f && angle < 225f) return GridEdge.South;
            return GridEdge.West;
        }

        private GridEdge GetOppositeEdge(GridEdge edge)
        {
            switch (edge)
            {
                case GridEdge.North: return GridEdge.South;
                case GridEdge.East: return GridEdge.West;
                case GridEdge.South: return GridEdge.North;
                default: return GridEdge.East;
            }
        }

        private bool TryGetNewPosition(bool isStart, out Vector3 position, TumbleweedData data)
        {
            position = Vector3.zero;
            if (_edgeCells.Count == 0) return false;

            List<Vector2Int> suitableCells = new List<Vector2Int>();
            GridEdge targetEdge = isStart ? _startEdge : _endEdge;

            foreach (var cell in _edgeCells)
                if (IsCellInEdge(cell, targetEdge))
                    suitableCells.Add(cell);

            if (suitableCells.Count == 0)
                suitableCells = _edgeCells;

            Vector2Int chosenCell = suitableCells[Random.Range(0, suitableCells.Count)];
            position = _grid.GetWorldPosition(chosenCell) + Vector3.up * _modelHeight;
            
            position = _grid.GetWorldPosition(chosenCell) + 
                       Vector3.up * (_modelHeight + data.movement.GetComponent<SphereCollider>().radius);
            
            return true;
        }

        private bool IsCellInEdge(Vector2Int cell, GridEdge edge)
        {
            return edge switch
            {
                GridEdge.North => cell.y == _grid.Height - 1,
                GridEdge.East => cell.x == _grid.Width - 1,
                GridEdge.South => cell.y == 0,
                GridEdge.West => cell.x == 0,
                _ => false
            };
        }

        private void OnDestroy()
        {
            StopAllCoroutines();
            
            foreach (var data in _tumbleweeds)
            {
                if (data.instance != null)
                {
                    if (data.movement != null)
                        data.movement.OnMovementComplete = null;
                    Destroy(data.instance);
                }
            }
            _tumbleweeds.Clear();
        }
    }
}
