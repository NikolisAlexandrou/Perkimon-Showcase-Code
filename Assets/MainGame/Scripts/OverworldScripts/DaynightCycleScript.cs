using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering.Universal;
using DG.Tweening;
using UnityEngine.Rendering;

public enum SEASON
{
    AUTUMN,
    WINTER,
    SPRING,
    SUMMER
}
public enum WEATHER
{
    CLEAR,
    RAIN,
    SNOW,
    SANDSTORM
}
public class DaynightCycleScript : MonoBehaviour
{
    public static DaynightCycleScript i;  // References the current instance of this class

    [Header("Events")]

    [Tooltip("Events playing when season changes")]
    public UnityEvent OnSeasonChanged;

    [Tooltip("Events playing when weather condition changes")]
    public UnityEvent OnWeatherChanged;

    [Header("Seasons")]

    [Tooltip("Current season in the overworld")]
    [SerializeField] private SEASON _currentSeason = SEASON.AUTUMN;

    [Tooltip("How many in game days to change the season to next one")]
    [SerializeField] private float _seasonChangeDays = 7;

    [Header("Time Settings")]

    [Tooltip("Number of days that has passed since the start of the game")]
    [SerializeField] private int _dayNumber;

    [Tooltip("Time in real time minutes for one full game day to pass (default 60)")]
    [SerializeField] private float _timeForOneCycle = 60; // real time minutes

    [Tooltip("Time that the game will start in hours")]
    [SerializeField] private float _startTime = 8;

    [Tooltip("Time to start night in hours")]
    [SerializeField] private float _nightStartTime = 20;

    [Tooltip("Time to start morning in hours")]
    [SerializeField] private float _morningStartTime = 6;

    [Tooltip("Current time in game in minutes")]
    [SerializeField] private float _currentTime; //minutes

    [Header("Light Settings")]

    [Tooltip("Maximum light intensity during the day")]
    [SerializeField] private float _maximumDayIntensity = 1f;

    [Tooltip("Minimum light intensity during the night")]
    [SerializeField] private float _minimumNightLightIntensity = 0.15f;

    [Tooltip("Speed of light changing when transitioning from day to night and reverse")]
    [SerializeField] private float _lightSmoothSpeed = 5f;

    [Tooltip("Global light object that controls the light for all the scene")]
    [SerializeField] private Light2D _globalLight;

    [Tooltip("Text object in UI that will display current time")]
    [SerializeField] private TimeDisplayScript _timeDisplayUI;

    [Header("Weather System")]

    [Tooltip("Current season in the overworld")]
    [SerializeField] private Weather _currentWeather;

    [Tooltip("Clear weather scriptable object")]
    [SerializeField] private Weather _clearedWeather;

    [Tooltip("Timer in seconds to recheck for weather change")]
    [SerializeField] private float _weatherCheckTimer = 200;

    [Tooltip("Chance to change weather after weatherCheckTimer time passes")]
    [Range(0,100)]
    [SerializeField] private float _weatherCheckChance = 10;

    private GameObject _weatherOverworldEffect;  //  current overworld weather effect

    private bool _hasWeather;    //  if true, there is global weather, if false, there is no weather effect
    private bool _checkedForWeather; //  Flag for currently checking for weather change
    private float _currentWeatherTimer;  //  Current weather check timer
    private float _weatherStayTimer; //  Timer for current weather condition to stay active

    /// <summary>
    /// Get the current season in game and change it
    /// </summary>
    /// <returns></returns>
    public SEASON Season
    {
        get => _currentSeason;
        set 
        { 
            _currentSeason = value;
            OnSeasonChanged?.Invoke();
            //ZoneManager.i.RefreshTilemaps();
        }
    }

    /// <summary>
    /// Returns true if night time
    /// </summary>
    /// <returns></returns>
    public bool IsNight => (TimeHours >= _nightStartTime || TimeHours <= _morningStartTime) ? true : false;

    /// <summary>
    /// Get the current in game time in hours
    /// </summary>
    /// <returns></returns>
    public float TimeHours => _currentTime / 60;

    /// <summary>
    /// Get the current in game time in minutes
    /// </summary>
    /// <returns></returns>
    public int TimeMinutes => (int)_currentTime % 60;

    /// <summary>
    /// Returns true if weather condition is not clear and game has a weather condition
    /// </summary>
    /// <returns></returns>
    public bool HasWeather => _hasWeather;

    /// <summary>
    /// Gets the current weather condition in game and can change it
    /// </summary>
    /// <returns></returns>
    public Weather CurrentWeather { get => _currentWeather; set => _currentWeather = value; }

    /// <summary>
    /// Get the next season from current season
    /// </summary>
    /// <returns></returns>
    public SEASON NextSeason()
    {
        switch (_currentSeason)
        {
            case SEASON.AUTUMN: return SEASON.WINTER;
            case SEASON.WINTER: return SEASON.SPRING;
            case SEASON.SPRING: return SEASON.SUMMER;
            case SEASON.SUMMER: return SEASON.AUTUMN;
            default: return _currentSeason;
        }
    }

    private void Awake()
    {
        if (i != null)
            Destroy(i);

        i = this;
    }

    private void Start()
    {
        _currentTime = _startTime * 60; // startTime = hours
        _currentWeather = _clearedWeather;
    }

    private void FixedUpdate()
    {
        IncreaseTime();
        ChangeGlobalLightIntensity();
        CheckForStartWeather();
        CheckForEndWeather();
    }

