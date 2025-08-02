using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SO
{
    /// <summary>
    /// Represents a single feedback message with its unique key and localized text.
    /// </summary>
    [Serializable]
    public class FeedbackMessage : IEquatable<FeedbackMessage>
    {
        [SerializeField] 
        [Tooltip("Unique identifier for this message")]
        private string key;

        [SerializeField]
        [TextArea(2, 4)]
        [Tooltip("The message text. Can include formatting placeholders like {0}, {1}, etc.")]
        private string message;

        public string Key => key?.Trim();
        public string Message => message ?? string.Empty;

        public FeedbackMessage() { }

        public FeedbackMessage(string key, string message)
        {
            this.key = key;
            this.message = message;
        }

        public bool Equals(FeedbackMessage other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(Key, other.Key, StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((FeedbackMessage)obj);
        }

        public override int GetHashCode()
        {
            return Key?.ToLowerInvariant().GetHashCode() ?? 0;
        }
    }

    /// <summary>
    /// Manages a collection of localized feedback messages for the game.
    /// </summary>
    [CreateAssetMenu(fileName = "FeedbackMessages", menuName = "Spookie/UI/Feedback Messages")]
    public class FeedbackMessagesSO : ScriptableObject
    {
        [SerializeField]
        [Tooltip("List of feedback messages. Keys must be unique.")]
        private List<FeedbackMessage> messages = new List<FeedbackMessage>();
        
        private Dictionary<string, string> messageCache;
        private readonly object cacheLock = new object();

        private void OnEnable()
        {
            ValidateMessages();
            BuildCache();
        }

        private void ValidateMessages()
        {
            if (messages == null)
            {
                messages = new List<FeedbackMessage>();
                return;
            }

            // Eliminar mensajes inválidos
            messages.RemoveAll(msg => string.IsNullOrWhiteSpace(msg?.Key));

            // Detectar y reportar duplicados
            var duplicates = messages
                .GroupBy(msg => msg.Key, StringComparer.OrdinalIgnoreCase)
                .Where(g => g.Count() > 1)
                .ToList();

            if (duplicates.Any())
            {
                var duplicateKeys = string.Join(", ", duplicates.Select(g => g.Key));
                Debug.LogError($"Duplicate message keys found in {name}: {duplicateKeys}", this);
                
                // Mantener solo la primera ocurrencia de cada clave duplicada
                var uniqueMessages = new HashSet<FeedbackMessage>(messages);
                messages = new List<FeedbackMessage>(uniqueMessages);
            }
        }

        private void BuildCache()
        {
            lock (cacheLock)
            {
                messageCache = new Dictionary<string, string>(
                    messages.ToDictionary(
                        msg => msg.Key.ToLowerInvariant(),
                        msg => msg.Message,
                        StringComparer.OrdinalIgnoreCase
                    )
                );
            }
        }

        /// <summary>
        /// Gets a message by its key. If the key is not found, returns a default message.
        /// </summary>
        /// <param name="key">The message key to look up</param>
        /// <param name="args">Optional format arguments for the message</param>
        /// <returns>The formatted message or a default message if key not found</returns>
        public string GetMessage(string key, params object[] args)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key), "Message key cannot be null or empty");

            lock (cacheLock)
            {
                if (messageCache == null) BuildCache();

                string normalizedKey = key.ToLowerInvariant();
                if (messageCache.TryGetValue(normalizedKey, out string message))
                {
                    try
                    {
                        return args?.Length > 0 ? string.Format(message, args) : message;
                    }
                    catch (FormatException ex)
                    {
                        Debug.LogError($"Format error in message '{key}': {ex.Message}", this);
                        return message; // Devolver el mensaje sin formato en caso de error
                    }
                }
            }
            
            Debug.LogWarning($"Message not found for key: '{key}'", this);
            return $"[Missing: {key}]";
        }

        /// <summary>
        /// Adds or updates a message at runtime. Changes persist until scene reload.
        /// </summary>
        public void SetMessage(string key, string message)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));
            if (string.IsNullOrEmpty(message))
                throw new ArgumentNullException(nameof(message));

            lock (cacheLock)
            {
                var normalizedKey = key.ToLowerInvariant();
                messageCache[normalizedKey] = message;

                // Actualizar también la lista serializada para el editor
                var existingMsg = messages.FirstOrDefault(m => 
                    string.Equals(m.Key, key, StringComparison.OrdinalIgnoreCase));
                
                if (existingMsg != null)
                {
                    messages[messages.IndexOf(existingMsg)] = new FeedbackMessage(key, message);
                }
                else
                {
                    messages.Add(new FeedbackMessage(key, message));
                }
            }
        }

    #if UNITY_EDITOR
        private void OnValidate()
        {
            ValidateMessages();
            if (Application.isPlaying)
            {
                BuildCache();
            }
        }
    #endif
    }
}
