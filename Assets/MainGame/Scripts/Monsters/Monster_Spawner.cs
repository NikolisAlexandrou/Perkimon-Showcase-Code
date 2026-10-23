using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Tilemaps;
using System.Runtime.CompilerServices;

/// <summary>
/// Used for spawning monsters in the overworld (Spawner)
/// </summary>
[System.Serializable]
public class OverworldMonster
{
    [Header("Monster Data")]
    [SerializeField] private GameObject _monster;
    [SerializeField] private int _level;

    [Tooltip("If true, this monster may only spawn on water.")]
    [SerializeField] private bool _isWaterSpawn;

    public GameObject Monster => _monster;
    public int Level => _level;
    public bool IsWaterSpawn => _isWaterSpawn;
}

public class Monster_Spawner : MonoBehaviour
{
    [Header("Player Distance Settings")]
    [Tooltip("When player is this value distance away from spawner, hide all monsters.")]
    [SerializeField] private float _playerFarAway = 20f;

    [Header("Spawn Area Settings")]
    [Tooltip("Monsters will spawn in X-direction from the spawner object to this value.")]
    [SerializeField] private float _spawnRangeX = 5f;

    [Tooltip("Monsters will spawn in Y-direction from the spawner object to this value.")]
    [SerializeField] private float _spawnRangeY = 5f;

    [Header("Environment Layers")]
    [Tooltip("Layer used for obstacles where monsters cannot spawn.")]
    [SerializeField] private LayerMask _obstacleLayer;

    [Tooltip("Layer used for water tiles.")]
    [SerializeField] private LayerMask _waterLayer;

    [Header("Monster Spawn Lists")]
    [Tooltip("List of the possible common monsters to spawn from this spawner.")]
    [SerializeField] private List<OverworldMonster> _commonMonsters = new List<OverworldMonster>();

    [Tooltip("Chance (weight) of spawning a common monster.")]
    [SerializeField] private float _commonChance = 65f;

    [Tooltip("List of the possible uncommon monsters to spawn from this spawner.")]
    [SerializeField] private List<OverworldMonster> _uncommonMonsters = new List<OverworldMonster>();

    [Tooltip("Chance (weight) of spawning an uncommon monster.")]
    [SerializeField] private float _uncommonChance = 30f;

    [Tooltip("List of the possible rare monsters to spawn from this spawner.")]
    [SerializeField] private List<OverworldMonster> _rareMonsters = new List<OverworldMonster>();

    [Tooltip("Chance (weight) of spawning a rare monster.")]
    [SerializeField] private float _rareChance = 5f;

    [Tooltip("List of the possible night-only monsters to spawn from this spawner.")]
    [SerializeField] private List<OverworldMonster> _nightMonsters = new List<OverworldMonster>();

    [Tooltip("Additional chance (weight) of spawning a night monster, added only at night.")]
    [SerializeField] private float _nightChance = 15f;

    [Header("Extra Details")]
    [Tooltip("Maximum number of monster spawns the spawner can have at once.")]
    [SerializeField] private int _maxSpawns = 5;

    [Tooltip("Time in seconds between each spawn attempt.")]
    [SerializeField] private float _spawnCooldown = 1f;

    [Header("Editor Settings")]

    [Tooltip("Enable if you want to see the despawn range that when player leaves that range, monsters despawn")]
    [SerializeField] private bool _showDespawnRange;

    [Tooltip("Color of the circle that shows the despawn range")]
    [SerializeField] private Color _despawnRangeColor = Color.blueViolet;

    [Tooltip("Enable if you want to see the area tiles the monsters can spawn")]
    [SerializeField] private bool _showSpawnArea;

    [Tooltip("Color of squares showing the area allowed for monsters to spawn")]
    [SerializeField] private Color _spawnAreaAllowedColor = Color.green;

    [Tooltip("Color of squares showing the area NOT allowed for monsters to spawn")]
    [SerializeField] private Color _spawnAreaNotAllowedColor = Color.red;

    [Tooltip("Color of squares showing water tiles (water-only spawns).")]
    [SerializeField] private Color _spawnAreaWaterColor = Color.cyan;

    [Tooltip("Scale of a singular box highlighted in the area")]
    [SerializeField] private float _cellSize = 0.75f;

    [Tooltip("All tilemaps that count as valid ground for spawning.")]
    [SerializeField] private Tilemap[] _groundTilemaps;

    [Tooltip("All tilemaps that count as obstacles (no spawn).")]
    [SerializeField] private Tilemap[] _obstacleTilemaps;

    [Tooltip("All tilemaps that count as water tiles.")]
    [SerializeField] private Tilemap[] _waterTilemaps;


    /// <summary>Holds the currently spawned monster game objects.</summary>
    private List<GameObject> _currentMonsters = new List<GameObject>();

