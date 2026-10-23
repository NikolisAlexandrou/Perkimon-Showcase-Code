using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace island
{
    /// <summary>
    /// Handles saving/loading the persistent game state.
    ///
    /// The negative execution order is important:
    /// its Start() runs before normal world-streaming Start() methods.
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    public sealed class SaveManager : MonoBehaviour
    {
        [Header("References")]
        private TerrainGenerator _terrainGenerator;

        [SerializeField]
        private WorldModificationStore _worldModificationStore;

        [SerializeField]
        private Transform _player;

        [Header("Startup")]
        [Tooltip(
            "Automatically loads the existing save when the scene starts.")]
        [SerializeField]
        private bool _loadSaveOnStart = true;

        [Tooltip(
            "Automatically saves when the application closes.")]
        [SerializeField]
        private bool _saveOnQuit = true;

        [Header("File")]
        [SerializeField]
        private string _saveFileName =
            "save_01.json";

        private Vector3 _newGameSpawnPosition;

        private bool _initialized;
        
        private GameSaveData _loadedSaveData;

        private TerrainGenerator _currentTerrainGenerator;

        private ChunkManager _currentChunkManager;

        private int _currentWorldSeed;

        private bool _worldInitialized;
        
        public bool HasLoadedSave => _loadedSaveData != null;

        private void OnEnable()
        {
            SceneManager.sceneLoaded +=
                OnSceneLoaded;

            SceneManager.sceneUnloaded +=
                OnSceneUnloaded;
        }
        
        private void OnDisable()
        {
            SceneManager.sceneLoaded -=
                OnSceneLoaded;

            SceneManager.sceneUnloaded -=
                OnSceneUnloaded;
        }
        
        private void OnSceneLoaded(
            Scene scene,
            LoadSceneMode mode)
        {
            if (!TryFindWorldComponents(
                    scene,
                    out TerrainGenerator terrainGenerator,
                    out ChunkManager chunkManager))
            {
                return;
            }

            _currentTerrainGenerator =
                terrainGenerator;

            _currentChunkManager =
                chunkManager;

            InitializeWorldScene();
        }
        
        private void OnSceneUnloaded(
            Scene scene)
        {
            if (_currentTerrainGenerator == null)
                return;

            if (_currentTerrainGenerator
                    .gameObject.scene != scene)
            {
                return;
            }

            _currentTerrainGenerator = null;
            _currentChunkManager = null;

            _worldInitialized = false;
        }
        
        private bool TryFindWorldComponents(
            Scene scene,
            out TerrainGenerator terrainGenerator,
            out ChunkManager chunkManager)
        {
            terrainGenerator = null;
            chunkManager = null;

            GameObject[] roots =
                scene.GetRootGameObjects();

            foreach (GameObject root in roots)
            {
                if (terrainGenerator == null)
                {
                    terrainGenerator =
                        root.GetComponentInChildren<
                            TerrainGenerator>(true);
                }

                if (chunkManager == null)
                {
                    chunkManager =
                        root.GetComponentInChildren<
                            ChunkManager>(true);
                }

                if (terrainGenerator != null &&
                    chunkManager != null)
                {
                    return true;
                }
            }

            return false;
        }
        
        private void InitializeWorldScene()
        {
            if (_worldInitialized &&
                _currentTerrainGenerator != null)
            {
                return;
            }
            
            if (_currentTerrainGenerator == null ||
                _currentChunkManager == null)
            {
                return;
            }

            // Make absolutely sure streaming isn't running
            // while the world state is being restored.
            _currentChunkManager.StopStreaming(
                clearLoadedChunks: true);

            if (_loadedSaveData != null)
            {
                InitializeLoadedWorld();
            }
            else
            {
                InitializeNewWorld();
            }

            // Only NOW can chunks begin spawning.
            _currentChunkManager
                .BeginStreaming();

            _worldInitialized = true;
        }

        private bool TryFindTerrainGenerator()
        {
            _terrainGenerator =
                FindFirstObjectByType<
                    TerrainGenerator>();

            return _terrainGenerator != null;
        }

        private string SavePath =>
            Path.Combine(
                Application.persistentDataPath,
                _saveFileName);

        private void Start()
        {
            ResolvePersistentReferences();

            if (_player == null)
            {
                Debug.LogError(
                    "SaveManager could not find the player.",
                    this);

                return;
            }

            _newGameSpawnPosition =
                _player.position;

            if (_loadSaveOnStart &&
                File.Exists(SavePath))
            {
                ReadSaveFile();
            }

            _initialized = true;
            TryInitializeAlreadyLoadedWorld();
        }
        
        private void TryInitializeAlreadyLoadedWorld()
        {
            for (int i = 0;
                 i < SceneManager.sceneCount;
                 i++)
            {
                Scene scene =
                    SceneManager.GetSceneAt(i);

                if (!scene.isLoaded)
                    continue;

                if (!TryFindWorldComponents(
                        scene,
                        out TerrainGenerator terrainGenerator,
                        out ChunkManager chunkManager))
                {
                    continue;
                }

                _currentTerrainGenerator =
                    terrainGenerator;

                _currentChunkManager =
                    chunkManager;

                InitializeWorldScene();

                return;
            }
        }
        
        private void InitializeLoadedWorld()
        {
            GameSaveData saveData =
                _loadedSaveData;

            // -----------------------------------
            // 1. Restore procedural world seed
            // -----------------------------------

            _currentTerrainGenerator
                .InitializeWorld(
                    saveData.worldSeed);

            _currentWorldSeed =
                saveData.worldSeed;

            // -----------------------------------
            // 2. Restore world modifications
            // -----------------------------------

            _worldModificationStore
                .ImportSaveData(
                    saveData.worldModifications);

            // -----------------------------------
            // 3. Restore player location
            // -----------------------------------

            if (saveData.playerPosition != null)
            {
                _player.position =
                    new Vector3(
                        saveData.playerPosition.x,
                        saveData.playerPosition.y,
                        saveData.playerPosition.z);
            }

            Debug.Log(
                $"Loaded world seed {_currentWorldSeed}.",
                this);
        }
        
        private void InitializeNewWorld()
        {
            // Old destroyed resources must never carry
            // into a new world.
            _worldModificationStore
                .ClearAllModifications();

            _currentTerrainGenerator
                .InitializeNewWorld();

            _currentWorldSeed =
                _currentTerrainGenerator
                    .ActiveSeed;

            _player.position =
                _newGameSpawnPosition;

            Debug.Log(
                $"Created new world seed " +
                $"{_currentWorldSeed}.",
                this);
        }

        private void ResolvePersistentReferences()
        {
            if (_worldModificationStore == null)
            {
                _worldModificationStore =
                    FindFirstObjectByType<
                        WorldModificationStore>();
            }

            if (_player == null &&
                GameUtils.i != null &&
                GameUtils.i.player != null)
            {
                _player =
                    GameUtils.i.player.transform;
            }
        }
        
        private void ReadSaveFile()
        {
            try
            {
                string json =
                    File.ReadAllText(
                        SavePath);

                _loadedSaveData =
                    JsonUtility.FromJson<
                        GameSaveData>(json);

                if (_loadedSaveData == null)
                {
                    Debug.LogError(
                        "Failed to read save file.",
                        this);

                    return;
                }

                Debug.Log(
                    $"Save data loaded. World seed: " +
                    $"{_loadedSaveData.worldSeed}",
                    this);
            }
            catch (System.Exception exception)
            {
                Debug.LogError(
                    $"Failed to read save:\n" +
                    $"{exception}",
                    this);
            }
        }

        private bool ValidateReferences()
        {
            bool valid = true;

            if (_terrainGenerator == null)
            {
                Debug.LogError(
                    "SaveManager requires a TerrainGenerator.",
                    this);

                valid = false;
            }

            if (_worldModificationStore == null)
            {
                Debug.LogError(
                    "SaveManager requires a WorldModificationStore.",
                    this);

                valid = false;
            }

            if (_player == null)
            {
                Debug.LogError(
                    "SaveManager requires the player Transform.",
                    this);

                valid = false;
            }

            return valid;
        }

        /// <summary>
        /// Starts a completely clean world.
        /// </summary>
        public void StartNewGame()
        {
            _worldModificationStore
                .ClearAllModifications();

            // Generates either the configured seed
            // or a new random seed depending on
            // TerrainGenerator settings.
            _terrainGenerator
                .InitializeSeed();

            _player.position =
                _newGameSpawnPosition;

            _initialized = true;

            Debug.Log(
                $"Started new world. Seed: " +
                $"{_terrainGenerator.ActiveSeed}",
                this);
        }

        /// <summary>
        /// Writes the current game state to disk.
        /// </summary>
        [ContextMenu("Save Game")]
        public void SaveGame()
        {
            if (!_initialized ||
                !_worldInitialized)
            {
                Debug.LogWarning(
                    "Cannot save because no world is currently initialized.",
                    this);

                return;
            }

            GameSaveData saveData =
                new GameSaveData();

            saveData.worldSeed =
                _currentWorldSeed;

            Vector3 position =
                _player.position;

            saveData.playerPosition.x =
                position.x;

            saveData.playerPosition.y =
                position.y;

            saveData.playerPosition.z =
                position.z;

            saveData.worldModifications =
                _worldModificationStore
                    .ExportSaveData();

            try
            {
                string json =
                    JsonUtility.ToJson(
                        saveData,
                        true);

                File.WriteAllText(
                    SavePath,
                    json);

                // The in-memory representation now matches
                // what exists on disk.
                _loadedSaveData =
                    saveData;

                Debug.Log(
                    $"Game saved:\n{SavePath}",
                    this);
            }
            catch (System.Exception exception)
            {
                Debug.LogError(
                    $"Failed to save game:\n" +
                    $"{exception}",
                    this);
            }
        }

        /// <summary>
        /// This method is intended for initial scene startup.
        /// Runtime/hot loading will be added once we connect
        /// it to the active ChunkManager.
        /// </summary>
        private void LoadGameAtStartup()
        {
            try
            {
                string json =
                    File.ReadAllText(
                        SavePath);

                GameSaveData saveData =
                    JsonUtility.FromJson<
                        GameSaveData>(json);

                if (saveData == null)
                {
                    Debug.LogError(
                        "Save file could not be read. " +
                        "Starting a new game.",
                        this);

                    StartNewGame();
                    return;
                }

                // --------------------------------------
                // LOAD ORDER IS VERY IMPORTANT
                // --------------------------------------

                // 1. Recreate exact procedural world.
                _terrainGenerator.InitializeWorld(
                    saveData.worldSeed);

                // 2. Restore changes BEFORE chunks spawn.
                _worldModificationStore
                    .ImportSaveData(
                        saveData.worldModifications);

                // 3. Restore player location.
                if (saveData.playerPosition != null)
                {
                    _player.position =
                        new Vector3(
                            saveData.playerPosition.x,
                            saveData.playerPosition.y,
                            saveData.playerPosition.z);
                }

                Debug.Log(
                    $"Loaded world. Seed: " +
                    $"{saveData.worldSeed}",
                    this);
            }
            catch (Exception exception)
            {
                Debug.LogError(
                    $"Failed to load save:\n" +
                    $"{exception}\n" +
                    "Starting a new game.",
                    this);

                StartNewGame();
            }
        }

        [ContextMenu("Delete Save File")]
        public void DeleteSaveFile()
        {
            try
            {
                if (!File.Exists(SavePath))
                {
                    Debug.Log(
                        "There is no save file to delete.",
                        this);

                    return;
                }

                File.Delete(
                    SavePath);

                Debug.Log(
                    "Save file deleted.",
                    this);
            }
            catch (Exception exception)
            {
                Debug.LogError(
                    $"Failed to delete save:\n" +
                    $"{exception}",
                    this);
            }
        }

        [ContextMenu("Print Save Location")]
        public void PrintSaveLocation()
        {
            Debug.Log(
                SavePath,
                this);
        }

        private void OnApplicationQuit()
        {
            if (!_saveOnQuit ||
                !_initialized)
            {
                return;
            }

            SaveGame();
        }
    }
}