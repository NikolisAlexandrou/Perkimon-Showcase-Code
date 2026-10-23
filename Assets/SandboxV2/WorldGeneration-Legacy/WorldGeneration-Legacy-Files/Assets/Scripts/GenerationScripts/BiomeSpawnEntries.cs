using System;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace island
{
    [Serializable]
    public sealed class BiomeDecorationEntry
    {
        [SerializeField] private TileBase _tile;
        [SerializeField, Min(0f)] private float _weight = 1f;
        [SerializeField, Range(0f, 1f)] private float _minimumNoise = 0f;
        [SerializeField, Range(0f, 1f)] private float _maximumNoise = 1f;

        public TileBase Tile => _tile;
        public float Weight => Mathf.Max(0f, _weight);
        public float MinimumNoise => Mathf.Clamp01(_minimumNoise);
        public float MaximumNoise => Mathf.Clamp01(_maximumNoise);
        public bool AllowsNoise(float noise) =>
            noise >= MinimumNoise && noise <= MaximumNoise;
    }

    [Serializable]
    public sealed class BiomePropEntry
    {
        [SerializeField] private GameObject _prefab;

        [Tooltip("Stable number used by deterministic placement. Give each prop entry a different value.")]
        [SerializeField] private int _deterministicSalt = 1001;

        [SerializeField, Min(0f)] private float _weight = 1f;

        [Tooltip("Minimum grid spacing used for this prop type.")]
        [SerializeField, Min(1)] private int _minimumSpacing = 6;

        [Tooltip("Lowest elevation where this prop may appear.")]
        [SerializeField, Min(0)] private int _minimumElevation;

        [Tooltip("Highest elevation where this prop may appear. Set to 32 for any normal level.")]
        [SerializeField, Range(0, 32)] private int _maximumElevation = 32;

        [SerializeField] private Vector3 _positionOffset;
        [SerializeField] private bool _randomFlipX = true;
        
        [Tooltip("Category used when saving this procedural world entity.")]
        public WorldEntityCategory worldEntityCategory = WorldEntityCategory.Prop;

        public GameObject Prefab => _prefab;
        public int StableSalt => _deterministicSalt;
        public float Weight => Mathf.Max(0f, _weight);
        public int MinimumSpacing => Mathf.Max(1, _minimumSpacing);
        public int MinimumElevation => Mathf.Max(0, _minimumElevation);
        public int MaximumElevation => Mathf.Max(MinimumElevation, _maximumElevation);
        public Vector3 PositionOffset => _positionOffset;
        public bool RandomFlipX => _randomFlipX;

        public bool AllowsElevation(int elevation) =>
            elevation >= MinimumElevation && elevation <= MaximumElevation;
    }

[System.Serializable]
public sealed class BiomeTreeEntry
{
    [Tooltip("Tree prefab used by this biome.")]
    public GameObject prefab;

    [Tooltip("Relative selection weight compared with other tree entries.")]
    [Min(0f)] public float weight = 1f;

    [Tooltip("Optional local position offset applied after spawning.")]
    public Vector3 positionOffset;

    [Tooltip("Randomly mirror the tree horizontally for visual variation.")]
    public bool randomFlipX;

    [Tooltip("Stable salt used for deterministic prefab selection/variation.")]
    public int deterministicSalt = 1701;
}

}
