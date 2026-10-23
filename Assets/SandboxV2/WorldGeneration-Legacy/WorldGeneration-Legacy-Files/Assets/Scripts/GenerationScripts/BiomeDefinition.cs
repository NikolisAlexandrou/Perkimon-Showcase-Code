using UnityEngine;
using UnityEngine.Tilemaps;

namespace island
{
    [CreateAssetMenu(
        fileName = "BiomeDefinition",
        menuName = "Project Atlas/World/Biome Definition")]
    public sealed class BiomeDefinition : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private BiomeType _biomeType = BiomeType.Plains;

        [Header("Terrain")]
        [SerializeField] private TileBase _groundTile;
        [SerializeField] private TileBase _baseTile;
        
        [Tooltip("Transparent edge artwork belonging to this biome, painted over a lower-priority neighbouring terrain.")]
        //[Tooltip("Optional 8-neighbour transition tiles for this biome's ground. Falls back to Ground Tile when unassigned.")]
        [SerializeField] private TerrainTransitionAutotileSet _groundTransitionSet;
        
        [Header("Mountain Visuals")]
        [Tooltip("Mountain outline tiles used for the outer/top contour in this biome.")]
        [SerializeField]
        private MountainAutotileSet _mountainPeakAutotileSet;

        [Tooltip("Mountain outline tiles used between elevation levels in this biome.")]
        [SerializeField]
        private MountainAutotileSet _mountainElevationStepAutotileSet;

        [Header("Biome Mountains")]
        [SerializeField] private bool _allowMountains = true;

        [Tooltip("Larger values create broader mountain regions in this biome.")]
        [SerializeField, Min(0.0001f)] private float _mountainNoiseScale = 55f;

        [Tooltip("Higher values make mountains less common in this biome.")]
        [SerializeField, Range(0f, 1f)] private float _mountainThreshold = 0.68f;
        
        [Tooltip("0 uses normal blob-like noise. 1 uses fully ridged noise.")] [SerializeField, Range(0f, 1f)]
        private float _mountainRidgeInfluence = 0.6f;
        // suggested:
        //plains: 0.15
        //Forest: 0.35
        //Jungle: 0.4
        //Desert: 0.5
        //Snow: 0.65
        //Mountain 0.85
        //Alpine 0.95
        //Volcanic 0.75
        

        [Tooltip("Keeps mountains away from beaches and low terrain.")]
        [SerializeField, Range(0f, 0.3f)] private float _mountainLandBuffer = 0.08f;

        [SerializeField, Range(1, 32)] private int _maximumElevationLevels = 6;
        [SerializeField, Range(0.25f, 4f)] private float _elevationCurvePower = 1.5f;

        [Tooltip("Larger values create broader regions with similar mountain heights.")]
        [SerializeField, Min(0.0001f)] private float _heightVariationScale = 140f;

        [SerializeField, Range(0.05f, 1f)] private float _minimumHeightFraction = 0.35f;

        [Header("Elevation Population")]
        [SerializeField] private bool _allowTreesOnElevatedCells;
        [SerializeField] private bool _allowDecorationsOnElevatedCells = true;
        [SerializeField] private bool _allowPropsOnElevatedCells = true;

        [Header("Trees")]
        [SerializeField, Min(0f)] private float _treeChanceMultiplier = 1f;
        
        [Header("Biome Trees")]
        [Tooltip("Weighted tree prefabs that can spawn in this biome.")]
        [SerializeField]
        private BiomeTreeEntry[] _treeEntries =
            System.Array.Empty<BiomeTreeEntry>();
        

        [Header("Tile Decorations")]
        [Tooltip("Overall chance that an eligible cell receives one decoration tile.")]
        [SerializeField, Range(0f, 1f)] private float _decorationChance = 0.3f;

        [SerializeField] private BiomeDecorationEntry[] _decorations =
            System.Array.Empty<BiomeDecorationEntry>();

        [Header("Scarce World Props")]
        [Tooltip("Overall chance checked before selecting a weighted prop entry.")]
        [SerializeField, Range(0f, 1f)] private float _propChance = 0.025f;

        [SerializeField] private BiomePropEntry[] _props =
            System.Array.Empty<BiomePropEntry>();

        public BiomeType BiomeType => _biomeType;
        public TileBase GroundTile => _groundTile;
        public TileBase BaseTile => _baseTile;
        public TerrainTransitionAutotileSet GroundTransitionSet =>
            _groundTransitionSet;
        
