using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "FeedbackMessages", menuName = "Spookie/Data/Feedback Messages")]
public class FeedbackMessagesSO : ScriptableObject
{
    [System.Serializable]
    public class FeedbackMessage
    {
        public string key;
        [TextArea]
        public string message;
    }

    public List<FeedbackMessage> messages = new List<FeedbackMessage> {
        new FeedbackMessage { key = "label_age", message = "Age: {0}" },
        new FeedbackMessage { key = "label_status", message = "Status: {0}" },
        new FeedbackMessage { key = "notification_dwarf_idle", message = "{0} is idle!" },
        new FeedbackMessage { key = "Idle", message = "Idle" },
        new FeedbackMessage { key = "Moving", message = "Moving" },
        new FeedbackMessage { key = "Mining", message = "Mining" },
        new FeedbackMessage { key = "Sleeping", message = "Sleeping" },
        new FeedbackMessage { key = "Wandering", message = "Wandering" }
    };

    private Dictionary<string, string> messageDictionary;

    private void OnEnable()
    {
        messageDictionary = new Dictionary<string, string>();
        foreach (var pair in messages)
        {
            if (!messageDictionary.ContainsKey(pair.key))
            {
                messageDictionary.Add(pair.key, pair.message);
            }
        }
    }

    public string GetMessage(string key, string defaultMessage = "")
    {
        if (messageDictionary.TryGetValue(key, out string message))
        {
            return message;
        }

        Debug.LogWarning($"Feedback message key '{key}' not found!");
        return defaultMessage;
    }
}

// ScriptRole: A database for storing and retrieving localizable UI text snippets.
// Dependencies: None
// HandlesEvents: None
// TriggersEvents: None
// UsesSO: None
// NeedsSetup: Create one instance from Assets menu and populate it with key-message pairs. 