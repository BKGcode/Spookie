using UnityEngine;
using PlayerController; // IInteractable
using Game.UI; // LowerMessageController
using Game.Core; // PauseManager, PauseReason

namespace Game.Interaction
{
    /// <summary>
    /// Interactable that shows a lower-band message when the player interacts. Optional trigger mode via checkbox.
    /// KISS: one component to drop on objects; requires OutlineHighlighter for visual feedback (PlayerInteraction toggles it).
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(OutlineHighlighter))]
    [AddComponentMenu("Spookie/Text Message Interactable")]
    public class TextMessageInteractable : MonoBehaviour, IInteractable
    {
        public enum FireMode { Interaction, Trigger }

        [Header("Message Target")]
        [Tooltip("LowerMessageController that will display the text.")]
        [SerializeField] private LowerMessageController lowerMessage;

        [Header("Content")]
        [TextArea(2, 6)]
        [SerializeField] private string messageText;
        [SerializeField] private AudioClip messageAudio;
        [Tooltip("Override seconds to autoclose (<=0 means require input). -1 to use controller default.")]
        [SerializeField] private float autoCloseSecondsOverride = -1f;

        [Header("Rules")]
        [Tooltip("Default is Interaction. Enable Trigger to auto-fire on player entry.")]
        [SerializeField] private FireMode fireMode = FireMode.Interaction;
        [Tooltip("Only relevant in Trigger mode: which tag can trigger it.")]
        [SerializeField] private string requiredTag = "Player";
        [Tooltip("If true, only fires once per play session.")]
        [SerializeField] private bool oneShotRuntime = false;
        [Tooltip("Optional persistence key. If set and One Shot is true, it won't fire again across sessions.")]
        [SerializeField] private string persistentId;
        [Tooltip("Cooldown in seconds between fires (0 = no cooldown). Applies to interaction and trigger.")]
        [SerializeField] private float cooldownSeconds = 0f;
        [Tooltip("If true, will not fire when game is paused by PauseManager (safer UX).")]
        [SerializeField] private bool blockWhenPaused = true;
    [Tooltip("If true, when paused by Oniric the message will be enqueued as 'repeatable' to bypass the banner global cooldown.")]
    [SerializeField] private bool forceRepeatableWhenOniric = true;

    [Header("Integration")]
    [Tooltip("Optional PauseManager to block firing while paused.")]
    [SerializeField] private Game.Core.PauseManager pauseManager;

        [Header("Optional: Collider for Trigger mode")]
        [Tooltip("Assign/ensure a Collider with isTrigger = true when Fire Mode = Trigger.")]
        [SerializeField] private Collider triggerCollider;

        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = false;

        // State
        private bool _firedRuntime;
        private float _lastFireTime = -999f;

        private const string PP_Prefix = "Spookie.MsgSeen.";

        private void Reset()
        {
            // Try to auto-wire a trigger collider if present
            if (triggerCollider == null)
            {
                TryGetComponent(out triggerCollider);
            }
            if (triggerCollider == null)
            {
                triggerCollider = GetComponentInChildren<Collider>();
            }
            // If still none, add a simple BoxCollider so PlayerInteraction raycast can hit it
            if (triggerCollider == null)
            {
                var box = gameObject.AddComponent<BoxCollider>();
                box.isTrigger = false;
                triggerCollider = box;
            }
            // Default to Interaction mode collider config
            if (triggerCollider != null) triggerCollider.isTrigger = (fireMode == FireMode.Trigger);
        }

        private void OnValidate()
        {
            if (lowerMessage == null)
            {
                lowerMessage = FindObjectOfType<LowerMessageController>();
            }
            if (pauseManager == null)
            {
                pauseManager = FindObjectOfType<Game.Core.PauseManager>();
            }
            // Ensure there's a collider and its isTrigger matches the selected mode
            if (triggerCollider == null)
            {
                TryGetComponent(out triggerCollider);
                if (triggerCollider == null) triggerCollider = GetComponentInChildren<Collider>();
            }
            if (triggerCollider == null)
            {
                var box = GetComponent<BoxCollider>();
                if (box == null) box = gameObject.AddComponent<BoxCollider>();
                triggerCollider = box;
            }
            if (triggerCollider != null)
            {
                bool wantTrigger = (fireMode == FireMode.Trigger);
                if (triggerCollider.isTrigger != wantTrigger)
                {
                    triggerCollider.isTrigger = wantTrigger;
                }
            }
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
            return string.IsNullOrWhiteSpace(messageText) ? "Interact" : "Leer";
        }

        private void TryFire()
        {
            if (lowerMessage == null) return;
            if (string.IsNullOrWhiteSpace(messageText)) return;

            // Pause-aware gating: allow enqueue during Oniric (will queue and run on resume),
            // gate during UI/Transition to avoid spurious triggers under menus/loads.
            if (blockWhenPaused && pauseManager != null && pauseManager.IsPaused)
            {
                var reasons = pauseManager.GetActiveReasons();
                bool hasUI = ContainsReason(reasons, PauseReason.UI);
                bool hasTransition = ContainsReason(reasons, PauseReason.Transition);
                bool hasOniric = ContainsReason(reasons, PauseReason.Oniric);

                if (hasUI || hasTransition)
                {
                    if (showDebugLogs)
                    {
                        #if UNITY_EDITOR || DEVELOPMENT_BUILD
                        Debug.Log($"[TextMessageInteractable] Gated by {(hasTransition ? "Transition" : "UI")} on {name}");
                        #endif
                    }
                    return;
                }
                // If only Oniric (or other non-UI/Transition reasons), allow enqueue
                if (hasOniric)
                {
                    #if UNITY_EDITOR || DEVELOPMENT_BUILD
                    if (showDebugLogs) Debug.Log($"[TextMessageInteractable] Enqueue during Oniric on {name} (will show after)");
                    #endif
                }
            }

            // One-shot (runtime and/or persistent)
            if (oneShotRuntime && _firedRuntime) return;
            if (!string.IsNullOrEmpty(persistentId))
            {
                if (PlayerPrefs.GetInt(PP_Prefix + persistentId, 0) == 1) return;
            }

            // Cooldown
            if (cooldownSeconds > 0f && Time.time - _lastFireTime < cooldownSeconds) return;

            bool repeatable = false;
            if (pauseManager != null && pauseManager.IsPaused && forceRepeatableWhenOniric)
            {
                var reasons = pauseManager.GetActiveReasons();
                if (ContainsReason(reasons, PauseReason.Oniric)) repeatable = true;
            }
            lowerMessage.EnqueueText(
                messageText,
                messageAudio,
                autoCloseSecondsOverride >= 0f ? autoCloseSecondsOverride : -1f,
                repeatable
            );

            _firedRuntime = true;
            _lastFireTime = Time.time;
            if (!string.IsNullOrEmpty(persistentId))
            {
                PlayerPrefs.SetInt(PP_Prefix + persistentId, 1);
                PlayerPrefs.Save();
            }
        }
        private static bool ContainsReason(System.Collections.Generic.IReadOnlyCollection<PauseReason> set, PauseReason r)
        {
            if (set == null) return false;
            foreach (var it in set)
            {
                if (it == r) return true;
            }
            return false;
        }
    }
}

// ScriptRole | RelatedScripts | UsesSO | ReceivesFrom | SendsTo
// ScriptRole: Interactuable para lanzar mensajes inferiores por interacción (por defecto) o por trigger (opcional).
// RelatedScripts: PlayerController.PlayerInteraction, Game.UI.LowerMessageController, Game.Interaction.OutlineHighlighter
// UsesSO: No
// ReceivesFrom: PlayerInteraction.Interact o Collider trigger
// SendsTo: LowerMessageController.EnqueueText
// Adjuntar a: Objeto interactuable. Requiere OutlineHighlighter. Si Trigger: añadir Collider isTrigger=true. Asignar LowerMessageController en escena.
