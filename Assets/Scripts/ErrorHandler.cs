using UnityEngine;
using System.Collections.Generic;

public static class ErrorHandler
{
    private static readonly List<string> errorLog = new List<string>();
    private static readonly int maxErrorLogSize = 100;
    
    public static void LogError(string message, Object context = null)
    {
        string errorMessage = $"[ERROR] {message}";
        Debug.LogError(errorMessage, context);
        
        // Add to error log
        errorLog.Add(errorMessage);
        if (errorLog.Count > maxErrorLogSize)
        {
            errorLog.RemoveAt(0);
        }
    }
    
    public static void LogWarning(string message, Object context = null)
    {
        string warningMessage = $"[WARNING] {message}";
        Debug.LogWarning(warningMessage, context);
        
        // Add to error log
        errorLog.Add(warningMessage);
        if (errorLog.Count > maxErrorLogSize)
        {
            errorLog.RemoveAt(0);
        }
    }
    
    public static void ValidateComponent<T>(T component, string componentName, Object context = null) where T : Component
    {
        if (component == null)
        {
            LogError($"{componentName} is missing!", context);
        }
    }
    
    public static void ValidateScriptableObject<T>(T scriptableObject, string soName, Object context = null) where T : ScriptableObject
    {
        if (scriptableObject == null)
        {
            LogError($"{soName} ScriptableObject is not assigned!", context);
        }
    }
    
    public static void ValidateReference<T>(T reference, string referenceName, Object context = null) where T : Object
    {
        if (reference == null)
        {
            LogError($"{referenceName} reference is missing!", context);
        }
    }
    
    public static bool TryGetComponent<T>(GameObject gameObject, out T component, string componentName = null) where T : Component
    {
        component = gameObject.GetComponent<T>();
        
        if (component == null)
        {
            string name = componentName ?? typeof(T).Name;
            LogError($"Failed to get {name} component from {gameObject.name}", gameObject);
            return false;
        }
        
        return true;
    }
    
    public static bool TryFindObjectOfType<T>(out T component, string componentName = null) where T : Component
    {
        component = Object.FindFirstObjectByType<T>();
        
        if (component == null)
        {
            string name = componentName ?? typeof(T).Name;
            LogError($"Failed to find {name} in scene");
            return false;
        }
        
        return true;
    }
    
    public static void ValidateLayerMask(LayerMask layerMask, string maskName, Object context = null)
    {
        if (layerMask.value == 0)
        {
            LogWarning($"{maskName} LayerMask is set to 'Nothing'", context);
        }
    }
    
    public static void ValidateRange(float value, float min, float max, string valueName, Object context = null)
    {
        if (value < min || value > max)
        {
            LogWarning($"{valueName} ({value}) is outside valid range [{min}, {max}]", context);
        }
    }
    
    public static void ValidatePositive(float value, string valueName, Object context = null)
    {
        if (value <= 0)
        {
            LogWarning($"{valueName} ({value}) should be positive", context);
        }
    }
    
    public static List<string> GetErrorLog()
    {
        return new List<string>(errorLog);
    }
    
    public static void ClearErrorLog()
    {
        errorLog.Clear();
    }
    
    public static int GetErrorCount()
    {
        return errorLog.Count;
    }
}

// ScriptRole: Centralized error handling and validation utility
// Dependencies: None
// UsesSO: None
// NeedsSetup: None (static utility class) 