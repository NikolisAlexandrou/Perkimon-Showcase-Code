using UnityEngine;
public static class DebugLogs
{
    [Tooltip("When enabled, console displays all logs")]
    public static bool DebugEnabled = false;

    /// <summary>
    /// Add a normal console message and it will display ONLY when DebugEnabled is true
    /// </summary>
    public static void Log(object message)
    {
        if (DebugEnabled)
            Debug.Log("[Perkimon] " + message);
    }

    /// <summary>
    /// Add a Warning console message
    /// </summary>
    public static void Warning(object message)
    {
        Debug.LogWarning("[Perkimon] " + message);
    }

    /// <summary>
    /// Add an Error console message
    /// </summary>
    public static void Error(object message)
    {
        Debug.LogError("[Perkimon] " + message);
    }
}