    /// <summary>
    /// Create the weather condition overworld effect object
    /// </summary>
    public void ApplyWeatherEffects() => _weatherOverworldEffect = Instantiate(_currentWeather.WeatherOverworldEffect, GameUtils.i.player.GetComponent<PlayerController>().CurrentArea.WeatherInitialisePosition, _currentWeather.WeatherOverworldEffect.transform.rotation);

    /// <summary>
    /// Clears the weather, destroys current weather condition overworld effect object
    /// and sets weather to cleared weather
    /// </summary>
    public void ClearWeatherEffects()
    {
        _currentWeather = _clearedWeather;
        Destroy(_weatherOverworldEffect.gameObject);
        _weatherOverworldEffect = _clearedWeather.WeatherOverworldEffect;
    }

    /// <summary>
    /// Every seasonChangeDays the season changes
    /// </summary>
    public void CheckForSeasonChange()
    {
        // Checks if current days are divisible by seasonChangeDays (eg. every seasonChangeDays has passed, season changes)
        if (_dayNumber % _seasonChangeDays == 0)
            _currentSeason = NextSeason();
    }

    /// <summary>
    /// Increases game time and days
    /// </summary>
    public void IncreaseTime()
    {
        _currentTime += ((24 * 60) / (_timeForOneCycle * 60)) * Time.deltaTime;
        if (TimeHours >= 24)
        {
            _currentTime = 0;
            _dayNumber++;
        }
        // Display the time in selection menu UI
        if (_timeDisplayUI != null) _timeDisplayUI.ChangeTimeText(formatTime((int)TimeHours) + ":" + formatTime(TimeMinutes));
    }

    /// <summary>
    /// Adds to current time in game minutes
    /// </summary>
    /// <param name="timeAmount"></param>
    public void ChangeTime(int timeAmount) => _currentTime += (timeAmount);


    /// <summary>
    /// Applys weather effect to current area
    /// </summary>
    public void ForceApplyWeather()
    {
        //  If weather condition is already applied dont do anything
        if (_currentWeather != _clearedWeather)
            return;

        //Apply weather
        _currentWeather = GameUtils.i.player.GetComponent<PlayerController>().CurrentArea.WeatherEffect;
        ApplyWeatherEffects();
        _checkedForWeather = true;
        _hasWeather = true;
        OnWeatherChanged?.Invoke();
    }

    /// <summary>
    /// Format time in form "01" if time is less or equal to 9 and "60" if more than 9
    /// </summary>
    /// <param name="time"></param>
    /// <returns></returns>
    public string formatTime(int time)
    {
        if (time >= 0 && time <= 9)
            return "0" + time;
        return time.ToString();
    }

    public void CheckForStartWeather()
    {
        //  Check if player is in a map area
        if (GameUtils.i.player.GetComponent<PlayerController>().CurrentArea == null)
        {
            DebugLogs.Warning("WEATHER > The player is not in a map area make sure to add a map area and triggers");
            return;
        }

        //  Check if at current map area the player is in there can be weather conditions
        if (GameUtils.i.player.GetComponent<PlayerController>().CurrentArea.CanApplyWeather == false)
        {
            ClearWeatherEffects();
            return;
        }

        //  Check if weather is not already checked
        if (_checkedForWeather == true)
            return;

        //  Progress weather check timer to check for next weather change
        if (_currentWeatherTimer <= _weatherCheckTimer)
        {
            _currentWeatherTimer += Time.deltaTime;
        }
        else//timer stops, check for chance
        {
            _currentWeatherTimer = 0;    //  Reset timer
            if (Random.Range(0, 100) <= _weatherCheckChance)//if chances of weather effect succeed
            {
               ForceApplyWeather();
            }
        }
    }
    public void CheckForEndWeather()
    {
        if (_checkedForWeather == false)
            return;

        //  Check if duration of weather has passed
        if (_weatherStayTimer <= Random.Range(_currentWeather.DurationMin, _currentWeather.DurationMax))//timer runs
        {
            _weatherStayTimer += Time.deltaTime;
        }
        else//timer stops, clear weather
        {
            //  Reset weather stay timer
            _weatherStayTimer = 0;

            //Apply clear weather
            ClearWeatherEffects();
            _checkedForWeather = false;
            _hasWeather = false;
            OnWeatherChanged?.Invoke();

        }
    }

    /// <summary>
    /// Changes global light to match the time of day
    /// </summary>
    public void ChangeGlobalLightIntensity()
    {
        float hour = TimeHours;

        float targetIntensity;

        // Morning sunrise
        if (hour >= _morningStartTime && hour < _morningStartTime + 2f)
        {
            float t = Mathf.InverseLerp(6f, 8f, hour);
            targetIntensity = Mathf.Lerp(_minimumNightLightIntensity, _maximumDayIntensity, t);
        }
        // full day, maximum intensity
        else if (hour >= _morningStartTime + 2f && hour < _nightStartTime)
        {
            targetIntensity = _maximumDayIntensity;
        }
        // Night sunset
        else if (hour >= _nightStartTime && hour < _nightStartTime + 2f)
        {
            float t = Mathf.InverseLerp(20f, 22f, hour);
            targetIntensity = Mathf.Lerp(_maximumDayIntensity, _minimumNightLightIntensity, t);
        }
        // Full night, minimum intensity
        else
        {
            targetIntensity = _minimumNightLightIntensity;
        }

        // Extra smoothing so it eases nicely even if time jumps a bit
        _globalLight.intensity = Mathf.Lerp(
            _globalLight.intensity,
            targetIntensity,
            _lightSmoothSpeed * Time.deltaTime
        );
    }
}
