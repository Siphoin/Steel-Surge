using UnityEditor;
using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using SteelSurge.LevelEditor.Configs;
using SteelSurge.LevelEditor.Services;

namespace SteelSurge.LevelEditor.Editor
{
    public class MapGeneratorWindow : OdinEditorWindow
    {
        [MenuItem("SteelSurge/Level Editor")]
        private static void OpenWindow()
        {
            GetWindow<MapGeneratorWindow>("Level Editor").Show();
        }

        [Title("Map Generation Settings")]
        [InlineEditor(InlineEditorObjectFieldModes.Boxed)]
        [SerializeField] private MapGenerationConfig _config;

        public enum MapOrientation
        {
            Horizontal,
            Vertical
        }

        [Title("Map Size")]
        [SerializeField] private MapOrientation _orientation = MapOrientation.Horizontal;
        [SerializeField, Min(10)] private int _width = 24;
        [SerializeField, Min(10)] private int _height = 16;

        [Title("Environment")]
        [SerializeField] private Material _skybox;
        [SerializeField] private float _cameraOrthoSize = 33.5f;

        [Title("Generation Parameters")]
        [SerializeField] private string _arenaName = "NewArena";
        [SerializeField] private int _seed = 12345;
        [SerializeField] private bool _useRandomSeed = true;

        private MapGenerator _generator;

        private Texture2D _previewTexture;

