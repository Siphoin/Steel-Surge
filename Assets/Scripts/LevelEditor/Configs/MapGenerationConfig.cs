using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using SteelSurge.Main.Configs;

namespace SteelSurge.LevelEditor.Configs
{
    [CreateAssetMenu(fileName = "MapGenerationConfig", menuName = "SteelSurge/LevelEditor/MapGenerationConfig")]
    public class MapGenerationConfig : ScriptableConfig
    {
        [FoldoutGroup("Map Settings")]
        [SerializeField] private MapArchetype _archetype = MapArchetype.Standard;
        [FoldoutGroup("Map Settings")]
        [SerializeField] private float _hexSize = 1f;

        [FoldoutGroup("Prefabs")]
        [SerializeField, Required] private GameObject _hexGrassPrefab;
        [FoldoutGroup("Prefabs")]
        [SerializeField] private GameObject _cameraSetupPrefab;
        [FoldoutGroup("Prefabs")]
        [SerializeField] private GameObject _navMeshSurfacePrefab;

        [FoldoutGroup("Prefabs/Water")]
        [SerializeField] private GameObject _waterSmallPrefab;
        [FoldoutGroup("Prefabs/Water")]
        [SerializeField] private GameObject _waterBigPrefab;
        [FoldoutGroup("Prefabs/Water")]
        [SerializeField, Range(0f, 1f)] private float _riverChance = 0.3f;

        [FoldoutGroup("Prefabs/Obstacles")]
        [SerializeField, ListDrawerSettings(ShowIndexLabels = true)] 
        private List<GameObject> _mountainPrefabs = new List<GameObject>();
        
        [FoldoutGroup("Prefabs/Obstacles")]
        [SerializeField, ListDrawerSettings(ShowIndexLabels = true)] 
        private List<GameObject> _treePrefabs = new List<GameObject>();
        
        [FoldoutGroup("Prefabs/Obstacles")]
        [SerializeField, ListDrawerSettings(ShowIndexLabels = true)] 
        private List<GameObject> _rockPrefabs = new List<GameObject>();

        [FoldoutGroup("Biomes")]
        [SerializeField, Required] private Material _baseMaterial;
        [FoldoutGroup("Biomes")]
        [SerializeField, Range(0.01f, 1f)] private float _noiseScale = 0.1f;
        [FoldoutGroup("Biomes")]
        [SerializeField, Range(1, 5)] private int _noiseOctaves = 3;
        [FoldoutGroup("Biomes")]
        [SerializeField, Range(0f, 1f)] private float _noisePersistence = 0.5f;
        [FoldoutGroup("Biomes")]
        [SerializeField, Range(1f, 3f)] private float _noiseLacunarity = 2f;
        [FoldoutGroup("Biomes")]
        [SerializeField, ListDrawerSettings(ShowIndexLabels = true)] 
        private List<BiomeLayer> _biomeLayers = new List<BiomeLayer>();

        [FoldoutGroup("Points of Interest")]
        [SerializeField] private PoiSpawnMode _poiSpawnMode = PoiSpawnMode.CenterEdges;
        [FoldoutGroup("Points of Interest")]
        [SerializeField] private Material _poiSpotMaterial;
        [FoldoutGroup("Points of Interest")]
        [SerializeField, Min(0)] private int _poiSpotRadius = 2;
        [FoldoutGroup("Points of Interest")]
        [SerializeField, Range(0, 3)] private int _poiSpotRadiusVariation = 0;

        [FoldoutGroup("Obstacles Settings")]
        [SerializeField, Range(0f, 1f)] private float _rockDensity = 0.05f;
        [FoldoutGroup("Obstacles Settings")]
        [SerializeField, Range(0f, 1f)] private float _treeDensity = 0.1f;
        [FoldoutGroup("Obstacles Settings")]
        [SerializeField, Range(0.01f, 1f)] private float _treeNoiseScale = 0.2f;
        [FoldoutGroup("Obstacles Settings")]
        [SerializeField, Range(0f, 1f)] private float _treeNoiseThreshold = 0.6f;
        [FoldoutGroup("Obstacles Settings")]
        [SerializeField, Range(0f, 1f)] private float _treeClusterDensity = 0.8f;
        [FoldoutGroup("Obstacles Settings")]
        [SerializeField, Range(1, 5)] private int _treesPerHex = 1;
        [FoldoutGroup("Obstacles Settings")]
        [SerializeField, Range(0f, 1f)] private float _treeSpreadRadius = 0.5f;
        [FoldoutGroup("Obstacles Settings")]
        [SerializeField, Min(1)] private int _safeZoneRadius = 3;

