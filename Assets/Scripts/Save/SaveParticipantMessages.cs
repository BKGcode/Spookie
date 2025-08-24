using System.Collections.Generic;
using UnityEngine;

namespace Game.Save
{
    /// <summary>
    /// Participante que persiste IDs de mensajes/interacciones vistas.
    /// KISS: se guarda como lista única sin duplicados. HashSet interno para lookups rápidos.
    /// </summary>
    [AddComponentMenu("Spookie/Save/Participant Messages")]
    public class SaveParticipantMessages : MonoBehaviour, ISaveParticipant
    {
        private static SaveParticipantMessages _instance;
        public static SaveParticipantMessages Instance => _instance;

        private readonly HashSet<string> _seen = new HashSet<string>();
        [SerializeField] private bool showDebugLogs = false;

        private void Awake()
        {
            if (_instance != null && _instance != this) { Destroy(gameObject); return; }
            _instance = this;
        }

        private void OnEnable()
        {
            SaveGameManager.Instance?.Register(this);
        }

        private void OnDisable()
        {
            SaveGameManager.Instance?.Unregister(this);
        }

        public static void MarkSeenStatic(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return;
            if (_instance == null) return; // aún no iniciado
            if (_instance._seen.Add(id))
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                if (_instance.showDebugLogs) Debug.Log($"[SaveParticipantMessages] Seen {id}");
#endif
            }
        }

        public void WriteTo(SaveData data)
        {
            if (data == null) return;
            data.seenMessageIds = new List<string>(_seen);
        }

        public void ReadFrom(SaveData data)
        {
            _seen.Clear();
            if (data?.seenMessageIds != null)
            {
                for (int i = 0; i < data.seenMessageIds.Count; i++)
                {
                    var id = data.seenMessageIds[i];
                    if (!string.IsNullOrWhiteSpace(id)) _seen.Add(id);
                }
            }
        }
    }
}

// ScriptRole: Persistencia de mensajes/interacciones vistas.
// RelatedScripts: DBTextInteractable, SaveGameManager
// UsesSO: No
// ReceivesFrom: DBTextInteractable (MarkSeenStatic)
// SendsTo: SaveData (seenMessageIds)
// Adjuntar: Un único GameObject persistente (_Systems) o en escena actual.