        [Button(ButtonSizes.Large, Name = "Generate Preview"), GUIColor(0.2f, 0.6f, 0.8f)]
        private void GeneratePreview()
        {
            if (_config == null)
            {
                Debug.LogError("MapGenerationConfig is missing!");
                return;
            }

            if (_useRandomSeed)
            {
                _seed = Random.Range(0, int.MaxValue);
            }

            Random.InitState(_seed);
            
            int actualWidth = _orientation == MapOrientation.Horizontal ? Mathf.Max(_width, _height) : Mathf.Min(_width, _height);
            int actualHeight = _orientation == MapOrientation.Horizontal ? Mathf.Min(_width, _height) : Mathf.Max(_width, _height);

            int texWidth = actualWidth * 10;
            int texHeight = actualHeight * 10;
            
            if (_previewTexture != null)
            {
                DestroyImmediate(_previewTexture);
            }
            
            _previewTexture = new Texture2D(texWidth, texHeight, TextureFormat.RGBA32, false);
            _previewTexture.filterMode = FilterMode.Point;

            Color baseColor = new Color(0.3f, 0.6f, 0.3f); // Base Grass
            Color lightGrassColor = new Color(0.5f, 0.8f, 0.5f);
            Color sandColor = new Color(0.8f, 0.8f, 0.4f);
            Color dirtColor = new Color(0.5f, 0.3f, 0.1f);
            Color mountainColor = new Color(0.5f, 0.5f, 0.5f);
            Color treeColor = new Color(0.1f, 0.4f, 0.1f);
            Color rockColor = new Color(0.7f, 0.7f, 0.7f);
            Color poiColor = new Color(0.4f, 0.7f, 0.4f);

            // Fill with base color
            Color[] pixels = new Color[texWidth * texHeight];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = baseColor;

            // Generate Biomes
            float offsetX = Random.Range(-10000f, 10000f);
            float offsetY = Random.Range(-10000f, 10000f);
            int halfHeight = _config.Symmetry == SymmetryType.None ? actualHeight : actualHeight / 2;

            for (int r = 0; r < halfHeight; r++)
            {
                for (int q = 0; q < actualWidth; q++)
                {
                    float noiseValue = Mathf.PerlinNoise((q + offsetX) * _config.NoiseScale, (r + offsetY) * _config.NoiseScale);
                    Color hexColor = baseColor;

                    if (_config.BiomeLayers != null)
                    {
                        var sortedLayers = System.Linq.Enumerable.ToList(System.Linq.Enumerable.OrderByDescending(_config.BiomeLayers, l => l.Threshold));
                        foreach (var layer in sortedLayers)
                        {
                            if (noiseValue > layer.Threshold)
                            {
                                // Map material to color roughly
                                if (layer.Material.name.Contains("Dirt")) hexColor = dirtColor;
                                else if (layer.Material.name.Contains("Sand")) hexColor = sandColor;
                                else hexColor = lightGrassColor;
                                break;
                            }
                        }
                    }

                    DrawHex(pixels, texWidth, q, r, hexColor);
                    ApplySymmetryPreview(pixels, texWidth, actualWidth, actualHeight, q, r, hexColor, DrawHex);
                }
            }

            // POI Spots
            int keep1Q = _orientation == MapOrientation.Horizontal ? 2 : actualWidth / 2;
            int keep1R = _orientation == MapOrientation.Horizontal ? actualHeight / 2 : 2;
            
            DrawPoiSpot(pixels, texWidth, actualWidth, actualHeight, keep1Q, keep1R, poiColor);
            
            if (_config.Symmetry != SymmetryType.None)
            {
                Vector2Int keep2Coords = GetSymmetricCoordinate(keep1Q, keep1R, actualWidth, actualHeight);
                DrawPoiSpot(pixels, texWidth, actualWidth, actualHeight, keep2Coords.x, keep2Coords.y, poiColor);
            }
            else
            {
                int keep2Q = _orientation == MapOrientation.Horizontal ? actualWidth - 3 : actualWidth / 2;
                int keep2R = _orientation == MapOrientation.Horizontal ? actualHeight / 2 : actualHeight - 3;
                DrawPoiSpot(pixels, texWidth, actualWidth, actualHeight, keep2Q, keep2R, poiColor);
            }

            // Borders
            for (int r = 0; r < actualHeight; r++)
            {
                for (int q = 0; q < actualWidth; q++)
                {
                    if (q == 0 || q == actualWidth - 1 || r == 0 || r == actualHeight - 1)
                    {
                        DrawHex(pixels, texWidth, q, r, mountainColor);
                    }
                }
            }

            // Obstacles
            int numClusters = Mathf.RoundToInt((actualWidth * actualHeight) * _config.TreeDensity * 0.05f);
            System.Collections.Generic.List<Vector2Int> treeClusters = new System.Collections.Generic.List<Vector2Int>();
            for (int i = 0; i < numClusters; i++)
            {
                treeClusters.Add(new Vector2Int(Random.Range(2, actualWidth - 2), Random.Range(2, halfHeight)));
            }

            float treeOffsetX = Random.Range(-10000f, 10000f);
            float treeOffsetY = Random.Range(-10000f, 10000f);
            int centerQ = actualWidth / 2;
            int centerR = actualHeight / 2;
            int safeCenterRadius = Mathf.RoundToInt(Mathf.Min(actualWidth, actualHeight) * 0.25f);

            for (int r = 1; r < halfHeight; r++)
            {
                for (int q = 1; q < actualWidth - 1; q++)
                {
                    if (GetDistance(q, r, keep1Q, keep1R) <= _config.PoiSpotRadius) continue;
                    if (_config.Symmetry != SymmetryType.None)
                    {
                        Vector2Int keep2Coords = GetSymmetricCoordinate(keep1Q, keep1R, actualWidth, actualHeight);
                        if (GetDistance(q, r, keep2Coords.x, keep2Coords.y) <= _config.PoiSpotRadius) continue;
                    }
                    else
                    {
                        int keep2Q = _orientation == MapOrientation.Horizontal ? actualWidth - 3 : actualWidth / 2;
                        int keep2R = _orientation == MapOrientation.Horizontal ? actualHeight / 2 : actualHeight - 3;
                        if (GetDistance(q, r, keep2Q, keep2R) <= _config.PoiSpotRadius) continue;
                    }

                    bool canSpawnTrees = GetDistance(q, r, centerQ, centerR) > safeCenterRadius;
                    bool inTreeCluster = false;

                    if (canSpawnTrees)
                    {
                        foreach (var cluster in treeClusters)
                        {
                            int dist = GetDistance(q, r, cluster.x, cluster.y);
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
                        DrawHex(pixels, texWidth, q, r, treeColor);
                        ApplySymmetryPreview(pixels, texWidth, actualWidth, actualHeight, q, r, treeColor, DrawHex);
                    }
                    else if (Random.value < _config.RockDensity)
                    {
                        DrawHex(pixels, texWidth, q, r, rockColor);
                        ApplySymmetryPreview(pixels, texWidth, actualWidth, actualHeight, q, r, rockColor, DrawHex);
                    }
                }
            }

            _previewTexture.SetPixels(pixels);
            _previewTexture.Apply();
        }

        private void DrawHex(Color[] pixels, int texWidth, int q, int r, Color color)
        {
            int px = q * 10;
            int py = r * 10;
            if (r % 2 != 0) px += 5;

            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    int idx = (py + y) * texWidth + (px + x);
                    if (idx >= 0 && idx < pixels.Length)
                    {
                        pixels[idx] = color;
                    }
                }
            }
        }

