using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "FeedbackMessages", menuName = "Spookie/Feedback Messages")]
public class FeedbackMessagesSO : ScriptableObject
{
    [System.Serializable]
    public class MessageEntry
    {
        public string key;
        [TextArea(2, 5)]
        public string message;
    }

    [Header("Game Messages")]
    public List<MessageEntry> messages = new List<MessageEntry>();

    private Dictionary<string, string> messageDictionary;

    private void OnEnable()
    {
        Debug.Log("FeedbackMessagesSO: Initializing message dictionary");
        InitializeDictionary();
    }

    private void InitializeDictionary()
    {
        messageDictionary = new Dictionary<string, string>();
        
        foreach (var entry in messages)
        {
            if (!string.IsNullOrEmpty(entry.key) && !messageDictionary.ContainsKey(entry.key))
            {
                messageDictionary.Add(entry.key, entry.message);
            }
        }
        
        Debug.Log($"FeedbackMessagesSO: Loaded {messageDictionary.Count} messages");
    }

    public string GetMessage(string key)
    {
        if (messageDictionary == null)
        {
            InitializeDictionary();
        }

        if (messageDictionary.TryGetValue(key, out string message))
        {
            return message;
        }

        Debug.LogWarning($"FeedbackMessagesSO: Message key '{key}' not found. Returning default message.");
        return "Message not found";
    }

    public bool HasMessage(string key)
    {
        if (messageDictionary == null)
        {
            InitializeDictionary();
        }

        return messageDictionary.ContainsKey(key);
    }
}

// ScriptRole: Manages all text messages in the game to avoid hardcoded strings
// UsesSO: None
// ReceivesFrom: UI scripts, game managers
// SendsTo: UI components that need text messages 