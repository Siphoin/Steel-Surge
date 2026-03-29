using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Cysharp.Threading.Tasks;
using SteelSurge.LevelEditor.Configs;

namespace SteelSurge.LevelEditor.Services
{
    public class MapGenerator
    {
        private readonly MapGenerationConfig _config;
        private readonly HexGridService _gridService;
        private readonly Transform _parent;
        private readonly int _seed;
        private readonly int _width;
        private readonly int _height;

        private Dictionary<Vector2Int, GameObject> _spawnedHexes = new Dictionary<Vector2Int, GameObject>();
        private Dictionary<Vector2Int, GameObject> _spawnedObstacles = new Dictionary<Vector2Int, GameObject>();

        public Vector2Int Poi1 { get; private set; }
        public Vector2Int Poi2 { get; private set; }

        public MapGenerator(MapGenerationConfig config, int width, int height, Transform parent, int seed)
        {
            _config = config;
            _width = width;
            _height = height;
            _parent = parent;
            _seed = seed;
            _gridService = new HexGridService(config.HexSize);
        }

        public async UniTask GenerateAsync()
        {
            Clear();
            Random.InitState(_seed);

            CalculatePoiPositions();

            await GenerateBaseGridAsync();
            await GenerateBiomesAsync();
            await GeneratePoiSpotsAsync();
            await GenerateBordersAsync();
            await GenerateObstaclesAsync();
        }

        public void Clear()
        {
            foreach (var hex in _spawnedHexes.Values)
            {
                if (hex != null) Object.DestroyImmediate(hex);
            }
            _spawnedHexes.Clear();

            foreach (var obs in _spawnedObstacles.Values)
            {
                if (obs != null) Object.DestroyImmediate(obs);
            }
            _spawnedObstacles.Clear();

            // Fallback to clear all children if any left
            for (int i = _parent.childCount - 1; i >= 0; i--)
            {
                Object.DestroyImmediate(_parent.GetChild(i).gameObject);
            }
        }

        private void CalculatePoiPositions()
        {
            bool isVertical = _width < _height;
            
            int q1 = isVertical ? _width / 2 : 2;
            int r1 = isVertical ? 2 : _height / 2;

            if (_config.PoiSpawnMode == PoiSpawnMode.RandomEdges)
            {
                if (isVertical) q1 = Random.Range(2, _width - 2);
                else r1 = Random.Range(2, _height - 2);
            }
            else if (_config.PoiSpawnMode == PoiSpawnMode.Diagonal)
            {
                if (isVertical) q1 = Random.value > 0.5f ? 2 : _width - 3;
                else r1 = Random.value > 0.5f ? 2 : _height - 3;
            }

            Poi1 = new Vector2Int(q1, r1);

            if (_config.Symmetry != SymmetryType.None)
            {
                Poi2 = _gridService.GetSymmetricCoordinate(Poi1.x, Poi1.y, _width, _height, _config.Symmetry);
            }
            else
            {
                int q2 = isVertical ? _width / 2 : _width - 3;
                int r2 = isVertical ? _height - 3 : _height / 2;
                
                if (_config.PoiSpawnMode == PoiSpawnMode.RandomEdges)
                {
                    if (isVertical) q2 = Random.Range(2, _width - 2);
                    else r2 = Random.Range(2, _height - 2);
                }
                else if (_config.PoiSpawnMode == PoiSpawnMode.Diagonal)
                {
                    if (isVertical) q2 = (_width - 1) - Poi1.x;
                    else r2 = (_height - 1) - Poi1.y;
                }
                Poi2 = new Vector2Int(q2, r2);
            }
        }

        private async UniTask GenerateBaseGridAsync()
        {
            int batchSize = 50;
            int count = 0;

            for (int r = 0; r < _height; r++)
            {
                for (int q = 0; q < _width; q++)
                {
                    SpawnHex(q, r, _config.HexGrassPrefab, _config.BaseMaterial);
                    
                    count++;
                    if (count >= batchSize)
                    {
                        count = 0;
                        await UniTask.Yield();
                    }
                }
            }
        }