        public MountainAutotileSet MountainPeakAutotileSet =>
            _mountainPeakAutotileSet;

        public MountainAutotileSet MountainElevationStepAutotileSet =>
            _mountainElevationStepAutotileSet;
        public bool AllowMountains => _allowMountains;
        public float MountainNoiseScale => Mathf.Max(0.0001f, _mountainNoiseScale);
        public float MountainThreshold => Mathf.Clamp01(_mountainThreshold);
        public float MountainRidgeInfluence =>
            Mathf.Clamp01(
                _mountainRidgeInfluence);
        public float MountainLandBuffer => Mathf.Clamp(_mountainLandBuffer, 0f, 0.3f);
        public int MaximumElevationLevels => Mathf.Clamp(_maximumElevationLevels, 1, 32);
        public float ElevationCurvePower => Mathf.Clamp(_elevationCurvePower, 0.25f, 4f);
        public float HeightVariationScale => Mathf.Max(0.0001f, _heightVariationScale);
        public float MinimumHeightFraction => Mathf.Clamp(_minimumHeightFraction, 0.05f, 1f);
        public bool AllowTreesOnElevatedCells => _allowTreesOnElevatedCells;
        public bool AllowDecorationsOnElevatedCells => _allowDecorationsOnElevatedCells;
        public bool AllowPropsOnElevatedCells => _allowPropsOnElevatedCells;
        public float TreeChanceMultiplier => Mathf.Max(0f, _treeChanceMultiplier);
        public BiomeTreeEntry[] TreeEntries => _treeEntries;
        public float DecorationChance => Mathf.Clamp01(_decorationChance);
        public float PropChance => Mathf.Clamp01(_propChance);
        public BiomeDecorationEntry[] Decorations => _decorations;
        public BiomePropEntry[] Props => _props;

        public TileBase SelectDecoration(float detailNoise, float selectionRoll)
        {
            if (_decorations == null || _decorations.Length == 0)
                return null;

            float totalWeight = 0f;
            foreach (BiomeDecorationEntry entry in _decorations)
            {
                if (entry != null && entry.Tile != null &&
                    entry.AllowsNoise(detailNoise))
                {
                    totalWeight += entry.Weight;
                }
            }

            if (totalWeight <= 0f)
                return null;

            float target = Mathf.Clamp01(selectionRoll) * totalWeight;
            foreach (BiomeDecorationEntry entry in _decorations)
            {
                if (entry == null || entry.Tile == null ||
                    !entry.AllowsNoise(detailNoise))
                {
                    continue;
                }

                target -= entry.Weight;
                if (target <= 0f)
                    return entry.Tile;
            }

            return null;
        }

        public BiomePropEntry SelectProp(float selectionRoll, int elevation)
        {
            if (_props == null || _props.Length == 0)
                return null;

            float totalWeight = 0f;
            foreach (BiomePropEntry entry in _props)
            {
                if (entry != null && entry.Prefab != null &&
                    entry.AllowsElevation(elevation))
                {
                    totalWeight += entry.Weight;
                }
            }

            if (totalWeight <= 0f)
                return null;

            float target = Mathf.Clamp01(selectionRoll) * totalWeight;
            foreach (BiomePropEntry entry in _props)
            {
                if (entry == null || entry.Prefab == null ||
                    !entry.AllowsElevation(elevation))
                {
                    continue;
                }

                target -= entry.Weight;
                if (target <= 0f)
                    return entry;
            }

            return null;
        }

        private void OnValidate()
        {
            _mountainNoiseScale = Mathf.Max(0.0001f, _mountainNoiseScale);
            _mountainThreshold = Mathf.Clamp01(_mountainThreshold);
            _mountainLandBuffer = Mathf.Clamp(_mountainLandBuffer, 0f, 0.3f);
            _maximumElevationLevels = Mathf.Clamp(_maximumElevationLevels, 1, 32);
            _elevationCurvePower = Mathf.Clamp(_elevationCurvePower, 0.25f, 4f);
            _heightVariationScale = Mathf.Max(0.0001f, _heightVariationScale);
            _minimumHeightFraction = Mathf.Clamp(_minimumHeightFraction, 0.05f, 1f);
            _treeChanceMultiplier = Mathf.Max(0f, _treeChanceMultiplier);
            _decorationChance = Mathf.Clamp01(_decorationChance);
            _propChance = Mathf.Clamp01(_propChance);
        }
    }
}
