using UnityEngine;
using System.Collections.Generic;
using System.Linq; // Needed for FirstOrDefault

[System.Serializable]
public class FeedbackMessage
{
    public string key;
    [TextArea(3, 5)] // Allows multi-line input in Inspector
    public string message;
    [Tooltip("Si está marcado, el mensaje permanecerá visible hasta que otro mensaje lo reemplace o el contexto cambie.")]
    public bool isPermanent = false;
}

[CreateAssetMenu(fileName = "FeedbackMessages", menuName = "Spookie/UI/Feedback Messages", order = 1)]
public class FeedbackMessagesSO : ScriptableObject
{
    [Tooltip("Duración por defecto en segundos para mostrar los mensajes de feedback (a menos que estén marcados como permanentes).")]
    public float defaultMessageDuration = 3f;

    public List<FeedbackMessage> messages = new List<FeedbackMessage>();

    // Cache for faster lookups at runtime
    // Changed cache to store the whole FeedbackMessage object
    private Dictionary<string, FeedbackMessage> messageCache;

    private void OnEnable()
    {
        // Build cache when the asset is loaded
        messageCache = new Dictionary<string, FeedbackMessage>();
        if (messages != null)
        {
            foreach (var msg in messages)
            {
                if (!string.IsNullOrEmpty(msg.key) && !messageCache.ContainsKey(msg.key))
                {
                    // Store the whole message object
                    messageCache.Add(msg.key, msg);
                }
                else
                {
                     Debug.LogWarning($"Duplicate or empty key found in FeedbackMessagesSO: '{msg.key}'");
                }
            }
        }
    }

    public string GetMessage(string key, string defaultMessage = "...")
    {
        if (messageCache != null && messageCache.TryGetValue(key, out FeedbackMessage messageInfo))
        {
            return messageInfo.message;
        }

        Debug.LogWarning($"Feedback message key not found: '{key}'. Returning default.");
        // Fallback: try searching the list directly (slower, useful for in-editor changes)
        #if UNITY_EDITOR
        var foundMsg = messages.FirstOrDefault(m => m.key == key);
        if (foundMsg != null) return foundMsg.message;
        #endif

        return defaultMessage; // Return default if key not found
    }

    // New method to get the full message info, including permanency
    public FeedbackMessage GetMessageInfo(string key)
    {
        if (messageCache != null && messageCache.TryGetValue(key, out FeedbackMessage messageInfo))
        {
            return messageInfo;
        }

        // Fallback for editor changes (slower)
        #if UNITY_EDITOR
        var foundMsg = messages.FirstOrDefault(m => m.key == key);
        if (foundMsg != null) return foundMsg;
        #endif

        Debug.LogWarning($"Feedback message key not found: '{key}'. Returning null info.");
        return null; // Return null if key not found
    }
}

// --- Summary Block ---
// ScriptRole: Holds a configurable list of key-value pairs for UI feedback messages. Provides a method to retrieve messages by key.
// RelatedScripts: TaskManager, TaskListUI, ActiveTasksUI, ConfirmationPanelUI (reference this to get display text)
// UsesSO: None 