        private float GetFractalNoise(float x, float y)
        {
            float amplitude = 1f;
            float frequency = 1f;
            float noiseHeight = 0f;
            float maxAmplitude = 0f;

            for (int i = 0; i < _config.NoiseOctaves; i++)
            {
                float sampleX = x * frequency;
                float sampleY = y * frequency;
                noiseHeight += Mathf.PerlinNoise(sampleX, sampleY) * amplitude;
                maxAmplitude += amplitude;
                amplitude *= _config.NoisePersistence;
                frequency *= _config.NoiseLacunarity;
            }

            return noiseHeight / maxAmplitude;
        }

        private async UniTask GenerateBiomesAsync()
        {
            if (_config.BiomeLayers == null || _config.BiomeLayers.Count == 0) return;

            var sortedLayers = _config.BiomeLayers.OrderByDescending(l => l.Threshold).ToList();

            float offsetX = Random.Range(-10000f, 10000f);
            float offsetY = Random.Range(-10000f, 10000f);

            int halfHeight = _config.Symmetry == SymmetryType.None ? _height : _height / 2;
            int batchSize = 50;
            int count = 0;

            for (int r = 0; r < halfHeight; r++)
            {
                for (int q = 0; q < _width; q++)
                {
                    float noiseValue = GetFractalNoise((q + offsetX) * _config.NoiseScale, (r + offsetY) * _config.NoiseScale);
                    Material materialToApply = null;

                    foreach (var layer in sortedLayers)
                    {
                        if (noiseValue > layer.Threshold)
                        {
                            materialToApply = layer.Material;
                            break;
                        }
                    }

                    if (materialToApply != null)
                    {
                        ApplyMaterialToHex(q, r, materialToApply);
                        ApplySymmetry(q, r, materialToApply, ApplyMaterialToHex);
                    }

                    count++;
                    if (count >= batchSize)
                    {
                        count = 0;
                        await UniTask.Yield();
                    }
                }
            }
        }

        private async UniTask GeneratePoiSpotsAsync()
        {
            if (_config.PoiSpotMaterial == null) return;

            ApplyPoiSpot(Poi1.x, Poi1.y);
            await UniTask.Yield();
            ApplyPoiSpot(Poi2.x, Poi2.y);
            await UniTask.Yield();
        }

        private void ApplyPoiSpot(int centerQ, int centerR)
        {
            for (int r = 0; r < _height; r++)
            {
                for (int q = 0; q < _width; q++)
                {
                    if (_gridService.GetDistance(q, r, centerQ, centerR) <= _config.PoiSpotRadius)
                    {
                        ApplyMaterialToHex(q, r, _config.PoiSpotMaterial);
                    }
                }
            }
        }

        private async UniTask GenerateBordersAsync()
        {
            if (_config.MountainPrefabs == null || _config.MountainPrefabs.Count == 0) return;

            float borderOffsetX = Random.Range(-10000f, 10000f);
            float borderOffsetY = Random.Range(-10000f, 10000f);
            int halfHeight = _config.Symmetry == SymmetryType.None ? _height : _height / 2;
            
            int batchSize = 20;
            int count = 0;

            for (int r = 0; r < halfHeight; r++)
            {
                for (int q = 0; q < _width; q++)
                {
                    int distX = Mathf.Min(q, _width - 1 - q);
                    int distY = Mathf.Min(r, _height - 1 - r);
                    int distToEdge = Mathf.Min(distX, distY);

                    bool isMountain = false;

                    if (distToEdge == 0)
                    {
                        isMountain = true;
                    }
                    else if (distToEdge <= _config.MaxBorderDepth)
                    {
                        float noise = Mathf.PerlinNoise((q + borderOffsetX) * _config.BorderNoiseScale, (r + borderOffsetY) * _config.BorderNoiseScale);
                        float adjustedThreshold = _config.BorderNoiseThreshold + (distToEdge * 0.15f);
                        if (noise > adjustedThreshold)
                        {
                            isMountain = true;
                        }
                    }

                    if (isMountain)
                    {
                        GameObject randomMountain = _config.MountainPrefabs[Random.Range(0, _config.MountainPrefabs.Count)];
                        SpawnObstacle(q, r, randomMountain, false);
                        ApplySymmetry(q, r, randomMountain, (sq, sr, prefab) => SpawnObstacle(sq, sr, prefab, false));
                        
                        count++;
                        if (count >= batchSize)
                        {
                            count = 0;
                            await UniTask.Yield();
                        }
                    }
                }
            }
        }

