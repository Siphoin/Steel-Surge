# Level Editor — Technical Documentation

## Overview

The Level Editor is a procedural map generation system for hexagonal grid-based maps in Unity. It generates complete arena maps with biomes, water features, obstacles, and symmetric player spawn points.

**Assembly:** `LevelEditor.asmdef`  
**Namespace:** `SteelSurge.LevelEditor`

---

## Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                     MapGenerationConfig                      │
│  (ScriptableObject — Configuration Data)                     │
└─────────────────────┬───────────────────────────────────────┘
                      │
                      ▼
┌─────────────────────────────────────────────────────────────┐
│                       MapGenerator                           │
│  (Core Generation Logic — Main Thread + Thread Pool)         │
│  ┌──────────────────────────────────────────────────────┐   │
│  │ 1. PrepareParams()    — Config → GenerationParams    │   │
│  │ 2. GenerateData()     — Thread Pool, produces HexData │   │
│  │ 3. InstantiateMapAsync() — Main Thread, spawns prefabs│   │
│  │ 4. BakeNavMeshAsync()  — NavMesh baking              │   │
│  └──────────────────────────────────────────────────────┘   │
└─────────────────────┬───────────────────────────────────────┘
                      │
                      ▼
┌─────────────────────────────────────────────────────────────┐
│                      HexGridService                          │
│  (Hex Grid Mathematics — Coordinate Conversion, Distance)    │
└─────────────────────────────────────────────────────────────┘
```

**NavMesh Integration:**
- `NavMeshSurfacePrefab` is stored in `MapGenerationConfig`
- After map instantiation, prefab is spawned and `BuildNavMesh()` is called
- Prefab should contain `NavMeshSurface` component configured for your scene

---

## Components

### 1. MapGenerationConfig

**Path:** `Configs/MapGenerationConfig.cs`  
**Type:** `ScriptableObject` (inherits `ScriptableConfig`)

Configuration asset that stores all generation parameters.

#### Properties

| Category | Property | Type | Description |
|----------|----------|------|-------------|
| **Map** | `Archetype` | `MapArchetype` | Map layout type (`Standard`, `ChokePoint`, `Divide`, `Forest`, `Canyon`, `Plains`, `Lowland`, `Mountainous`) |
| | `HexSize` | `float` | Size of hexagon in world units (default: 1.0) |
| **Prefabs** | `HexGrassPrefab` | `GameObject` | Base hex tile prefab |
| | `CameraSetupPrefab` | `GameObject` | Camera rig prefab |
| | `WaterSmallPrefab` | `GameObject` | Shallow water prefab |
| | `WaterBigPrefab` | `GameObject` | Deep water prefab |
| | `MountainPrefabs` | `List<GameObject>` | Mountain obstacle prefabs |
| | `TreePrefabs` | `List<GameObject>` | Tree obstacle prefabs |
| | `RockPrefabs` | `List<GameObject>` | Rock obstacle prefabs |
| **Biomes** | `BaseMaterial` | `Material` | Default ground material |
| | `NoiseScale` | `float` | Perlin noise scale for biomes |
| | `NoiseOctaves` | `int` | Fractal noise octaves |
| | `NoisePersistence` | `float` | Amplitude multiplier per octave |
| | `NoiseLacunarity` | `float` | Frequency multiplier per octave |
| | `BiomeLayers` | `List<BiomeLayer>` | Biome materials with thresholds |
| **POI** | `PoiSpawnMode` | `PoiSpawnMode` | Spawn mode (`CenterEdges`, `RandomEdges`, `Diagonal`) |
| | `PoiSpotMaterial` | `Material` | Material for base spawn zones |
| | `PoiSpotRadius` | `int` | Radius of cleared zone around POI |
| **Obstacles** | `RockDensity` | `float` | Rock spawn probability |
| | `TreeDensity` | `float` | Tree cluster density |
| | `TreeNoiseThreshold` | `float` | Noise threshold for tree clusters |
| | `TreeClusterDensity` | `float` | Spawn chance within cluster |
| | `SafeZoneRadius` | `int` | No-spawn radius around POIs |
| **Borders** | `MaxBorderDepth` | `int` | Maximum mountain border depth |
| | `BorderNoiseScale` | `float` | Noise scale for jagged borders |
| | `BorderNoiseThreshold` | `float` | Noise threshold for mountains |
| **Symmetry** | `Symmetry` | `SymmetryType` | Symmetry type (`None`, `Point`, `Horizontal`, `Vertical`) |
| | `SymmetryChaos` | `float` | Chance to break symmetry per hex |

#### BiomeLayer Structure

```csharp
[Serializable]
public class BiomeLayer
{
    public Material Material;      // Material to apply
    public float Threshold;        // Noise threshold (higher = smaller patches)
}
```

---

### 2. MapGenerator

**Path:** `Services/MapGenerator.cs`  
**Type:** `Class`

Core generation engine. Executes in three phases using UniTask for async/await pattern.

#### Constructor

```csharp
public MapGenerator(MapGenerationConfig config, int width, int height, Transform parent, int seed, GameObject navMeshSurfacePrefab = null)
```

| Parameter | Type | Description |
|-----------|------|-------------|
| `config` | `MapGenerationConfig` | Configuration asset |
| `width` | `int` | Map width in hexes |
| `height` | `int` | Map height in hexes |
| `parent` | `Transform` | Parent transform for spawned objects |
| `seed` | `int` | Random seed for reproducibility |
| `navMeshSurfacePrefab` | `GameObject` | Optional. Prefab with NavMeshSurface component (also read from config). |

#### Public API

| Method | Returns | Description |
|--------|---------|-------------|
| `GenerateAsync()` | `UniTask` | Main entry point. Executes full generation pipeline. |
| `Poi1` | `Vector2Int` | Coordinates of first POI (getter) |
| `Poi2` | `Vector2Int` | Coordinates of second POI (getter) |

#### Generation Pipeline

```
┌─────────────────┐     ┌─────────────────┐     ┌─────────────────┐     ┌─────────────────┐
│  PrepareParams  │ ──► │  GenerateData   │ ──► │ InstantiateMap  │ ──► │  BakeNavMesh    │
│  (Main Thread)  │     │  (Thread Pool)  │     │  (Main Thread)  │     │  (Thread Pool)  │
└─────────────────┘     └─────────────────┘     └─────────────────┘     └─────────────────┘
```

##### Phase 1: PrepareParams()

Converts `MapGenerationConfig` to `GenerationParams` struct. Sorts biome layers by threshold (descending).

##### Phase 2: GenerateData()

**Thread:** Thread Pool (via `UniTask.RunOnThreadPool`)  
**Returns:** `(HexData[,] Grid, Vector2Int Poi1, Vector2Int Poi2)`

Generation steps (in order):

1. **Initialize Grid** — Creates `HexData[,]` array
2. **Calculate POIs** — Determines spawn points based on `PoiSpawnMode` and `Symmetry`
3. **Mark POI Spots** — Sets `IsPoi = true` within `PoiSpotRadius`
4. **Generate Biomes** — Fractal noise → biome assignment
5. **Generate Water** — Separate noise pass for lakes (avoids POIs)
6. **Generate River** — Optional vertical river with bridges (if `RiverChance` met)
7. **Generate Borders** — Mountains on map edges with jagged noise
8. **Generate Obstacles** — Trees (clustered) and rocks (random)

##### Phase 3: InstantiateMapAsync()

**Thread:** Main Thread (Unity objects)
**Batching:** Yields every 50 hexes via `UniTask.Yield()`

Spawns prefabs based on `HexData`:
- Water hexes → `WaterSmallPrefab` / `WaterBigPrefab`
- Land hexes → `HexGrassPrefab` with biome material
- Obstacles → Respective prefab at hex position

##### Phase 4: BakeNavMeshAsync()

**Thread:** Main Thread

1. Instantiates `NavMeshSurfacePrefab` under map root
2. Calls `NavMeshSurface.BuildNavMesh()` to bake navigation data
3. Skipped if prefab was not provided

**Prefab Requirements:**
- Must have `NavMeshSurface` component
- Configure `Agent Type`, `Agent Radius`, `Agent Height` as needed
- Set up `Include Layers` for walkable objects (hexes, obstacles)

**Setup:**
- Assign prefab in `MapGenerationConfig` → `Prefabs` → `NavMesh Surface Prefab`

---

### 3. HexGridService

**Path:** `Services/HexGridService.cs`  
**Type:** `Class`

Hexagonal grid mathematics using **odd-r offset coordinates**.

#### Constructor

```csharp
public HexGridService(float hexSize)
```

#### Public API

| Method | Returns | Description |
|--------|---------|-------------|
| `GetWorldPosition(q, r)` | `Vector3` | Converts hex coords to world position |
| `GetSymmetricCoordinate(q, r, w, h, type)` | `Vector2Int` | Returns symmetric coordinate |
| `GetDistance(q1, r1, q2, r2)` | `int` | Hex distance (Chebyshev on cube coords) |

#### Coordinate System

**Grid Type:** Pointy-topped hexagons with odd-r offset

| Axis | Formula |
|------|---------|
| **X** | `2.0 * q + (r % 2 != 0 ? 1.0 : 0.0)` |
| **Z** | `1.732051 * r` (√3 ≈ 1.732) |

**Distance Calculation:** Converts to cube coordinates, then Chebyshev distance:
```
q_cube = q - (r - (r & 1)) / 2
r_cube = r
s_cube = -q_cube - r_cube
distance = max(|q1-q2|, |r1-r2|, |s1-s2|)
```

---

## Data Structures

### HexData

```csharp
public class HexData
{
    public int BiomeIndex;           // -1 = no biome
    public bool IsPoi;               // true = base spawn zone
    public ObstacleType ObsType;     // None, Tree, Rock, Mountain
    public int ObsPrefabIndex;       // Index in prefab list
    public bool IsTreeCluster;       // true = part of forest cluster
    public bool IsWater;             // true = water hex
    public bool IsDeepWater;         // true = deep water (different prefab)
}
```

### ObstacleType

```csharp
public enum ObstacleType { None, Tree, Rock, Mountain }
```

### GenerationParams

Struct containing all generation parameters (internal use). Populated by `PrepareParams()`.

---

## Generation Algorithms

### Fractal Noise (Perlin)

```csharp
float GetFractalNoise(float x, float y, GenerationParams p)
{
    // Multi-octave Perlin noise
    // amplitude *= persistence, frequency *= lacunarity
    // Returns normalized value [0, 1]
}
```

### Symmetry Application

```csharp
void ApplySymmetryToGrid(grid, params, rng, q, r, action)
{
    action(grid[q, r]);  // Apply to source
    
    if (symmetry == None) return;
    if (chaos > 0 && rng.NextDouble() < chaos) return;  // Break symmetry
    
    action(grid[symQ, symR]);  // Apply to symmetric coordinate
}
```

### Tree Clustering (Warcraft 3 Style)

1. Generate `numClusters` random cluster centers
2. For each hex, check distance to nearest cluster
3. Spawn chance = `TreeClusterDensity * (1 - dist / clusterRadius)`
4. Multiple trees per hex possible (`TreesPerHex`, `TreeSpreadRadius`)

### River Generation

- Vertical path through map center
- Perlin noise wiggle for natural curves
- Bridges at center and ±4 rows
- Deep water center, shallow banks
- Symmetric duplicate if symmetry enabled

---

## Constraints & Safeguards

### Spawn Restrictions

| Feature | Restriction |
|---------|-------------|
| **Water** | Never spawns within `PoiSpotRadius` of POIs |
| **Mountains** | Never spawns on water hexes (`!IsWater` check) |
| **Obstacles** | Never spawns on water, POI zones, or center (25% of map) |
| **Trees** | Blocked in center for `Standard` archetype |

### ChokePoint Archetype

Forces mountain walls along vertical center line (except ±2 rows from center).
Creates a narrow passage for RTS battles.

```
[Горы] [Горы] [Горы] [Горы] [Горы]
[Горы] [Лес] [Проход] [Лес] [Горы]
[Горы] [Лес] [Проход] [Лес] [Горы]
[Горы] [Горы] [Горы] [Горы] [Горы]
```

### Divide Archetype

River/division down the center with bridges (every 8 rows, 3 hexes wide).
RTS-style map with two sides connected by bridges.

```
[База 1] [Лес] [Горы] [Река] [Горы] [Лес] [База 2]
[Лес] [Лес] [Горы] [Мост] [Горы] [Лес] [Лес]
[Лес] [Лес] [Лес] [Река] [Лес] [Лес] [Лес]
```

**Features:**
- Mountain banks along the divide
- Bridges every 8 rows (3 hexes wide)
- Clear zones around POIs for base building
- Neutral center zone

### Forest Archetype

Dense forest with clear paths for RTS unit movement.

```
[Деревья] [Деревья] [Тропа] [Деревья] [Деревья]
[Деревья] [Тропа] [Поляна] [Тропа] [Деревья]
[Деревья] [Деревья] [Тропа] [Деревья] [Деревья]
```

**Features:**
- Direct path between POIs (main battle lane)
- Paths from POIs to center
- Central clearing for battles
- Dense forest everywhere else (limits unit movement)

### Canyon Archetype

Canyon with mountain walls and narrow passage through center.

```
[Горы] [Горы] [Лес] [Лес] [Лес] [Горы] [Горы]
[Горы] [Стены] [Каньон] [Каньон] [Стены] [Горы]
[Горы] [Горы] [Камни] [Камни] [Горы] [Горы]
```

**Features:**
- Mountain walls on both sides of canyon
- Scattered rocks on canyon floor (15% chance)
- Clear POI zones for base building
- Narrow central passage for tactical battles

### Plains Archetype

Open plains with minimal obstacles.

```
[Трава] [Трава] [Трава] [Трава] [Трава]
[Трава] [Трава] [Камни] [Трава] [Трава]
[Трава] [Трава] [Трава] [Трава] [Трава]
```

**Features:**
- No trees (open visibility)
- Very sparse rocks (5% chance)
- Ideal for ranged combat
- Fast unit movement

### Lowland Archetype

Lowland with wetlands in center and higher ground on edges.

```
[Камни] [Камни] [Камни] [Камни] [Камни]
[Камни] [Деревья] [Деревья] [Деревья] [Камни]
[Камни] [Деревья] [Болото] [Деревья] [Камни]
```

**Features:**
- Trees in central lowland area
- Rocks on higher ground (edges)
- Natural defensive positions on edges
- Wetland-like center

### Mountainous Archetype (Кавказ)

Mountainous terrain with scattered peaks and valleys like Caucasus.

```
[Горы] [Лес] [Горы] [Горы] [Лес]
[Лес] [Горы] [Лес] [Горы] [Лес]
[Горы] [Лес] [Горы] [Лес] [Горы]
```

**Features:**
- 30% of map covered with scattered mountains
- Perlin noise distribution for natural look
- Valleys have sparse trees
- Larger clear zones around POIs
- Multiple possible paths through mountains

---

## Known Issues

### Water vs Mountains

**Fixed:** Mountains no longer spawn on water hexes. Check added in `GenerateBorders()`:
```csharp
if (isMountain && p.MountainPrefabsCount > 0 && !grid[q, r].IsWater)
```

---

## Usage Example

### In Editor

1. Open `SteelSurge → Level Editor`
2. Select a config from the dropdown (auto-loads from `Assets/System/Configs/LevelEditor`)
3. Adjust map size and orientation
4. Click **Generate Preview** to see the map
5. Click **Generate & Save Scene** to create the full map

### Available Configs

| Config | Archetype | Description |
|--------|-----------|-------------|
| StandardArena | Standard | Balanced map with moderate obstacles |
| ChokePointArena | ChokePoint | Narrow central passage |
| DivideArena | Divide | River with bridges |
| ForestArena | Forest | Dense forest with clear paths |
| CanyonArena | Canyon | Mountain walls with central passage |
| PlainsArena | Plains | Open terrain, minimal obstacles |
| LowlandArena | Lowland | Trees in center, rocks on edges |
| MountainousArena | Mountainous | Scattered mountains (Caucasus-style) |

### Via Code

```csharp
// In Editor or Runtime
var config = Resources.Load<MapGenerationConfig>("MapGenerationConfig");
var parent = new GameObject("Map").transform;
var generator = new MapGenerator(config, width: 40, height: 30, parent, seed: 12345);

await generator.GenerateAsync();

// Access POI coordinates
var poi1 = generator.Poi1;  // e.g., (2, 15)
var poi2 = generator.Poi2;  // e.g., (37, 15)
```

---

## Changelog

### v1.0.1 — Bugfix: Water/Mountain Conflict

**Issue:** Mountains spawning on water hexes, causing visual clipping.

**Root Cause:** Generation order:
1. `GenerateBiomes()` → Water
2. `GenerateBorders()` → Mountains

Mountains set `ObsType = Mountain` without checking `IsWater` flag.

**Fix:** Added `!grid[q, r].IsWater` check in `GenerateBorders()`.

---

## Dependencies

| Package | Version | Usage |
|---------|---------|-------|
| Unity Engine | — | Core |
| Cysharp UniTask | — | Async/await pattern |
| Odin Inspector | — | Editor UI (if using Editor Window) |
