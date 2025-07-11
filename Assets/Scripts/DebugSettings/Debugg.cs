public static class Debugg
{
    public static bool EnableLogs = true;

    public static void Log(object message)
    {
        if (EnableLogs)
            UnityEngine.Debug.Log(message);
    }

    public static void LogWarning(object message)
    {
        if (EnableLogs)
            UnityEngine.Debug.LogWarning(message);
    }

    public static void LogError(object message)
    {
        if (EnableLogs)
            UnityEngine.Debug.LogError(message);
    }

    // Optional: Add context overloads
    public static void Log(object message, UnityEngine.Object context)
    {
        if (EnableLogs)
            UnityEngine.Debug.Log(message, context);
    }

    public static void LogWarning(object message, UnityEngine.Object context)
    {
        if (EnableLogs)
            UnityEngine.Debug.LogWarning(message, context);
    }

    public static void LogError(object message, UnityEngine.Object context)
    {
        if (EnableLogs)
            UnityEngine.Debug.LogError(message, context);
    }
}