        private async UniTask GenerateObstaclesAsync()
        {
            int halfHeight = _config.Symmetry == SymmetryType.None ? _height : _height / 2;

            Vector2Int keep1 = Poi1;
            Vector2Int keep2 = Poi2;

            bool hasRocks = _config.RockPrefabs != null && _config.RockPrefabs.Count > 0;
            bool hasTrees = _config.TreePrefabs != null && _config.TreePrefabs.Count > 0;

            int numClusters = Mathf.RoundToInt((_width * _height) * _config.TreeDensity * 0.05f);
            List<Vector2Int> treeClusters = new List<Vector2Int>();
            
            for (int i = 0; i < numClusters; i++)
            {
                int cq = Random.Range(2, _width - 2);
                int cr = Random.Range(2, halfHeight);
                treeClusters.Add(new Vector2Int(cq, cr));
            }

            float treeOffsetX = Random.Range(-10000f, 10000f);
            float treeOffsetY = Random.Range(-10000f, 10000f);

            int centerQ = _width / 2;
            int centerR = _height / 2;
            int safeCenterRadius = Mathf.RoundToInt(_width * 0.25f);

            int batchSize = 15;
            int count = 0;

            for (int r = 1; r < halfHeight; r++)
            {
                for (int q = 1; q < _width - 1; q++)
                {
                    Vector2Int coord = new Vector2Int(q, r);
                    if (_spawnedObstacles.ContainsKey(coord)) continue;

                    if (_gridService.GetDistance(q, r, keep1.x, keep1.y) <= _config.PoiSpotRadius ||
                        _gridService.GetDistance(q, r, keep2.x, keep2.y) <= _config.PoiSpotRadius)
                        continue;

                    if (_gridService.GetDistance(q, r, keep1.x, keep1.y) <= _config.SafeZoneRadius ||
                        _gridService.GetDistance(q, r, keep2.x, keep2.y) <= _config.SafeZoneRadius)
                        continue;

                    GameObject prefabToSpawn = null;
                    bool canSpawnTrees = hasTrees;
                    bool forceMountain = false;

                    if (_config.Archetype == MapArchetype.ChokePoint)
                    {
                        if (Mathf.Abs(q - centerQ) <= 1)
                        {
                            if (Mathf.Abs(r - centerR) > 2)
                            {
                                forceMountain = true;
                            }
                        }
                    }
                    else if (_config.Archetype == MapArchetype.Divided)
                    {
                        if (q == centerQ)
                        {
                            if (r != centerR && r != centerR - 3 && r != centerR + 3)
                            {
                                forceMountain = true;
                            }
                        }
                    }

                    if (forceMountain && _config.MountainPrefabs != null && _config.MountainPrefabs.Count > 0)
                    {
                        prefabToSpawn = _config.MountainPrefabs[Random.Range(0, _config.MountainPrefabs.Count)];
                        SpawnObstacle(q, r, prefabToSpawn, false);
                        ApplySymmetry(q, r, prefabToSpawn, (sq, sr, p) => SpawnObstacle(sq, sr, p, false));
                        
                        count++;
                        if (count >= batchSize)
                        {
                            count = 0;
                            await UniTask.Yield();
                        }
                        continue;
                    }

                    if (_config.Archetype == MapArchetype.Standard && _gridService.GetDistance(q, r, centerQ, centerR) <= safeCenterRadius)
                    {
                        canSpawnTrees = false;
                    }

                    bool inTreeCluster = false;
                    if (canSpawnTrees)
                    {
                        foreach (var cluster in treeClusters)
                        {
                            int dist = _gridService.GetDistance(q, r, cluster.x, cluster.y);
                            int clusterRadius = Mathf.RoundToInt((1f - _config.TreeNoiseThreshold) * 10f);
                            
                            if (dist <= clusterRadius)
                            {
                                float spawnChance = _config.TreeClusterDensity * (1f - (float)dist / (clusterRadius + 1));
                                if (Random.value < spawnChance)
                                {
                                    inTreeCluster = true;
                                    break;
                                }
                            }
                        }
                    }
                    
                    if (inTreeCluster)
                    {
                        prefabToSpawn = _config.TreePrefabs[Random.Range(0, _config.TreePrefabs.Count)];
                    }
                    else if (hasRocks && Random.value < _config.RockDensity)
                    {
                        prefabToSpawn = _config.RockPrefabs[Random.Range(0, _config.RockPrefabs.Count)];
                    }

                    if (prefabToSpawn != null)
                    {
                        SpawnObstacle(q, r, prefabToSpawn, inTreeCluster);
                        ApplySymmetry(q, r, prefabToSpawn, (sq, sr, p) => SpawnObstacle(sq, sr, p, inTreeCluster));
                        
                        count++;
                        if (count >= batchSize)
                        {
                            count = 0;
                            await UniTask.Yield();
                        }
                    }
                }
            }
        }