    /// <summary>If true, the spawner will not spawn new monsters.</summary>
    private bool _isSpawnInterrupted;

    /// <summary>Timer for spawn cooldown.</summary>
    private float _spawnTimer;

    /// <summary>True if the player is currently considered far away.</summary>
    private bool _isPlayerFarAway;

    /// <summary>Has the spawner already reset for the current day/night state.</summary>
    private bool _hasResetForCurrentPhase;

    /// <summary>True while a reset coroutine is active.</summary>
    private bool _isCurrentlyResetting;

    /// <summary>Used to avoid repeatedly triggering an "empty" reset.</summary>
    private bool _hasTriggeredEmptyReset;

    // Cached squared distance threshold (so we avoid sqrt each frame)
    private float _playerFarAwaySqr;

    // Cached references
    Transform playerTransform;
    DaynightCycleScript dayNight;


    private void Start()
    {
        playerTransform = GameUtils.i.player.transform;
        dayNight = DaynightCycleScript.i;
        _playerFarAwaySqr = _playerFarAway * _playerFarAway;
    }

    private void Update()
    {
        SpawnMonster();

        if (!_isCurrentlyResetting)
        {
            DespawnAllWhenFarAway();
            RespawnWhenEmpty();
        }

        HandleDayNightReset();
    }

    #region Handle monster spawning

    /// <summary>
    /// Hides or shows all monsters based on the player's distance from the spawner.
    /// Monsters are disabled when the player is far, and re-enabled when near.
    /// </summary>
    private void DespawnAllWhenFarAway()
    {
        Vector2 toPlayer = (Vector2)(transform.position - playerTransform.position);
        bool isFarNow = toPlayer.sqrMagnitude >= _playerFarAwaySqr;

        // No change in state – do nothing.
        if (isFarNow == _isPlayerFarAway)
            return;

        _isPlayerFarAway = isFarNow;

        bool shouldBeActive = !_isPlayerFarAway;
        for (int i = 0; i < _currentMonsters.Count; i++)
        {
            if (_currentMonsters[i] != null)
                _currentMonsters[i].SetActive(shouldBeActive);
        }
    }

    /// <summary>
    /// Spawns monsters over time up to the maximum count, respecting:
    /// - Player distance (no spawn if player is far)
    /// - Spawn cooldown
    /// - Obstacles and water layers
    /// </summary>
    private void SpawnMonster()
    {
        if (_isPlayerFarAway)
            return;

        if (AliveCount() >= _maxSpawns)
            return;

        if (_isSpawnInterrupted)
            return;

        // Cooldown timer
        if (_spawnTimer > 0f)
        {
            _spawnTimer -= Time.deltaTime;
            return;
        }

        // Find a random position around the spawner
        float x = Random.Range(transform.position.x - _spawnRangeX, transform.position.x + _spawnRangeX);
        float y = Random.Range(transform.position.y - _spawnRangeY, transform.position.y + _spawnRangeY);
        Vector2 spawnPos = new Vector2(x, y);

        // Check for obstacles (use OverlapCircle to avoid array allocations)
        Collider2D obstacleHit = Physics2D.OverlapCircle(spawnPos, 1f, _obstacleLayer);
        if (obstacleHit != null)
        {
            // Position blocked, try again after cooldown
            _spawnTimer = _spawnCooldown;
            return;
        }

        OverworldMonster monsterData = GetMonsterToSpawn();
        if (monsterData == null || monsterData.Monster == null)
        {
            DebugLogs.Warning($"Spawner {name}: GetMonsterToSpawn() returned null or had no prefab assigned.");
            _spawnTimer = _spawnCooldown;
            return;
        }

        // Check water/non-water conditions
        bool isSpawnPosWater = Physics2D.OverlapCircle(spawnPos, 1f, _waterLayer) != null;

        // If monster must be in water but this position isn't water -> skip
        if (monsterData.IsWaterSpawn && !isSpawnPosWater)
        {
            _spawnTimer = _spawnCooldown;
            return;
        }

        // If monster must be on land but this position is water -> skip
        if (!monsterData.IsWaterSpawn && isSpawnPosWater)
        {
            _spawnTimer = _spawnCooldown;
            return;
        }

        // Finally spawn the monster
        GameObject monsterInstance = Instantiate(monsterData.Monster, spawnPos, Quaternion.identity);

        var behaviour = monsterInstance.GetComponent<Overworld_MonsterBehaviour>();
        if (behaviour != null && behaviour.GetMonster != null)
            behaviour.GetMonster.SetLevel(monsterData.Level);
        else
            DebugLogs.Warning($"Spawner {name}: Spawned monster {monsterInstance.name} is missing Overworld_MonsterBehaviour or its monster data.");

        _currentMonsters.Add(monsterInstance);
        DebugLogs.Log($"Spawned: {monsterData.Monster.GetComponent<Overworld_MonsterBehaviour>().GetMonster.Base.Name}  total: {_currentMonsters.Count}");

        // Reset timer after attempting a spawn
        _spawnTimer = _spawnCooldown;
    }

