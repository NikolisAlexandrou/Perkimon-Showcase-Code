using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace island
{
    public sealed class WorldPropGenerator : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private WorldModificationStore _worldModificationStore;
        
        [Header("Hierarchy")]
        [SerializeField] private Transform _activeParent;
        [SerializeField] private Transform _poolParent;

        private readonly Dictionary<GameObject, Queue<GameObject>> _pools = new();
        private readonly Dictionary<GameObject, GameObject> _instanceToPrefab = new();
        private readonly Dictionary<ChunkCoordinate, List<GameObject>> _propsByChunk = new();

        private void Awake()
        {
            EnsureParents();
            
            if (_worldModificationStore == null)
            {
                _worldModificationStore = FindFirstObjectByType< WorldModificationStore>();
            }
        }

        public bool PassesSpacing(Vector2Int worldCell, BiomePropEntry entry, int seed, int salt)
        {
            if (entry == null || entry.Prefab == null)
                return false;

            int spacing = Mathf.Max(1, entry.MinimumSpacing);
            int offsetX = PositiveMod(Hash(seed, salt, 193), spacing);
            int offsetY = PositiveMod(Hash(seed, salt, 719), spacing);

            return PositiveMod(worldCell.x - offsetX, spacing) == 0 && PositiveMod(worldCell.y - offsetY, spacing) == 0;
        }

        public void Spawn(BiomePropEntry entry, Vector2Int worldCell, Tilemap referenceTilemap, ChunkCoordinate chunk, bool flipX, int seed, int salt)
        {
            if (entry == null || entry.Prefab == null || referenceTilemap == null)
                return;
            
            if (!ResolveModificationStore())
            {
                Debug.LogError(
                    "WorldPropGenerator cannot spawn persistent " +
                    "props because WorldModificationStore was not found.",
                    this);

                return;
            }
            
            // Create the same ID every time this
            // procedural prop is generated.
            string persistentId = WorldEntityId.Create(seed, chunk, worldCell, WorldEntityCategory.Prop, salt);
            
            Debug.Log(
                $"[Prop Spawn Candidate] " +
                $"{worldCell} => {persistentId}",
                this);
            
            // This prop existed originally,
            // but the player destroyed it.
            if (_worldModificationStore.IsDestroyed(
                    chunk,
                    persistentId))
            {
                Debug.Log(
                    $"[Persistence] Skipping destroyed prop: " +
                    $"{persistentId}",
                    this);

                return;
            }
            
            GameObject instance = Get(entry.Prefab);
            
            Vector3Int cell = new Vector3Int(worldCell.x, worldCell.y, 0);
            Vector3 position = referenceTilemap.GetCellCenterWorld(cell) + entry.PositionOffset;

            instance.transform.SetParent(_activeParent, false);
            instance.transform.SetPositionAndRotation(position, Quaternion.identity);

            Vector3 scale = instance.transform.localScale;
            scale.x = Mathf.Abs(scale.x) * (entry.RandomFlipX && flipX ? -1f : 1f);
            instance.transform.localScale = scale;
            WorldEntity worldEntity = instance.GetComponent<WorldEntity>();
            
            if(worldEntity == null)
                worldEntity = instance.AddComponent<WorldEntity>();
            
            worldEntity.Initialize(persistentId, chunk, worldCell, _worldModificationStore, () => DespawnProp(instance, chunk));
            
            instance.SetActive(true);

            if (!_propsByChunk.TryGetValue(chunk, out List<GameObject> list))
            {
                list = new List<GameObject>(4);
                _propsByChunk.Add(chunk, list);
            }

            list.Add(instance);
        }

        public void ClearChunk(ChunkCoordinate chunk)
        {
            if (!_propsByChunk.TryGetValue(chunk, out List<GameObject> list))
                return;

            foreach (GameObject instance in list)
                Release(instance);

            list.Clear();
            _propsByChunk.Remove(chunk);
        }

        public void ClearAll()
        {
            foreach (List<GameObject> list in _propsByChunk.Values)
            {
                foreach (GameObject instance in list)
                    Release(instance);
            }

            _propsByChunk.Clear();
        }
        
        private void DespawnProp(GameObject instance, ChunkCoordinate chunk)
        {
            if (instance == null)
                return;

            if (_propsByChunk.TryGetValue(chunk, out List<GameObject> list))
            {
                list.Remove(instance);

                if (list.Count == 0)
                {
                    _propsByChunk.Remove(chunk);
                }
            }

            Release(instance);
        }
        
        private bool ResolveModificationStore()
        {
            if (_worldModificationStore != null)
                return true;

            _worldModificationStore =
                FindFirstObjectByType<
                    WorldModificationStore>();

            return _worldModificationStore != null;
        }

        private GameObject Get(GameObject prefab)
        {
            if (!_pools.TryGetValue(prefab, out Queue<GameObject> pool))
            {
                pool = new Queue<GameObject>();
                _pools.Add(prefab, pool);
            }

            while (pool.Count > 0)
            {
                GameObject instance = pool.Dequeue();
                if (instance != null)
                    return instance;
            }

            GameObject created = Instantiate(prefab, _poolParent);
            created.name = prefab.name;
            created.SetActive(false);
            _instanceToPrefab[created] = prefab;
            return created;
        }

        private void Release(GameObject instance)
        {
            if (instance == null)
                return;

            if (!_instanceToPrefab.TryGetValue(instance, out GameObject prefab))
            {
                Destroy(instance);
                return;
            }

            instance.SetActive(false);
            instance.transform.SetParent(_poolParent, false);
            instance.transform.localScale = Vector3.one;

            if (!_pools.TryGetValue(prefab, out Queue<GameObject> pool))
            {
                pool = new Queue<GameObject>();
                _pools.Add(prefab, pool);
            }

            pool.Enqueue(instance);
        }

        private void EnsureParents()
        {
            if (_activeParent == null)
            {
                GameObject root = new GameObject("Active Biome Props");
                root.transform.SetParent(transform, false);
                _activeParent = root.transform;
            }

            if (_poolParent == null)
            {
                GameObject root = new GameObject("Biome Prop Pool");
                root.transform.SetParent(transform, false);
                _poolParent = root.transform;
            }
        }

        private static int Hash(int a, int b, int c)
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + a;
                hash = hash * 31 + b;
                hash = hash * 31 + c;
                hash ^= hash >> 16;
                return hash;
            }
        }

        private static int PositiveMod(int value, int modulus)
        {
            int result = value % modulus;
            return result < 0 ? result + modulus : result;
        }
    }
}
