using UnityEngine;

public static class ValidationHelper
{
    public static bool ValidateRequiredComponents<T>(T component) where T : Component
    {
        var requiredAttributes = typeof(T).GetCustomAttributes(typeof(RequireComponent), true);
        
        foreach (RequireComponent attribute in requiredAttributes)
        {
            if (attribute.m_Type0 != null && component.GetComponent(attribute.m_Type0) == null)
            {
                ErrorHandler.LogError($"Missing required component: {attribute.m_Type0.Name}", component);
                return false;
            }
            
            if (attribute.m_Type1 != null && component.GetComponent(attribute.m_Type1) == null)
            {
                ErrorHandler.LogError($"Missing required component: {attribute.m_Type1.Name}", component);
                return false;
            }
            
            if (attribute.m_Type2 != null && component.GetComponent(attribute.m_Type2) == null)
            {
                ErrorHandler.LogError($"Missing required component: {attribute.m_Type2.Name}", component);
                return false;
            }
        }
        
        return true;
    }
    
    public static bool ValidateScriptableObjectReferences<T>(T component) where T : Component
    {
        var fields = typeof(T).GetFields(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        bool allValid = true;
        
        foreach (var field in fields)
        {
            if (field.FieldType.IsSubclassOf(typeof(ScriptableObject)))
            {
                var value = field.GetValue(component) as ScriptableObject;
                if (value == null)
                {
                    ErrorHandler.LogWarning($"ScriptableObject reference '{field.Name}' is not assigned in {component.name}", component);
                    allValid = false;
                }
            }
        }
        
        return allValid;
    }
    
    public static bool ValidateTransformReferences<T>(T component) where T : Component
    {
        var fields = typeof(T).GetFields(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        bool allValid = true;
        
        foreach (var field in fields)
        {
            if (field.FieldType == typeof(Transform))
            {
                var value = field.GetValue(component) as Transform;
                if (value == null)
                {
                    ErrorHandler.LogWarning($"Transform reference '{field.Name}' is not assigned in {component.name}", component);
                    allValid = false;
                }
            }
        }
        
        return allValid;
    }
    
    public static bool ValidateLayerMask(LayerMask layerMask, string maskName, Object context = null)
    {
        if (layerMask.value == 0)
        {
            ErrorHandler.LogWarning($"{maskName} LayerMask is set to 'Nothing'", context);
            return false;
        }
        return true;
    }
    
    public static bool ValidateRange(float value, float min, float max, string valueName, Object context = null)
    {
        if (value < min || value > max)
        {
            ErrorHandler.LogWarning($"{valueName} ({value}) is outside valid range [{min}, {max}]", context);
            return false;
        }
        return true;
    }
    
    public static bool ValidatePositive(float value, string valueName, Object context = null)
    {
        if (value <= 0)
        {
            ErrorHandler.LogWarning($"{valueName} ({value}) should be positive", context);
            return false;
        }
        return true;
    }
    
    public static bool ValidateNotNull<T>(T obj, string objName, Object context = null) where T : class
    {
        if (obj == null)
        {
            ErrorHandler.LogError($"{objName} is null!", context);
            return false;
        }
        return true;
    }
}

// ScriptRole: Utility class for component validation using attributes
// Dependencies: ErrorHandler
// UsesSO: None
// NeedsSetup: None (static utility class) 