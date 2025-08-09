using UnityEngine;
using Game.Core;
using PlayerController;
using System;

namespace Game.UI
{
    /// <summary>
    /// Fires a lower message when the player enters a trigger or interacts with the object.
    /// KISS: Inspector-first. Supports one-shot or cooldown. Respects PauseManager (won't fire while paused).
    /// Note: For OnTriggerEnter to work with CharacterController players, put a kinematic Rigidbody on this trigger object.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    [Obsolete("Deprecated: usa Game.Interaction.TextMessageInteractable para mensajes inferiores (por interacción por defecto y trigger opcional).")]
    public class MessageTrigger : MonoBehaviour, PlayerController.IInteractable
    {
        public enum TriggerMode { OnTriggerEnter, OnInteract }

        [Header("Mode")]
        [SerializeField] private TriggerMode mode = TriggerMode.OnTriggerEnter;

        [Header("Message")]
        [Tooltip("Controller that shows lower messages.")]
        [SerializeField] private Game.UI.LowerMessageController lowerMessageController;
        [Tooltip("Text to display when triggered.")]
        [TextArea(2, 5)]
        [SerializeField] private string messageText;
        [Tooltip("Optional audio clip to play with the message.")]
        [SerializeField] private AudioClip audioClip;
        [Tooltip("Override auto-close seconds. <0 uses controller default.")]
        [SerializeField] private float autoCloseOverride = -1f;

        [Header("Rules")]
        [Tooltip("If true, triggers only once.")]
        [SerializeField] private bool oneShot = true;
        [Tooltip("Cooldown in seconds before this trigger can fire again (only when oneShot=false).")]
        [Min(0f)]
        [SerializeField] private float cooldownSeconds = 3f;
        [Tooltip("If true, ignore trigger requests while any pause reason is active.")]
        [SerializeField] private bool blockWhenPaused = true;

        [Header("Integration (optional)")]
        [Tooltip("Pause manager to check paused state (recommended).")]
        [SerializeField] private PauseManager pauseManager;

        [Header("Filter")]
        [Tooltip("Optional: require the other collider to have this tag. Leave empty to accept player by component.")]
        [SerializeField] private string requiredTag = "";

        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;

        private bool _firedOnce;
        private float _lastFireTime = -99999f;

        private void Reset()
        {
            // Ensure trigger collider by default
            var col = GetComponent<Collider>();
            col.isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (mode != TriggerMode.OnTriggerEnter) return;
            if (!PassesFilter(other)) return;
            TryFire();
        }

        public void Interact(GameObject interactor)
        {
            if (mode != TriggerMode.OnInteract) return;
            if (!PassesInteractor(interactor)) return;
            TryFire();
        }

        public string GetInteractionPrompt()
        {
            return string.IsNullOrEmpty(messageText) ? "Interact" : "Interact"; // simple prompt for now
        }

        private bool PassesFilter(Collider other)
        {
            if (!string.IsNullOrEmpty(requiredTag) && !other.CompareTag(requiredTag)) return false;
            // Accept if the other or its parents contain PlayerMovement
            if (other.GetComponentInParent<PlayerMovement>() != null) return true;
            // Or if explicitly tagged Player by convention
            if (string.IsNullOrEmpty(requiredTag) && other.CompareTag("Player")) return true;
            return false;
        }

        private bool PassesInteractor(GameObject go)
        {
            if (go == null) return false;
            if (!string.IsNullOrEmpty(requiredTag) && !go.CompareTag(requiredTag)) return false;
            if (go.GetComponentInParent<PlayerMovement>() != null) return true;
            if (string.IsNullOrEmpty(requiredTag) && go.CompareTag("Player")) return true;
            return false;
        }

        private void TryFire()
        {
            if (lowerMessageController == null)
            {
                #if UNITY_EDITOR || DEVELOPMENT_BUILD
                if (showDebugLogs) Debug.LogWarning($"[MessageTrigger] No LowerMessageController assigned on {name}");
                #endif
                return;
            }
            if (blockWhenPaused && pauseManager != null && pauseManager.IsPaused)
            {
                if (showDebugLogs)
                {
                    #if UNITY_EDITOR || DEVELOPMENT_BUILD
                    Debug.Log($"[MessageTrigger] Blocked while paused ({name})");
                    #endif
                }
                return;
            }
            if (oneShot && _firedOnce)
            {
                return;
            }
            if (!oneShot && (Time.time - _lastFireTime) < Mathf.Max(0f, cooldownSeconds))
            {
                return;
            }

            _firedOnce = true;
            _lastFireTime = Time.time;
            lowerMessageController.EnqueueText(messageText, audioClip, autoCloseOverride);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.3f, 0.8f, 1f, 0.4f);
            var col = GetComponent<Collider>();
            if (col != null)
            {
                var bounds = col.bounds;
                Gizmos.DrawWireCube(bounds.center, bounds.size);
            }
        }
    }
}

// ScriptRole | RelatedScripts | UsesSO | ReceivesFrom | SendsTo
// ScriptRole: Dispara un mensaje inferior por trigger o interacción, con one-shot/cooldown.
// RelatedScripts: Game.UI.LowerMessageController, PlayerController.PlayerInteraction
// UsesSO: No
// ReceivesFrom: OnTriggerEnter, IInteractable.Interact
// SendsTo: LowerMessageController.EnqueueText