    /// <summary>
    /// Resets (despawns) all monsters with a fade-out effect.
    /// </summary>
    private IEnumerator ResetSpawns()
    {
        _isCurrentlyResetting = true;

        // Small delay before starting fade/destruction
        yield return new WaitForSeconds(1f);

        foreach (GameObject item in _currentMonsters)
        {
            if (item != null)
            {
                var sr = item.GetComponent<SpriteRenderer>();
                if (sr != null)
                    sr.DOFade(0f, 0.2f);

                Destroy(item, 0.3f);
            }
        }

        _currentMonsters.Clear();

        // Delay before allowing spawner to consider itself non-empty again
        yield return new WaitForSeconds(0.5f);

        _hasTriggeredEmptyReset = false;
        _isCurrentlyResetting = false;
    }

    /// <summary>
    /// If all monsters are gone, triggers a reset once.
    /// </summary>
    private void RespawnWhenEmpty()
    {
        if (_hasTriggeredEmptyReset)
            return;

        if (IsSpawnerEmpty())
        {
            _hasTriggeredEmptyReset = true;
            StartCoroutine(ResetSpawns());
        }
    }

    /// <summary>
    /// Handles resetting spawns when transitioning between day and night.
    /// </summary>
    private void HandleDayNightReset()
    {
        bool isNightNow = dayNight.IsNight;

        if (!_hasResetForCurrentPhase)
        {
            // First time we detect this phase (day or night), trigger reset.
            _hasResetForCurrentPhase = true;
            StartCoroutine(ResetSpawns());
        }

        // When phase changes, allow another reset
        // (we detect this by noticing that our stored flag doesn't match current night/day)
        if (isNightNow != dayNight.IsNight)
        {
            _hasResetForCurrentPhase = false;
        }
    }



    /// <summary>
    /// Picks which monster data to spawn based on weighted chances and day/night state.
    /// - commonChance, uncommonChance, rareChance are always used.
    /// - nightChance is only added to the pool at night.
    /// </summary>
    public OverworldMonster GetMonsterToSpawn()
    {
        bool isNight = dayNight.IsNight;

        float totalChance = _commonChance + _uncommonChance + _rareChance;
        if (isNight)
            totalChance += _nightChance;

        // Guard against having no monsters configured
        if (_commonMonsters.Count == 0 &&
            _uncommonMonsters.Count == 0 &&
            _rareMonsters.Count == 0 &&
            (!isNight || _nightMonsters.Count == 0))
        {
            DebugLogs.Warning($"Spawner {name}: No monsters configured.");
            return null;
        }

        float random = Random.Range(0f, totalChance);

        // Common
        if (random < _commonChance && _commonMonsters.Count > 0)
            return _commonMonsters[Random.Range(0, _commonMonsters.Count)];

        random -= _commonChance;

        // Uncommon
        if (random < _uncommonChance && _uncommonMonsters.Count > 0)
            return _uncommonMonsters[Random.Range(0, _uncommonMonsters.Count)];

        random -= _uncommonChance;

        // Rare
        if (random < _rareChance && _rareMonsters.Count > 0)
            return _rareMonsters[Random.Range(0, _rareMonsters.Count)];

        // Night-only (only if night and list not empty)
        if (isNight && _nightMonsters.Count > 0)
            return _nightMonsters[Random.Range(0, _nightMonsters.Count)];

        // Fallbacks – if something about the weights didn't match the available lists.
        if (_commonMonsters.Count > 0)
            return _commonMonsters[Random.Range(0, _commonMonsters.Count)];

        if (_uncommonMonsters.Count > 0)
            return _uncommonMonsters[Random.Range(0, _uncommonMonsters.Count)];

        if (_rareMonsters.Count > 0)
            return _rareMonsters[Random.Range(0, _rareMonsters.Count)];

        if (isNight && _nightMonsters.Count > 0)
            return _nightMonsters[Random.Range(0, _nightMonsters.Count)];

        return null;
    }

    /// <summary>
    /// Counts the number of currently active (non-null, activeInHierarchy) monsters.
    /// </summary>
    private int AliveCount()
    {
        int count = 0;
        for (int i = 0; i < _currentMonsters.Count; i++)
        {
            if (_currentMonsters[i] != null && _currentMonsters[i].activeInHierarchy)
                count++;
        }
        return count;
    }

