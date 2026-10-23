using System;
using UnityEngine;

namespace island
{
    /// <summary>
    /// This component identifies one runtime GameObject as a specific procedural world entity.
    /// </summary>
    public sealed class WorldEntity : MonoBehaviour
    {
        public string PersistentId { get; private set; }

        public ChunkCoordinate Chunk { get; private set; }

        public Vector2Int WorldCell { get; private set; }

        private WorldModificationStore _modificationStore;

        private Action _despawnAction;

        private bool _destroyed;

        /// <summary>
        /// This is particularly important because you're using object pooling.
        /// so Initialize() assigns its new identity every time it spawns.
        /// 
        /// </summary>
        /// <param name="persistentId"></param>
        /// <param name="chunk"></param>
        /// <param name="worldCell"></param>
        /// <param name="modificationStore"></param>
        /// <param name="despawnAction"></param>
        public void Initialize(string persistentId, ChunkCoordinate chunk, Vector2Int worldCell, WorldModificationStore modificationStore, Action despawnAction)
        {
            PersistentId = persistentId;

            Chunk = chunk;

            WorldCell = worldCell;

            _modificationStore =
                modificationStore;

            if (_modificationStore == null)
            {
                _modificationStore =
                    FindFirstObjectByType<WorldModificationStore>();
            }

            _despawnAction = despawnAction;

            // Very important for pooled objects.
            _destroyed = false;
        }

        public void DestroyPermanently()
        {
            if (_destroyed)
                return;
            
            if (_modificationStore == null)
            {
                _modificationStore =
                    FindFirstObjectByType<
                        WorldModificationStore>();
            }

            if (_modificationStore == null)
            {
                Debug.LogError(
                    $"{name} cannot persist destruction because " +
                    "WorldModificationStore is null.",
                    this);

                return;
            }
            
            _destroyed = true;

            _modificationStore.MarkDestroyed(
                Chunk,
                PersistentId);

            _despawnAction?.Invoke();
        }
    }
}