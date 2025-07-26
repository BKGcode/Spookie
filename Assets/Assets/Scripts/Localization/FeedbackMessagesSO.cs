using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "FeedbackMessages", menuName = "Spookie/Data/Feedback Messages")]
public class FeedbackMessagesSO : ScriptableObject
{
    [System.Serializable]
    public struct MessagePair
    {
        public string key;
        [TextArea] public string message;
    }

    [SerializeField] private List<MessagePair> messages;

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

    public string GetMessage(string key, string defaultMessage = "...")
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