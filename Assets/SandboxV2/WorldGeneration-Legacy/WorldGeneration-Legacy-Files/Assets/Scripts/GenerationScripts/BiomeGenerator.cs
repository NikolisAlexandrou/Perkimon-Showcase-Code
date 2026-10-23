using System;
using UnityEngine;

namespace island
{
    public sealed class BiomeGenerator : MonoBehaviour
    {
        [Header("Climate Noise")]
        [Tooltip("Larger values create broader temperature zones.")]
        [SerializeField, Min(0.0001f)] private float _temperatureScale = 180f;

        [Tooltip("Larger values create broader moisture zones.")]
        [SerializeField, Min(0.0001f)] private float _moistureScale = 150f;

        [SerializeField, Range(0f, 1f)] private float _coldThreshold = 0.30f;
        [SerializeField, Range(0f, 1f)] private float _hotThreshold = 0.68f;
        [SerializeField, Range(0f, 1f)] private float _dryThreshold = 0.36f;
        [SerializeField, Range(0f, 1f)] private float _wetThreshold = 0.60f;
        
        [Header("Biome Shape Warping")]
        [Tooltip("Larger values create broader, smoother biome distortion.")]
        [SerializeField, Min(1f)]
        private float _biomeWarpScale = 180f;

        [Tooltip("How far climate sampling coordinates are distorted.")]
        [SerializeField, Range(0f, 100f)]
        private float _biomeWarpStrength = 35f;

        [Header("Jungle")]
        [Tooltip("Minimum temperature required before wet terrain becomes Jungle instead of Forest.")]
        [SerializeField, Range(0f, 1f)] private float _jungleTemperatureThreshold = 0.62f;

        [Tooltip("Minimum moisture required for Jungle.")]
        [SerializeField, Range(0f, 1f)] private float _jungleMoistureThreshold = 0.68f;

        [Header("Mountain / Alpine Regions")]
        [Tooltip("Broad noise scale used to select Mountain and Alpine as full world biomes.")]
        [SerializeField, Min(0.0001f)] private float _mountainRegionScale = 280f;

        [Tooltip("Cells above this regional noise become Mountain biome candidates.")]
        [SerializeField, Range(0f, 1f)] private float _mountainRegionThreshold = 0.70f;

        [Tooltip("Colder high-noise mountain regions become Alpine.")]
        [SerializeField, Range(0f, 1f)] private float _alpineTemperatureThreshold = 0.34f;

       

        [Header("Volcanic Regions")]
        [Tooltip("Large values create broader volcanic regions.")]
        [SerializeField, Min(0.0001f)] private float _volcanicScale = 240f;

        [Tooltip("Higher values make volcanic regions rarer.")]
        [SerializeField, Range(0f, 1f)] private float _volcanicThreshold = 0.79f;

        [Tooltip("Volcanic regions must be at least this warm.")]
        [SerializeField, Range(0f, 1f)] private float _volcanicMinimumTemperature = 0.52f;

        [Header("Elevation Overrides")]
        [Tooltip("Elevation at or above which ordinary elevated land becomes Mountain.")]
        [SerializeField, Min(1)] private int _mountainBiomeElevation = 1;

        [Tooltip("Elevation at or above which ordinary Mountain becomes Alpine.")]
        [SerializeField, Min(1)] private int _alpineElevation = 5;

        [Header("Biome Assets")]
        [SerializeField] private BiomeDefinition[] _biomes =
            Array.Empty<BiomeDefinition>();

        private Vector2 _temperatureOffset;
        private Vector2 _moistureOffset;
        private Vector2 _volcanicOffset;
        private Vector2 _mountainRegionOffset;
        
        private Vector2 _biomeWarpOffsetX;
        private Vector2 _biomeWarpOffsetY;
        
       

