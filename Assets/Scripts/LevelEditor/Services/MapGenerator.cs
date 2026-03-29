using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using Cysharp.Threading.Tasks;
using SteelSurge.LevelEditor.Configs;
using Unity.AI.Navigation;
using UnityEditor;

namespace SteelSurge.LevelEditor.Services
{
    public enum ObstacleType { None, Tree, Rock, Mountain }

    public class HexData
    {
        public int BiomeIndex = -1;
        public bool IsPoi = false;
        public ObstacleType ObsType = ObstacleType.None;
        public int ObsPrefabIndex = -1;
        public bool IsTreeCluster = false;
        
        public bool IsWater = false;
        public bool IsDeepWater = false;
    }

    public struct GenerationParams
    {
        public int Width;
        public int Height;
        public int Seed;
        public SymmetryType Symmetry;
        public float SymmetryChaos;
        public MapArchetype Archetype;
        public PoiSpawnMode PoiSpawnMode;
        public int PoiSpotRadius;
        public int SafeZoneRadius;
        
        public float NoiseScale;
        public int NoiseOctaves;
        public float NoisePersistence;
        public float NoiseLacunarity;
        public float[] BiomeThresholds;
        public int[] BiomeOriginalIndices;
        
        public int MaxBorderDepth;
        public float BorderNoiseScale;
        public float BorderNoiseThreshold;
        
        public float RiverChance;

        public float TreeDensity;
        public float TreeNoiseThreshold;
        public float TreeClusterDensity;
        public float RockDensity;
        
        public int MountainPrefabsCount;
        public int TreePrefabsCount;
        public int RockPrefabsCount;

        public bool HasWaterPrefab;
        public float WaterNoiseScale;
        public float WaterNoiseThreshold;
        public float DeepWaterNoiseThreshold;
    }

    public class MapGenerator
    {
        private readonly MapGenerationConfig _config;
        private readonly HexGridService _gridService;
        private readonly Transform _parent;
        private readonly int _seed;
        private readonly int _width;
        private readonly int _height;
        private readonly GameObject _navMeshSurfacePrefab;

        private Dictionary<Vector2Int, GameObject> _spawnedHexes = new Dictionary<Vector2Int, GameObject>();
        private Dictionary<Vector2Int, GameObject> _spawnedObstacles = new Dictionary<Vector2Int, GameObject>();
        private NavMeshSurface _navMeshSurface;

        public Vector2Int Poi1 { get; private set; }
        public Vector2Int Poi2 { get; private set; }

        public MapGenerator(MapGenerationConfig config, int width, int height, Transform parent, int seed, GameObject navMeshSurfacePrefab = null)
        {
            _config = config;
            _width = width;
            _height = height;
            _parent = parent;
            _seed = seed;
            _navMeshSurfacePrefab = navMeshSurfacePrefab;
            _gridService = new HexGridService(config.HexSize);
        }

        public async UniTask GenerateAsync()
        {
            Clear();

            // 1. Prepare data for background thread (Unity Objects cannot be accessed off main thread)
            GenerationParams genParams = PrepareParams();

            // 2. Run heavy calculations on Thread Pool
            var result = await UniTask.RunOnThreadPool(() => GenerateData(genParams));

            HexData[,] gridData = result.Grid;
            Poi1 = result.Poi1;
            Poi2 = result.Poi2;

            // 3. Instantiate on Main Thread
            await InstantiateMapAsync(gridData);

            // 4. Bake NavMesh
            await BakeNavMeshAsync();
        }

