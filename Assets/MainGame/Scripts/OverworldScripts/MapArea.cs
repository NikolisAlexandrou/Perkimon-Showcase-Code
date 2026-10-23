using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Contains information about an area in the map like name, description, weather conditions,
/// battle arena to spawn, etc.
/// </summary>
public class MapArea : MonoBehaviour
{
    [Tooltip("The Area scriptable object that contains details about this area")]
    [SerializeField] private Area _area;

    /// <summary>
    /// Get the area name
    /// </summary>
    public string AreaName => _area.name;

    /// <summary>
    /// Get the weather conditions applicable to this area
    /// </summary>
    public Weather WeatherEffect => _area.WeatherEffect;

    /// <summary>
    /// Get the position that the weather effects object will get initialised
    /// </summary>
    public Vector2 WeatherInitialisePosition => _area.WeatherInitialisePosition;

    /// <summary>
    /// Wether or not the area has any weather conditions
    /// </summary>
    public bool CanApplyWeather => _area.WeatherEffect == null ? false : true;

    /// <summary>
    /// Get the animation object when the battle starts just after transition to battle
    /// </summary>
    public GameObject BattleStartAnimationObject => _area.BattleStartAnimationObject;

    /// <summary>
    /// Get the prefab of the battle arena
    /// </summary>
    public GameObject BattleArenaPrefab => _area.BattleArenaPrefab;

    /// <summary>
    /// Get the Top background of loading screen
    /// </summary>
    public Sprite BackgroundUpLoading => _area.BackgroundUpLoading;

    /// <summary>
    /// Get the Bottom background of loading screen
    /// </summary>
    public Sprite BackgroundDownLoading => _area.BackgroundDownLoading;
}

//---------------------------------------------------------------------------------------------------
//  Used for map in diary to display location found of monster
//---------------------------------------------------------------------------------------------------
/// <summary>
/// Get the game object that is displayed in the map
/// </summary>
[System.Serializable]
public class AreaMapGameObject
{
    [SerializeField] private Area _areaFound;
    [SerializeField] private GameObject _areaInMapGameObject;  // this is the gameobject of the route found in the map of diary

    /// <summary>
    /// Get the area of the map game object
    /// </summary>
    public Area AreaFound => _areaFound; 

    /// <summary>
    /// Get the game object that will be displayed in map of area
    /// </summary>
    public GameObject AreaInMapGameObject => _areaInMapGameObject;
}