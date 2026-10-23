using System;
using UnityEngine;
using System.Collections.Generic;
using Random = UnityEngine.Random;

/// <summary>
/// Total number of segments to spawn is 4 from the pool _allSegmentsPrefabs
///
/// ------------------------------------------------------------------------------------------------
/// Boss Trainers
/// ------------------------------------------------------------------------------------------------
///
/// - Player needs to defeat at least 3 trial bosses to proceed to the central area boss
/// - if Player defeaats 4 trial bosses, bonus items/exp will be awarded at the end of the run
///
///
/// </summary>
///
/// TODO NOT DONE
/// - Take the tilemap from each segment and combine it to a global tilemap used in the scene instead
/// - Think about it, it might not be a good design decision tho, maybe spawning the 4 separate tilemaps
/// might be a better idea
public class ProceduralWorldGenerator : MonoBehaviour
{
    [Tooltip("The position of the center of the map")]
    [SerializeField] private Vector2 _centralPoint;

    [Tooltip("The map area of the center of the map (The extraction point of the session)")]
    [SerializeField] private GameObject _centralPointMap;

    [Tooltip("The total number of tiles from left to right of a single segment (an area in the map)")]
    [SerializeField] private float _totalSegmentDistance;

    [Tooltip("A list containing all the possible segments to spawn")]
    [SerializeField] private List<GameObject> _allSegmentsPrefabs;
    
    // Stores the current 4 segments that are spawned
    private List<GameObject> _allCurrentSegments;
    
    /// <summary>
    /// Get all the segments currently spawned
    /// </summary>
    public List<GameObject>  GetAllSegmentsPrefabs => _allCurrentSegments;

    private void Start()
    {
        SpawnMap();
    }

    /// <summary>
    /// Spawns the whole map, first the segments to determine which tiles need to be removed for
    /// the central object when it spawns
    /// </summary>
    public void SpawnMap()
    {
        SpawnAllSegments();
        SpawnCentralPoint();
    }

    /// <summary>
    /// Spawns the central object at the center of the map
    /// </summary>
    public void SpawnCentralPoint()
    {
        GameObject centralObject = Instantiate(_centralPointMap, _centralPoint, Quaternion.identity);
    }

    /// <summary>
    /// Spawns all 4 segments of the map using the _totalSegmentDistance set,
    /// relative to the centralPoint position
    /// </summary>
    public void SpawnAllSegments()
    {
        Vector2 topLeftPos = new Vector2((_centralPoint.x - _totalSegmentDistance)/2, (_centralPoint.y + _totalSegmentDistance)/2);
        Vector2 topRightPos = new Vector2((_centralPoint.x + _totalSegmentDistance)/2, (_centralPoint.y + _totalSegmentDistance)/2);
        Vector2 bottomLeftPos = new Vector2((_centralPoint.x - _totalSegmentDistance)/2, (_centralPoint.y - _totalSegmentDistance)/2);
        Vector2 bottomRightPos = new Vector2((_centralPoint.x + _totalSegmentDistance)/2, (_centralPoint.y - _totalSegmentDistance)/2);

        //TODO
        // 1. Spawn Segment from randomly selecting from pool
        // 2. Call function in SegmentProperties to Block unwanted entrances
        
        GameObject topLeftSegment = Instantiate(_allSegmentsPrefabs[Random.Range(0, _allSegmentsPrefabs.Count)], topLeftPos, Quaternion.identity);
        GameObject topRightSegment = Instantiate(_allSegmentsPrefabs[Random.Range(0, _allSegmentsPrefabs.Count)], topRightPos, Quaternion.identity);
        GameObject bottomLeftSegment = Instantiate(_allSegmentsPrefabs[Random.Range(0, _allSegmentsPrefabs.Count)], bottomLeftPos, Quaternion.identity);
        GameObject bottomRightSegment = Instantiate(_allSegmentsPrefabs[Random.Range(0, _allSegmentsPrefabs.Count)], bottomRightPos, Quaternion.identity);
        
        // Block the unwanted entrances, prevents player from walking out of map bounds
        topLeftSegment.GetComponent<SegmentProperties>().InitialiseSegmentProperties(PositionType.TopLeft);
        topRightSegment.GetComponent<SegmentProperties>().InitialiseSegmentProperties(PositionType.TopRight);
        bottomLeftSegment.GetComponent<SegmentProperties>().InitialiseSegmentProperties(PositionType.BottomLeft);
        bottomRightSegment.GetComponent<SegmentProperties>().InitialiseSegmentProperties(PositionType.BottomRight);
    }
}
