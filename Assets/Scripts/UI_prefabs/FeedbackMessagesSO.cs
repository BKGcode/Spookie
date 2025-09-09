
using UnityEngine;
using System.Collections.Generic;

namespace Brainamics.UI.Prototypes
{
    [System.Serializable]
    public struct MessageEntry
    {
        [Tooltip("Clave única para identificar este mensaje.")]
        public string Key;
        [Tooltip("Texto del mensaje."), TextArea]
        public string Message;
    }

    [CreateAssetMenu(fileName = "FeedbackMessages", menuName = "Brainamics/UI Prototypes/Feedback Messages")]
    public class FeedbackMessagesSO : ScriptableObject
    {
        [Header("Configuración de Mensajes")]
        [Tooltip("Lista de todos los mensajes de la UI.")]
        [SerializeField] private List<MessageEntry> messages = new List<MessageEntry>();

        private readonly Dictionary<string, string> _messageMap = new Dictionary<string, string>();

        private void OnEnable()
        {
            _messageMap.Clear();
            foreach (var entry in messages)
            {
                if (!string.IsNullOrEmpty(entry.Key) && !_messageMap.ContainsKey(entry.Key))
                {
                    _messageMap.Add(entry.Key, entry.Message);
                }
            }
        }

        public string GetMessage(string key, string fallback = "")
        {
            if (string.IsNullOrEmpty(key) || !_messageMap.ContainsKey(key))
            {
                Debug.LogWarning($"[FeedbackMessagesSO] Clave no encontrada: '{key}'. Usando fallback.");
                return fallback;
            }
            return _messageMap[key];
        }
    }

    // ScriptRole | RelatedScripts | UsesSO | ReceivesFrom | SendsTo
    // Role: Base de datos centralizada para textos de la UI.
    // Related: Cualquier script que muestre texto (CustomButton, ModalWindow).
    // UsesSO: N/A
    // ReceivesFrom: N/A
    // SendsTo: Provee textos a quien lo solicite vía GetMessage.
}
