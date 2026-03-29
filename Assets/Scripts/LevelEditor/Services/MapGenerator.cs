using System.Collections.Generic;
using System.Linq;
using UnityEngine;
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

        public MapGenerator(MapGenerationConfig config, int width, int height, Transform parent, int seed)
        {
            _config = config;
            _width = width;
            _height = height;
            _parent = parent;
            _seed = seed;
            _gridService = new HexGridService(config.HexSize);
        }

        public void Generate()
        {
            Clear();
            Random.InitState(_seed);

            GenerateBaseGrid();
            GenerateBiomes();
            GeneratePoiSpots();
            GenerateBorders();
            GenerateObstacles();
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

        private void GenerateBaseGrid()
        {
            for (int r = 0; r < _height; r++)
            {
                for (int q = 0; q < _width; q++)
                {
                    SpawnHex(q, r, _config.HexGrassPrefab, _config.BaseMaterial);
                }
            }
        }

        private void GenerateBiomes()
        {
            if (_config.BiomeLayers == null || _config.BiomeLayers.Count == 0) return;

            // Sort layers by threshold descending so highest threshold is checked first
            var sortedLayers = _config.BiomeLayers.OrderByDescending(l => l.Threshold).ToList();

            float offsetX = Random.Range(-10000f, 10000f);
            float offsetY = Random.Range(-10000f, 10000f);

            int halfHeight = _config.Symmetry == SymmetryType.None ? _height : _height / 2;

            for (int r = 0; r < halfHeight; r++)
            {
                for (int q = 0; q < _width; q++)
                {
                    float noiseValue = Mathf.PerlinNoise((q + offsetX) * _config.NoiseScale, (r + offsetY) * _config.NoiseScale);
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
                }
            }
        }

        private void GeneratePoiSpots()
        {
            if (_config.PoiSpotMaterial == null) return;

            int keep1Q = 2;
            int keep1R = _height / 2;

            ApplyPoiSpot(keep1Q, keep1R);

            if (_config.Symmetry != SymmetryType.None)
            {
                Vector2Int keep2Coords = _gridService.GetSymmetricCoordinate(keep1Q, keep1R, _width, _height, _config.Symmetry);
                ApplyPoiSpot(keep2Coords.x, keep2Coords.y);
            }
            else
            {
                ApplyPoiSpot(_width - 3, _height / 2);
            }
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

        private void GenerateBorders()
        {
            if (_config.MountainPrefabs == null || _config.MountainPrefabs.Count == 0) return;

            for (int r = 0; r < _height; r++)
            {
                for (int q = 0; q < _width; q++)
                {
                    if (q == 0 || q == _width - 1 || r == 0 || r == _height - 1)
                    {
                        GameObject randomMountain = _config.MountainPrefabs[Random.Range(0, _config.MountainPrefabs.Count)];
                        SpawnObstacle(q, r, randomMountain, false);
                    }
                }
            }
        }

        private void GenerateObstacles()
        {
            int halfHeight = _config.Symmetry == SymmetryType.None ? _height : _height / 2;

            Vector2Int keep1 = new Vector2Int(2, _height / 2);
            Vector2Int keep2 = _config.Symmetry != SymmetryType.None 
                ? _gridService.GetSymmetricCoordinate(keep1.x, keep1.y, _width, _height, _config.Symmetry)
                : new Vector2Int(_width - 3, _height / 2);

            bool hasRocks = _config.RockPrefabs != null && _config.RockPrefabs.Count > 0;
            bool hasTrees = _config.TreePrefabs != null && _config.TreePrefabs.Count > 0;

            // Generate cluster centers for trees (Warcraft 3 style)
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
            int safeCenterRadius = Mathf.RoundToInt(_width * 0.25f); // Keep the center 25% of the map open

            for (int r = 1; r < halfHeight; r++)
            {
                for (int q = 1; q < _width - 1; q++)
                {
                    Vector2Int coord = new Vector2Int(q, r);
                    if (_spawnedObstacles.ContainsKey(coord)) continue;

                    // Strict check: Do not spawn ANY obstacles inside the POI radius
                    if (_gridService.GetDistance(q, r, keep1.x, keep1.y) <= _config.PoiSpotRadius ||
                        _gridService.GetDistance(q, r, keep2.x, keep2.y) <= _config.PoiSpotRadius)
                        continue;

                    // Also check SafeZoneRadius just in case it's larger than PoiSpotRadius
                    if (_gridService.GetDistance(q, r, keep1.x, keep1.y) <= _config.SafeZoneRadius ||
                        _gridService.GetDistance(q, r, keep2.x, keep2.y) <= _config.SafeZoneRadius)
                        continue;

                    GameObject prefabToSpawn = null;
                    bool canSpawnTrees = hasTrees;

                    // Keep the center of the map open (like ArenaForest)
                    if (_gridService.GetDistance(q, r, centerQ, centerR) <= safeCenterRadius)
                    {
                        canSpawnTrees = false; // No trees in the center
                    }

                    // Check if this hex is near any tree cluster center
                    bool inTreeCluster = false;
                    if (canSpawnTrees)
                    {
                        foreach (var cluster in treeClusters)
                        {
                            int dist = _gridService.GetDistance(q, r, cluster.x, cluster.y);
                            // Radius of cluster based on noise threshold (inverted for size)
                            int clusterRadius = Mathf.RoundToInt((1f - _config.TreeNoiseThreshold) * 10f);
                            
                            if (dist <= clusterRadius)
                            {
                                // The closer to center, the higher the chance to spawn
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
                        // Rocks are still random
                        prefabToSpawn = _config.RockPrefabs[Random.Range(0, _config.RockPrefabs.Count)];
                    }

                    if (prefabToSpawn != null)
                    {
                        SpawnObstacle(q, r, prefabToSpawn, inTreeCluster);
                        ApplySymmetry(q, r, prefabToSpawn, (sq, sr, p) => SpawnObstacle(sq, sr, p, inTreeCluster));
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

            Vector2Int symCoord = _gridService.GetSymmetricCoordinate(q, r, _width, _height, _config.Symmetry);
            if (symCoord.x != q || symCoord.y != r)
            {
                action(symCoord.x, symCoord.y, data);
            }
        }
    }
}
