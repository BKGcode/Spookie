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
    [SerializeField] private List<FeedbackMessage> messages;
    
    private Dictionary<string, string> messageCache;

    private void OnEnable()
    {
        BuildCache();
    }

    private void BuildCache()
    {
        if (messages == null) return;
        
        messageCache = new Dictionary<string, string>(messages.Count);
        foreach (var message in messages)
        {
            if (!string.IsNullOrEmpty(message.Key) && !messageCache.ContainsKey(message.Key))
            {
                messageCache.Add(message.Key, message.Message);
            }
        }
    }

    public string GetMessage(string key)
    {
        if (messageCache == null) BuildCache();
        
        if (messageCache.TryGetValue(key, out string message))
        {
            return message;
        }
        
        Debug.LogWarning($"FeedbackMessagesSO: Mensaje no encontrado para clave: '{key}'. Devolviendo default.", this);
        return $"[MESSAGE NOT FOUND: {key}]";
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

// ScriptRole: Provides localized messages for UI feedback.
// Dependencies: None
// HandlesEvents: None
// TriggersEvents: None
// UsesSO: None
// NeedsSetup: Create one instance in the project. Populate the list with key-message pairs for the UI. 