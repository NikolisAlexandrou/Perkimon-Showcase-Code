using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace island
{
    public class TreeGenerator : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private WorldModificationStore _worldModificationStore;
        
        [Header("Fallback Tree")]
        [Tooltip("Used only when a biome has no tree prefab entries configured.")]
        [SerializeField] private GameObject _treePrefab;

        [Header("Tree Placement")]
        [SerializeField, Range(0f, 1f)] private float _treeChance = 0.1f;
        [SerializeField, Min(1)] private int _minTreeDistance = 3;
        [SerializeField, Range(0f, 1f)] private float _treeThreshold = 0.65f;

        [Header("Pooling")]
        [SerializeField, Min(0)] private int _prewarmPerPrefab = 8;
        [SerializeField] private Transform _activeTreeParent;
        [SerializeField] private Transform _pooledTreeParent;

        private readonly Dictionary<ChunkCoordinate, List<GameObject>> _treesByChunk = new();

        private readonly Dictionary<GameObject, Queue<GameObject>> _poolsByPrefab = new();

        private readonly Dictionary<GameObject, GameObject> _instancePrefabLookup = new();

        public float TreeChance { get => _treeChance; set => _treeChance = Mathf.Clamp01(value); }

        public int TreeDensity { get => _minTreeDistance; set => _minTreeDistance = Mathf.Max(1, value); }

        private void Awake()
        {
            EnsurePoolParents();

            if (_treePrefab != null)
                PrewarmPrefab(_treePrefab);
            
            if (_worldModificationStore == null)
            {
                _worldModificationStore =
                    FindFirstObjectByType<WorldModificationStore>();
            }
        }

        public bool ShouldPlaceTree(Vector2Int worldCell, float treeNoise, float deterministicRoll, int seed, float chanceMultiplier = 1f)
        {
            if (treeNoise <= _treeThreshold || deterministicRoll >= Mathf.Clamp01(_treeChance * Mathf.Max(0f, chanceMultiplier)))
            {
                return false;
            }

            int spacing = Mathf.Max(1, _minTreeDistance);
            int offsetX = PositiveMod(Hash(seed, 9137), spacing);
            int offsetY = PositiveMod(Hash(seed, 3571), spacing);

            return PositiveMod(worldCell.x - offsetX, spacing) == 0 && PositiveMod(worldCell.y - offsetY, spacing) == 0;
        }
        
        public void SpawnTree(Vector2Int worldCell, Tilemap referenceTilemap, ChunkCoordinate chunk, BiomeDefinition biome, int seed)
        {
            if (referenceTilemap == null)
                return;

            BiomeTreeEntry entry = SelectTreeEntry(biome, worldCell.x, worldCell.y, seed);

            GameObject prefab = entry != null && entry.prefab != null ? entry.prefab : _treePrefab;
            
            int deterministicSalt = entry != null ? entry.deterministicSalt : 0;

            string persistentId = WorldEntityId.Create(seed, chunk, worldCell, WorldEntityCategory.Tree, deterministicSalt);
            
            if (prefab == null)
                return;

            Vector3Int cell = new Vector3Int(worldCell.x, worldCell.y, 0);

            Vector3 worldPosition = referenceTilemap.GetCellCenterWorld(cell);

            if (entry != null)
                worldPosition += entry.positionOffset;
            
            if (!ResolveModificationStore())
            {
                Debug.LogError(
                    "TreeGenerator cannot create persistent trees " +
                    "because WorldModificationStore was not found.",
                    this);

                return;
            }
            
            if (_worldModificationStore != null && _worldModificationStore.IsDestroyed(chunk, persistentId))
                return;

            GameObject tree = GetTreeFromPool(prefab);

            tree.transform.SetParent(_activeTreeParent, false);

            tree.transform.SetPositionAndRotation(worldPosition, Quaternion.identity);

            Vector3 scale = tree.transform.localScale;
            scale.x = Mathf.Abs(scale.x);

            if (entry != null && entry.randomFlipX && Deterministic01(worldCell.x, worldCell.y, seed, entry.deterministicSalt + 991) < 0.5f)
            {
                scale.x = -scale.x;
            }

            tree.transform.localScale = scale;
            
            WorldEntity worldEntity = tree.GetComponent<WorldEntity>();
            if(worldEntity == null)
                worldEntity = tree.AddComponent<WorldEntity>();
            
            if (_worldModificationStore == null)
            {
                Debug.LogError(
                    $"Cannot initialize {tree.name}: " +
                    "WorldModificationStore is null.",
                    tree);

                ReleaseTree(tree);
                return;
            }
            
            worldEntity.Initialize(persistentId, chunk, worldCell, _worldModificationStore, () => DespawnTree(tree, chunk));
            
            tree.SetActive(true);

            if (!_treesByChunk.TryGetValue(chunk, out List<GameObject> trees))
            {
                trees = new List<GameObject>(8);
                _treesByChunk.Add(chunk, trees);
            }

            trees.Add(tree);
        }

        // Compatibility overload for older callers.
        public void SpawnTree(Vector2Int worldCell, Tilemap referenceTilemap, ChunkCoordinate chunk)
        {
            SpawnTree(worldCell, referenceTilemap, chunk, null, 0);
        }
        
        private void DespawnTree(GameObject tree, ChunkCoordinate chunk)
        {
            if (tree == null)
                return;

            if (_treesByChunk.TryGetValue(chunk, out List<GameObject> trees))
            {
                trees.Remove(tree);
            }

            ReleaseTree(tree);
        }
        
        private bool ResolveModificationStore()
        {
            if (_worldModificationStore != null)
                return true;

            _worldModificationStore =
                FindFirstObjectByType<WorldModificationStore>();

            return _worldModificationStore != null;
        }

        public void ClearChunk(ChunkCoordinate chunk)
        {
            if (!_treesByChunk.TryGetValue(chunk, out List<GameObject> trees))
            {
                return;
            }

            foreach (GameObject tree in trees) ReleaseTree(tree);

            trees.Clear();
            _treesByChunk.Remove(chunk);
        }

        public void ClearAllTrees()
        {
            foreach (List<GameObject> trees in _treesByChunk.Values)
            {
                foreach (GameObject tree in trees)
                    ReleaseTree(tree);

                trees.Clear();
            }

            _treesByChunk.Clear();
        }

        public void clearTrees(Tilemap tilemap)
        {
            ClearAllTrees();
        }

        private BiomeTreeEntry SelectTreeEntry(BiomeDefinition biome, int worldX, int worldY, int seed)
        {
            BiomeTreeEntry[] entries = biome != null ? biome.TreeEntries : null;

            if (entries == null || entries.Length == 0)
                return null;

            float totalWeight = 0f;

            foreach (BiomeTreeEntry entry in entries)
            {
                if (entry == null || entry.prefab == null || entry.weight <= 0f)
                {
                    continue;
                }

                totalWeight += entry.weight;
            }

            if (totalWeight <= 0f)
                return null;

            float roll = Deterministic01(worldX, worldY, seed, 4703) * totalWeight;

            float cumulative = 0f;

            foreach (BiomeTreeEntry entry in entries)
            {
                if (entry == null || entry.prefab == null || entry.weight <= 0f)
                {
                    continue;
                }

                cumulative += entry.weight;

                if (roll <= cumulative)
                    return entry;
            }

            return null;
        }

        private GameObject GetTreeFromPool(GameObject prefab)
        {
            if (!_poolsByPrefab.TryGetValue(prefab, out Queue<GameObject> pool))
            {
                pool = new Queue<GameObject>();
                _poolsByPrefab.Add(prefab, pool);
                PrewarmPrefab(prefab);
            }

            while (pool.Count > 0)
            {
                GameObject tree = pool.Dequeue();

                if (tree != null)
                    return tree;
            }

            return CreatePooledTree(prefab);
        }

        private void ReleaseTree(GameObject tree)
        {
            if (tree == null)
                return;

            tree.SetActive(false);
            tree.transform.SetParent(_pooledTreeParent, false);

            if (!_instancePrefabLookup.TryGetValue(tree, out GameObject prefab) || prefab == null)
            {
                Destroy(tree);
                return;
            }

            if (!_poolsByPrefab.TryGetValue(prefab, out Queue<GameObject> pool))
            {
                pool = new Queue<GameObject>();
                _poolsByPrefab.Add(prefab, pool);
            }

            pool.Enqueue(tree);
        }

        private void PrewarmPrefab(GameObject prefab)
        {
            if (prefab == null)
                return;

            if (!_poolsByPrefab.TryGetValue(prefab, out Queue<GameObject> pool))
            {
                pool = new Queue<GameObject>();
                _poolsByPrefab.Add(prefab, pool);
            }

            while (pool.Count < _prewarmPerPrefab)
            {
                GameObject tree = CreatePooledTree(prefab);

                tree.SetActive(false);
                pool.Enqueue(tree);
            }
        }

        private GameObject CreatePooledTree(GameObject prefab)
        {
            GameObject tree = Instantiate(prefab, Vector3.zero, Quaternion.identity, _pooledTreeParent);

            tree.name = prefab.name;
            _instancePrefabLookup[tree] = prefab;

            return tree;
        }

        private void EnsurePoolParents()
        {
            if (_activeTreeParent == null)
            {
                GameObject activeRoot = new GameObject("Active Trees");

                activeRoot.transform.SetParent(transform, false);

                _activeTreeParent = activeRoot.transform;
            }

            if (_pooledTreeParent == null)
            {
                GameObject poolRoot = new GameObject("Tree Pool");

                poolRoot.transform.SetParent(transform, false);

                _pooledTreeParent = poolRoot.transform;
            }
        }

        private static int Hash(int a, int b)
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + a;
                hash = hash * 31 + b;
                hash ^= hash >> 16;
                return hash;
            }
        }

        private static float Deterministic01(int x, int y, int seed, int salt)
        {
            unchecked
            {
                int hash = seed;
                hash = hash * 397 ^ x;
                hash = hash * 397 ^ y;
                hash = hash * 397 ^ salt;
                hash ^= hash >> 16;

                return ((uint)hash & 0x00FFFFFF) /
                       16777215f;
            }
        }

        private static int PositiveMod(int value, int modulus)
        {
            int result = value % modulus;

            return result < 0 ? result + modulus : result;
        }

        private void OnValidate()
        {
            _treeChance = Mathf.Clamp01(_treeChance);

            _minTreeDistance = Mathf.Max(1, _minTreeDistance);

            _treeThreshold = Mathf.Clamp01(_treeThreshold);

            _prewarmPerPrefab = Mathf.Max(0, _prewarmPerPrefab);
        }
    }
}
