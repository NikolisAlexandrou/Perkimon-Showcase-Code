using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace island
{
    [RequireComponent(typeof(TerrainGenerator))]
    public class ChunkManager : MonoBehaviour
    {
        [Header("Streaming")]
        [SerializeField] private Transform _target;
        [SerializeField, Min(0)] private int _loadRadius = 2;
        [SerializeField, Min(0)] private int _unloadRadius = 3;
        [SerializeField] private bool _generateOnStart = true;

        [Header("Frame Budget")]
        [Tooltip("How many terrain rows are generated before yielding for one frame.")]
        [SerializeField, Range(1, 32)] private int _generationRowsPerFrame = 8;

        [Tooltip("Frames to wait between completed chunk loads. Zero loads the next chunk immediately.")]
        [SerializeField, Min(0)] private int _framesBetweenChunkLoads;

        [Tooltip("Maximum chunks removed in one frame.")]
        [SerializeField, Range(1, 8)] private int _maxUnloadsPerFrame = 2;

        [Header("Debug")]
        [SerializeField] private bool _logChunkChanges;

        private TerrainGenerator _terrainGenerator;

        private readonly HashSet<ChunkCoordinate> _loadedChunks = new();
        private readonly HashSet<ChunkCoordinate> _queuedLoads = new();
        private readonly HashSet<ChunkCoordinate> _queuedUnloads = new();
        private readonly List<ChunkCoordinate> _loadQueue = new();
        private readonly Queue<ChunkCoordinate> _unloadQueue = new();

        private ChunkCoordinate _currentChunk;
        private Coroutine _activeLoadRoutine;
        private int _nextLoadFrame;
        private bool _initialized;
        private bool _streamingStarted;

        public IReadOnlyCollection<ChunkCoordinate> LoadedChunks => _loadedChunks;
        public int PendingLoadCount => _loadQueue.Count;
        public int PendingUnloadCount => _unloadQueue.Count;

        private void Awake()
        {
            _terrainGenerator = GetComponent<TerrainGenerator>();
        }

        private void Start()
        {
            ResolveTarget();

            if (_target == null)
            {
                Debug.LogError(
                    "ChunkManager requires a target Transform, normally the player.",
                    this);

                enabled = false;
                return;
            }

            _unloadRadius =
                Mathf.Max(
                    _loadRadius + 1,
                    _unloadRadius);

            // Normally SaveManager starts streaming after
            // the seed/save data has been restored.
            //
            // This remains useful for testing the world scene
            // directly without the persistent systems scene.
            if (_generateOnStart)
            {
                BeginStreaming();
            }
        }

        private void Update()
        {
            if (!_initialized ||
                !_streamingStarted)
            {
                return;
            }

            ChunkCoordinate newChunk = GetTargetChunk();

            if (!newChunk.Equals(_currentChunk))
            {
                _currentChunk = newChunk;
                RebuildStreamingQueues();
            }

            ProcessStreaming();
        }

        public void RegenerateWorld()
        {
            StopStreaming(
                clearLoadedChunks: true);

            _terrainGenerator
                .InitializeSeed();

            BeginStreaming();
        }
        
        public void BeginStreaming()
        {
            
            if (_streamingStarted)
                return;
            
            if (!_terrainGenerator.IsWorldInitialized)
            {
                Debug.LogError(
                    "ChunkManager cannot begin streaming because " +
                    "TerrainGenerator has not been initialized.",
                    this);

                return;
            }
            
            ResolveTarget();

            if (_target == null)
            {
                Debug.LogError(
                    "Cannot begin chunk streaming because " +
                    "no target was found.",
                    this);

                return;
            }

            _unloadRadius =
                Mathf.Max(
                    _loadRadius + 1,
                    _unloadRadius);

            _currentChunk =
                GetTargetChunk();

            _loadQueue.Clear();
            _unloadQueue.Clear();
            _queuedLoads.Clear();
            _queuedUnloads.Clear();

            _initialized = true;
            _streamingStarted = true;

            RebuildStreamingQueues();

            if (_logChunkChanges)
            {
                Debug.Log(
                    $"Chunk streaming started at {_currentChunk}.",
                    this);
            }
        }
        
        public void StopStreaming(
            bool clearLoadedChunks = true)
        {
            _streamingStarted = false;
            _initialized = false;

            if (_activeLoadRoutine != null)
            {
                StopCoroutine(
                    _activeLoadRoutine);

                _activeLoadRoutine = null;
            }

            _loadQueue.Clear();
            _unloadQueue.Clear();

            _queuedLoads.Clear();
            _queuedUnloads.Clear();

            if (!clearLoadedChunks)
                return;

            foreach (
                ChunkCoordinate coordinate
                in _loadedChunks)
            {
                _terrainGenerator.ClearChunk(
                    coordinate);
            }

            _loadedChunks.Clear();
        }

        private void ResolveTarget()
        {
            if (_target != null)
                return;

            if (GameUtils.i != null && GameUtils.i.player != null)
            {
                _target = GameUtils.i.player.transform;
                return;
            }

            if (Camera.main != null)
                _target = Camera.main.transform;
        }

        private void RebuildStreamingQueues()
        {
            _loadQueue.Clear();
            _unloadQueue.Clear();
            _queuedLoads.Clear();
            _queuedUnloads.Clear();

            for (int y = -_loadRadius; y <= _loadRadius; y++)
            {
                for (int x = -_loadRadius; x <= _loadRadius; x++)
                {
                    ChunkCoordinate coordinate = new ChunkCoordinate(
                        _currentChunk.X + x,
                        _currentChunk.Y + y);

                    if (_loadedChunks.Contains(coordinate))
                        continue;

                    _loadQueue.Add(coordinate);
                    _queuedLoads.Add(coordinate);
                }
            }

            // Nearest chunks load first, so the player-facing area appears first.
            _loadQueue.Sort(CompareLoadPriority);

            foreach (ChunkCoordinate coordinate in _loadedChunks)
            {
                if (ChebyshevDistance(coordinate, _currentChunk) <= _unloadRadius)
                    continue;

                _unloadQueue.Enqueue(coordinate);
                _queuedUnloads.Add(coordinate);
            }
        }

        private void ProcessStreaming()
        {
            ProcessUnloads();

            if (_activeLoadRoutine != null ||
                _loadQueue.Count == 0 ||
                Time.frameCount < _nextLoadFrame)
            {
                return;
            }

            ChunkCoordinate coordinate = _loadQueue[0];
            _loadQueue.RemoveAt(0);
            _queuedLoads.Remove(coordinate);

            if (_loadedChunks.Contains(coordinate))
                return;

            _activeLoadRoutine = StartCoroutine(LoadChunkRoutine(coordinate));
        }

        private IEnumerator LoadChunkRoutine(ChunkCoordinate coordinate)
        {
            yield return _terrainGenerator.GenerateChunkRoutine(
                coordinate,
                _generationRowsPerFrame);

            _loadedChunks.Add(coordinate);
            _activeLoadRoutine = null;
            _nextLoadFrame = Time.frameCount + _framesBetweenChunkLoads;

            if (_logChunkChanges)
                Debug.Log($"Loaded chunk {coordinate}", this);
        }

        private void ProcessUnloads()
        {
            int unloadedThisFrame = 0;

            while (_unloadQueue.Count > 0 &&
                   unloadedThisFrame < _maxUnloadsPerFrame)
            {
                ChunkCoordinate coordinate = _unloadQueue.Dequeue();
                _queuedUnloads.Remove(coordinate);

                // The player may have moved back before this queued unload ran.
                if (ChebyshevDistance(coordinate, _currentChunk) <= _unloadRadius)
                    continue;

                if (!_loadedChunks.Remove(coordinate))
                    continue;

                _terrainGenerator.ClearChunk(coordinate);
                unloadedThisFrame++;

                if (_logChunkChanges)
                    Debug.Log($"Unloaded chunk {coordinate}", this);
            }
        }

        private int CompareLoadPriority(
            ChunkCoordinate a,
            ChunkCoordinate b)
        {
            int aDistance = SquaredDistance(a, _currentChunk);
            int bDistance = SquaredDistance(b, _currentChunk);

            if (aDistance != bDistance)
                return aDistance.CompareTo(bDistance);

            if (a.Y != b.Y)
                return a.Y.CompareTo(b.Y);

            return a.X.CompareTo(b.X);
        }

        private static int SquaredDistance(
            ChunkCoordinate a,
            ChunkCoordinate b)
        {
            int x = a.X - b.X;
            int y = a.Y - b.Y;
            return x * x + y * y;
        }

        private static int ChebyshevDistance(
            ChunkCoordinate a,
            ChunkCoordinate b)
        {
            return Mathf.Max(
                Mathf.Abs(a.X - b.X),
                Mathf.Abs(a.Y - b.Y));
        }

        private ChunkCoordinate GetTargetChunk()
        {
            return ChunkCoordinate.FromWorldPosition(
                _target.position,
                _terrainGenerator.ChunkSize);
        }

        private void OnValidate()
        {
            _loadRadius = Mathf.Max(0, _loadRadius);
            _unloadRadius = Mathf.Max(_loadRadius + 1, _unloadRadius);
            _generationRowsPerFrame = Mathf.Clamp(_generationRowsPerFrame, 1, 32);
            _framesBetweenChunkLoads = Mathf.Max(0, _framesBetweenChunkLoads);
            _maxUnloadsPerFrame = Mathf.Clamp(_maxUnloadsPerFrame, 1, 8);
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            TerrainGenerator generator = GetComponent<TerrainGenerator>();

            if (_target == null || generator == null)
                return;

            ChunkCoordinate center = ChunkCoordinate.FromWorldPosition(
                _target.position,
                generator.ChunkSize);

            Gizmos.color = Color.yellow;

            for (int y = -_loadRadius; y <= _loadRadius; y++)
            {
                for (int x = -_loadRadius; x <= _loadRadius; x++)
                {
                    Vector3Int origin =
                        new ChunkCoordinate(center.X + x, center.Y + y)
                            .CellOrigin(generator.ChunkSize);

                    Gizmos.DrawWireCube(
                        new Vector3(
                            origin.x + generator.ChunkSize * 0.5f,
                            origin.y + generator.ChunkSize * 0.5f,
                            0f),
                        new Vector3(
                            generator.ChunkSize,
                            generator.ChunkSize,
                            0f));
                }
            }
        }
#endif
    }
}
