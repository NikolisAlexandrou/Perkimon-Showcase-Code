using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public class FootstepByTileDataMultiScene : MonoBehaviour
{
    [System.Serializable] 
    public class Entry 
    { 
        public TileBase tile; 
        public SurfaceAudioSet audio; 
    }

    [Header("Audio")]

    [Tooltip("Footstep audio master object")]
    [SerializeField] private AudioSource _audioSource;

    [Tooltip("Default sounds playing when player is walking (leave empty for no default sounds)")]
    [SerializeField] private SurfaceAudioSet _defaultFootstepSoundSet;

    [Header("Tile to Audio mapping")]

    [Tooltip("Contains information about on which tiles certain sounds will be played when player walks on them")]
    [SerializeField] private Entry[] entries;

    [Header("Feet sample point")]
    [SerializeField] private Transform _playerFeet;

    [Header("(Optional) Extra Tilemaps (checked first, top to bottom)")]

    [Tooltip("If two tilemaps overlap, the priority ones will be checked first")]
    [SerializeField] private List<Tilemap> _priorityTilemaps = new List<Tilemap>();

    private Dictionary<TileBase, SurfaceAudioSet> _map; //  Contains all entries mapped as a dictionary
    private List<Tilemap> _cachePriorityTilemaps = new List<Tilemap>(); 

    void Awake()
    {
        // Build the per tile mapping once
        _map = new Dictionary<TileBase, SurfaceAudioSet>();
        foreach (var e in entries)
            if (e.tile && e.audio) _map[e.tile] = e.audio;

        RefreshTilemapCache();

        //  Subscribe to unity load and unload event for refreshing tilemaps
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }

    /// <summary>
    /// When scene is loaded, refresh the tilemap
    /// </summary>
    /// <param name="s"></param>
    /// <param name="m"></param>
    void OnSceneLoaded(Scene s, LoadSceneMode m) => RefreshTilemapCache();

    /// <summary>
    /// When scene is unloaded, refresh tilemap
    /// </summary>
    /// <param name="s"></param>
    void OnSceneUnloaded(Scene s) => RefreshTilemapCache();

    /// <summary>
    /// Initialises tilemap cache to contain correct order of tilemaps across tilemaps set by user in inspector
    /// and all other remaining tilemaps in all loaded scenes, with priority given to user set tilemaps > sorting layer > transform
    /// </summary>
    void RefreshTilemapCache()
    {
        _cachePriorityTilemaps.Clear();

        //  1. Start with any explicit priority tilemaps (user-defined order)
        foreach (var tileMap in _priorityTilemaps)
            if (tileMap) _cachePriorityTilemaps.Add(tileMap);

        //  2, Add all other Tilemaps across loaded scenes (include inactive if needed)
#if UNITY_2023_1_OR_NEWER
        var allTilemaps = FindObjectsByType<Tilemap>(FindObjectsInactive.Include, FindObjectsSortMode.None);
#elif UNITY_2020_1_OR_NEWER
        var allTilemaps = FindObjectsOfType<Tilemap>(true);
#else
        var allTilemaps = FindObjectsOfType<Tilemap>();
#endif
        foreach (var tileMap in allTilemaps)
        {
            if (tileMap && !_cachePriorityTilemaps.Contains(tileMap))
                _cachePriorityTilemaps.Add(tileMap);
        }

        //  3. Sort by visual priority (highest first) so overlays/decals win.
        //  If a TilemapRenderer exists, use its sorting order and fallback to transform order
        _cachePriorityTilemaps = _cachePriorityTilemaps
            .OrderByDescending(tm =>
            {
                var r = tm.GetComponent<TilemapRenderer>();
                return r ? r.sortingOrder : 0;
            })
            .ThenByDescending(tm => tm.transform.position.z)
            .ToList();
    }

    /// <summary>
    /// Get the corresponding sound of the tile the player is correndly on and play it
    /// Call this from an animation event or a distance timer.
    /// </summary>
    public void PlayFootstep()
    {
        if (!_audioSource || !_playerFeet) return;

        //  Find the first tile under the feet across all tilemaps
        TileBase tile = GetTopmostTileAt(_playerFeet.position);

        SurfaceAudioSet set = null;

        //  Check if current tile is mapped into a sound and play it
        //  otherwise play the default step sound if it exists
        if (tile && _map.TryGetValue(tile, out set) && set)
        {
            var clip = set.GetRandom();
            if (clip) _audioSource.PlayOneShot(clip);
        }
        else if (_defaultFootstepSoundSet)
        {
            var clip = _defaultFootstepSoundSet.GetRandom();
            if (clip) _audioSource.PlayOneShot(clip);
        }
    }

    /// <summary>
    /// Get the visually topmost tile out of all tilemaps overlapping in current position
    /// </summary>
    /// <param name="worldPos"></param>
    /// <returns></returns>
    TileBase GetTopmostTileAt(Vector3 worldPos)
    {
        //  Loop through all tiles in tilemaps
        for (int i = 0; i < _cachePriorityTilemaps.Count; i++)
        {
            var currentTilemap = _cachePriorityTilemaps[i];

            //  Check if tilemap actually exists and is active
            if (!currentTilemap || !currentTilemap.gameObject.activeInHierarchy) continue;

            //  Get the current tile in position given
            Vector3Int cell = currentTilemap.WorldToCell(worldPos);

            // Quick reject using local bounds if available
            if (!currentTilemap.cellBounds.Contains(cell)) continue;

            var tile = currentTilemap.GetTile(cell);
            if (tile != null)
                return tile;
        }
        return null;
    }
}