        public void Initialize(int seed)
        {
            System.Random random =
                new System.Random(seed ^ 0x5F3759DF);

            _temperatureOffset = new Vector2(
                random.Next(-100000, 100000),
                random.Next(-100000, 100000));

            _moistureOffset = new Vector2(
                random.Next(-100000, 100000),
                random.Next(-100000, 100000));

            _volcanicOffset = new Vector2(
                random.Next(-100000, 100000),
                random.Next(-100000, 100000));

            _mountainRegionOffset = new Vector2(
                random.Next(-100000, 100000),
                random.Next(-100000, 100000));
            
            _biomeWarpOffsetX = new Vector2(
                random.Next(-100000, 100000),
                random.Next(-100000, 100000));

            _biomeWarpOffsetY = new Vector2(
                random.Next(-100000, 100000),
                random.Next(-100000, 100000));
        }

        public ClimateSample SampleClimate(
            int worldX,
            int worldY)
        {
            float warpScale =
                Mathf.Max(1f, _biomeWarpScale);

            float warpX =
                (Mathf.PerlinNoise(
                     (worldX + _biomeWarpOffsetX.x) /
                     warpScale,
                     (worldY + _biomeWarpOffsetX.y) /
                     warpScale) -
                 0.5f) *
                2f *
                _biomeWarpStrength;

            float warpY =
                (Mathf.PerlinNoise(
                     (worldX + _biomeWarpOffsetY.x) /
                     warpScale,
                     (worldY + _biomeWarpOffsetY.y) /
                     warpScale) -
                 0.5f) *
                2f *
                _biomeWarpStrength;

            float warpedX =
                worldX + warpX;

            float warpedY =
                worldY + warpY;

            float temperature =
                SampleFractal(
                    warpedX,
                    warpedY,
                    _temperatureScale,
                    _temperatureOffset,
                    19.7f,
                    -41.2f);

            float moisture =
                SampleFractal(
                    warpedX,
                    warpedY,
                    _moistureScale,
                    _moistureOffset,
                    -72.4f,
                    33.8f);

            return new ClimateSample(
                temperature,
                moisture);
        }


public BiomeType ResolveBaseLandBiome(
    int worldX,
    int worldY,
    float temperature,
    float moisture)
{
    if (IsVolcanic(worldX, worldY, temperature))
        return BiomeType.Volcanic;

    float mountainRegion = SampleFractal(
        worldX,
        worldY,
        _mountainRegionScale,
        _mountainRegionOffset,
        -24.7f,
        63.4f);

    // Mountain and Alpine are now proper large-scale biomes. Their
    // own BiomeDefinition mountain settings control the actual
    // elevation generation inside those regions.
    if (mountainRegion >= _mountainRegionThreshold)
    {
        return temperature <= _alpineTemperatureThreshold
            ? BiomeType.Alpine
            : BiomeType.Mountain;
    }

    if (temperature <= _coldThreshold)
        return BiomeType.Snow;

    if (temperature >= _jungleTemperatureThreshold &&
        moisture >= _jungleMoistureThreshold)
    {
        return BiomeType.Jungle;
    }

    if (temperature >= _hotThreshold &&
        moisture <= _dryThreshold)
    {
        return BiomeType.Desert;
    }

    if (moisture >= _wetThreshold)
        return BiomeType.Forest;

    return BiomeType.Plains;
}

        // Compatibility overload for older callers.
        public BiomeType ResolveBaseLandBiome(
            float temperature,
            float moisture)
        {
            if (temperature <= _coldThreshold)
                return BiomeType.Snow;

            if (temperature >= _jungleTemperatureThreshold &&
                moisture >= _jungleMoistureThreshold)
            {
                return BiomeType.Jungle;
            }

            if (temperature >= _hotThreshold &&
                moisture <= _dryThreshold)
            {
                return BiomeType.Desert;
            }

            if (moisture >= _wetThreshold)
                return BiomeType.Forest;

            return BiomeType.Plains;
        }


