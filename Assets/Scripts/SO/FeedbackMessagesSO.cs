using UnityEngine;
using System.Collections.Generic;

namespace DayNightSystem
{
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
        
        [Header("Messages")]
        [SerializeField] private List<MessageEntry> messages = new List<MessageEntry>();
        
        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;
        
        private void OnValidate()
        {
            // Ensure no duplicate keys
            var keys = new HashSet<string>();
            for (int i = 0; i < messages.Count; i++)
            {
                if (string.IsNullOrEmpty(messages[i].key))
                {
                    messages[i].key = $"message_{i}";
                }
                
                if (keys.Contains(messages[i].key))
                {
                    Debug.LogWarning($"[FeedbackMessagesSO] Duplicate key found: {messages[i].key}");
                }
                else
                {
                    keys.Add(messages[i].key);
                }
            }
        }
        
        public string GetMessage(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                Debug.LogWarning("[FeedbackMessagesSO] Key is null or empty");
                return "Message not found";
            }
            
            foreach (var entry in messages)
            {
                if (entry.key == key)
                {
                    return entry.message;
                }
            }
            
            // Key not found, return default message
            if (showDebugLogs)
                Debug.LogWarning($"[FeedbackMessagesSO] Key '{key}' not found, returning default message");
            
            return GetDefaultMessage(key);
        }
        
        private string GetDefaultMessage(string key)
        {
            switch (key)
            {
                // Time and status messages
                case "time_remaining":
                    return "Time Remaining: {0}";
                case "status_day":
                    return "Day - Explore and investigate";
                case "status_night":
                    return "Night - Return to spawn to rest";
                case "status_exhausted":
                    return "Exhausted - Return to spawn quickly!";
                case "status_sleeping":
                    return "Sleeping...";
                case "status_fainted":
                    return "Fainted - You will be penalized";
                case "status_transitioning":
                    return "Transitioning...";
                case "status_unknown":
                    return "";
                
                // Warning messages
                case "exhaustion_warning":
                    return "Warning: Exhaustion approaching!";
                case "warning_exhaustion_started":
                    return "Exhaustion started!";
                case "warning_fainted":
                    return "You fainted!";
                case "warning_night_blocked":
                    return "Night has fallen - movement restricted";
                case "exhaustion_active":
                    return "You are exhausted! Return to spawn!";
                case "new_day_started":
                    return "A new day has begun!";
                case "night_falling":
                    return "Night is falling...";
                
                // Penalty messages
                case "exhaustion_penalty":
                    return "You are exhausted! Speed reduced.";
                case "fainted_penalty":
                    return "You fainted! Severe speed penalty applied.";
                case "penalty_exhaustion":
                    return "Exhaustion - Movement reduced";
                case "penalty_fainted":
                    return "Fainted - Movement heavily reduced";
                case "penalty_unknown":
                    return "";
                
                // Sleep and faint messages
                case "slept_correctly":
                    return "You slept well and feel refreshed!";
                case "fainted":
                    return "You fainted from exhaustion!";
                
                // Spawn proximity messages
                case "safe_at_spawn":
                    return "You are safe at the spawn point";
                case "return_to_spawn":
                    return "Return to spawn point to rest";
                case "spawn_too_far":
                    return "Spawn point is too far away!";
                case "status_safe_area":
                    return "Safe area";
                case "status_warning_zone":
                    return "Warning zone";
                case "proximity_safe_night":
                    return "Safe at night: you can sleep";
                
                // Contextual messages
                case "day_exploration":
                    return "Day - Time to explore and investigate";
                case "night_rest":
                    return "Night - Time to rest at spawn";
                case "urgency_warning":
                    return "URGENT: Return to spawn immediately!";
                case "penalty_active":
                    return "Penalty active - Movement restricted";
                
                // Fainting timer messages
                case "fainting_timer":
                    return "You will faint in {0}s! Return to spawn!";
                case "fainting_imminent":
                    return "Fainting imminent! Return to spawn NOW!";
                case "fainting_prevented":
                    return "You returned to spawn just in time!";
                case "fainting_occurred":
                    return "You fainted from exhaustion!";
                
                // Night blocked messages
                case "night_blocked":
                    return "Night has fallen - you cannot move until dawn";
                case "night_message":
                    return "...a strange night passes...";
                case "night_restricted":
                    return "Movement restricted during night";
                
                // Distance and warning zone messages
                case "warning_zone_entered":
                    return "Warning: Getting far from spawn point";
                case "warning_zone_left":
                    return "You are closer to spawn now";
                case "distance_update":
                    return "Distance to spawn: {0}m";
                case "safe_rest_area":
                    return "Safe Rest Area";
                case "spawn_range":
                    return "Spawn Range";
                case "warning_zone":
                    return "Warning Zone";
                case "far_from_spawn":
                    return "Far from spawn";
                
                // Interaction messages
                case "interaction_press_e":
                    return "Press E to interact";
                
                default:
                    return $"Message for '{key}' not found";
            }
        }
        
        public void AddMessage(string key, string message)
        {
            // Check if key already exists
            for (int i = 0; i < messages.Count; i++)
            {
                if (messages[i].key == key)
                {
                    messages[i].message = message;
                    if (showDebugLogs)
                        Debug.Log($"[FeedbackMessagesSO] Updated message for key: {key}");
                    return;
                }
            }
            
            // Add new message
            messages.Add(new MessageEntry { key = key, message = message });
            
            if (showDebugLogs)
                Debug.Log($"[FeedbackMessagesSO] Added new message for key: {key}");
        }
        
        public void RemoveMessage(string key)
        {
            for (int i = 0; i < messages.Count; i++)
            {
                if (messages[i].key == key)
                {
                    messages.RemoveAt(i);
                    if (showDebugLogs)
                        Debug.Log($"[FeedbackMessagesSO] Removed message for key: {key}");
                    return;
                }
            }
        }
        
        public bool HasMessage(string key)
        {
            foreach (var entry in messages)
            {
                if (entry.key == key)
                    return true;
            }
            return false;
        }
        
        public List<string> GetAllKeys()
        {
            var keys = new List<string>();
            foreach (var entry in messages)
            {
                keys.Add(entry.key);
            }
            return keys;
        }
        
        // Helper method to add default messages if they don't exist
        public void AddDefaultMessages()
        {
            var defaultMessages = new Dictionary<string, string>
            {
                {"time_remaining", "Time Remaining: {0}"},
                {"status_day", "Day - Explore and investigate"},
                {"status_night", "Night - Return to spawn to rest"},
                {"status_exhausted", "Exhausted - Return to spawn quickly!"},
                {"status_sleeping", "Sleeping..."},
                {"status_fainted", "Fainted - You will be penalized"},
                {"exhaustion_warning", "Warning: Exhaustion approaching!"},
                {"exhaustion_active", "You are exhausted! Return to spawn!"},
                {"slept_correctly", "You slept well and feel refreshed!"},
                {"fainted", "You fainted from exhaustion!"},
                {"safe_at_spawn", "You are safe at the spawn point"},
                {"return_to_spawn", "Return to spawn point to rest"},
                {"spawn_too_far", "Spawn point is too far away!"},
                {"day_exploration", "Day - Time to explore and investigate"},
                {"night_rest", "Night - Time to rest at spawn"},
                {"urgency_warning", "URGENT: Return to spawn immediately!"},
                {"penalty_active", "Penalty active - Movement restricted"}
            };
            
            foreach (var kvp in defaultMessages)
            {
                if (!HasMessage(kvp.Key))
                {
                    AddMessage(kvp.Key, kvp.Value);
                }
            }
            
            if (showDebugLogs)
                Debug.Log("[FeedbackMessagesSO] Added default messages");
        }
    }
}

// ScriptRole: Centralized message management with contextual day/night messages
// RelatedScripts: DayNightUI, MessageSystem
// UsesSO: None
// ReceivesFrom: UI scripts
// SendsTo: UI scripts via GetMessage()
