using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Game.Core;

namespace Game.UI
{
    /// <summary>
    /// Pause Menu stub: toggles a pause panel and requests/releases pause with reason UI.
    /// KISS: Inspector-first. No Time.timeScale changes. Input via InputActionReference.
    /// </summary>
    public class PauseMenuController : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Pause manager handling gameplay pausing.")]
        [SerializeField] private PauseManager pauseManager;
        [Tooltip("Root GameObject of the pause UI panel (enable/disable).")]
        [SerializeField] private GameObject pausePanelRoot;

        [Header("Input (New Input System)")]
        [Tooltip("Action to toggle pause (e.g., ESC/Start). Assign from Input Actions asset.")]
        [SerializeField] private InputActionReference togglePauseAction;

        [Header("Cursor")]
        [Tooltip("If true, shows the cursor while the UI reason is active; hides it on resume.")]
        [SerializeField] private bool manageCursorWhilePaused = true;

        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;

        // Internal
        private bool _subscribed;

        private void Awake()
        {
            if (pausePanelRoot != null) pausePanelRoot.SetActive(false);
        }

        private void OnEnable()
        {
            if (togglePauseAction != null)
            {
                // Optional dev fallback: allow ESC to toggle if no action wired
                #if UNITY_EDITOR || DEVELOPMENT_BUILD
                if (togglePauseAction == null)
                {
                    if (UnityEngine.Input.GetKeyDown(KeyCode.Escape))
                    {
                        TogglePause();
                    }
                }
                #endif
                try
                {
                    togglePauseAction.action.started += OnTogglePauseStarted;
                    togglePauseAction.action.Enable();
                }
                catch {}
            }

            if (pauseManager != null && !_subscribed)
            {
                pauseManager.OnPauseChanged += OnPauseChanged;
                _subscribed = true;
            }
        }

        private void OnDisable()
        {
            if (togglePauseAction != null)
            {
                try
                {
                    togglePauseAction.action.started -= OnTogglePauseStarted;
                    togglePauseAction.action.Disable();
                }
                catch {}
            }

            if (pauseManager != null && _subscribed)
            {
                pauseManager.OnPauseChanged -= OnPauseChanged;
                _subscribed = false;
            }
        }

        private void OnTogglePauseStarted(InputAction.CallbackContext ctx)
        {
            TogglePause();
        }

        public void TogglePause()
        {
            if (pauseManager == null) return;

            // If UI reason is active, release; otherwise request
            bool uiActive = ContainsReason(pauseManager.GetActiveReasons(), PauseReason.UI);
            if (uiActive)
            {
                pauseManager.ReleasePause(PauseReason.UI);
            }
            else
            {
                pauseManager.RequestPause(PauseReason.UI);
            }
        }

        public void OpenPause()
        {
            if (pauseManager == null) return;
            pauseManager.RequestPause(PauseReason.UI);
        }

        public void ClosePause()
        {
            if (pauseManager == null) return;
            pauseManager.ReleasePause(PauseReason.UI);
        }

        private void OnPauseChanged(bool paused, System.Collections.Generic.IReadOnlyCollection<PauseReason> reasons)
        {
            // Only show the panel if the UI reason is active
            bool uiActive = ContainsReason(reasons, PauseReason.UI);
            if (pausePanelRoot != null)
            {
                pausePanelRoot.SetActive(uiActive);
            }

            if (manageCursorWhilePaused)
            {
                try
                {
                    Cursor.lockState = uiActive ? CursorLockMode.None : CursorLockMode.Locked;
                    Cursor.visible = uiActive;
                }
                catch {}
            }

            if (showDebugLogs)
            {
                #if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.Log($"[PauseMenuController] UI reason {(uiActive ? "active" : "inactive")} | paused:{paused} | total reasons:{reasons?.Count}");
                #endif
            }
        }

        private static bool ContainsReason(IReadOnlyCollection<PauseReason> set, PauseReason r)
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
// ScriptRole: Control mínimo del menú de pausa. Alterna panel UI y sincroniza con PauseManager (razón UI).
// RelatedScripts: Game.Core.PauseManager
// UsesSO: No
// ReceivesFrom: InputAction (toggle), botones UI
// SendsTo: PauseManager (Request/Release), GameObject Pause Panel (SetActive)
