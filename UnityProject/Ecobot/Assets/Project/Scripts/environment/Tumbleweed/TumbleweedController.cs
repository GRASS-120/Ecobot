// TumbleweedController.cs
using UnityEngine;
using Grid;
using System.Collections;
using System.Collections.Generic;

namespace environment.Tumbleweed
{
    public class TumbleweedController : MonoBehaviour
    {
        [Header("Base Settings")]
        [SerializeField] private float heightOffset = 0.5f;
        [SerializeField] private GameObject tumbleweedPrefab;
        [SerializeField] private float fadeTime = 1f;
        
        [Header("References")]
        [SerializeField] private GridMap gridMap;

        private GridBase<GridNode> _grid;
        private GameObject _currentTumbleweed;
        private TumbleweedMovement _movement;
        private Renderer _renderer;
        private List<Vector2Int> _walkableCells = new List<Vector2Int>();
        private Coroutine _currentFadeCoroutine;

        private void Start()
        {
            InitializeGrid();
            CacheWalkableCells();
            StartCoroutine(SpawnCycle());
        }

        private IEnumerator SpawnCycle()
        {
            while (true)
            {
                // Создание нового куста
                _currentTumbleweed = Instantiate(tumbleweedPrefab);
                _movement = _currentTumbleweed.GetComponent<TumbleweedMovement>();
                _renderer = _currentTumbleweed.GetComponent<Renderer>();

                // Плавное появление
                yield return FadeEffect(0f, 1f, fadeTime);

                // Основной цикл движения
                while (true)
                {
                    if (TryGetNewPosition(out Vector3 newPosition))
                    {
                        _movement.StartMovement(newPosition);
                        yield return new WaitUntil(() => !_movement.IsMoving);
                    }
                    
                    // Плавное исчезновение
                    yield return FadeEffect(1f, 0f, fadeTime);
                    
                    // Телепортация
                    if (TryGetNewPosition(out newPosition))
                    {
                        _currentTumbleweed.transform.position = newPosition;
                    }
                    
                    // Плавное появление
                    yield return FadeEffect(0f, 1f, fadeTime);
                }
            }
        }

        private IEnumerator FadeEffect(float startAlpha, float endAlpha, float duration)
        {
            if (_renderer == null) yield break;
            
            Material material = _renderer.material;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                material.color = new Color(
                    material.color.r,
                    material.color.g,
                    material.color.b,
                    Mathf.Lerp(startAlpha, endAlpha, elapsed/duration)
                );
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            material.color = new Color(
                material.color.r,
                material.color.g,
                material.color.b,
                endAlpha
            );
        }

        private void InitializeGrid()
        {
            if (gridMap == null) return;
            _grid = gridMap.Grid;
        }

        private void CacheWalkableCells()
        {
            _walkableCells.Clear();
            for (int x = 0; x < _grid.Width; x++)
            {
                for (int y = 0; y < _grid.Height; y++)
                {
                    GridNode node = _grid.GetGridObject(new Vector2Int(x, y));
                    if (node != null && node.IsWalkable)
                    {
                        _walkableCells.Add(new Vector2Int(x, y));
                    }
                }
            }
        }

        private bool TryGetNewPosition(out Vector3 position)
        {
            position = Vector3.zero;
            if (_walkableCells.Count == 0) return false;

            Vector2Int cell = _walkableCells[Random.Range(0, _walkableCells.Count)];
            position = _grid.GetWorldPosition(cell) + Vector3.up * heightOffset;
            return true;
        }

        private void OnDestroy()
        {
            if (_currentFadeCoroutine != null)
            {
                StopCoroutine(_currentFadeCoroutine);
            }
        }
    }
}