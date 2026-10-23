using UnityEngine;

namespace island
{
    /// <summary>
    /// Deterministic continuous rivers for an infinite chunk world.
    ///
    /// River paths are evaluated entirely in world coordinates, so chunk
    /// boundaries cannot break them. Each active river corridor has a stable
    /// meandering center line. A second family of rivers can cross the first
    /// to create tributary-like networks.
    /// </summary>
    public sealed class RiverGenerator : MonoBehaviour
    {
        [Header("Primary Rivers")]
        [SerializeField, Min(24f)] private float _riverSpacing = 130f;
        [SerializeField, Range(0.75f, 8f)] private float _riverHalfWidth = 2f;
        [SerializeField, Min(8f)] private float _meanderScale = 100f;
        [SerializeField, Range(0f, 60f)] private float _meanderStrength = 22f;
        [SerializeField, Range(0f, 1f)] private float _corridorChance = 0.85f;

        [Header("Secondary Rivers")]
        [SerializeField] private bool _enableSecondaryRivers = true;
        [SerializeField, Min(24f)] private float _secondarySpacing = 210f;
        [SerializeField, Range(0.5f, 6f)] private float _secondaryHalfWidth = 1.25f;
        [SerializeField, Range(0f, 1f)] private float _secondaryCorridorChance = 0.45f;

        [Header("Terrain Restrictions")]
        [Tooltip("Only suppress rivers extremely close to the sea.")]
        [SerializeField, Range(0f, 0.1f)] private float _minimumInlandBuffer = 0.005f;

        [Tooltip("Set high enough that rivers can cross hilly terrain. The river carve itself flattens the cell.")]
        [SerializeField, Range(0, 32)] private int _maximumRiverElevation = 5;

        [SerializeField, Range(0f, 1.5f)] private float _widthVariation = 0.4f;

        private int _seed;
        private float _primaryOffset;
        private float _secondaryOffset;
        private Vector2 _primaryNoiseOffset;
        private Vector2 _secondaryNoiseOffset;

        public void Initialize(int seed)
        {
            _seed = seed;
            System.Random random = new System.Random(seed ^ 0x36A9F17B);

            _primaryOffset = (float)random.NextDouble() * _riverSpacing;
            _secondaryOffset = (float)random.NextDouble() * _secondarySpacing;

            _primaryNoiseOffset = new Vector2(
                random.Next(-100000, 100000),
                random.Next(-100000, 100000));

            _secondaryNoiseOffset = new Vector2(
                random.Next(-100000, 100000),
                random.Next(-100000, 100000));
        }

        public bool IsRiverCell(
            int worldX,
            int worldY,
            float terrainHeight,
            float landThreshold,
            int elevationLevel)
        {
            // Do not reject low coastal cells here. The caller already
            // prevents carving true Ocean cells, so allowing the full
            // sand/beach height range lets rivers open a mouth directly
            // into the sea instead of leaving a blocking sand strip.
            if (elevationLevel > _maximumRiverElevation)
                return false;

            return IsPrimaryRiver(worldX, worldY) ||
                   (_enableSecondaryRivers &&
                    IsSecondaryRiver(worldX, worldY));
        }

        private bool IsPrimaryRiver(int worldX, int worldY)
        {
            float spacing = Mathf.Max(24f, _riverSpacing);
            int corridor = Mathf.FloorToInt(
                (worldX - _primaryOffset) / spacing);

            if (!IsCorridorActive(corridor, 191, _corridorChance))
                return false;

            float center = corridor * spacing +
                           _primaryOffset +
                           spacing * 0.5f;

            center += SampleSignedNoise(
                worldY,
                corridor,
                _meanderScale,
                _primaryNoiseOffset) *
                _meanderStrength;

            float width = GetVariableWidth(
                worldY,
                corridor,
                _riverHalfWidth,
                431);

            return Mathf.Abs(worldX - center) <= width;
        }

        private bool IsSecondaryRiver(int worldX, int worldY)
        {
            float spacing = Mathf.Max(24f, _secondarySpacing);
            int corridor = Mathf.FloorToInt(
                (worldY - _secondaryOffset) / spacing);

            if (!IsCorridorActive(
                    corridor,
                    733,
                    _secondaryCorridorChance))
            {
                return false;
            }

            float center = corridor * spacing +
                           _secondaryOffset +
                           spacing * 0.5f;

            center += SampleSignedNoise(
                worldX,
                corridor,
                _meanderScale * 1.2f,
                _secondaryNoiseOffset) *
                (_meanderStrength * 0.7f);

            float width = GetVariableWidth(
                worldX,
                corridor,
                _secondaryHalfWidth,
                947);

            return Mathf.Abs(worldY - center) <= width;
        }

        private float GetVariableWidth(
            int alongAxis,
            int corridor,
            float baseWidth,
            int salt)
        {
            float noise = Mathf.PerlinNoise(
                (alongAxis + _seed * 0.013f + salt) /
                    Mathf.Max(8f, _meanderScale * 0.8f),
                corridor * 0.173f + salt * 0.01f);

            return Mathf.Max(
                0.5f,
                baseWidth +
                (noise - 0.5f) * 2f * _widthVariation);
        }

        private float SampleSignedNoise(
            int alongAxis,
            int corridor,
            float scale,
            Vector2 offset)
        {
            float noise = Mathf.PerlinNoise(
                (alongAxis + offset.x) / Mathf.Max(8f, scale),
                corridor * 0.117f + offset.y * 0.0001f);

            return (noise - 0.5f) * 2f;
        }

        private bool IsCorridorActive(
            int corridor,
            int salt,
            float chance)
        {
            unchecked
            {
                int hash = _seed;
                hash = hash * 397 ^ corridor;
                hash = hash * 397 ^ salt;
                hash ^= hash >> 16;

                uint value = (uint)hash;
                float roll =
                    (value & 0x00FFFFFF) / 16777215f;

                return roll <= chance;
            }
        }

        private void OnValidate()
        {
            _riverSpacing = Mathf.Max(24f, _riverSpacing);
            _secondarySpacing = Mathf.Max(24f, _secondarySpacing);
            _meanderScale = Mathf.Max(8f, _meanderScale);
            _corridorChance = Mathf.Clamp01(_corridorChance);
            _secondaryCorridorChance =
                Mathf.Clamp01(_secondaryCorridorChance);
            _maximumRiverElevation =
                Mathf.Clamp(_maximumRiverElevation, 0, 32);
        }
    }
}
