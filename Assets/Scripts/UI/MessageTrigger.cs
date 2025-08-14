using UnityEngine;
using Game.Messages;

namespace Game.UI
{
    /// <summary>
    /// Simple trigger to enqueue a LowerMessage by localization key. One-shot runtime with optional PlayerPrefs persistence and cooldown.
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Spookie/Message Trigger (Unified)")]
    public class MessageTrigger : MonoBehaviour
    {
        [SerializeField] private string messageId;
        [SerializeField] private bool oneShot = false;
        [SerializeField] private string requiredTag = "Player";

        [Header("Integration")]
        [SerializeField] private MessageOrchestrator orchestrator;

        private bool _fired;

        private void Reset()
        {
            orchestrator = FindObjectOfType<MessageOrchestrator>();
        }

        private void OnValidate()
        {
            if (orchestrator == null) orchestrator = FindObjectOfType<MessageOrchestrator>();
        }

        private void Awake()
        {
            if (orchestrator == null) orchestrator = FindObjectOfType<MessageOrchestrator>();
        }

    // Disparo explícito o por trigger. Sin auto OnStart/OnEnable.

        private void OnTriggerEnter(Collider other)
        {
            if (!string.IsNullOrEmpty(requiredTag) && !other.CompareTag(requiredTag)) return;
            Fire();
        }

        public void Fire()
        {
            if (_fired && oneShot) return;
            if (orchestrator == null)
            {
                orchestrator = FindObjectOfType<MessageOrchestrator>();
                if (orchestrator == null) return;
            }
            if (string.IsNullOrWhiteSpace(messageId)) return;
            orchestrator.ShowById(messageId);
            _fired = true;
        }
    }
}

// ScriptRole: Disparador simple por ID que delega en MessageOrchestrator
// RelatedScripts: Game.Messages.MessageOrchestrator, Game.UI.LowerMessageController, Game.UI.OniricOverlayController
// UsesSO: No directo (usa DB vía Orchestrator)
// ReceivesFrom: Start/OnEnable/OnTriggerEnter/UnityEvent
// SendsTo: MessageOrchestrator.ShowById

