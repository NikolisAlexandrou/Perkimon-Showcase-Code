using System;

namespace island
{
    [Serializable]
    public struct TerrainCellData
    {
        public int WorldX;
        public int WorldY;

        public float Height;
        public float MountainNoise;
        public float MountainHeightVariation;
        public float Temperature;
        public float Moisture;

        public TerrainType TerrainType;
        public BiomeType BiomeType;

        /// <summary>
        /// True when the cell belongs to the deterministic river network.
        /// Rivers are overlay features and do not replace the underlying TerrainType.
        /// </summary>
        public bool IsRiver;
        
        /// <summary>
        /// True when this cell contains a lava-lake overlay.
        /// The underlying terrain remains Volcanic.
        /// </summary>
        public bool IsLava;

        /// <summary>
        /// 0 is normal ground. Values above 0 are elevated plateaus.
        /// </summary>
        public int ElevationLevel;

        /// <summary>
        /// True when at least one neighbouring cell is lower.
        /// </summary>
        public bool IsElevationOutline;

        public bool HasRamp;
        public RampDirection RampDirection;

        public bool IsMountainOutline => IsElevationOutline;
        public bool IsElevated => ElevationLevel > 0;
        public bool IsOcean =>
            TerrainType == TerrainType.Water;

        public TerrainCellData(
            int worldX,
            int worldY,
            float height,
            float mountainNoise,
            float mountainHeightVariation,
            float temperature,
            float moisture,
            TerrainType terrainType,
            BiomeType biomeType,
            bool isRiver,
            bool isLava,
            int elevationLevel,
            bool isElevationOutline,
            bool hasRamp,
            RampDirection rampDirection)
        {
            WorldX = worldX;
            WorldY = worldY;
            Height = height;
            MountainNoise = mountainNoise;
            MountainHeightVariation = mountainHeightVariation;
            Temperature = temperature;
            Moisture = moisture;
            TerrainType = terrainType;
            BiomeType = biomeType;
            IsRiver = isRiver;
            IsLava = isLava;
            ElevationLevel = Math.Max(0, elevationLevel);
            IsElevationOutline = isElevationOutline;
            HasRamp = hasRamp;
            RampDirection = hasRamp
                ? rampDirection
                : RampDirection.None;
        }

        public bool IsWalkableBase =>
            TerrainType == TerrainType.Grass ||
            TerrainType == TerrainType.Sand ||
            TerrainType == TerrainType.Mountain;
    }
}
