using UnityEngine;

[CreateAssetMenu(fileName = "new Area", menuName = "New Area")]
public class Area : ScriptableObject
{

    [Header("Properties of an Area")]

    [Tooltip("Name of the area")]
    public string AreaName;

    [TextArea]
    [Tooltip("Description of the area")]
    public string AreaDescription;

    [Header("For battle")]

    [Tooltip("When initiating a battle in that area, spawn this battle arena")]
    public GameObject BattleArenaPrefab;

    [Tooltip("When initiating a battle this animation plays as a transition")]
    public GameObject BattleStartAnimationObject;

    [Header("For entering loading screens")]

    [Tooltip("When entering this area, display this loading image (plays animation dropping from top of screen) (used to display area name, weather, etc")]
    public Sprite BackgroundUpLoading;

    [Tooltip("When entering this area, display this loading image (plays animation coming up to screen) (used to display area name, weather, etc")]
    public Sprite BackgroundDownLoading;

    [Header("Weather And Season Settings")]
    //TODO MAKE A CLASS TO ADD Weather, Season of the weather and chance
    [Tooltip("The weather effect that the area can have")]
    public Weather WeatherEffect;

    [Tooltip("The world position to spawn for example rain particles in the area")]
    public Vector2 WeatherInitialisePosition;
}
