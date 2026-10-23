using UnityEngine;
using UnityEngine.Tilemaps;

namespace island
{
    /// <summary>
    /// Tracks the logical elevation under the player. No transform Z movement is required.
    /// Other systems can read CurrentElevation for AI, rendering, or battle checks.
    /// </summary>
    public sealed class PlayerElevationTracker : MonoBehaviour
    {
        [SerializeField] private TerrainGenerator _terrainGenerator;
        [SerializeField] private Tilemap _referenceTilemap;
        [SerializeField] private bool _logChanges;

        public int CurrentElevation { get; private set; }
        public TerrainCellData CurrentCell { get; private set; }

        private Vector3Int _lastCell = new Vector3Int(int.MinValue, int.MinValue, 0);

        private void Awake()
        {
            if (_terrainGenerator == null)
                _terrainGenerator = FindFirstObjectByType<TerrainGenerator>();
        }

        private void Update()
        {
            if (_terrainGenerator == null || _referenceTilemap == null)
                return;

            Vector3Int cell = _referenceTilemap.WorldToCell(transform.position);
            if (cell == _lastCell)
                return;

            _lastCell = cell;
            CurrentCell = _terrainGenerator.GetCellData(cell.x, cell.y);

            if (CurrentCell.ElevationLevel == CurrentElevation)
                return;

            CurrentElevation = CurrentCell.ElevationLevel;

            if (_logChanges)
                Debug.Log($"Player elevation: {CurrentElevation}", this);
        }
    }
}
