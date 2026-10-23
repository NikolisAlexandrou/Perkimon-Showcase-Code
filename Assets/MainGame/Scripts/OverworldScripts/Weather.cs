using UnityEngine;

/// <summary>
/// Creates new weather scriptable object
/// </summary>
[CreateAssetMenu(fileName = "Weather", menuName = "New Weather")]
public class Weather : ScriptableObject
{
    [Header("Properties of Weather")]

    [Tooltip("Weather type")]
    public WEATHER WeatherType;

    [Tooltip("image used in loading screens when entering an area to show what weather condition the area is in")]
    public Sprite TransitionImage;

    [Tooltip("Minimum time the weather will be active in seconds")]
    public float DurationMin;

    [Tooltip("Maximum time the weather will be active in seconds")]
    public float DurationMax;

    [Tooltip("The actual effect object of the weather for example particle effect for rain")]
    public GameObject WeatherOverworldEffect;


    [Header("Battle Boosts")]
    [Tooltip("The monster boosted that contains that element affected by weather in battle")]
    public ElementType ElementBoosting;

    [Tooltip("Multiplier for attack for that monster containing that element in battle")]
    public float AttackBoost = 1;

    [Tooltip("Multiplier for defense for that monster containing that element in battle")]
    public float DefenseBoost = 1;
}
