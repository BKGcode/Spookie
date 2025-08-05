using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "FeedbackMessages", menuName = "Spookie/Feedback Messages")]
public class FeedbackMessagesSO : ScriptableObject
{
    [System.Serializable]
    public class MessagePair
    {
        public string key;
        [TextArea(2, 5)]
        public string message;
    }

    [SerializeField] private List<MessagePair> messages = new List<MessagePair>();
    private Dictionary<string, string> messageDictionary = new Dictionary<string, string>();

    private void OnEnable()
    {
        BuildDictionary();
    }

    private void BuildDictionary()
    {
        messageDictionary.Clear();
        foreach (var pair in messages)
        {
            if (!string.IsNullOrEmpty(pair.key))
            {
                messageDictionary[pair.key] = pair.message;
            }
        }
    }

    public string GetMessage(string key)
    {
        if (messageDictionary.ContainsKey(key))
        {
            return messageDictionary[key];
        }

        Debug.LogWarning($"Message key '{key}' not found in FeedbackMessagesSO");
        return "Message not found";
    }

    public void AddMessage(string key, string message)
    {
        if (!messageDictionary.ContainsKey(key))
        {
            var newPair = new MessagePair { key = key, message = message };
            messages.Add(newPair);
            messageDictionary[key] = message;
        }
    }
} 