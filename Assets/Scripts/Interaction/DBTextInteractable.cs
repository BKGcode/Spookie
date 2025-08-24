using UnityEngine;
using PlayerController; // IInteractable
using Game.Messages; // MessageOrchestrator
using Game.Save; // Save progress

namespace Game.Interaction
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Spookie/DB Text Interactable (Unified)")]
    public class DBTextInteractable : MonoBehaviour, IInteractable
    {
    public enum FireMode { Interaction, Trigger }

        [Header("Message")]
        [Tooltip("Message id to resolve in MessageDB.")]
        [SerializeField] private string messageId;

        // Integration simplified: use orchestrator
        [Header("Integration")]
        [SerializeField] private MessageOrchestrator orchestrator;

        [Header("Rules")]
        [SerializeField] private FireMode fireMode = FireMode.Interaction;
        [SerializeField] private string requiredTag = "Player";
        [Tooltip("Cooldown seconds between fires (0 = none, <0 = use global settings if available)")]
        [SerializeField] private float cooldownSeconds = -1f;

        [Header("Debug")] [SerializeField] private bool showDebugLogs = false;

        // State
        private float _lastFireTime = -999f;
        private OutlineHighlighter _highlighter;

        public string MessageId { get => messageId; set => messageId = value; }

        private void Reset()
        {
            if (orchestrator == null) orchestrator = FindObjectOfType<MessageOrchestrator>();
            // Ensure collider for raycast/trigger
            var col = GetComponent<Collider>();
            if (col == null) col = gameObject.AddComponent<BoxCollider>();
            col.isTrigger = (fireMode == FireMode.Trigger);
        }

        private void OnValidate()
        {
            if (orchestrator == null) orchestrator = FindObjectOfType<MessageOrchestrator>();
            var col = GetComponent<Collider>();
            if (col != null) col.isTrigger = (fireMode == FireMode.Trigger);
        }

        private void Awake()
        {
            _highlighter = GetComponent<OutlineHighlighter>();
        }

        private void OnDisable()
        {
            SetHighlighted(false);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (fireMode != FireMode.Trigger) return;
            if (!string.IsNullOrEmpty(requiredTag) && !other.CompareTag(requiredTag)) return;
            TryFire();
        }

        public void Interact(GameObject interactor)
        {
            if (fireMode != FireMode.Interaction) { TryFire(); return; }
            TryFire();
        }

        public string GetInteractionPrompt()
        {
            return string.IsNullOrWhiteSpace(messageId) ? "Interact" : "Leer";
        }

        public void SetHighlighted(bool enabled)
        {
            if (_highlighter != null) _highlighter.SetHighlighted(enabled);
        }

        private void TryFire()
        {
            if (orchestrator == null)
            {
                orchestrator = FindObjectOfType<MessageOrchestrator>();
                if (orchestrator == null) { Warn("No MessageOrchestrator found in scene"); return; }
            }
            if (string.IsNullOrWhiteSpace(messageId)) return;

            // Gate en Orchestrator; aquí solo cooldown local mínimo si aplica
            float cd = cooldownSeconds;
            if (cd > 0f && Time.time - _lastFireTime < cd) return;

            orchestrator.ShowById(messageId);
            // Marcar mensaje visto si tiene ID persistente
            var eid = GetComponent<SaveEntityID>();
            if (eid != null && !string.IsNullOrWhiteSpace(eid.id))
            {
                SaveParticipantMessages.MarkSeenStatic(eid.id);
            }
            _lastFireTime = Time.time;
        }

        private void Warn(string msg)
        {
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (showDebugLogs) Debug.LogWarning($"[DBTextInteractable] {msg}");
            #endif
        }
    }
}

// ScriptRole: Interactuable de texto que dispara mensajes por ID a través del Orchestrator
// RelatedScripts: PlayerController.PlayerInteraction, Game.Messages.MessageOrchestrator
// UsesSO: No
// ReceivesFrom: PlayerInteraction.Interact o Trigger
// SendsTo: MessageOrchestrator.ShowById
// Adjuntar a: Objetos investigables. Asignar Orchestrator (o deja auto-find). Collider requerido.
