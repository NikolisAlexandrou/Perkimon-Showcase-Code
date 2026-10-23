using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Object = System.Object;
using Random = UnityEngine.Random;

/// <summary>
/// Holds the information about a segment such as:
/// - Entrances objects and positions
/// - Trainers
/// - Monster Spawners
/// - Houses
/// - Bosses
/// </summary>
public class SegmentProperties : MonoBehaviour
{
    [Tooltip("A Dictionary containing all the entrances and where they are located within the segment (e.g. TOPLEFT : Entrance1)")]
    [SerializeField] private List<EntranceType> _entrances = new List<EntranceType>();

    [Header("Trainers Details")]
    [Tooltip("A list containing all available trainers in the area")] 
    [SerializeField] private List<GameObject> _trainers = new List<GameObject>();

    [Tooltip("Determines how many trainers can be spawned max in this segment, caps at the trainer's list count")]
    [SerializeField] private int _totalTrainersToSpawn;
    
    [Tooltip("The minimum number of trainers to spawn in the area.")]
    [SerializeField] private int _minimumTrainersToSpawn;

    [Tooltip("If true, random number of trainers will spawn in the segment")] 
    [SerializeField] private bool _spawnRandomTrainers;

    [Tooltip("If true, trainers limited to total number of trainers to spawn will spawn at random preset locations, otherwise the first total number of trainers will spawn")]
    [SerializeField] private bool _spawnRandomTrainerPositions;
    
    [Header("Spawner Details")]
    [Tooltip("A list containing all available monster spawners in the area")] 
    [SerializeField] private List<GameObject> _monstersSpawners = new List<GameObject>();
    
    [Tooltip("Determines how many Spawners can be spawned max in this segment, caps at the spawner's list count")]
    [SerializeField] private int _totalSpawnersToSpawn;
    
    [Tooltip("The minimum number of spawners to spawn in the area.")]
    [SerializeField] private int _minimumSpawnersToSpawn;

    [Tooltip("If true, random number of spawners will spawn in the segment")] 
    [SerializeField] private bool _spawnRandomSpawner;

    [Tooltip("If true, trainers limited to total number of spawners to spawn will spawn at random preset locations, otherwise the first total number of spawners will spawn")]
    [SerializeField] private bool _spawnRandomSpawnerPositions;

    [Header("Map Area Details")]
    [Tooltip("A list containing all Areas in the segment")] 
    [SerializeField] private List<MapArea> _mapAreas = new List<MapArea>();

    [Header("Trial Bosses Details")]
    [Tooltip("The boss trainer in the area needed to be defeated")]
    [SerializeField] private GameObject _bossTrainer;

    // Stores the assigned position in the whole map e.g. topLeft of the 4 segments
    // useful for determining which entrances should be blocked in the segment
    private PositionType _assignedMapPosition;

    /// <summary>
    /// Gets all entrances in the current area
    /// </summary>
    public List<EntranceType> Entrances => _entrances;

    /// <summary>
    /// Gets all the trainers in the current area
    /// </summary>
    public List<GameObject> Trainers => _trainers;

    /// <summary>
    /// Gets all the monster spawners in the current area
    /// </summary>
    public List<GameObject> MonstersSpawners => _monstersSpawners;

    /// <summary>
    /// Gets all the map areas in the current area
    /// </summary>
    public List<MapArea> MapAreas => _mapAreas;

    /// <summary>
    /// Gets or sets the assigned map position
    /// </summary>
    public PositionType AssignedMapPosition
    {
        get => _assignedMapPosition;
        set => _assignedMapPosition = value;
    }
    
    /// <summary>
    /// Check if boss trainer is defeated
    /// </summary>
    /// <returns></returns>
    public bool CheckBossTrainerDefeted() => _bossTrainer.GetComponent<BattleTrainer>().IsDefeated;

