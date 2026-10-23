using UnityEngine;
using UnityEngine.Tilemaps;

namespace island
{
    /// <summary>
    /// Animated Tile whose phase is always based on the Tilemap's global
    /// animation clock instead of a random/per-instance start offset.
    ///
    /// Use this asset for water that is streamed in chunk-by-chunk. Tiles
    /// placed later will join the same animation phase as tiles that were
    /// already loaded.
    /// </summary>
    [CreateAssetMenu(
        fileName = "SynchronizedAnimatedTile",
        menuName = "Project Atlas/World/Synchronized Animated Tile")]
    public sealed class SynchronizedAnimatedTile : TileBase
    {
        [SerializeField] private Sprite[] _frames;
        [SerializeField, Min(0.01f)] private float _animationSpeed = 1f;
        [SerializeField] private Tile.ColliderType _colliderType =
            Tile.ColliderType.None;

        public override void GetTileData(
            Vector3Int position,
            ITilemap tilemap,
            ref TileData tileData)
        {
            tileData.sprite =
                _frames != null && _frames.Length > 0
                    ? _frames[0]
                    : null;

            tileData.color = Color.white;
            tileData.transform = Matrix4x4.identity;
            tileData.flags = TileFlags.LockAll;
            tileData.colliderType = _colliderType;
        }

        public override bool GetTileAnimationData(
            Vector3Int position,
            ITilemap tilemap,
            ref TileAnimationData tileAnimationData)
        {
            if (_frames == null || _frames.Length == 0)
                return false;

            tileAnimationData.animatedSprites = _frames;
            tileAnimationData.animationSpeed =
                Mathf.Max(0.01f, _animationSpeed);

            // Zero means every cell uses the same Tilemap animation timeline.
            // There is no per-cell or per-chunk randomized phase.
            tileAnimationData.animationStartTime = 0f;

            return true;
        }

        private void OnValidate()
        {
            _animationSpeed = Mathf.Max(0.01f, _animationSpeed);
        }
    }
}