        private GenerationParams PrepareParams()
        {
            var p = new GenerationParams
            {
                Width = _width,
                Height = _height,
                Seed = _seed,
                Symmetry = _config.Symmetry,
                SymmetryChaos = _config.SymmetryChaos,
                Archetype = _config.Archetype,
                PoiSpawnMode = _config.PoiSpawnMode,
                PoiSpotRadius = _config.PoiSpotRadius,
                SafeZoneRadius = _config.SafeZoneRadius,
                
                NoiseScale = _config.NoiseScale,
                NoiseOctaves = _config.NoiseOctaves,
                NoisePersistence = _config.NoisePersistence,
                NoiseLacunarity = _config.NoiseLacunarity,
                
                MaxBorderDepth = _config.MaxBorderDepth,
                BorderNoiseScale = _config.BorderNoiseScale,
                BorderNoiseThreshold = _config.BorderNoiseThreshold,
                
                RiverChance = _config.RiverChance,

                TreeDensity = _config.TreeDensity,
                TreeNoiseThreshold = _config.TreeNoiseThreshold,
                TreeClusterDensity = _config.TreeClusterDensity,
                RockDensity = _config.RockDensity,
                
                MountainPrefabsCount = _config.MountainPrefabs?.Count ?? 0,
                TreePrefabsCount = _config.TreePrefabs?.Count ?? 0,
                RockPrefabsCount = _config.RockPrefabs?.Count ?? 0,
                
                HasWaterPrefab = _config.WaterSmallPrefab != null,
                WaterNoiseScale = _config.NoiseScale * 1.5f, // Slightly smaller patches than biomes
                WaterNoiseThreshold = 0.65f, // Threshold for shallow water
                DeepWaterNoiseThreshold = 0.75f // Threshold for deep water
            };

            if (_config.BiomeLayers != null)
            {
                var sorted = _config.BiomeLayers
                    .Select((layer, index) => new { layer, index })
                    .OrderByDescending(x => x.layer.Threshold)
                    .ToList();
                    
                p.BiomeThresholds = sorted.Select(x => x.layer.Threshold).ToArray();
                p.BiomeOriginalIndices = sorted.Select(x => x.index).ToArray();
            }
            else
            {
                p.BiomeThresholds = new float[0];
                p.BiomeOriginalIndices = new int[0];
            }

            return p;
        }