        public BiomeType ResolveBiome(
            TerrainType terrainType,
            bool isRiver,
            int elevationLevel,
            BiomeType baseLandBiome)
        {
            if (terrainType == TerrainType.Water)
                return BiomeType.Ocean;

            // Keep Snow identity so its coast can use ice.
            if (terrainType == TerrainType.Sand &&
                baseLandBiome == BiomeType.Snow)
            {
                return BiomeType.Snow;
            }

            if (terrainType == TerrainType.Sand)
                return BiomeType.Beach;

            // Elevation no longer changes biome.
            return baseLandBiome;
        }

        public BiomeDefinition GetDefinition(
            BiomeType biomeType)
        {
            if (_biomes == null)
                return null;

            foreach (BiomeDefinition definition in _biomes)
            {
                if (definition != null &&
                    definition.BiomeType == biomeType)
                {
                    return definition;
                }
            }

            return null;
        }

        private bool IsVolcanic(
            int worldX,
            int worldY,
            float temperature)
        {
            if (temperature < _volcanicMinimumTemperature)
                return false;

            float noise = SampleFractal(
                worldX,
                worldY,
                _volcanicScale,
                _volcanicOffset,
                84.1f,
                -12.9f);

            return noise >= _volcanicThreshold;
        }

        private static float SampleFractal(
            float worldX,
            float worldY,
            float scale,
            Vector2 offset,
            float detailOffsetX,
            float detailOffsetY)
        {
            scale = Mathf.Max(0.0001f, scale);

            float x = (worldX + offset.x) / scale;
            float y = (worldY + offset.y) / scale;

            float broad = Mathf.PerlinNoise(x, y);
            float detail = Mathf.PerlinNoise(
                x * 2f + detailOffsetX,
                y * 2f + detailOffsetY);

            return Mathf.Clamp01(
                broad * 0.8f +
                detail * 0.2f);
        }

        private void OnValidate()
        {
            _temperatureScale = Mathf.Max(
                0.0001f,
                _temperatureScale);

            _moistureScale = Mathf.Max(
                0.0001f,
                _moistureScale);

            _volcanicScale = Mathf.Max(
                0.0001f,
                _volcanicScale);

            _mountainRegionScale = Mathf.Max(
                0.0001f,
                _mountainRegionScale);

            _mountainRegionThreshold = Mathf.Clamp01(
                _mountainRegionThreshold);

            _alpineTemperatureThreshold = Mathf.Clamp01(
                _alpineTemperatureThreshold);

            _coldThreshold = Mathf.Clamp01(
                _coldThreshold);

            _hotThreshold = Mathf.Clamp01(
                _hotThreshold);

            _dryThreshold = Mathf.Clamp01(
                _dryThreshold);

            _wetThreshold = Mathf.Clamp01(
                _wetThreshold);

            _jungleTemperatureThreshold = Mathf.Clamp01(
                _jungleTemperatureThreshold);

            _jungleMoistureThreshold = Mathf.Clamp01(
                _jungleMoistureThreshold);

            _volcanicThreshold = Mathf.Clamp01(
                _volcanicThreshold);

            _volcanicMinimumTemperature = Mathf.Clamp01(
                _volcanicMinimumTemperature);

            _mountainBiomeElevation = Mathf.Max(
                1,
                _mountainBiomeElevation);

            _alpineElevation = Mathf.Max(
                _mountainBiomeElevation + 1,
                _alpineElevation);

            if (_hotThreshold < _coldThreshold)
                _hotThreshold = _coldThreshold;

            if (_wetThreshold < _dryThreshold)
                _wetThreshold = _dryThreshold;
        }
    }

    [Serializable]
    public readonly struct ClimateSample
    {
        public float Temperature { get; }
        public float Moisture { get; }

        public ClimateSample(
            float temperature,
            float moisture)
        {
            Temperature = Mathf.Clamp01(
                temperature);

            Moisture = Mathf.Clamp01(
                moisture);
        }
    }
}
