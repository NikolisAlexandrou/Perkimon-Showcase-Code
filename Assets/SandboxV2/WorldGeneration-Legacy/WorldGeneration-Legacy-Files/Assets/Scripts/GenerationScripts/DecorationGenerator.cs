using UnityEngine;
using UnityEngine.Tilemaps;

namespace island
{
    public class DecorationGenerator : MonoBehaviour
    {
        [Header("Tall Grass")] [SerializeField]
        private TileBase _tallGrassTile;

        [SerializeField, Range(0f, 1f)] private float _tallGrassChance = 0.3f;
        [SerializeField, Range(0f, 1f)] private float _tallGrassThreshold = 0.6f;

        public float TallGrassChance
        {
            get => _tallGrassChance;
            set => _tallGrassChance = Mathf.Clamp01(value);
        }

        public TileBase TallGrassTile
        {
            get => _tallGrassTile;
            set => _tallGrassTile = value;
        }

        public float TallGrassThreshold
        {
            get => _tallGrassThreshold;
            set => _tallGrassThreshold = Mathf.Clamp01(value);
        }

        public TileBase GetDecorationTile(
            float detailNoise,
            float deterministicRoll,
            float chanceMultiplier = 1f,
            TileBase tileOverride = null)
        {
            TileBase selectedTile = tileOverride != null
                ? tileOverride
                : _tallGrassTile;

            if (selectedTile == null)
                return null;

            float chance = Mathf.Clamp01(
                _tallGrassChance * Mathf.Max(0f, chanceMultiplier));

            return detailNoise > _tallGrassThreshold &&
                   deterministicRoll < chance
                ? selectedTile
                : null;
        }

        // Kept for compatibility with the old project code.
        public void PlaceDecorationTiles(TileBase[] tileMap, float detailNoise, int tileIndex)
        {
            if (tileMap == null || tileIndex < 0 || tileIndex >= tileMap.Length)
                return;

            if (detailNoise > _tallGrassThreshold && Random.value < _tallGrassChance)
                tileMap[tileIndex] = _tallGrassTile;
        }
    }
}
