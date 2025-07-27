using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class FeedbackMessage
{
    [SerializeField] private string key;
    [TextArea(2, 4)]
    [SerializeField] private string message;

    public string Key => key;
    public string Message => message;
}

[CreateAssetMenu(fileName = "FeedbackMessages", menuName = "Spookie/Terrain/Feedback Messages")]
public class FeedbackMessagesSO : ScriptableObject
{
    [SerializeField] private List<FeedbackMessage> messages = new List<FeedbackMessage>();
    
    private Dictionary<string, string> messageCache;

    void OnEnable()
    {
        Debug.Log("FeedbackMessagesSO: OnEnable_Start");
        BuildCache();
        Debug.Log("FeedbackMessagesSO: OnEnable_End");
    }

    private void BuildCache()
    {
        messageCache = new Dictionary<string, string>();
        
        if (messages == null) return;
        
        foreach (var msg in messages)
        {
            if (string.IsNullOrEmpty(msg.Key))
            {
                Debug.LogWarning("FeedbackMessagesSO: Mensaje con clave vacía encontrado.", this);
                continue;
            }
            
            if (messageCache.ContainsKey(msg.Key))
            {
                Debug.LogWarning($"FeedbackMessagesSO: Clave duplicada encontrada: '{msg.Key}'", this);
                continue;
            }
            
            messageCache.Add(msg.Key, msg.Message);
        }
        
        Debug.Log($"FeedbackMessagesSO: Cache construido con {messageCache.Count} mensajes.");
    }

    public string GetMessage(string key)
    {
        if (messageCache == null) BuildCache();
        
        if (messageCache.TryGetValue(key, out string message))
        {
            return message;
        }
        
        Debug.LogWarning($"FeedbackMessagesSO: Mensaje no encontrado para clave: '{key}'. Devolviendo default.", this);
        return $"[MISSING: {key}]";
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        // Rebuild cache when values change in editor
        if (Application.isPlaying)
        {
            BuildCache();
        }
    }
#endif
}

// ScriptRole: Provides localized messages for UI feedback throughout the terrain generation system
// Dependencies: None
// HandlesEvents: None
// TriggersEvents: None
// UsesSO: None
// NeedsSetup: Create asset in Resources/Messages/ folder and populate with key-message pairs 