    /// <summary>
    /// Returns true if there are no active monsters left.
    /// </summary>
    private bool IsSpawnerEmpty()
    {
        for (int i = 0; i < _currentMonsters.Count; i++)
        {
            if (_currentMonsters[i] != null && _currentMonsters[i].activeInHierarchy)
                return false;
        }
        return true;
    }

    #endregion

    #region tilemap settings
    private Tilemap GetFirstValidTilemap(Tilemap[] maps)
    {
        if (maps == null) return null;
        for (int i = 0; i < maps.Length; i++)
        {
            if (maps[i] != null)
                return maps[i];
        }
        return null;
    }

    private bool AnyHasTile(Tilemap[] maps, Vector3Int cell)
    {
        if (maps == null) return false;

        for (int i = 0; i < maps.Length; i++)
        {
            if (maps[i] != null && maps[i].HasTile(cell))
                return true;
        }
        return false;
    }

    #endregion

    #region editor settings

    private void OnDrawGizmos()
    {
        DrawSpawnArea();
        DrawDespawnRange();
    }

    //  Draw a wire sphere of the area that monsters will be shown if player is in
    //  If player is out of this area monsters despawn for better performance
    private void DrawDespawnRange()
    {
        if (_showDespawnRange == false)
            return;

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, _playerFarAway);
    }


    //  Draw the area of spawner that can actually spawn the monsters
    //  -   Green boxes for allowed tile to spawn
    //  -   Red boxes for not allowed tile to spawn
    //  -   Blue boxes for tile to spawn ONLY water monsters
    private void DrawSpawnArea()
    {
        if (_showSpawnArea == false)
            return;

        // Check what we actually have
        bool hasGround = _groundTilemaps != null && _groundTilemaps.Length > 0;
        bool hasObstacle = _obstacleTilemaps != null && _obstacleTilemaps.Length > 0;
        bool hasWater = _waterTilemaps != null && _waterTilemaps.Length > 0;

        // If we have no tilemaps set at all, just draw the bounding box like before
        if (!hasGround && !hasObstacle && !hasWater)
        {
            Gizmos.color = _spawnAreaAllowedColor;
            Vector3 center = transform.position;
            Vector3 size = new Vector3(_spawnRangeX * 2f, _spawnRangeY * 2f, 0f);
            Gizmos.DrawWireCube(center, size);
            return;
        }

        // Choose a reference tilemap to do world<->cell conversion
        Tilemap refMap = GetFirstValidTilemap(_groundTilemaps);
        if (refMap == null)
            refMap = GetFirstValidTilemap(_obstacleTilemaps);
        if (refMap == null)
            refMap = GetFirstValidTilemap(_waterTilemaps);

        if (refMap == null)
        {
            // Nothing valid, fallback
            Gizmos.color = _spawnAreaAllowedColor;
            Vector3 center = transform.position;
            Vector3 size = new Vector3(_spawnRangeX * 2f, _spawnRangeY * 2f, 0f);
            Gizmos.DrawWireCube(center, size);
            return;
        }

        // World-space bounds of the spawn area
        Vector2 minWorld = new Vector2(
            transform.position.x - _spawnRangeX,
            transform.position.y - _spawnRangeY
        );
        Vector2 maxWorld = new Vector2(
            transform.position.x + _spawnRangeX,
            transform.position.y + _spawnRangeY
        );

        // Convert bounds to cell coordinates using the reference tilemap
        Vector3Int minCell = refMap.WorldToCell(minWorld);
        Vector3Int maxCell = refMap.WorldToCell(maxWorld);

        Vector3 tileSize = new Vector3(_cellSize, _cellSize, 0.01f);

        // Loop through all cells in the spawn area
        for (int x = minCell.x; x <= maxCell.x; x++)
        {
            for (int y = minCell.y; y <= maxCell.y; y++)
            {
                Vector3Int cell = new Vector3Int(x, y, 0);

                bool isGround = AnyHasTile(_groundTilemaps, cell);
                bool isObstacleTile = AnyHasTile(_obstacleTilemaps, cell);
                bool isWaterTile = AnyHasTile(_waterTilemaps, cell);

                // If nothing is here in any group, skip
                if (!isGround && !isObstacleTile && !isWaterTile)
                    continue;

                // Decide color
                // Priority: obstacle > water > ground
                if (isObstacleTile)
                    Gizmos.color = _spawnAreaNotAllowedColor;
                else if (isWaterTile)
                    Gizmos.color = _spawnAreaWaterColor;
                else // ground
                    Gizmos.color = _spawnAreaAllowedColor;

                // Draw tile
                Vector3 center = refMap.GetCellCenterWorld(cell);
                Gizmos.DrawCube(center, tileSize);

                // Optional: outline
                Gizmos.color = Color.black;
                Gizmos.DrawWireCube(center, tileSize);
            }
        }
    }
    #endregion
}