        private (HexData[,] Grid, Vector2Int Poi1, Vector2Int Poi2) GenerateData(GenerationParams p)
        {
            System.Random rng = new System.Random(p.Seed);
            HexData[,] grid = new HexData[p.Width, p.Height];
            for (int x = 0; x < p.Width; x++)
            {
                for (int y = 0; y < p.Height; y++)
                {
                    grid[x, y] = new HexData();
                }
            }

            // Calculate POIs
            bool isVertical = p.Width < p.Height;
            int q1 = isVertical ? p.Width / 2 : 2;
            int r1 = isVertical ? 2 : p.Height / 2;

            if (p.PoiSpawnMode == PoiSpawnMode.RandomEdges)
            {
                if (isVertical) q1 = rng.Next(2, p.Width - 2);
                else r1 = rng.Next(2, p.Height - 2);
            }
            else if (p.PoiSpawnMode == PoiSpawnMode.Diagonal)
            {
                if (isVertical) q1 = rng.NextDouble() > 0.5 ? 2 : p.Width - 3;
                else r1 = rng.NextDouble() > 0.5 ? 2 : p.Height - 3;
            }

            Vector2Int poi1 = new Vector2Int(q1, r1);
            Vector2Int poi2;

            if (p.Symmetry != SymmetryType.None)
            {
                poi2 = GetSymmetricCoordinate(poi1.x, poi1.y, p.Width, p.Height, p.Symmetry);
            }
            else
            {
                int q2 = isVertical ? p.Width / 2 : p.Width - 3;
                int r2 = isVertical ? p.Height - 3 : p.Height / 2;
                
                if (p.PoiSpawnMode == PoiSpawnMode.RandomEdges)
                {
                    if (isVertical) q2 = rng.Next(2, p.Width - 2);
                    else r2 = rng.Next(2, p.Height - 2);
                }
                else if (p.PoiSpawnMode == PoiSpawnMode.Diagonal)
                {
                    if (isVertical) q2 = (p.Width - 1) - poi1.x;
                    else r2 = (p.Height - 1) - poi1.y;
                }
                poi2 = new Vector2Int(q2, r2);
            }

            // Mark POI spots
            MarkPoiSpot(grid, p, poi1.x, poi1.y);
            MarkPoiSpot(grid, p, poi2.x, poi2.y);

            // Biomes & Water
            float offsetX = (float)(rng.NextDouble() * 20000.0 - 10000.0);
            float offsetY = (float)(rng.NextDouble() * 20000.0 - 10000.0);
            int halfHeight = p.Symmetry == SymmetryType.None ? p.Height : p.Height / 2;

            for (int r = 0; r < halfHeight; r++)
            {
                for (int q = 0; q < p.Width; q++)
                {
                    float noiseValue = GetFractalNoise(q + offsetX, r + offsetY, p);
                    int biomeIdx = -1;

                    for (int i = 0; i < p.BiomeThresholds.Length; i++)
                    {
                        if (noiseValue > p.BiomeThresholds[i])
                        {
                            biomeIdx = p.BiomeOriginalIndices[i];
                            break;
                        }
                    }

                    if (biomeIdx != -1)
                    {
                        ApplySymmetryToGrid(grid, p, rng, q, r, d => d.BiomeIndex = biomeIdx);
                    }

                    // Generate Lakes
                    if (p.HasWaterPrefab)
                    {
                        float waterNoise = GetFractalNoise(q + offsetX + 5000f, r + offsetY + 5000f, p);
                        if (waterNoise > p.WaterNoiseThreshold)
                        {
                            // Don't spawn water on POIs
                            if (GetDistance(q, r, poi1.x, poi1.y) > p.PoiSpotRadius &&
                                GetDistance(q, r, poi2.x, poi2.y) > p.PoiSpotRadius)
                            {
                                bool isDeep = waterNoise > p.DeepWaterNoiseThreshold;
                                ApplySymmetryToGrid(grid, p, rng, q, r, d => 
                                {
                                    d.IsWater = true;
                                    d.IsDeepWater = isDeep;
                                });
                            }
                        }
                    }
                }
            }

            // Random River Generation
            if (p.HasWaterPrefab && rng.NextDouble() < p.RiverChance)
            {
                int riverQ = p.Width / 2;
                int riverCenterR = p.Height / 2;
                
                // Add some noise to the river path
                float riverNoiseOffset = (float)(rng.NextDouble() * 1000f);
                
                for (int r = 0; r < p.Height; r++)
                {
                    // Create bridges (land) at specific intervals
                    bool isBridge = (r == riverCenterR || r == riverCenterR - 4 || r == riverCenterR + 4);
                    
                    if (!isBridge)
                    {
                        // Wiggle the river slightly
                        int wiggle = Mathf.RoundToInt(Mathf.PerlinNoise(r * 0.2f, riverNoiseOffset) * 2f - 1f);
                        int currentQ = Mathf.Clamp(riverQ + wiggle, 2, p.Width - 3);

                        grid[currentQ, r].IsWater = true;
                        grid[currentQ, r].IsDeepWater = true;
                        
                        // Shallow banks
                        grid[currentQ - 1, r].IsWater = true;
                        grid[currentQ - 1, r].IsDeepWater = false;
                        grid[currentQ + 1, r].IsWater = true;
                        grid[currentQ + 1, r].IsDeepWater = false;

                        if (p.Symmetry != SymmetryType.None)
                        {
                            Vector2Int symCoord = GetSymmetricCoordinate(currentQ, r, p.Width, p.Height, p.Symmetry);
                            grid[symCoord.x, symCoord.y].IsWater = true;
                            grid[symCoord.x, symCoord.y].IsDeepWater = true;
                            
                            if (symCoord.x > 0)
                            {
                                grid[symCoord.x - 1, symCoord.y].IsWater = true;
                                grid[symCoord.x - 1, symCoord.y].IsDeepWater = false;
                            }
                            if (symCoord.x < p.Width - 1)
                            {
                                grid[symCoord.x + 1, symCoord.y].IsWater = true;
                                grid[symCoord.x + 1, symCoord.y].IsDeepWater = false;
                            }
                        }
                    }
                    else
                    {
                        // Ensure bridges are clear of water
                        int currentQ = riverQ;
                        grid[currentQ, r].IsWater = false;
                        grid[currentQ - 1, r].IsWater = false;
                        grid[currentQ + 1, r].IsWater = false;
                        
                        if (p.Symmetry != SymmetryType.None)
                        {
                            Vector2Int symCoord = GetSymmetricCoordinate(currentQ, r, p.Width, p.Height, p.Symmetry);
                            grid[symCoord.x, symCoord.y].IsWater = false;
                            if (symCoord.x > 0) grid[symCoord.x - 1, symCoord.y].IsWater = false;
                            if (symCoord.x < p.Width - 1) grid[symCoord.x + 1, symCoord.y].IsWater = false;
                        }
                    }
                }
            }

            // Borders
            float borderOffsetX = (float)(rng.NextDouble() * 20000.0 - 10000.0);
            float borderOffsetY = (float)(rng.NextDouble() * 20000.0 - 10000.0);

            for (int r = 0; r < halfHeight; r++)
            {
                for (int q = 0; q < p.Width; q++)
                {
                    int distX = Mathf.Min(q, p.Width - 1 - q);
                    int distY = Mathf.Min(r, p.Height - 1 - r);
                    int distToEdge = Mathf.Min(distX, distY);

                    bool isMountain = false;

                    if (distToEdge == 0)
                    {
                        isMountain = true;
                    }
                    else if (distToEdge <= p.MaxBorderDepth)
                    {
                        float noise = Mathf.PerlinNoise((q + borderOffsetX) * p.BorderNoiseScale, (r + borderOffsetY) * p.BorderNoiseScale);
                        float adjustedThreshold = p.BorderNoiseThreshold + (distToEdge * 0.15f);
                        if (noise > adjustedThreshold)
                        {
                            isMountain = true;
                        }
                    }

                    // Do not spawn mountains on water
                    if (isMountain && p.MountainPrefabsCount > 0 && !grid[q, r].IsWater)
                    {
                        int prefabIdx = rng.Next(0, p.MountainPrefabsCount);
                        ApplySymmetryToGrid(grid, p, rng, q, r, d =>
                        {
                            d.ObsType = ObstacleType.Mountain;
                            d.ObsPrefabIndex = prefabIdx;
                        });
                    }
                }
            }

            // Obstacles
            bool hasRocks = p.RockPrefabsCount > 0;
            bool hasTrees = p.TreePrefabsCount > 0;

            int numClusters = Mathf.RoundToInt((p.Width * p.Height) * p.TreeDensity * 0.05f);
            List<Vector2Int> treeClusters = new List<Vector2Int>();
            for (int i = 0; i < numClusters; i++)
            {
                treeClusters.Add(new Vector2Int(rng.Next(2, p.Width - 2), rng.Next(2, halfHeight)));
            }

            int centerQ = p.Width / 2;
            int centerR = p.Height / 2;
            int safeCenterRadius = Mathf.RoundToInt(Mathf.Min(p.Width, p.Height) * 0.25f);

            for (int r = 1; r < halfHeight; r++)
            {
                for (int q = 1; q < p.Width - 1; q++)
                {
                    if (grid[q, r].ObsType != ObstacleType.None) continue; // Already occupied (e.g. border)
                    if (grid[q, r].IsWater) continue; // Do not spawn obstacles on water

                    if (GetDistance(q, r, poi1.x, poi1.y) <= p.PoiSpotRadius ||
                        GetDistance(q, r, poi2.x, poi2.y) <= p.PoiSpotRadius)
                        continue;

                    if (GetDistance(q, r, poi1.x, poi1.y) <= p.SafeZoneRadius ||
                        GetDistance(q, r, poi2.x, poi2.y) <= p.SafeZoneRadius)
                        continue;

                    bool canSpawnTrees = hasTrees;
                    bool forceMountain = false;

                    if (p.Archetype == MapArchetype.ChokePoint)
                    {
                        if (Mathf.Abs(q - centerQ) <= 1 && Mathf.Abs(r - centerR) > 2)
                            forceMountain = true;
                    }

                    if (forceMountain && p.MountainPrefabsCount > 0)
                    {
                        int prefabIdx = rng.Next(0, p.MountainPrefabsCount);
                        ApplySymmetryToGrid(grid, p, rng, q, r, d => 
                        {
                            d.ObsType = ObstacleType.Mountain;
                            d.ObsPrefabIndex = prefabIdx;
                        });
                        continue;
                    }

                    if (p.Archetype == MapArchetype.Standard && GetDistance(q, r, centerQ, centerR) <= safeCenterRadius)
                    {
                        canSpawnTrees = false;
                    }

                    bool inTreeCluster = false;
                    if (canSpawnTrees)
                    {
                        foreach (var cluster in treeClusters)
                        {
                            int dist = GetDistance(q, r, cluster.x, cluster.y);
                            int clusterRadius = Mathf.RoundToInt((1f - p.TreeNoiseThreshold) * 10f);
                            
                            if (dist <= clusterRadius)
                            {
                                float spawnChance = p.TreeClusterDensity * (1f - (float)dist / (clusterRadius + 1));
                                if (rng.NextDouble() < spawnChance)
                                {
                                    inTreeCluster = true;
                                    break;
                                }
                            }
                        }
                    }
                    
                    if (inTreeCluster)
                    {
                        int prefabIdx = rng.Next(0, p.TreePrefabsCount);
                        ApplySymmetryToGrid(grid, p, rng, q, r, d => 
                        {
                            d.ObsType = ObstacleType.Tree;
                            d.ObsPrefabIndex = prefabIdx;
                            d.IsTreeCluster = true;
                        });
                    }
                    else if (hasRocks && rng.NextDouble() < p.RockDensity)
                    {
                        int prefabIdx = rng.Next(0, p.RockPrefabsCount);
                        ApplySymmetryToGrid(grid, p, rng, q, r, d => 
                        {
                            d.ObsType = ObstacleType.Rock;
                            d.ObsPrefabIndex = prefabIdx;
                        });
                    }
                }
            }

            return (grid, poi1, poi2);
        }