        [FoldoutGroup("Border Settings")]
        [SerializeField, Range(1, 5)] private int _maxBorderDepth = 3;
        [FoldoutGroup("Border Settings")]
        [SerializeField, Range(0.01f, 1f)] private float _borderNoiseScale = 0.2f;
        [FoldoutGroup("Border Settings")]
        [SerializeField, Range(0f, 1f)] private float _borderNoiseThreshold = 0.4f;

        [FoldoutGroup("Symmetry")]
        [SerializeField] private SymmetryType _symmetryType = SymmetryType.Point;
        [FoldoutGroup("Symmetry")]
        [SerializeField, Range(0f, 1f)] private float _symmetryChaos = 0.05f;

        public MapArchetype Archetype => _archetype;
        public float HexSize => _hexSize;

        public GameObject HexGrassPrefab => _hexGrassPrefab;
        public GameObject CameraSetupPrefab => _cameraSetupPrefab;
        public GameObject NavMeshSurfacePrefab => _navMeshSurfacePrefab;

        public GameObject WaterSmallPrefab => _waterSmallPrefab;
        public GameObject WaterBigPrefab => _waterBigPrefab;
        public float RiverChance => _riverChance;

        public IReadOnlyList<GameObject> MountainPrefabs => _mountainPrefabs;
        public IReadOnlyList<GameObject> TreePrefabs => _treePrefabs;
        public IReadOnlyList<GameObject> RockPrefabs => _rockPrefabs;

        public Material BaseMaterial => _baseMaterial;
        public float NoiseScale => _noiseScale;
        public int NoiseOctaves => _noiseOctaves;
        public float NoisePersistence => _noisePersistence;
        public float NoiseLacunarity => _noiseLacunarity;
        public IReadOnlyList<BiomeLayer> BiomeLayers => _biomeLayers;

        public PoiSpawnMode PoiSpawnMode => _poiSpawnMode;
        public Material PoiSpotMaterial => _poiSpotMaterial;
        public int PoiSpotRadius => _poiSpotRadius;
        public int PoiSpotRadiusVariation => _poiSpotRadiusVariation;

        public float RockDensity => _rockDensity;
        public float TreeDensity => _treeDensity;
        public float TreeNoiseScale => _treeNoiseScale;
        public float TreeNoiseThreshold => _treeNoiseThreshold;
        public float TreeClusterDensity => _treeClusterDensity;
        public int TreesPerHex => _treesPerHex;
        public float TreeSpreadRadius => _treeSpreadRadius;
        public int SafeZoneRadius => _safeZoneRadius;

        public int MaxBorderDepth => _maxBorderDepth;
        public float BorderNoiseScale => _borderNoiseScale;
        public float BorderNoiseThreshold => _borderNoiseThreshold;

        public SymmetryType Symmetry => _symmetryType;
        public float SymmetryChaos => _symmetryChaos;
    }

    [Serializable]
    public class BiomeLayer
    {
        [Required] public Material Material;
        [Range(0f, 1f), Tooltip("Noise threshold above which this material is applied. Higher threshold = smaller patches.")]
        public float Threshold = 0.5f;
    }

    public enum SymmetryType
    {
        None,
        Point,
        Horizontal,
        Vertical
    }

    public enum MapArchetype
    {
        Standard,
        ChokePoint,
        Divide,
        Forest,
        Canyon,
        Plains,
        Lowland,
        Mountainous
    }

    public enum PoiSpawnMode
    {
        CenterEdges,
        RandomEdges,
        Diagonal
    }
}
