using UnityEngine;

public class MessageSetup : MonoBehaviour
{
    [Header("Message Configuration")]
    public bool autoSetup = true;
    public FeedbackMessagesSO feedbackMessages;
    
    [System.Serializable]
    public class MessageData
    {
        public string key;
        [TextArea(2, 5)]
        public string message;
    }
    
    [Header("Default Messages")]
    public MessageData[] defaultMessages = new MessageData[]
    {
        new MessageData { key = "go_to_bed_message", message = "Es hora de ir a la cama. Te sientes cansado..." },
        new MessageData { key = "sleeping_message", message = "Duermes profundamente..." },
        new MessageData { key = "sleep_pressure_message", message = "Te sientes cada vez más cansado. Necesitas descansar." },
        new MessageData { key = "sleep_prompt", message = "Presiona E para dormir" },
        new MessageData { key = "not_sleep_time_message", message = "No es hora de dormir aún" },
        new MessageData { key = "bed_available_message", message = "La cama está disponible" },
        new MessageData { key = "respawning_message", message = "Respawneando..." },
        new MessageData { key = "interaction_prompt", message = "Presiona E para interactuar" },
        new MessageData { key = "day_start_message", message = "Un nuevo día comienza" },
        new MessageData { key = "night_start_message", message = "La noche cae sobre el mundo" },
        new MessageData { key = "sleep_sound", message = "Sonido de dormir" },
        new MessageData { key = "wake_sound", message = "Sonido de despertar" },
        new MessageData { key = "footstep_sound", message = "Sonido de pasos" },
        new MessageData { key = "ambient_day", message = "Ambiente de día" },
        new MessageData { key = "ambient_night", message = "Ambiente de noche" },
        new MessageData { key = "cutscene_sleep", message = "Cutscene de dormir" }
    };
    
    private void Start()
    {
        if (autoSetup)
        {
            Debug.Log("MessageSetup: Starting automatic message setup");
            SetupMessages();
        }
    }
    
    [ContextMenu("Setup Messages")]
    public void SetupMessages()
    {
        if (feedbackMessages == null)
        {
            Debug.LogError("MessageSetup: No FeedbackMessagesSO assigned!");
            return;
        }
        
        Debug.Log("MessageSetup: Setting up messages");
        
        // Clear existing messages
        feedbackMessages.messages.Clear();
        
        // Add default messages
        foreach (MessageData messageData in defaultMessages)
        {
            FeedbackMessagesSO.MessageEntry entry = new FeedbackMessagesSO.MessageEntry();
            entry.key = messageData.key;
            entry.message = messageData.message;
            feedbackMessages.messages.Add(entry);
        }
        
        // Initialize dictionary
        feedbackMessages.InitializeDictionary();
        
        Debug.Log($"MessageSetup: Added {defaultMessages.Length} messages to FeedbackMessagesSO");
    }
    
    [ContextMenu("Add Custom Message")]
    public void AddCustomMessage(string key, string message)
    {
        if (feedbackMessages == null)
        {
            Debug.LogError("MessageSetup: No FeedbackMessagesSO assigned!");
            return;
        }
        
        FeedbackMessagesSO.MessageEntry entry = new FeedbackMessagesSO.MessageEntry();
        entry.key = key;
        entry.message = message;
        feedbackMessages.messages.Add(entry);
        
        // Reinitialize dictionary
        feedbackMessages.InitializeDictionary();
        
        Debug.Log($"MessageSetup: Added custom message '{key}'");
    }
    
    [ContextMenu("Test Messages")]
    public void TestMessages()
    {
        if (feedbackMessages == null)
        {
            Debug.LogError("MessageSetup: No FeedbackMessagesSO assigned!");
            return;
        }
        
        Debug.Log("MessageSetup: Testing messages...");
        
        foreach (MessageData messageData in defaultMessages)
        {
            string message = feedbackMessages.GetMessage(messageData.key);
            Debug.Log($"MessageSetup: '{messageData.key}' -> '{message}'");
        }
    }
}

// ScriptRole: Automatically sets up FeedbackMessagesSO with all necessary game messages
// Dependencies: FeedbackMessagesSO
// UsesSO: FeedbackMessagesSO
// ReceivesFrom: None
// SendsTo: None (setup only) 