        private void SpawnHex(int q, int r, GameObject prefab, Material material)
        {
            if (prefab == null) return;
            Vector3 pos = _gridService.GetWorldPosition(q, r);
            
#if UNITY_EDITOR
            GameObject instance = (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(prefab, _parent);
            instance.transform.position = pos;
            instance.transform.rotation = Quaternion.identity;
#else
            GameObject instance = Object.Instantiate(prefab, pos, Quaternion.identity, _parent);
#endif
            instance.name = $"Hex_{q}_{r}";
            
            if (material != null)
            {
                var renderer = instance.GetComponentInChildren<MeshRenderer>();
                if (renderer != null)
                {
                    renderer.sharedMaterial = material;
                }
            }
            
            _spawnedHexes[new Vector2Int(q, r)] = instance;
        }

        private void ApplyMaterialToHex(int q, int r, Material material)
        {
            Vector2Int coord = new Vector2Int(q, r);
            if (_spawnedHexes.TryGetValue(coord, out GameObject hex))
            {
                var renderer = hex.GetComponentInChildren<MeshRenderer>();
                if (renderer != null)
                {
                    renderer.sharedMaterial = material;
                }
            }
        }

        private void SpawnObstacle(int q, int r, GameObject prefab, bool isTree = false)
        {
            if (prefab == null) return;
            Vector2Int coord = new Vector2Int(q, r);
            if (_spawnedObstacles.ContainsKey(coord)) return;

            Vector3 basePos = _gridService.GetWorldPosition(q, r);
            
            if (isTree && _config.TreesPerHex > 1)
            {
                // Spawn multiple trees per hex with random offsets
                GameObject container = new GameObject($"Trees_{q}_{r}");
                container.transform.SetParent(_parent);
                container.transform.position = basePos;

                for (int i = 0; i < _config.TreesPerHex; i++)
                {
                    Vector2 randomCircle = Random.insideUnitCircle * _config.TreeSpreadRadius;
                    Vector3 offsetPos = basePos + new Vector3(randomCircle.x, 0, randomCircle.y);
                    
#if UNITY_EDITOR
                    GameObject instance = (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(prefab, container.transform);
                    instance.transform.position = offsetPos;
                    instance.transform.rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
#else
                    GameObject instance = Object.Instantiate(prefab, offsetPos, Quaternion.Euler(0, Random.Range(0f, 360f), 0), container.transform);
#endif
                }
                _spawnedObstacles[coord] = container;
            }
            else
            {
                // Standard single spawn
#if UNITY_EDITOR
                GameObject instance = (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(prefab, _parent);
                instance.transform.position = basePos;
                instance.transform.rotation = isTree ? Quaternion.Euler(0, Random.Range(0f, 360f), 0) : Quaternion.identity;
#else
                GameObject instance = Object.Instantiate(prefab, basePos, isTree ? Quaternion.Euler(0, Random.Range(0f, 360f), 0) : Quaternion.identity, _parent);
#endif
                instance.name = $"{prefab.name}_{q}_{r}";
                _spawnedObstacles[coord] = instance;
            }
        }

        private void ApplySymmetry<T>(int q, int r, T data, System.Action<int, int, T> action)
        {
            if (_config.Symmetry == SymmetryType.None) return;
            if (_config.SymmetryChaos > 0f && Random.value < _config.SymmetryChaos) return; // Chaos! Break symmetry here

            Vector2Int symCoord = _gridService.GetSymmetricCoordinate(q, r, _width, _height, _config.Symmetry);
            if (symCoord.x != q || symCoord.y != r)
            {
                action(symCoord.x, symCoord.y, data);
            }
        }
    }
}