        private void MarkPoiSpot(HexData[,] grid, GenerationParams p, int centerQ, int centerR)
        {
            for (int r = 0; r < p.Height; r++)
            {
                for (int q = 0; q < p.Width; q++)
                {
                    if (GetDistance(q, r, centerQ, centerR) <= p.PoiSpotRadius)
                    {
                        grid[q, r].IsPoi = true;
                    }
                }
            }
        }

        private void ApplySymmetryToGrid(HexData[,] grid, GenerationParams p, System.Random rng, int q, int r, System.Action<HexData> action)
        {
            action(grid[q, r]);

            if (p.Symmetry == SymmetryType.None) return;
            if (p.SymmetryChaos > 0f && rng.NextDouble() < p.SymmetryChaos) return;

            Vector2Int symCoord = GetSymmetricCoordinate(q, r, p.Width, p.Height, p.Symmetry);
            if (symCoord.x != q || symCoord.y != r)
            {
                action(grid[symCoord.x, symCoord.y]);
            }
        }

        private float GetFractalNoise(float x, float y, GenerationParams p)
        {
            float amplitude = 1f;
            float frequency = 1f;
            float noiseHeight = 0f;
            float maxAmplitude = 0f;

            for (int i = 0; i < p.NoiseOctaves; i++)
            {
                float sampleX = x * p.NoiseScale * frequency;
                float sampleY = y * p.NoiseScale * frequency;
                noiseHeight += Mathf.PerlinNoise(sampleX, sampleY) * amplitude;
                maxAmplitude += amplitude;
                amplitude *= p.NoisePersistence;
                frequency *= p.NoiseLacunarity;
            }

            return noiseHeight / maxAmplitude;
        }

