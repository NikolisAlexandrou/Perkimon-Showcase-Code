using UnityEngine;

namespace island
{
    public sealed class LavaLakeGenerator :
        MonoBehaviour
    {
        [Header("Lava Lake Shape")]
        [Tooltip("Large-scale noise controls the main lake regions.")]
        [SerializeField, Min(1f)]
        private float _largeScale = 65f;

        [Tooltip("Smaller noise adds irregular shorelines.")]
        [SerializeField, Min(1f)]
        private float _detailScale = 22f;

        [SerializeField, Range(0f, 1f)]
        private float _lavaThreshold = 0.72f;

        [Header("Restrictions")]
        [SerializeField, Range(0, 20)]
        private int _maximumElevation = 2;

        private Vector2 _largeOffset;
        private Vector2 _detailOffset;

        public void Initialize(
            int seed)
        {
            System.Random random =
                new System.Random(
                    seed ^
                    0x4A3B2C1D);

            _largeOffset =
                new Vector2(
                    random.Next(
                        -100000,
                        100000),
                    random.Next(
                        -100000,
                        100000));

            _detailOffset =
                new Vector2(
                    random.Next(
                        -100000,
                        100000),
                    random.Next(
                        -100000,
                        100000));
        }

        public bool IsLavaCell(
            int worldX,
            int worldY,
            BiomeType biome,
            TerrainType terrainType,
            int elevation,
            bool isRiver)
        {
            if (biome !=
                BiomeType.Volcanic)
            {
                return false;
            }

            if (terrainType ==
                TerrainType.Water)
            {
                return false;
            }

            if (isRiver)
                return false;

            if (elevation >
                _maximumElevation)
            {
                return false;
            }

            float large =
                Mathf.PerlinNoise(
                    (worldX +
                     _largeOffset.x) /
                        Mathf.Max(
                            1f,
                            _largeScale),

                    (worldY +
                     _largeOffset.y) /
                        Mathf.Max(
                            1f,
                            _largeScale));

            float detail =
                Mathf.PerlinNoise(
                    (worldX +
                     _detailOffset.x) /
                        Mathf.Max(
                            1f,
                            _detailScale),

                    (worldY +
                     _detailOffset.y) /
                        Mathf.Max(
                            1f,
                            _detailScale));

            float lavaNoise =
                large * 0.8f +
                detail * 0.2f;

            return lavaNoise >=
                   _lavaThreshold;
        }
    }
}