        private void DrawPoiSpot(Color[] pixels, int texWidth, int actualWidth, int actualHeight, int centerQ, int centerR, Color color)
        {
            for (int r = 0; r < actualHeight; r++)
            {
                for (int q = 0; q < actualWidth; q++)
                {
                    if (GetDistance(q, r, centerQ, centerR) <= _config.PoiSpotRadius)
                    {
                        DrawHex(pixels, texWidth, q, r, color);
                    }
                }
            }
        }

        private void ApplySymmetryPreview(Color[] pixels, int texWidth, int actualWidth, int actualHeight, int q, int r, Color color, System.Action<Color[], int, int, int, Color> drawAction)
        {
            if (_config.Symmetry == SymmetryType.None) return;
            Vector2Int symCoord = GetSymmetricCoordinate(q, r, actualWidth, actualHeight);
            if (symCoord.x != q || symCoord.y != r)
            {
                drawAction(pixels, texWidth, symCoord.x, symCoord.y, color);
            }
        }

        private Vector2Int GetSymmetricCoordinate(int q, int r, int actualWidth, int actualHeight)
        {
            switch (_config.Symmetry)
            {
                case SymmetryType.Point: return new Vector2Int(actualWidth - 1 - q, actualHeight - 1 - r);
                case SymmetryType.Horizontal: return new Vector2Int(q, actualHeight - 1 - r);
                case SymmetryType.Vertical: return new Vector2Int(actualWidth - 1 - q, r);
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

        [OnInspectorGUI]
        private void DrawPreview()
        {
            if (_previewTexture != null)
            {
                GUILayout.Space(10);
                GUILayout.Label("Map Preview", EditorStyles.boldLabel);
                
                float aspect = (float)_previewTexture.width / _previewTexture.height;
                float width = EditorGUIUtility.currentViewWidth - 40;
                float height = width / aspect;
                
                Rect rect = GUILayoutUtility.GetRect(width, height);
                GUI.DrawTexture(rect, _previewTexture, ScaleMode.ScaleToFit);
            }
        }

        [Button(ButtonSizes.Large, Name = "Generate & Save Scene"), GUIColor(0.2f, 0.8f, 0.2f)]
        private void Generate()
        {
            if (_config == null)
            {
                Debug.LogError("MapGenerationConfig is missing!");
                return;
            }

            if (string.IsNullOrWhiteSpace(_arenaName))
            {
                Debug.LogError("Arena Name cannot be empty!");
                return;
            }

            if (_useRandomSeed)
            {
                _seed = Random.Range(0, int.MaxValue);
            }

            int actualWidth = _orientation == MapOrientation.Horizontal ? Mathf.Max(_width, _height) : Mathf.Min(_width, _height);
            int actualHeight = _orientation == MapOrientation.Horizontal ? Mathf.Min(_width, _height) : Mathf.Max(_width, _height);

            // Deselect everything BEFORE creating a new scene to prevent MissingReferenceException
            Selection.objects = new UnityEngine.Object[0];
            EditorApplication.delayCall += () =>
            {
                string scenePath = $"Assets/Scenes/Arenas/{_arenaName}.unity";
                
                // Create new scene
                var newScene = UnityEditor.SceneManagement.EditorSceneManager.NewScene(UnityEditor.SceneManagement.NewSceneSetup.EmptyScene, UnityEditor.SceneManagement.NewSceneMode.Single);
                newScene.name = _arenaName;

                // Setup Camera
                if (_config.CameraSetupPrefab != null)
                {
                    var camSetup = (GameObject)PrefabUtility.InstantiatePrefab(_config.CameraSetupPrefab);
                    camSetup.name = "CameraSetup";
                    
                    // Adjust camera position to center on the generated map
                    float mapWidthWorld = actualWidth * _config.HexSize * 2f;
                    float mapHeightWorld = actualHeight * _config.HexSize * 1.732051f;
                    
                    // Center of the map
                    Vector3 mapCenter = new Vector3(mapWidthWorld / 2f, 0, mapHeightWorld / 2f);
                    
                    if (_orientation == MapOrientation.Horizontal)
                    {
                        camSetup.transform.position = mapCenter + new Vector3(-60f, 70f, 0f);
                        camSetup.transform.rotation = Quaternion.Euler(50.1f, 90f, 0f);
                    }
                    else
                    {
                        // For vertical maps, rotate camera 90 degrees to look along Z axis
                        camSetup.transform.position = mapCenter + new Vector3(0f, 70f, -60f);
                        camSetup.transform.rotation = Quaternion.Euler(50.1f, 0f, 0f);
                    }
                    
                    // Adjust orthographic size if it's an orthographic camera
                    var cam = camSetup.GetComponentInChildren<Camera>();
                    if (cam != null && cam.orthographic)
                    {
                        cam.orthographicSize = _cameraOrthoSize;
                    }
                    
                    // Also adjust Cinemachine camera if present using reflection to avoid assembly dependency issues
                    var vcam = camSetup.GetComponentInChildren(System.Type.GetType("Unity.Cinemachine.CinemachineCamera, Unity.Cinemachine"));
                    if (vcam != null)
                    {
                        var lensProp = vcam.GetType().GetProperty("Lens");
                        if (lensProp != null)
                        {
                            var lens = lensProp.GetValue(vcam);
                            var orthoProp = lens.GetType().GetField("OrthographicSize");
                            if (orthoProp != null)
                            {
                                orthoProp.SetValue(lens, _cameraOrthoSize);
                                lensProp.SetValue(vcam, lens);
                            }
                        }
                    }
                }
                else
                {
                    // Fallback if no camera prefab
                    var cam = new GameObject("Main Camera");
                    cam.AddComponent<Camera>();
                    cam.tag = "MainCamera";
                    
                    var light = new GameObject("Directional Light");
                    var l = light.AddComponent<Light>();
                    l.type = LightType.Directional;
                    light.transform.rotation = Quaternion.Euler(50, -30, 0);
                }

                // Setup MapRoot
                GameObject mapRoot = new GameObject("MapRoot");
                
                _generator = new MapGenerator(_config, actualWidth, actualHeight, mapRoot.transform, _seed);
                _generator.Generate();

                if (_skybox != null)
                {
                    RenderSettings.skybox = _skybox;
                }
                
                // Save scene
                if (!System.IO.Directory.Exists("Assets/Scenes/Arenas"))
                {
                    System.IO.Directory.CreateDirectory("Assets/Scenes/Arenas");
                }
                
                UnityEditor.SceneManagement.EditorSceneManager.SaveScene(newScene, scenePath);
                
                Debug.Log($"Map generated and saved to {scenePath} with seed: {_seed}");
            };
        }

        [Button(ButtonSizes.Medium, Name = "Clear Map"), GUIColor(0.8f, 0.2f, 0.2f)]
        private void Clear()
        {
            if (_generator != null)
            {
                _generator.Clear();
            }
            else
            {
                GameObject mapRoot = GameObject.Find("MapRoot");
                if (mapRoot != null)
                {
                    for (int i = mapRoot.transform.childCount - 1; i >= 0; i--)
                    {
                        DestroyImmediate(mapRoot.transform.GetChild(i).gameObject);
                    }
                }
            }
            Debug.Log("Map cleared.");
        }
    }
}
