using System.Collections.Generic;
using UnityEngine;

namespace island
{
    public sealed class WorldModificationStore :
        MonoBehaviour
    {
        private readonly Dictionary<ChunkCoordinate, HashSet<string>> _destroyedEntities = new Dictionary<ChunkCoordinate, HashSet<string>>();

        private int _worldSeed;

        public int WorldSeed => _worldSeed;

        public void InitializeWorld(int worldSeed)
        {
            // Same world - preserve current runtime changes.
            if (_worldSeed == worldSeed)
                return;

            _worldSeed = worldSeed;

            _destroyedEntities.Clear();
        }

        public bool IsDestroyed(ChunkCoordinate chunk, string persistentId)
        {
            if (string.IsNullOrEmpty(persistentId))
            {
                return false;
            }

            if (!_destroyedEntities.TryGetValue(chunk, out HashSet<string> destroyed))
            {
                return false;
            }

            return destroyed.Contains(persistentId);
        }

        public void MarkDestroyed(ChunkCoordinate chunk, string persistentId)
        {
            if (string.IsNullOrEmpty(persistentId))
            {
                Debug.LogWarning("Tried to persist an entity with an empty ID.");

                return;
            }

            if (!_destroyedEntities.TryGetValue(chunk, out HashSet<string> destroyed))
            {
                destroyed = new HashSet<string>();

                _destroyedEntities.Add(chunk, destroyed);
            }

            destroyed.Add(persistentId);
            Debug.Log(
                $"[Persistence] DESTROYED\n" +
                $"Chunk: {chunk}\n" +
                $"ID: {persistentId}",
                this);
        }

        public WorldModificationSaveData ExportSaveData()
        {
            WorldModificationSaveData saveData = new WorldModificationSaveData { worldSeed = _worldSeed };

            foreach (KeyValuePair<ChunkCoordinate, HashSet<string>> pair in _destroyedEntities)
            {
                if (pair.Value.Count == 0)
                    continue;

                ChunkModificationData chunkData = new ChunkModificationData { chunkX = pair.Key.X,chunkY = pair.Key.Y, destroyedEntityIds = new List<string>(pair.Value) };

                saveData.chunks.Add(chunkData);
            }

            return saveData;
        }

        public void ImportSaveData(WorldModificationSaveData saveData)
        {
            _destroyedEntities.Clear();

            if (saveData == null)
                return;

            _worldSeed = saveData.worldSeed;

            if (saveData.chunks == null)
                return;

            foreach (ChunkModificationData chunkData in saveData.chunks)
            {
                if (chunkData == null)
                    continue;

                ChunkCoordinate chunk = new ChunkCoordinate(chunkData.chunkX, chunkData.chunkY);

                HashSet<string> destroyed = new HashSet<string>();

                if (chunkData.destroyedEntityIds != null)
                {
                    foreach (string id in chunkData.destroyedEntityIds)
                    {
                        if (!string.IsNullOrEmpty(id))
                            destroyed.Add(id);
                    }
                }

                if (destroyed.Count > 0)
                {
                    _destroyedEntities[chunk] = destroyed;
                }
            }
        }

        public void ClearAllModifications()
        {
            _destroyedEntities.Clear();
        }
    }
}