        private Vector2Int GetSymmetricCoordinate(int q, int r, int width, int height, SymmetryType symmetry)
        {
            switch (symmetry)
            {
                case SymmetryType.Point: return new Vector2Int(width - 1 - q, height - 1 - r);
                case SymmetryType.Horizontal: return new Vector2Int(q, height - 1 - r);
                case SymmetryType.Vertical: return new Vector2Int(width - 1 - q, r);
                default: return new Vector2Int(q, r);
            }
        }

        private int GetDistance(int q1, int r1, int q2, int r2)
        {
            int q1Cube = q1 - (r1 - (r1 & 1)) / 2;
            int q2Cube = q2 - (r2 - (r2 & 1)) / 2;
            int s1 = -q1Cube - r1;
            int s2 = -q2Cube - r2;
            return Mathf.Max(Mathf.Abs(q1Cube - q2Cube), Mathf.Abs(r1 - r2), Mathf.Abs(s1 - s2));
        }

        private async UniTask InstantiateMapAsync(HexData[,] grid)
        {
            int batchSize = 50;
            int count = 0;

            UnityEngine.Random.InitState(_seed);

            for (int r = 0; r < _height; r++)
            {
                for (int q = 0; q < _width; q++)
                {
                    HexData data = grid[q, r];

                    // 1. Spawn Base Hex or Water
                    if (data.IsWater)
                    {
                        GameObject waterPrefab = data.IsDeepWater ? _config.WaterBigPrefab : _config.WaterSmallPrefab;
                        if (waterPrefab != null)
                        {
                            SpawnHex(q, r, waterPrefab, null);
                        }
                        else
                        {
                            // Fallback if prefabs are missing
                            SpawnHex(q, r, _config.HexGrassPrefab, _config.BaseMaterial);
                        }
                    }
                    else
                    {
                        Material mat = _config.BaseMaterial;
                        if (data.IsPoi && _config.PoiSpotMaterial != null) 
                            mat = _config.PoiSpotMaterial;
                        else if (data.BiomeIndex >= 0 && data.BiomeIndex < _config.BiomeLayers.Count) 
                            mat = _config.BiomeLayers[data.BiomeIndex].Material;

                        SpawnHex(q, r, _config.HexGrassPrefab, mat);
                    }

                    // 2. Spawn Obstacle
                    if (data.ObsType != ObstacleType.None)
                    {
                        GameObject prefab = null;
                        bool isTree = false;

                        if (data.ObsType == ObstacleType.Mountain && data.ObsPrefabIndex < _config.MountainPrefabs.Count)
                            prefab = _config.MountainPrefabs[data.ObsPrefabIndex];
                        else if (data.ObsType == ObstacleType.Rock && data.ObsPrefabIndex < _config.RockPrefabs.Count)
                            prefab = _config.RockPrefabs[data.ObsPrefabIndex];
                        else if (data.ObsType == ObstacleType.Tree && data.ObsPrefabIndex < _config.TreePrefabs.Count)
                        {
                            prefab = _config.TreePrefabs[data.ObsPrefabIndex];
                            isTree = true;
                        }

                        if (prefab != null)
                        {
                            SpawnObstacle(q, r, prefab, isTree);
                        }
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

        private void SpawnHex(int q, int r, GameObject prefab, Material material, float rotationY = 0f)
        {
            if (prefab == null) return;
            Vector3 pos = _gridService.GetWorldPosition(q, r);
            Quaternion rot = Quaternion.Euler(0, rotationY, 0);
            
#if UNITY_EDITOR
            GameObject instance = (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(prefab, _parent);
            instance.transform.position = pos;
            instance.transform.rotation = rot;
#else
            GameObject instance = Object.Instantiate(prefab, pos, rot, _parent);
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

        private void SpawnObstacle(int q, int r, GameObject prefab, bool isTree = false)
        {
            if (prefab == null) return;
            Vector2Int coord = new Vector2Int(q, r);
            if (_spawnedObstacles.ContainsKey(coord)) return;

            Vector3 basePos = _gridService.GetWorldPosition(q, r);
            
            if (isTree && _config.TreesPerHex > 1)
            {
                GameObject container = new GameObject($"Trees_{q}_{r}");
                container.transform.SetParent(_parent);
                container.transform.position = basePos;

                for (int i = 0; i < _config.TreesPerHex; i++)
                {
                    Vector2 randomCircle = UnityEngine.Random.insideUnitCircle * _config.TreeSpreadRadius;
                    Vector3 offsetPos = basePos + new Vector3(randomCircle.x, 0, randomCircle.y);
                    
#if UNITY_EDITOR
                    GameObject instance = (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(prefab, container.transform);
                    instance.transform.position = offsetPos;
                    instance.transform.rotation = Quaternion.Euler(0, UnityEngine.Random.Range(0f, 360f), 0);
#else
                    GameObject instance = Object.Instantiate(prefab, offsetPos, Quaternion.Euler(0, UnityEngine.Random.Range(0f, 360f), 0), container.transform);
#endif
                }
                _spawnedObstacles[coord] = container;
            }
            else
            {
#if UNITY_EDITOR
                GameObject instance = (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(prefab, _parent);
                instance.transform.position = basePos;
                instance.transform.rotation = isTree ? Quaternion.Euler(0, UnityEngine.Random.Range(0f, 360f), 0) : Quaternion.identity;
#else
                GameObject instance = Object.Instantiate(prefab, basePos, isTree ? Quaternion.Euler(0, UnityEngine.Random.Range(0f, 360f), 0) : Quaternion.identity, _parent);
#endif
                instance.name = $"{prefab.name}_{q}_{r}";
                _spawnedObstacles[coord] = instance;
            }
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

            for (int i = _parent.childCount - 1; i >= 0; i--)
            {
                Object.DestroyImmediate(_parent.GetChild(i).gameObject);
            }
        }

        private async UniTask BakeNavMeshAsync()
        {
            if (_navMeshSurfacePrefab == null)
            {
                Debug.LogWarning("NavMeshSurface prefab not assigned. Skipping NavMesh bake.");
                return;
            }

            // Spawn NavMeshSurface from prefab
            var navMeshSurfaceObj = (GameObject)PrefabUtility.InstantiatePrefab(_navMeshSurfacePrefab, _parent);
            navMeshSurfaceObj.name = "NavMeshSurface";
            _navMeshSurface = navMeshSurfaceObj.GetComponent<NavMeshSurface>();

            if (_navMeshSurface == null)
            {
                Debug.LogError("NavMeshSurface component not found on prefab!");
                return;
            }

            // Build NavMesh on main thread
            _navMeshSurface.BuildNavMesh();

            Debug.Log($"NavMesh baked successfully.");
        }
    }
}