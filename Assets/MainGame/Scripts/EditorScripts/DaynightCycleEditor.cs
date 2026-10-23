using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(DaynightCycleScript))]
public class DaynightCycleEditor : Editor
{
    private int _timeAmount;

    public override void OnInspectorGUI()
    {
        // This draws the normal inspector fields
        DrawDefaultInspector();

        // Reference to your WeatherSystem script
        DaynightCycleScript daynightScript = (DaynightCycleScript)target;

        GUILayout.Space(15);

        GUILayout.Label("Editor Controls", EditorStyles.boldLabel);

        // Player-editable variables
        _timeAmount = EditorGUILayout.IntField("Change time amount", _timeAmount);

        AddTimeButton(daynightScript);
        RemoveTimeButton(daynightScript);
        AddApplyWeatherButton(daynightScript);
        AddClearWeatherButton(daynightScript);
        GUILayout.Space(10);
        ChangeSeasonButton(daynightScript, SEASON.SUMMER);
        ChangeSeasonButton(daynightScript, SEASON.AUTUMN);
        ChangeSeasonButton(daynightScript, SEASON.WINTER);
        ChangeSeasonButton(daynightScript, SEASON.SPRING);
        ChangeToNextSeasonButton(daynightScript);
    }

    // Editor button for applying a weather condition in current area
    private void AddApplyWeatherButton(DaynightCycleScript daynightScript)
    {
        // Add some space between buttons
        GUILayout.Space(5);

        if (GUILayout.Button("Apply Weather condition"))
        {
            daynightScript.ForceApplyWeather();
            DebugLogs.Log("DAYNIGHT EDITOR > Applied weather condition to current area");
        }
    }

    // Editor button for clearing all weather conditions in current area
    private void AddClearWeatherButton(DaynightCycleScript daynightScript)
    {
        // Add some space between buttons
        GUILayout.Space(5);

        if (GUILayout.Button("Clear Weather conditions"))
        {
            daynightScript.ClearWeatherEffects();
            DebugLogs.Log("DAYNIGHT EDITOR > Cleared weather conditions to current area");
        }
    }

    // Editor button for adding some time to current time
    private void AddTimeButton(DaynightCycleScript daynightScript)
    {
        // Add some space between buttons
        GUILayout.Space(5);

        if (GUILayout.Button("Add Time +" + _timeAmount +" game minutes"))
        {
            daynightScript.ChangeTime(_timeAmount);
            DebugLogs.Log("DAYNIGHT EDITOR > Added "+ _timeAmount + " minutes to current time");
        }
    }

    // Editor button for removing some time from current time
    private void RemoveTimeButton(DaynightCycleScript daynightScript)
    {
        // Add some space between buttons
        GUILayout.Space(5);

        if (GUILayout.Button("Remove Time  -" + _timeAmount +" game minutes"))
        {
            daynightScript.ChangeTime(-_timeAmount);
            DebugLogs.Log("DAYNIGHT EDITOR > Removed " + _timeAmount + " minutes to current time");
        }
    }

    // Editor button for changing current season
    private void ChangeSeasonButton(DaynightCycleScript daynightScript, SEASON season)
    {
        // Add some space between buttons
        GUILayout.Space(5);

        if (GUILayout.Button("Change Season to " + season.ToString()))
        {
            daynightScript.Season = season;
            DebugLogs.Log("DAYNIGHT EDITOR > Changed season to " + season.ToString());
        }
    }

    // Editor button for changing current season to next season in cycle
    private void ChangeToNextSeasonButton(DaynightCycleScript daynightScript)
    {
        // Add some space between buttons
        GUILayout.Space(10);

        if (GUILayout.Button("Change Season to next season: " + daynightScript.NextSeason().ToString()))
        {
            daynightScript.Season = daynightScript.NextSeason();
            DebugLogs.Log("DAYNIGHT EDITOR > Changed season to " + daynightScript.NextSeason().ToString());
        }
    }

}