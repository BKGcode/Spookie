using UnityEngine;
using System.Collections.Generic;
using System.Linq; // Needed for FirstOrDefault

[System.Serializable]
public class FeedbackMessage
{
    public string key;
    [TextArea(3, 5)] // Allows multi-line input in Inspector
    public string message;
}

[CreateAssetMenu(fileName = "FeedbackMessages", menuName = "Spookie/UI/Feedback Messages", order = 1)]
public class FeedbackMessagesSO : ScriptableObject
{
    public List<FeedbackMessage> messages = new List<FeedbackMessage>();

    // Cache for faster lookups at runtime
    private Dictionary<string, string> messageCache;

    private void OnEnable()
    {
        // Build cache when the asset is loaded
        messageCache = new Dictionary<string, string>();
        if (messages != null)
        {
            foreach (var msg in messages)
            {
                if (!string.IsNullOrEmpty(msg.key) && !messageCache.ContainsKey(msg.key))
                {
                    messageCache.Add(msg.key, msg.message);
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
        if (messageCache != null && messageCache.TryGetValue(key, out string message))
        {
            return message;
        }

        Debug.LogWarning($"Feedback message key not found: '{key}'. Returning default.");
        // Fallback: try searching the list directly (slower, useful for in-editor changes)
        #if UNITY_EDITOR
        var foundMsg = messages.FirstOrDefault(m => m.key == key);
        if (foundMsg != null) return foundMsg.message;
        #endif

        return defaultMessage; // Return default if key not found
    }
}

// --- Summary Block ---
// ScriptRole: Holds a configurable list of key-value pairs for UI feedback messages. Provides a method to retrieve messages by key.
// RelatedScripts: TaskManager, TaskListUI, ActiveTasksUI, ConfirmationPanelUI (reference this to get display text)
// UsesSO: None 