using System.Collections.Generic;
using UnityEngine;

namespace environment
{
    [RequireComponent(typeof(Terrain))] // Обязательный компонент Terrain
    public class TerrainGrassGenerator : MonoBehaviour
    {
        [Header("Настройки генерации")]
        [SerializeField] private float density = 0.7f; // Плотность (0-1)
        [SerializeField] private float clusterRadius = 3f; // Радиус кластеризации
        [SerializeField] private float maxSlopeAngle = 45f; // Макс. угол наклона для спавна

        [Header("Префабы травы")]
        [SerializeField] private GameObject[] dryGrassPrefabs;
        [SerializeField] private GameObject[] normalGrassPrefabs;

        [Header("Настройки вариаций")]
        [SerializeField] private Vector2 scaleRange = new Vector2(0.8f, 1.2f);
        [SerializeField] private float maxRotation = 360f;

        private Terrain terrain;
        private TerrainData terrainData;
        private List<Vector3> spawnedPositions = new List<Vector3>();

        void Start()
        {
            terrain = GetComponent<Terrain>();
            if (terrain == null)
            {
                Debug.LogError("Скрипт должен быть прикреплен к Terrain!");
                return;
            }

            terrainData = terrain.terrainData;
            GenerateGrass();
        }

        void GenerateGrass()
        {
            Vector3 terrainSize = terrainData.size;
            int totalPoints = Mathf.FloorToInt(terrainSize.x * terrainSize.z * density);

            for (int i = 0; i < totalPoints; i++)
            {
                Vector3 randomPos = new Vector3(
                    Random.Range(0, terrainSize.x),
                    0,
                    Random.Range(0, terrainSize.z)
                );

                // Корректируем высоту и проверяем наклон
                randomPos.y = terrain.SampleHeight(randomPos);
                if (!IsPositionValid(randomPos)) continue;

                // Определяем тип травы
                float noiseValue = Mathf.PerlinNoise(
                    randomPos.x * 0.3f + 1000, 
                    randomPos.z * 0.3f + 2000
                );

                GameObject[] prefabArray;
            
                if (noiseValue > 0.6f)
                {
                    prefabArray = normalGrassPrefabs;
                    CreateCluster(randomPos, dryGrassPrefabs);
                }
                else if (noiseValue > 0.3f)
                {
                    prefabArray = dryGrassPrefabs;
                }
                else continue;

                SpawnGrass(prefabArray, randomPos);
            }
        }

        void SpawnGrass(GameObject[] prefabs, Vector3 position)
        {
            GameObject grass = Instantiate(
                prefabs[Random.Range(0, prefabs.Length)],
                transform
            );

            grass.transform.position = position + terrain.transform.position;
            grass.transform.rotation = Quaternion.Euler(0, Random.Range(0, maxRotation), 0);
            grass.transform.localScale = Vector3.one * Random.Range(scaleRange.x, scaleRange.y);

            spawnedPositions.Add(position);
        }

        bool IsPositionValid(Vector3 position)
        {
            // Проверка наклона
            float steepness = terrainData.GetSteepness(
                position.x / terrainData.size.x, 
                position.z / terrainData.size.z
            );
            if (steepness > maxSlopeAngle) return false;

            // Проверка расстояния
            foreach (Vector3 pos in spawnedPositions)
            {
                if (Vector3.Distance(pos, position) < 0.5f) return false;
            }
            return true;
        }

        void CreateCluster(Vector3 center, GameObject[] prefabs)
        {
            for (int i = 0; i < Random.Range(3, 6); i++)
            {
                Vector3 clusterPos = center + Random.insideUnitSphere * clusterRadius;
                clusterPos.y = terrain.SampleHeight(clusterPos);
                if (IsPositionValid(clusterPos)) SpawnGrass(prefabs, clusterPos);
            }
        }

        void OnDrawGizmosSelected()
        {
            if (terrain != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireCube(
                    terrain.transform.position + terrainData.size / 2,
                    terrainData.size
                );
            }
        }
    }
}