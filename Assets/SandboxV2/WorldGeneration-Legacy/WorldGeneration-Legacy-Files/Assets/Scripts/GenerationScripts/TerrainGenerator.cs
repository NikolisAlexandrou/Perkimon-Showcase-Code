using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace island
{
    [RequireComponent(typeof(TreeGenerator), typeof(DecorationGenerator), typeof(BiomeGenerator))]
    [RequireComponent(typeof(WorldPropGenerator), typeof(RiverGenerator), typeof(LavaLakeGenerator))]
    public class TerrainGenerator : MonoBehaviour
    {
        [Header("Script Dependencies")]
        [SerializeField] private TreeGenerator _treeGenerator;
        [SerializeField] private DecorationGenerator _decorationGenerator;
        [SerializeField] private BiomeGenerator _biomeGenerator;
        [SerializeField] private WorldPropGenerator _worldPropGenerator;
        [SerializeField] private RiverGenerator _riverGenerator;
        [SerializeField] private LavaLakeGenerator _lavaLakeGenerator;
        [SerializeField] private WorldModificationStore _worldModificationStore;

        [Header("World Settings")]
        [SerializeField, Min(4)] private int _chunkSize = 32;
        [Tooltip("Bigger values create smoother and larger land regions.")]
        [SerializeField, Min(0.0001f)] private float _noiseScale = 25f;
        [SerializeField, Range(0f, 1f)] private float _landThreshold = 0.45f;
        [SerializeField, Range(0.01f, 0.4f)] private float _sandBandWidth = 0.12f;
        [SerializeField] private bool _useRandomSeed;
        [SerializeField] private int _seed = 12345;


[Header("Island / Ocean World Shape")]
[Tooltip("Enables a large-scale land mask so the world contains oceans, large continents and smaller islands.")]
[SerializeField] private bool _useIslandWorldShape = true;

[Tooltip("Larger values produce larger continents and ocean basins.")]
[SerializeField, Min(32f)] private float _continentalScale = 520f;

[Tooltip("Secondary land noise creates smaller islands around the main continents.")]
[SerializeField, Min(16f)] private float _islandScale = 145f;

[SerializeField, Range(0f, 1f)] private float _continentThreshold = 0.49f;
[SerializeField, Range(0f, 1f)] private float _smallIslandThreshold = 0.68f;

[Tooltip("How strongly the continental mask affects normal terrain height.")]
[SerializeField, Range(0f, 1f)] private float _continentalInfluence = 0.72f;

[Header("Archipelago Islands")]
[Tooltip("Controls the average size of small offshore island clusters.")]
[SerializeField, Min(8f)]
private float _archipelagoScale = 70f;

[Tooltip("Higher values make archipelago islands rarer.")]
[SerializeField, Range(0f, 1f)]
private float _archipelagoThreshold = 0.72f;

[Tooltip("Archipelago islands only appear where continentalness is below this value.")]
[SerializeField, Range(0f, 1f)]
private float _archipelagoMaxContinentalness = 0.58f;

[Tooltip("Strength of archipelago islands in the final land mask.")]
[SerializeField, Range(0f, 1f)]
private float _archipelagoStrength = 0.85f;

        [Header("Fallback Mountains (used when a biome asset is missing)")]
        [Tooltip("Bigger values create broader mountain regions.")]
        [SerializeField, Min(0.0001f)] private float _mountainNoiseScale = 55f;
        [SerializeField, Range(0f, 1f)] private float _mountainThreshold = 0.68f;
        [Tooltip("Prevents mountain outlines from appearing too close to water and beaches.")]
        [SerializeField, Range(0f, 0.3f)] private float _mountainLandBuffer = 0.08f;

        [Header("Elevation Levels")]
        [Tooltip("Maximum number of nested mountain plateaus. Level 0 is normal ground.")]
        [SerializeField, Range(1, 32)] private int _maximumElevationLevels = 6;

        [Tooltip("Higher values keep low hills broad and make the highest peaks rarer.")]
        [SerializeField, Range(0.25f, 4f)] private float _elevationCurvePower = 1.5f;

        [Tooltip("Controls the broad regional noise that gives different mountains different maximum heights.")]
        [SerializeField, Min(0.0001f)] private float _mountainHeightVariationScale = 140f;

        [Tooltip("Lowest fraction of Maximum Elevation Levels that a mountain region may reach.")]
        [SerializeField, Range(0.05f, 1f)] private float _minimumMountainHeightFraction = 0.35f;

        [Header("Elevation Ramps")]
        [Tooltip("Enables deterministic openings that let the player cross one elevation step.")]
        [SerializeField] private bool _generateRamps = true;

        [Tooltip("Approximate chance that a suitable local ramp candidate is kept.")]
        [SerializeField, Range(0f, 1f)] private float _rampChance = 0.8f;

        [Tooltip("Minimum spacing between ramps on the same elevation contour.")]
        [SerializeField, Range(1, 12)] private int _rampSpacing = 4;

        [Tooltip("Requires a flat higher cell behind the ramp, producing cleaner entrances.")]
        [SerializeField] private bool _requireRampLanding = true;

        [Header("Tilemaps")]
        [SerializeField] private Tilemap _waterTilemap;
        [SerializeField] private Tilemap _sandTilemap;
        [SerializeField] private Tilemap _grassTilemap;
        [SerializeField] private Tilemap _tallGrassTilemap;
        [Tooltip("Add TilemapCollider2D to this Tilemap. Only mountain boundary cells are painted here.")]
        [SerializeField] private Tilemap _mountainOutlineTilemap;
        [Tooltip("Visual-only Tilemap. Do not add a collider to it.")]
        [SerializeField] private Tilemap _rampTilemap;
        
        [Header("Overlay Tilemaps")]
        [SerializeField] private Tilemap _groundTransitionTilemap;
        [SerializeField] private Tilemap _riverTilemap;
        [SerializeField] private Tilemap _riverTransitionTilemap;
        
        [SerializeField]
        private Tilemap _lavaTilemap;

        [SerializeField]
        private Tilemap _lavaTransitionTilemap;

        [Header("Tiles")]
        [SerializeField] private TileBase _waterTile;
        [SerializeField] private TileBase _grassTile;
        [SerializeField] private TileBase _sandTile;
        [SerializeField]
        private TileBase _lavaTile;

        [Tooltip("Base tile painted on the River Tilemap. The original terrain remains underneath.")] 
        [SerializeField] private TileBase _riverTile;

        [Tooltip("Used instead of sand where a Snow biome meets the ocean. Falls back to Sand Tile when unassigned.")]
        [SerializeField] private TileBase _iceCoastTile;

        [Header("Terrain Transition Autotiling")]
        [Tooltip("Transition set used for normal sand coastlines.")]
        [SerializeField] private TerrainTransitionAutotileSet _sandTransitionSet;

        [Tooltip("Transition set used for frozen/ice coastlines.")]
        [SerializeField] private TerrainTransitionAutotileSet _iceTransitionSet;

        [Tooltip("Optional transition set for water edges. The tiles in this set may themselves be SynchronizedAnimatedTile assets.")]
        [SerializeField] private TerrainTransitionAutotileSet _waterTransitionSet;
        
        [Tooltip("Transparent river edge tiles painted over the terrain beside a river.")]
        [SerializeField] private TerrainTransitionAutotileSet _riverTransitionSet;
        [SerializeField]
        private TerrainTransitionAutotileSet
            _lavaTransitionSet;

        [Header("Water Animation Sync")]
        [Tooltip("Tilemap animation rate shared by all water chunks. Use a SynchronizedAnimatedTile as Water Tile to guarantee all newly loaded chunks use the same animation phase.")]
        [SerializeField, Min(0.01f)] private float _waterAnimationFrameRate = 1f;

        [Header("Ramp Tiles")]
        [SerializeField] private TileBase _rampNorthTile;
        [SerializeField] private TileBase _rampEastTile;
        [SerializeField] private TileBase _rampSouthTile;
        [SerializeField] private TileBase _rampWestTile;

        [Header("Mountain Autotiling")]
        [Tooltip("Used around the outer/top boundary of a peak when there is no directly adjacent elevation exactly one level higher.")]
        [SerializeField] private MountainAutotileSet _mountainPeakAutotileSet;

        [Tooltip("Used around intermediate elevation contours when a neighbouring mountain cell is exactly one elevation level above the current cell.")]
        [SerializeField] private MountainAutotileSet _mountainElevationStepAutotileSet;

        [Tooltip("Legacy/fallback mountain set. Used if the specialised peak or elevation-step set is missing.")]
        [SerializeField] private MountainAutotileSet _mountainAutotileSet;

        [Tooltip("Optional fallback used when no matching mountain autotile is assigned.")]
        [SerializeField] private TileBase _mountainOutlineFallbackTile;

        private readonly Dictionary<ChunkCoordinate, TerrainCellData[,]> _terrainByChunk = new();
        
        private static readonly Vector2Int[]
            TransitionDirections =
            {
                new Vector2Int(0, 1),   // N  = 1
                new Vector2Int(1, 1),   // NE = 2
                new Vector2Int(1, 0),   // E  = 4
                new Vector2Int(1, -1),  // SE = 8
                new Vector2Int(0, -1),  // S  = 16
                new Vector2Int(-1, -1), // SW = 32
                new Vector2Int(-1, 0),  // W  = 64
                new Vector2Int(-1, 1)   // NW = 128
            };

        private bool _worldInitialized;

        private int _activeSeed;
        private Vector2 _terrainNoiseOffset;
        private Vector2 _mountainNoiseOffset;
        private Vector2 _mountainHeightVariationOffset;
        private Vector2 _continentalOffset;
        private Vector2 _islandOffset;
        private Vector2 _archipelagoOffset;
        
        public bool IsWorldInitialized => _worldInitialized;

        public int Width
        {
            get => _chunkSize;
            set => _chunkSize = Mathf.Max(4, value);
        }

        public int Height
        {
            get => _chunkSize;
            set => _chunkSize = Mathf.Max(4, value);
        }

        public int ChunkSize => _chunkSize;

        public float NoiseScale
        {
            get => _noiseScale;
            set => _noiseScale = Mathf.Max(0.0001f, value);
        }

        public float LandThreshold
        {
            get => _landThreshold;
            set => _landThreshold = Mathf.Clamp01(value);
        }

        public int ActiveSeed => _activeSeed;

        private void Awake()
        {
            if (_treeGenerator == null)
                _treeGenerator = GetComponent<TreeGenerator>();

            if (_decorationGenerator == null)
                _decorationGenerator = GetComponent<DecorationGenerator>();

            if (_biomeGenerator == null)
                _biomeGenerator = GetComponent<BiomeGenerator>();

            if (_worldPropGenerator == null)
                _worldPropGenerator = GetComponent<WorldPropGenerator>();

            if (_riverGenerator == null)
                _riverGenerator = GetComponent<RiverGenerator>();
            
            if (_lavaLakeGenerator == null)
                _lavaLakeGenerator = GetComponent<LavaLakeGenerator>();
            
            if (_worldModificationStore == null)
            {
                _worldModificationStore =
                    FindFirstObjectByType<WorldModificationStore>();
            }

            if (_waterTilemap != null)
                _waterTilemap.animationFrameRate = _waterAnimationFrameRate;
        }

        public void InitializeSeed()
        {
            int seed = _useRandomSeed
                ? Random.Range(int.MinValue, int.MaxValue)
                : _seed;

            InitializeWorld(seed);
        }
        
        /// <summary>
        /// For initialising a new world
        /// </summary>
        public void InitializeNewWorld()
        {
            InitializeSeed();
        }
        
        /// <summary>
        /// For initialising an existing world
        /// </summary>
        /// <param name="seed"></param>
        public void InitializeWorld(int seed)
        {
            _activeSeed = seed;

            System.Random random =
                new System.Random(_activeSeed);

            _terrainNoiseOffset =
                new Vector2(
                    random.Next(-100000, 100000),
                    random.Next(-100000, 100000));

            _mountainNoiseOffset =
                new Vector2(
                    random.Next(-100000, 100000),
                    random.Next(-100000, 100000));

            _mountainHeightVariationOffset =
                new Vector2(
                    random.Next(-100000, 100000),
                    random.Next(-100000, 100000));

            _continentalOffset =
                new Vector2(
                    random.Next(-100000, 100000),
                    random.Next(-100000, 100000));

            _islandOffset =
                new Vector2(
                    random.Next(-100000, 100000),
                    random.Next(-100000, 100000));

            _archipelagoOffset =
                new Vector2(
                    random.Next(-100000, 100000),
                    random.Next(-100000, 100000));

            _biomeGenerator?.Initialize(
                _activeSeed);

            _riverGenerator?.Initialize(
                _activeSeed);

            _lavaLakeGenerator?.Initialize(
                _activeSeed);

            _worldModificationStore?.InitializeWorld(
                _activeSeed);

            _worldInitialized = true;
            
            Debug.Log(
                $"World initialized with seed {_activeSeed}",
                this);
        }


/// <summary>
/// Compatibility method. Prefer GenerateChunkRoutine through ChunkManager
/// so generation work is spread across frames.
/// </summary>
public void GenerateChunk(ChunkCoordinate coordinate)
{
    IEnumerator routine = GenerateChunkRoutine(
        coordinate,
        Mathf.Max(1, _chunkSize));

    while (routine.MoveNext())
    {
        // Intentionally complete synchronously for legacy callers.
    }
}

public IEnumerator GenerateChunkRoutine(ChunkCoordinate coordinate, int rowsPerFrame)
{
    if (!_worldInitialized)
    {
        Debug.LogError(
            $"Tried to generate chunk {coordinate} " +
            "before TerrainGenerator was initialized.",
            this);

        yield break;
    }
    
    rowsPerFrame = Mathf.Max(1, rowsPerFrame);

    int size = _chunkSize;
    int tileCount = size * size;
    Vector3Int origin = coordinate.CellOrigin(size);

    TerrainCellData[,] terrainData =
        new TerrainCellData[size, size];

    // Stage 1: Generate logical terrain data incrementally.
    for (int localY = 0; localY < size; localY++)
    {
        for (int localX = 0; localX < size; localX++)
        {
            int worldX = origin.x + localX;
            int worldY = origin.y + localY;
            terrainData[localX, localY] =
                SampleTerrainCell(worldX, worldY);
        }

        if ((localY + 1) % rowsPerFrame == 0)
            yield return null;
    }

    _terrainByChunk[coordinate] = terrainData;

    TileBase[] waterTiles = new TileBase[tileCount];
    TileBase[] sandTiles = new TileBase[tileCount];
    TileBase[] grassTiles = new TileBase[tileCount];
    TileBase[] decorationTiles = new TileBase[tileCount];
    TileBase[] mountainOutlineTiles = new TileBase[tileCount];
    TileBase[] rampTiles = new TileBase[tileCount];
    
    TileBase[] groundTransitionTiles = new TileBase[tileCount];
    TileBase[] riverTransitionTiles = new TileBase[tileCount];
    TileBase[] riverTiles = new TileBase[tileCount];
    TileBase[] lavaTiles =
        new TileBase[tileCount];

    TileBase[] lavaTransitionTiles =
        new TileBase[tileCount];

    // Stage 2: Build render buffers and spawn pooled entities incrementally.
    for (int localY = 0; localY < size; localY++)
    {
        for (int localX = 0; localX < size; localX++)
        {
            int index = localX + localY * size;
            TerrainCellData cell = terrainData[localX, localY];

            switch (cell.TerrainType)
            {
                case TerrainType.Water:
                {
                    // Ocean always keeps its full base tile.
                    // Shoreline overlays are painted separately on top.
                    waterTiles[index] = _waterTile;
                    break;
                }

                case TerrainType.Sand:
                {
                    bool frozenCoast =
                        cell.BiomeType == BiomeType.Snow;

                    sandTiles[index] =
                        frozenCoast && _iceCoastTile != null
                            ? _iceCoastTile
                            : _sandTile;

                    break;
                }

                case TerrainType.Grass:
                    PaintGrassCell(
                        cell,
                        coordinate,
                        index,
                        grassTiles,
                        sandTiles,
                        decorationTiles,
                        terrainData,
                        origin,
                        allowTreesAndDecorations: true);
                    break;

                case TerrainType.Mountain:
                    PaintGrassCell(
                        cell,
                        coordinate,
                        index,
                        grassTiles,
                        sandTiles,
                        decorationTiles,
                        terrainData,
                        origin,
                        allowTreesAndDecorations: true);

                    if (!cell.IsElevationOutline)
                        break;

                    if (cell.HasRamp)
                    {
                        rampTiles[index] =
                            ResolveRampTile(cell.RampDirection);
                    }
                    else
                    {
                        int mask = GetElevationEightNeighbourMask(
                            cell.WorldX,
                            cell.WorldY,
                            cell.ElevationLevel);

                        bool hasDirectlyHigherNeighbour =
                            HasNeighbourExactlyOneLevelHigher(
                                cell.WorldX,
                                cell.WorldY,
                                cell.ElevationLevel);

                        mountainOutlineTiles[index] =
                            ResolveMountainOutlineTile(
                                cell,
                                mask,
                                hasDirectlyHigherNeighbour);
                    }
                    break;
            }
            // --------------------------------------------------
            // Overlay terrain transitions
            // --------------------------------------------------

            groundTransitionTiles[index] =
                GetGroundTransitionOverlay(
                    cell,
                    terrainData,
                    origin);

            // --------------------------------------------------
            // River overlay
            // --------------------------------------------------

            if (cell.IsRiver)
            {
                // Keep the original grass/sand underneath.
                riverTiles[index] = _riverTile;
            }
            else if (cell.TerrainType != TerrainType.Water)
            {
                riverTransitionTiles[index] =
                    GetRiverTransitionOverlay(
                        cell,
                        terrainData,
                        origin);
            }
            
            if (cell.IsLava)
            {
                lavaTiles[index] =
                    _lavaTile;
            }
            else
            {
                lavaTransitionTiles[index] =
                    GetLavaTransitionOverlay(
                        cell,
                        terrainData,
                        origin);
            }
            
        }

        if ((localY + 1) % rowsPerFrame == 0)
            yield return null;
    }

    BoundsInt bounds = new BoundsInt(
        origin.x,
        origin.y,
        0,
        size,
        size,
        1);

    // Stage 3: Spread Tilemap writes over several frames.
    
    _waterTilemap.SetTilesBlock(bounds, waterTiles);
    _sandTilemap.SetTilesBlock(bounds, sandTiles);
    yield return null;

    _grassTilemap.SetTilesBlock(bounds, grassTiles);

    if (_groundTransitionTilemap != null)
    {
        _groundTransitionTilemap.SetTilesBlock(
            bounds,
            groundTransitionTiles);
    }

    yield return null;

    if (_riverTilemap != null)
    {
        _riverTilemap.SetTilesBlock(
            bounds,
            riverTiles);
    }

    if (_riverTransitionTilemap != null)
    {
        _riverTransitionTilemap.SetTilesBlock(
            bounds,
            riverTransitionTiles);
    }
    
    if (_lavaTilemap != null)
    {
        _lavaTilemap.SetTilesBlock(
            bounds,
            lavaTiles);
    }

    if (_lavaTransitionTilemap != null)
    {
        _lavaTransitionTilemap.SetTilesBlock(
            bounds,
            lavaTransitionTiles);
    }

    _tallGrassTilemap.SetTilesBlock(
        bounds,
        decorationTiles);

    yield return null;

    _mountainOutlineTilemap.SetTilesBlock(
        bounds,
        mountainOutlineTiles);

    if (_rampTilemap != null)
    {
        _rampTilemap.SetTilesBlock(
            bounds,
            rampTiles);
    }

    // Custom autotiling uses normal Tile assets, so expensive per-cell
    // RefreshTile calls are unnecessary.
}

        public TerrainCellData[,] GenerateTerrainData(ChunkCoordinate coordinate)
        {
            int size = _chunkSize;
            Vector3Int origin = coordinate.CellOrigin(size);
            TerrainCellData[,] data = new TerrainCellData[size, size];

            for (int localY = 0; localY < size; localY++)
            {
                for (int localX = 0; localX < size; localX++)
                {
                    int worldX = origin.x + localX;
                    int worldY = origin.y + localY;
                    data[localX, localY] = SampleTerrainCell(worldX, worldY);
                }
            }

            return data;
        }


public TerrainCellData SampleTerrainCell(int worldX, int worldY)
{
    float height = SampleTerrainHeight(worldX, worldY);

    ClimateSample climate = _biomeGenerator != null
        ? _biomeGenerator.SampleClimate(worldX, worldY)
        : new ClimateSample(0.5f, 0.5f);

    BiomeType baseBiomeType = _biomeGenerator != null
        ? _biomeGenerator.ResolveBaseLandBiome(
            worldX,
            worldY,
            climate.Temperature,
            climate.Moisture)
        : BiomeType.Plains;

    BiomeDefinition baseBiome = _biomeGenerator != null
        ? _biomeGenerator.GetDefinition(baseBiomeType)
        : null;

    float mountainNoise = SampleMountainNoise(
        worldX,
        worldY,
        GetMountainNoiseScale(baseBiome),
        baseBiome);

    float heightVariation = SampleMountainHeightVariation(
        worldX,
        worldY,
        GetMountainHeightVariationScale(baseBiome));

    int elevationLevel = ResolveElevationLevel(
        height,
        mountainNoise,
        heightVariation,
        baseBiome);

    TerrainType terrainType = ResolveTerrainType(
        height,
        elevationLevel);

    bool isRiver = false;

    if (terrainType != TerrainType.Water &&
        _riverGenerator != null &&
        _riverGenerator.IsRiverCell(
            worldX,
            worldY,
            height,
            _landThreshold,
            elevationLevel))
    {
        isRiver = true;

        // River is now an overlay feature.
        // Do NOT replace the underlying Grass/Sand terrain with Water.
        //elevationLevel = 0;
    }

    BiomeType biomeType =
        _biomeGenerator != null
            ? _biomeGenerator.ResolveBiome(
                terrainType,
                isRiver,
                elevationLevel,
                baseBiomeType)
            : baseBiomeType;
    
    bool isLava =
        _lavaLakeGenerator != null &&
        _lavaLakeGenerator.IsLavaCell(
            worldX,
            worldY,
            biomeType,
            terrainType,
            elevationLevel,
            isRiver);

    // Frozen shoreline: keep Snow identity on coastal Sand cells.
    // This allows the painting stage to use ice instead of beach sand.
    if (terrainType == TerrainType.Sand &&
        baseBiomeType == BiomeType.Snow)
    {
        biomeType = BiomeType.Snow;
    }

    bool isOutline =
        !isRiver &&
        elevationLevel > 0 &&
        HasLowerEightNeighbour(
            worldX,
            worldY,
            elevationLevel);

    bool hasRamp = false;
    RampDirection rampDirection =
        RampDirection.None;

    if (_generateRamps && isOutline)
    {
        hasRamp = IsSelectedRamp(
            worldX,
            worldY,
            elevationLevel,
            out rampDirection);
    }

    return new TerrainCellData(
        worldX,
        worldY,
        height,
        mountainNoise,
        heightVariation,
        climate.Temperature,
        climate.Moisture,
        terrainType,
        biomeType,
        isRiver,
        isLava,
        elevationLevel,
        isOutline,
        hasRamp,
        rampDirection);
}

        public bool TryGetLoadedCellData(
            Vector2Int worldCell,
            out TerrainCellData cellData)
        {
            ChunkCoordinate chunk = ChunkCoordinate.FromWorldPosition(
                new Vector3(worldCell.x, worldCell.y, 0f),
                _chunkSize);

            if (!_terrainByChunk.TryGetValue(chunk, out TerrainCellData[,] chunkData))
            {
                cellData = default;
                return false;
            }

            Vector3Int origin = chunk.CellOrigin(_chunkSize);
            int localX = worldCell.x - origin.x;
            int localY = worldCell.y - origin.y;

            if (localX < 0 || localY < 0 || localX >= _chunkSize || localY >= _chunkSize)
            {
                cellData = default;
                return false;
            }

            cellData = chunkData[localX, localY];
            return true;
        }

        public void ClearChunk(ChunkCoordinate coordinate)
        {
            Vector3Int origin = coordinate.CellOrigin(_chunkSize);
            BoundsInt bounds = new BoundsInt(
                origin.x,
                origin.y,
                0,
                _chunkSize,
                _chunkSize,
                1);

            TileBase[] empty = new TileBase[_chunkSize * _chunkSize];

            _waterTilemap.SetTilesBlock(bounds, empty);
            _sandTilemap.SetTilesBlock(bounds, empty);
            _grassTilemap.SetTilesBlock(bounds, empty);
            
            if (_groundTransitionTilemap != null)
                _groundTransitionTilemap.SetTilesBlock(bounds, empty);

            if (_riverTilemap != null)
                _riverTilemap.SetTilesBlock(bounds, empty);

            if (_riverTransitionTilemap != null)
                _riverTransitionTilemap.SetTilesBlock(bounds, empty);
            
            _tallGrassTilemap.SetTilesBlock(bounds, empty);
            _mountainOutlineTilemap.SetTilesBlock(bounds, empty);
            if (_rampTilemap != null)
                _rampTilemap.SetTilesBlock(bounds, empty);

            _treeGenerator.ClearChunk(coordinate);
            _worldPropGenerator?.ClearChunk(coordinate);
            _terrainByChunk.Remove(coordinate);
            
            if (_lavaTilemap != null)
            {
                _lavaTilemap.SetTilesBlock(
                    bounds,
                    empty);
            }

            if (_lavaTransitionTilemap != null)
            {
                _lavaTransitionTilemap
                    .SetTilesBlock(
                        bounds,
                        empty);
            }
        }

        public void ClearWorld()
        {
            _waterTilemap.ClearAllTiles();
            _sandTilemap.ClearAllTiles();
            _grassTilemap.ClearAllTiles();
            
            _groundTransitionTilemap?.ClearAllTiles();
            _riverTilemap?.ClearAllTiles();
            _riverTransitionTilemap?.ClearAllTiles();
            
            _lavaTilemap?.ClearAllTiles();

            _lavaTransitionTilemap
                ?.ClearAllTiles();
            
            _tallGrassTilemap.ClearAllTiles();
            _mountainOutlineTilemap.ClearAllTiles();
            if (_rampTilemap != null)
                _rampTilemap.ClearAllTiles();
            _treeGenerator.ClearAllTrees();
            _worldPropGenerator?.ClearAll();
            _terrainByChunk.Clear();
        }

        public void GenerateIsland()
        {
            ChunkManager manager = GetComponent<ChunkManager>();
            if (manager != null)
                manager.RegenerateWorld();
            else
                Debug.LogWarning(
                    "GenerateIsland now requires ChunkManager on the same GameObject.",
                    this);
        }


private void PaintGrassCell(
    TerrainCellData cell,
    ChunkCoordinate coordinate,
    int index,
    TileBase[] grassTiles,
    TileBase[] sandTiles,
    TileBase[] decorationTiles,
    TerrainCellData[,] terrainData,
    Vector3Int chunkOrigin,
    bool allowTreesAndDecorations)
{
    BiomeDefinition biome = _biomeGenerator != null
        ? _biomeGenerator.GetDefinition(cell.BiomeType)
        : null;

    bool isElevated = cell.ElevationLevel > 0;
    bool allowTrees = allowTreesAndDecorations &&
                      (!isElevated || biome == null || biome.AllowTreesOnElevatedCells);
    bool allowDecorations = allowTreesAndDecorations &&
                            (!isElevated || biome == null || biome.AllowDecorationsOnElevatedCells);
    bool allowProps = allowTreesAndDecorations &&
                      (!isElevated || biome == null || biome.AllowPropsOnElevatedCells);

    TileBase groundFallback =
        biome != null && biome.GroundTile != null
            ? biome.GroundTile
            : _grassTile;

    TileBase groundTile =
        biome != null && biome.GroundTile != null
            ? biome.GroundTile
            : _grassTile;
    

    TileBase baseTile =
        biome != null && biome.BaseTile != null
            ? biome.BaseTile
            : _sandTile;

// Always paint the full base tiles.
// Transition sprites will be painted later on an overlay Tilemap.
    sandTiles[index] = baseTile;
    grassTiles[index] = groundTile;
    
    if (cell.IsLava)
        return;

    if (!allowTreesAndDecorations)
        return;

    float terrainScale = Mathf.Max(0.0001f, _noiseScale);
    float nx = (cell.WorldX + _terrainNoiseOffset.x) / terrainScale;
    float ny = (cell.WorldY + _terrainNoiseOffset.y) / terrainScale;
    Vector2Int worldCell = new Vector2Int(cell.WorldX, cell.WorldY);

    // Weighted tile decoration selected by the biome asset.
    if (allowDecorations && biome != null &&
        Deterministic01(cell.WorldX, cell.WorldY, 97) < biome.DecorationChance)
    {
        float detailNoise = Mathf.PerlinNoise(nx * 2f, ny * 2f);
        decorationTiles[index] = biome.SelectDecoration(
            detailNoise,
            Deterministic01(cell.WorldX, cell.WorldY, 101));
    }

    float treeHeightThreshold = _landThreshold + 0.05f;
    if (allowTrees && cell.Height > treeHeightThreshold)
    {
        float treeMultiplier = biome != null
            ? biome.TreeChanceMultiplier
            : 1f;

        float treeNoise = Mathf.PerlinNoise(nx * 3f, ny * 3f);
        float treeRoll = Deterministic01(cell.WorldX, cell.WorldY, 307);

        if (_treeGenerator.ShouldPlaceTree(
                worldCell,
                treeNoise,
                treeRoll,
                _activeSeed,
                treeMultiplier))
        {
            _treeGenerator.SpawnTree(
                worldCell,
                _grassTilemap,
                coordinate,
                biome,
                _activeSeed);
            return; // Prevent a scarce prop from overlapping a tree.
        }
    }

    if (!allowProps || biome == null || _worldPropGenerator == null ||
        Deterministic01(cell.WorldX, cell.WorldY, 1301) >= biome.PropChance)
    {
        return;
    }

    BiomePropEntry prop = biome.SelectProp(Deterministic01(cell.WorldX, cell.WorldY, 1307), cell.ElevationLevel);

    if (prop == null || !_worldPropGenerator.PassesSpacing(worldCell, prop, _activeSeed, prop.StableSalt))
    {
        return;
    }

    _worldPropGenerator.Spawn(
        prop,
        worldCell,
        _grassTilemap,
        coordinate,
        Deterministic01(cell.WorldX, cell.WorldY, 1319) > 0.5f,
        _activeSeed,
        prop.StableSalt);
}

        private TileBase ResolveRampTile(RampDirection direction)
        {
            return direction switch
            {
                RampDirection.North => _rampNorthTile,
                RampDirection.East => _rampEastTile,
                RampDirection.South => _rampSouthTile,
                RampDirection.West => _rampWestTile,
                _ => null
            };
        }

        private bool IsSelectedRamp(
            int worldX,
            int worldY,
            int elevation,
            out RampDirection direction)
        {
            direction = RampDirection.None;

            if (!TryGetRawRampDirection(worldX, worldY, elevation, out direction))
                return false;

            float score = Deterministic01(worldX, worldY, 1709 + elevation * 31);
            if (score > _rampChance)
                return false;

            // Keep only the lowest deterministic score within the spacing radius.
            // This prevents clusters of adjacent openings.
            int radius = Mathf.Max(1, _rampSpacing);
            for (int oy = -radius; oy <= radius; oy++)
            {
                for (int ox = -radius; ox <= radius; ox++)
                {
                    if (ox == 0 && oy == 0)
                        continue;

                    int otherX = worldX + ox;
                    int otherY = worldY + oy;

                    if (GetElevationLevel(otherX, otherY) != elevation)
                        continue;

                    if (!TryGetRawRampDirection(otherX, otherY, elevation, out _))
                        continue;

                    float otherScore = Deterministic01(
                        otherX,
                        otherY,
                        1709 + elevation * 31);

                    if (otherScore < score)
                        return false;
                }
            }

            return true;
        }

        private bool TryGetRawRampDirection(
            int worldX,
            int worldY,
            int elevation,
            out RampDirection direction)
        {
            direction = RampDirection.None;
            if (elevation <= 0)
                return false;

            int north = GetElevationLevel(worldX, worldY + 1);
            int east = GetElevationLevel(worldX + 1, worldY);
            int south = GetElevationLevel(worldX, worldY - 1);
            int west = GetElevationLevel(worldX - 1, worldY);

            // The ramp lives on the higher boundary cell. The returned direction
            // points inward, toward the higher plateau.
            if (south == elevation - 1 && north >= elevation &&
                HasRampSideSupport(east, west, elevation))
            {
                direction = RampDirection.North;
                return true;
            }

            if (north == elevation - 1 && south >= elevation &&
                HasRampSideSupport(east, west, elevation))
            {
                direction = RampDirection.South;
                return true;
            }

            if (west == elevation - 1 && east >= elevation &&
                HasRampSideSupport(north, south, elevation))
            {
                direction = RampDirection.East;
                return true;
            }

            if (east == elevation - 1 && west >= elevation &&
                HasRampSideSupport(north, south, elevation))
            {
                direction = RampDirection.West;
                return true;
            }

            return false;
        }

        private bool HasRampSideSupport(int sideA, int sideB, int elevation)
        {
            if (!_requireRampLanding)
                return true;

            // At least one side should remain part of the same contour, avoiding
            // ramps on tiny isolated tips.
            return sideA >= elevation || sideB >= elevation;
        }

        private TerrainType ResolveTerrainType(float height, int elevationLevel)
        {
            float waterThreshold = _landThreshold - _sandBandWidth;

            if (height < waterThreshold)
                return TerrainType.Water;

            if (height < _landThreshold)
                return TerrainType.Sand;

            return elevationLevel > 0
                ? TerrainType.Mountain
                : TerrainType.Grass;
        }

        private int ResolveElevationLevel(
            float terrainHeight,
            float mountainNoise,
            float heightVariation,
            BiomeDefinition biome)
        {
            bool allowMountains = biome == null || biome.AllowMountains;
            float threshold = biome != null
                ? biome.MountainThreshold
                : _mountainThreshold;
            float landBuffer = biome != null
                ? biome.MountainLandBuffer
                : _mountainLandBuffer;

            if (!allowMountains ||
                terrainHeight < _landThreshold + landBuffer ||
                mountainNoise < threshold)
            {
                return 0;
            }

            float normalizedMountain = Mathf.InverseLerp(
                threshold,
                1f,
                mountainNoise);

            float curvePower = biome != null
                ? biome.ElevationCurvePower
                : _elevationCurvePower;

            normalizedMountain = Mathf.Pow(
                Mathf.Clamp01(normalizedMountain),
                curvePower);

            int maximumLevels = biome != null
                ? biome.MaximumElevationLevels
                : _maximumElevationLevels;

            float minimumFraction = biome != null
                ? biome.MinimumHeightFraction
                : _minimumMountainHeightFraction;

            float regionalFraction = Mathf.Lerp(
                minimumFraction,
                1f,
                heightVariation);

            int regionalMaximum = Mathf.Clamp(
                Mathf.RoundToInt(maximumLevels * regionalFraction),
                1,
                maximumLevels);

            return Mathf.Clamp(
                1 + Mathf.FloorToInt(normalizedMountain * regionalMaximum),
                1,
                regionalMaximum);
        }
        
        private TileBase GetGroundTransitionOverlay(
    TerrainCellData current,
    TerrainCellData[,] chunkData,
    Vector3Int chunkOrigin)
{
    TerrainCellData bestSource = default;

    bool foundSource = false;

    int currentPriority =
        GetSurfacePriority(current);

    int bestPriority =
        currentPriority;

    // Find the highest-priority neighbouring terrain
    // that is allowed to visually overlap this cell.
    foreach (Vector2Int direction
             in TransitionDirections)
    {
        TerrainCellData neighbour =
            GetTransitionNeighbour(
                current.WorldX + direction.x,
                current.WorldY + direction.y,
                chunkData,
                chunkOrigin);

        int neighbourPriority =
            GetSurfacePriority(neighbour);

        if (neighbourPriority <=
            bestPriority)
        {
            continue;
        }

        // Avoid biome transition overlays between
        // different mountain elevation levels.
        if (current.TerrainType !=
                TerrainType.Water &&
            current.TerrainType !=
                TerrainType.Sand &&
            neighbour.TerrainType !=
                TerrainType.Water &&
            neighbour.TerrainType !=
                TerrainType.Sand &&
            current.ElevationLevel !=
                neighbour.ElevationLevel)
        {
            continue;
        }

        bestPriority =
            neighbourPriority;

        bestSource =
            neighbour;

        foundSource =
            true;
    }

    if (!foundSource)
        return null;

    TerrainTransitionAutotileSet set =
        GetTransitionSetForSurface(
            bestSource);

    if (set == null)
        return null;

    int sourceMask = 0;

    for (int i = 0;
         i < TransitionDirections.Length;
         i++)
    {
        Vector2Int direction =
            TransitionDirections[i];

        TerrainCellData neighbour =
            GetTransitionNeighbour(
                current.WorldX + direction.x,
                current.WorldY + direction.y,
                chunkData,
                chunkOrigin);

        if (IsSameOverlaySurface(
                bestSource,
                neighbour))
        {
            sourceMask |= 1 << i;
        }
    }

// The transition is being drawn OUTSIDE the source terrain,
// on top of the neighbouring lower-priority terrain.
//
// Because of that, the mask orientation must be mirrored.
// Example:
// Source terrain is North + West of this cell.
// This cell is therefore the Bottom-Right transition of the source.
//TODO THIS MAY WORK SO UNCOMMENT IF YOU WANT
    //int normalizedMask =
    //    NormalizeTransitionMask(sourceMask);
    //return set.GetTile(normalizedMask);
    return set.GetTile(sourceMask);
}
        
        private TileBase GetRiverTransitionOverlay(
            TerrainCellData current,
            TerrainCellData[,] chunkData,
            Vector3Int chunkOrigin)
        {
            if (_riverTransitionSet == null)
                return null;

            // Ocean already visually connects with river water.
            // We don't need a river-edge overlay on ocean cells.
            if (current.TerrainType ==
                TerrainType.Water)
            {
                return null;
            }

            int mask = 0;
            bool hasRiverNeighbour = false;

            for (int i = 0;
                 i < TransitionDirections.Length;
                 i++)
            {
                Vector2Int direction =
                    TransitionDirections[i];

                TerrainCellData neighbour =
                    GetTransitionNeighbour(
                        current.WorldX + direction.x,
                        current.WorldY + direction.y,
                        chunkData,
                        chunkOrigin);

                if (!neighbour.IsRiver)
                    continue;

                mask |= 1 << i;
                hasRiverNeighbour = true;
            }

            if (!hasRiverNeighbour)
                return null;

            return _riverTransitionSet.GetTile(mask);
        }
        
        private TileBase GetLavaTransitionOverlay(
            TerrainCellData current,
            TerrainCellData[,] chunkData,
            Vector3Int chunkOrigin)
        {
            if (_lavaTransitionSet == null)
                return null;

            if (current.IsLava)
                return null;

            int mask = 0;
            bool hasLavaNeighbour = false;

            for (int i = 0;
                 i < TransitionDirections.Length;
                 i++)
            {
                Vector2Int direction =
                    TransitionDirections[i];

                TerrainCellData neighbour =
                    GetTransitionNeighbour(
                        current.WorldX +
                        direction.x,
                        current.WorldY +
                        direction.y,
                        chunkData,
                        chunkOrigin);

                if (!neighbour.IsLava)
                    continue;

                mask |= 1 << i;
                hasLavaNeighbour =
                    true;
            }

            if (!hasLavaNeighbour)
                return null;

            return _lavaTransitionSet
                .GetTile(mask);
        }
        
        private int GetSurfacePriority(
            TerrainCellData cell)
        {
            if (cell.TerrainType == TerrainType.Water)
                return 0;

            if (cell.TerrainType == TerrainType.Sand)
                return 10;

            return cell.BiomeType switch
            {
                BiomeType.Plains => 20,
                BiomeType.Forest => 21,
                BiomeType.Jungle => 22,
                BiomeType.Desert => 23,
                BiomeType.Snow => 24,
                BiomeType.Mountain => 25,
                BiomeType.Alpine => 26,
                BiomeType.Volcanic => 27,

                _ => 20
            };
        }
        
        private TerrainTransitionAutotileSet
            GetTransitionSetForSurface(
                TerrainCellData source)
        {
            if (source.TerrainType == TerrainType.Sand)
            {
                bool frozen =
                    source.BiomeType == BiomeType.Snow;

                return frozen
                    ? _iceTransitionSet
                    : _sandTransitionSet;
            }

            if (source.TerrainType == TerrainType.Water)
            {
                return _waterTransitionSet;
            }

            BiomeDefinition biome =
                _biomeGenerator != null
                    ? _biomeGenerator.GetDefinition(
                        source.BiomeType)
                    : null;

            return biome != null
                ? biome.GroundTransitionSet
                : null;
        }
        
        private bool IsSameOverlaySurface(
            TerrainCellData a,
            TerrainCellData b)
        {
            if (a.TerrainType != b.TerrainType)
                return false;

            if (a.TerrainType == TerrainType.Water)
                return true;

            if (a.TerrainType == TerrainType.Sand)
            {
                bool aFrozen =
                    a.BiomeType == BiomeType.Snow;

                bool bFrozen =
                    b.BiomeType == BiomeType.Snow;

                return aFrozen == bFrozen;
            }

            return a.BiomeType == b.BiomeType &&
                   a.ElevationLevel ==
                   b.ElevationLevel;
        }
        
private TileBase ResolveTransitionTile(
    TerrainTransitionAutotileSet set,
    int mask,
    TileBase fallback)
{
    if (set != null)
    {
        TileBase tile = set.GetTile(mask);
        if (tile != null)
            return tile;
    }

    return fallback;
}

//TODO NOT USED REMOVE
private int MirrorTransitionMask(int mask)
{
    int mirrored = 0;

    // Original bit layout:
    //
    // NW = 128   N = 1    NE = 2
    // W  = 64             E  = 4
    // SW = 32    S = 16   SE = 8
    //
    // We rotate the relationship by 180 degrees:
    //
    // N  -> S
    // NE -> SW
    // E  -> W
    // SE -> NW
    // S  -> N
    // SW -> NE
    // W  -> E
    // NW -> SE

    if ((mask & 1) != 0)
        mirrored |= 16;   // N -> S

    if ((mask & 2) != 0)
        mirrored |= 32;   // NE -> SW

    if ((mask & 4) != 0)
        mirrored |= 64;   // E -> W

    if ((mask & 8) != 0)
        mirrored |= 128;  // SE -> NW

    if ((mask & 16) != 0)
        mirrored |= 1;    // S -> N

    if ((mask & 32) != 0)
        mirrored |= 2;    // SW -> NE

    if ((mask & 64) != 0)
        mirrored |= 4;    // W -> E

    if ((mask & 128) != 0)
        mirrored |= 8;    // NW -> SE

    return mirrored;
}

private int NormalizeTransitionMask(int mask)
{
    // Bit layout:
    //
    // NW = 128   N = 1    NE = 2
    // W  = 64             E  = 4
    // SW = 32    S = 16   SE = 8

    bool north = (mask & 1) != 0;
    bool northEast = (mask & 2) != 0;
    bool east = (mask & 4) != 0;
    bool southEast = (mask & 8) != 0;
    bool south = (mask & 16) != 0;
    bool southWest = (mask & 32) != 0;
    bool west = (mask & 64) != 0;
    bool northWest = (mask & 128) != 0;

    int normalized = 0;

    // Cardinals always count.
    if (north)
        normalized |= 1;

    if (east)
        normalized |= 4;

    if (south)
        normalized |= 16;

    if (west)
        normalized |= 64;

    // Diagonals only count when both connected
    // cardinal directions also exist.
    if (northEast &&
        north &&
        east)
    {
        normalized |= 2;
    }

    if (southEast &&
        south &&
        east)
    {
        normalized |= 8;
    }

    if (southWest &&
        south &&
        west)
    {
        normalized |= 32;
    }

    if (northWest &&
        north &&
        west)
    {
        normalized |= 128;
    }

    return normalized;
}

//TODO NO LONGER NEEDED
private int GetSurfaceTransitionMask(
    TerrainCellData current,
    TerrainCellData[,] chunkData,
    Vector3Int chunkOrigin)
{
    int mask = 0;

    if (MatchesSurfaceGroup(
            current,
            GetTransitionNeighbour(
                current.WorldX,
                current.WorldY + 1,
                chunkData,
                chunkOrigin)))
        mask |= 1;

    if (MatchesSurfaceGroup(
            current,
            GetTransitionNeighbour(
                current.WorldX + 1,
                current.WorldY + 1,
                chunkData,
                chunkOrigin)))
        mask |= 2;

    if (MatchesSurfaceGroup(
            current,
            GetTransitionNeighbour(
                current.WorldX + 1,
                current.WorldY,
                chunkData,
                chunkOrigin)))
        mask |= 4;

    if (MatchesSurfaceGroup(
            current,
            GetTransitionNeighbour(
                current.WorldX + 1,
                current.WorldY - 1,
                chunkData,
                chunkOrigin)))
        mask |= 8;

    if (MatchesSurfaceGroup(
            current,
            GetTransitionNeighbour(
                current.WorldX,
                current.WorldY - 1,
                chunkData,
                chunkOrigin)))
        mask |= 16;

    if (MatchesSurfaceGroup(
            current,
            GetTransitionNeighbour(
                current.WorldX - 1,
                current.WorldY - 1,
                chunkData,
                chunkOrigin)))
        mask |= 32;

    if (MatchesSurfaceGroup(
            current,
            GetTransitionNeighbour(
                current.WorldX - 1,
                current.WorldY,
                chunkData,
                chunkOrigin)))
        mask |= 64;

    if (MatchesSurfaceGroup(
            current,
            GetTransitionNeighbour(
                current.WorldX - 1,
                current.WorldY + 1,
                chunkData,
                chunkOrigin)))
        mask |= 128;

    return mask;
}

private TerrainCellData GetTransitionNeighbour(
    int worldX,
    int worldY,
    TerrainCellData[,] chunkData,
    Vector3Int chunkOrigin)
{
    int localX = worldX - chunkOrigin.x;
    int localY = worldY - chunkOrigin.y;

    if (chunkData != null &&
        localX >= 0 &&
        localY >= 0 &&
        localX < _chunkSize &&
        localY < _chunkSize)
    {
        return chunkData[localX, localY];
    }

    // Only cells outside the current chunk are sampled again.
    // This keeps cross-chunk transition masks seamless without
    // re-sampling every neighbour inside the chunk.
    return SampleTerrainCell(worldX, worldY);
}
//TODO NO LONGER NEEDED
private bool MatchesSurfaceGroup(
    TerrainCellData current,
    TerrainCellData neighbour)
{
    if (current.TerrainType == TerrainType.Water)
    {
        // Ocean and River share a continuous water surface.
        return neighbour.TerrainType == TerrainType.Water;
    }

    if (current.TerrainType == TerrainType.Sand)
    {
        bool currentFrozen =
            current.BiomeType == BiomeType.Snow;

        bool neighbourFrozen =
            neighbour.BiomeType == BiomeType.Snow;

        return neighbour.TerrainType == TerrainType.Sand &&
               currentFrozen == neighbourFrozen;
    }

    // Biome ground transitions use the same biome and elevation.
    // This gives clean edges between Plains/Forest/Jungle/Snow/etc.
    // while keeping each elevation plateau internally connected.
    return neighbour.TerrainType != TerrainType.Water &&
           neighbour.TerrainType != TerrainType.Sand &&
           neighbour.BiomeType == current.BiomeType &&
           neighbour.ElevationLevel ==
               current.ElevationLevel;
}

private int GetElevationEightNeighbourMask(
            int worldX,
            int worldY,
            int currentElevation)
        {
            int mask = 0;

            // A neighbour belongs to this contour when it is at the same
            // elevation or higher. This creates a separate nested outline
            // around every elevation step.
            if (GetElevationLevel(worldX, worldY + 1) >= currentElevation)     mask |= 1;
            if (GetElevationLevel(worldX + 1, worldY + 1) >= currentElevation) mask |= 2;
            if (GetElevationLevel(worldX + 1, worldY) >= currentElevation)     mask |= 4;
            if (GetElevationLevel(worldX + 1, worldY - 1) >= currentElevation) mask |= 8;
            if (GetElevationLevel(worldX, worldY - 1) >= currentElevation)     mask |= 16;
            if (GetElevationLevel(worldX - 1, worldY - 1) >= currentElevation) mask |= 32;
            if (GetElevationLevel(worldX - 1, worldY) >= currentElevation)     mask |= 64;
            if (GetElevationLevel(worldX - 1, worldY + 1) >= currentElevation) mask |= 128;

            return mask;
        }

        private TileBase ResolveMountainOutlineTile(
            TerrainCellData cell,
            int eightNeighbourMask,
            bool hasDirectlyHigherNeighbour)
        {
            BiomeDefinition biome =
                _biomeGenerator != null
                    ? _biomeGenerator.GetDefinition(
                        cell.BiomeType)
                    : null;

            MountainAutotileSet preferredSet = null;

            if (biome != null)
            {
                preferredSet =
                    hasDirectlyHigherNeighbour
                        ? biome.MountainElevationStepAutotileSet
                        : biome.MountainPeakAutotileSet;
            }

            if (preferredSet != null)
            {
                TileBase tile =
                    preferredSet.GetTile(
                        eightNeighbourMask);

                if (tile != null)
                    return tile;
            }

            // Global fallback.
            MountainAutotileSet globalSet =
                hasDirectlyHigherNeighbour
                    ? _mountainElevationStepAutotileSet
                    : _mountainPeakAutotileSet;

            if (globalSet != null)
            {
                TileBase tile =
                    globalSet.GetTile(
                        eightNeighbourMask);

                if (tile != null)
                    return tile;
            }

            if (_mountainAutotileSet != null)
            {
                TileBase tile =
                    _mountainAutotileSet.GetTile(
                        eightNeighbourMask);

                if (tile != null)
                    return tile;
            }

            return _mountainOutlineFallbackTile;
        }

        private bool HasNeighbourExactlyOneLevelHigher(
            int worldX,
            int worldY,
            int currentElevation)
        {
            int target = currentElevation + 1;

            return GetElevationLevel(worldX, worldY + 1) == target ||
                   GetElevationLevel(worldX + 1, worldY + 1) == target ||
                   GetElevationLevel(worldX + 1, worldY) == target ||
                   GetElevationLevel(worldX + 1, worldY - 1) == target ||
                   GetElevationLevel(worldX, worldY - 1) == target ||
                   GetElevationLevel(worldX - 1, worldY - 1) == target ||
                   GetElevationLevel(worldX - 1, worldY) == target ||
                   GetElevationLevel(worldX - 1, worldY + 1) == target;
        }

        private bool HasLowerEightNeighbour(
            int worldX,
            int worldY,
            int currentElevation)
        {
            return GetElevationLevel(worldX, worldY + 1) < currentElevation ||
                   GetElevationLevel(worldX + 1, worldY + 1) < currentElevation ||
                   GetElevationLevel(worldX + 1, worldY) < currentElevation ||
                   GetElevationLevel(worldX + 1, worldY - 1) < currentElevation ||
                   GetElevationLevel(worldX, worldY - 1) < currentElevation ||
                   GetElevationLevel(worldX - 1, worldY - 1) < currentElevation ||
                   GetElevationLevel(worldX - 1, worldY) < currentElevation ||
                   GetElevationLevel(worldX - 1, worldY + 1) < currentElevation;
        }

        public TerrainCellData GetCellData(int worldX, int worldY)
        {
            return SampleTerrainCell(worldX, worldY);
        }


public int GetElevationLevel(int worldX, int worldY)
{
    float terrainHeight =
        SampleTerrainHeight(worldX, worldY);

    ClimateSample climate = _biomeGenerator != null
        ? _biomeGenerator.SampleClimate(worldX, worldY)
        : new ClimateSample(0.5f, 0.5f);

    BiomeType baseBiomeType = _biomeGenerator != null
        ? _biomeGenerator.ResolveBaseLandBiome(
            worldX,
            worldY,
            climate.Temperature,
            climate.Moisture)
        : BiomeType.Plains;

    BiomeDefinition biome = _biomeGenerator != null
        ? _biomeGenerator.GetDefinition(baseBiomeType)
        : null;

    float mountainNoise = SampleMountainNoise(
        worldX,
        worldY,
        GetMountainNoiseScale(biome),
        biome);

    float heightVariation =
        SampleMountainHeightVariation(
            worldX,
            worldY,
            GetMountainHeightVariationScale(biome));

    int elevation = ResolveElevationLevel(
        terrainHeight,
        mountainNoise,
        heightVariation,
        biome);

    TerrainType originalTerrain =
        ResolveTerrainType(
            terrainHeight,
            elevation);

    if (originalTerrain != TerrainType.Water &&
        originalTerrain != TerrainType.Sand &&
        _riverGenerator != null &&
        _riverGenerator.IsRiverCell(
            worldX,
            worldY,
            terrainHeight,
            _landThreshold,
            elevation))
    {
        return 0;
    }

    return elevation;
}


private float SampleTerrainHeight(int worldX, int worldY)
{
    float scale = Mathf.Max(0.0001f, _noiseScale);

    float localHeight = Mathf.PerlinNoise(
        (worldX + _terrainNoiseOffset.x) / scale,
        (worldY + _terrainNoiseOffset.y) / scale);

    if (!_useIslandWorldShape)
        return localHeight;

    float continental = Mathf.PerlinNoise(
        (worldX + _continentalOffset.x) /
            Mathf.Max(32f, _continentalScale),
        (worldY + _continentalOffset.y) /
            Mathf.Max(32f, _continentalScale));

    float islandNoise = Mathf.PerlinNoise(
        (worldX + _islandOffset.x) /
            Mathf.Max(16f, _islandScale),
        (worldY + _islandOffset.y) /
            Mathf.Max(16f, _islandScale));
    
    float archipelagoNoise =
        Mathf.PerlinNoise(
            (worldX +
             _archipelagoOffset.x) /
            Mathf.Max(
                8f,
                _archipelagoScale),

            (worldY +
             _archipelagoOffset.y) /
            Mathf.Max(
                8f,
                _archipelagoScale));

    // Broad continental land plus smaller island clusters.
    float continentLand = Mathf.InverseLerp(
        _continentThreshold - 0.10f,
        _continentThreshold + 0.10f,
        continental);

    float smallIslandLand = Mathf.InverseLerp(
        _smallIslandThreshold - 0.08f,
        _smallIslandThreshold + 0.06f,
        islandNoise);

    // Small islands primarily matter in oceanic/low-continent areas.
    float landMask =
        Mathf.Max(
            continentLand,
            smallIslandLand *
            (1f - continentLand) *
            0.9f);
    
    float archipelagoLand =
        Mathf.InverseLerp(
            _archipelagoThreshold,
            1f,
            archipelagoNoise);

    bool canSpawnArchipelago =
        continental <
        _archipelagoMaxContinentalness;

    if (canSpawnArchipelago)
    {
        landMask =
            Mathf.Max(
                landMask,
                archipelagoLand *
                _archipelagoStrength);
    }

    float shapedHeight = Mathf.Lerp(
        localHeight,
        localHeight * 0.48f + landMask * 0.58f,
        _continentalInfluence);

    return Mathf.Clamp01(shapedHeight);
}

private float SampleMountainNoise(
    int worldX,
    int worldY,
    float scale,
    BiomeDefinition biome)
{
    scale =
        Mathf.Max(
            0.0001f,
            scale);

    float x =
        (worldX +
         _mountainNoiseOffset.x) /
        scale;

    float y =
        (worldY +
         _mountainNoiseOffset.y) /
        scale;

    float normalNoise =
        Mathf.PerlinNoise(
            x,
            y);

    float ridgeNoise =
        1f -
        Mathf.Abs(
            normalNoise * 2f -
            1f);

    float ridgeInfluence =
        biome != null
            ? biome.MountainRidgeInfluence
            : 0.5f;

    return Mathf.Lerp(
        normalNoise,
        ridgeNoise,
        ridgeInfluence);
}

        private float SampleMountainHeightVariation(
            int worldX,
            int worldY,
            float requestedScale)
        {
            float scale = Mathf.Max(0.0001f, requestedScale);
            float nx = (worldX + _mountainHeightVariationOffset.x) / scale;
            float ny = (worldY + _mountainHeightVariationOffset.y) / scale;

            float broad = Mathf.PerlinNoise(nx, ny);
            float detail = Mathf.PerlinNoise(nx * 1.75f - 29.4f, ny * 1.75f + 63.8f);
            return Mathf.Clamp01(broad * 0.8f + detail * 0.2f);
        }


        private float GetMountainNoiseScale(BiomeDefinition biome)
        {
            return biome != null
                ? biome.MountainNoiseScale
                : _mountainNoiseScale;
        }

        private float GetMountainHeightVariationScale(BiomeDefinition biome)
        {
            return biome != null
                ? biome.HeightVariationScale
                : _mountainHeightVariationScale;
        }

        private float Deterministic01(int worldX, int worldY, int salt)
        {
            unchecked
            {
                uint hash = (uint)_activeSeed;
                hash ^= (uint)worldX * 374761393u;
                hash ^= (uint)worldY * 668265263u;
                hash ^= (uint)salt * 2246822519u;
                hash = (hash ^ (hash >> 13)) * 1274126177u;
                hash ^= hash >> 16;
                return (hash & 0x00FFFFFFu) / 16777216f;
            }
        }


        private void OnValidate()
        {
            _waterAnimationFrameRate = Mathf.Max(
                0.01f,
                _waterAnimationFrameRate);
            _continentalScale = Mathf.Max(32f, _continentalScale);
            _islandScale = Mathf.Max(16f, _islandScale);
            _continentThreshold = Mathf.Clamp01(_continentThreshold);
            _smallIslandThreshold = Mathf.Clamp01(_smallIslandThreshold);
            _continentalInfluence = Mathf.Clamp01(_continentalInfluence);
            _chunkSize = Mathf.Max(4, _chunkSize);
            _noiseScale = Mathf.Max(0.0001f, _noiseScale);
            _mountainNoiseScale = Mathf.Max(0.0001f, _mountainNoiseScale);
            _mountainHeightVariationScale = Mathf.Max(0.0001f, _mountainHeightVariationScale);
            _maximumElevationLevels = Mathf.Clamp(_maximumElevationLevels, 1, 32);
            _elevationCurvePower = Mathf.Clamp(_elevationCurvePower, 0.25f, 4f);
            _minimumMountainHeightFraction = Mathf.Clamp(_minimumMountainHeightFraction, 0.05f, 1f);
            _sandBandWidth = Mathf.Clamp(_sandBandWidth, 0.01f, 0.4f);
            _mountainThreshold = Mathf.Clamp01(_mountainThreshold);
            _mountainLandBuffer = Mathf.Clamp(_mountainLandBuffer, 0f, 0.3f);
            _rampChance = Mathf.Clamp01(_rampChance);
            _rampSpacing = Mathf.Clamp(_rampSpacing, 1, 12);
        }
    }
}