    public void InitialiseSegmentProperties(PositionType segmentPosition)
    {
        BlockEntrances(segmentPosition);
        SpawnObjectsLists(_trainers, _totalTrainersToSpawn, _minimumTrainersToSpawn, _spawnRandomTrainers, _spawnRandomTrainerPositions);
        SpawnObjectsLists(_monstersSpawners, _totalSpawnersToSpawn, _minimumSpawnersToSpawn, _spawnRandomSpawner, _spawnRandomSpawnerPositions);
    }


    /// <summary>
    /// Selects all entrances that are on the outer layer of the segment which will result in out of bounds
    /// and blocks them by enabling a collider object.
    /// </summary>
    /// <param name="segmentPosition"></param>
    public void BlockEntrances(PositionType segmentPosition)
    {

        // Assign segment position type before blocking entrances
        _assignedMapPosition = segmentPosition;

        if (_entrances.Count == 0)
            return;

        for (int i = 0; i < _entrances.Count; i++)
        {
            // Blocks left, top entrances from a topLeft segment
            if ((segmentPosition == PositionType.TopLeft && (_entrances[i].PositionType == PositionType.Top ||
                                                             _entrances[i].PositionType == PositionType.Left))
                || (segmentPosition == PositionType.TopRight && (_entrances[i].PositionType == PositionType.Top ||
                                                                 _entrances[i].PositionType == PositionType.Right))
                || (segmentPosition == PositionType.BottomLeft && (_entrances[i].PositionType == PositionType.Bottom ||
                                                                   _entrances[i].PositionType == PositionType.Left))
                || (segmentPosition == PositionType.BottomRight && (_entrances[i].PositionType == PositionType.Bottom ||
                                                                    _entrances[i].PositionType == PositionType.Right)))
            {
                _entrances[i].EntranceObject.SetActive(true);
            }
            else
            {
                _entrances[i].EntranceObject.SetActive(false);
            }
        }
    }
    
    /// <summary>
    /// Spawns a certain number of objects in a list in the area
    /// </summary>
    public void SpawnObjectsLists(List<GameObject> primaryList, int totalListObjectsToSpawn, int minimumObjectsToSpawn, bool randomObjects, bool randomObjectPositions)
    {
        // Disable all trainers first
        for (int i = 0; i < primaryList.Count; i++)
        {
            primaryList[i].SetActive(false);
        }

        // Limit total trainers from 0 to maximum trainers available
        int totalObjectsToSpawn = Mathf.Clamp(totalListObjectsToSpawn, minimumObjectsToSpawn, primaryList.Count);

        // If random spawning is enabled, choose a random amount
        if (randomObjects)
        {
            totalObjectsToSpawn = Random.Range(minimumObjectsToSpawn, primaryList.Count + 1);
        }

        if (totalObjectsToSpawn == 0)
            return;

        if (randomObjectPositions)
        {
            List<GameObject> spawnedObjects = new List<GameObject>();

            while (spawnedObjects.Count < totalObjectsToSpawn)
            {
                int index = Random.Range(0, primaryList.Count);

                if (!spawnedObjects.Contains(primaryList[index]))
                {
                    spawnedObjects.Add(primaryList[index]);
                }
            }

            // Enable selected trainers
            for (int i = 0; i < spawnedObjects.Count; i++)
            {
                spawnedObjects[i].SetActive(true);
            }
        }
        else
        {
            // Enable the first X trainers
            for (int i = 0; i < totalObjectsToSpawn; i++)
            {
                primaryList[i].SetActive(true);
            }
        }
    }
}

[System.Serializable]
public class EntranceType
{
    [SerializeField] private PositionType _positionType;
    [SerializeField] private GameObject _entranceObject;
    
    public PositionType PositionType { get => _positionType; set => _positionType = value; }
    public GameObject EntranceObject { get => _entranceObject; set => _entranceObject = value; }
    
}

public enum PositionType
{
    Top,
    Left,
    Right,
    Bottom,
    TopLeft,
    TopRight,
    BottomLeft,
    BottomRight
}
