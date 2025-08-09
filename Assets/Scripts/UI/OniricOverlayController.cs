using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using Game.Core;

namespace Game.UI
{
    /// <summary>
    /// Fullscreen "Oniric Thought" overlay: typewriter text + optional audio, blocks gameplay via PauseManager (Oniric reason).
    /// KISS: single active instance, no queue. One-shot persistence by ID (PlayerPrefs) if desired.
    /// </summary>
    [DisallowMultipleComponent]
    public class OniricOverlayController : MonoBehaviour
    {
        [Header("UI")]
        [Tooltip("Root object of the fullscreen overlay (enable/disable). Should cover the screen with black background.")]
        [SerializeField] private GameObject overlayRoot;
        [Tooltip("TMP text field for the oniric content.")]
        [SerializeField] private TMP_Text oniricText;
        [Tooltip("Optional CanvasGroup to drive alpha if needed (not used in this slice).")]
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Audio (optional)")]
        [Tooltip("AudioSource for VO/SFX. If null, one will be added at runtime.")]
        [SerializeField] private AudioSource audioSource;

        [Header("Input (New Input System)")]
        [Tooltip("Action to close after typewriter+audio (e.g., Interact or ESC).")]
        [SerializeField] private InputActionReference closeAction;

        [Header("Behavior")]
        [Tooltip("Characters per second for typewriter.")]
        [SerializeField, Min(0.01f)] private float charsPerSecond = 25f;
        [Tooltip("Seconds to wait after completion before auto-close (<=0 to require input).")]
        [SerializeField] private float autoCloseSeconds = 0f;
        [Tooltip("If true, block closing before typewriter+audio completion.")]
        [SerializeField] private bool blockCloseUntilComplete = true;

        [Header("Integration")]
        [SerializeField] private PauseManager pauseManager;

        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;

        // State
        private bool _active;
        private bool _pausedExternally;
        private bool _closePressed;
        private Coroutine _runner;

        private const string PP_OniricSeenPrefix = "Spookie.PO.";

        private void Awake()
        {
            if (overlayRoot != null) overlayRoot.SetActive(false);
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
            }
        }

        private void OnEnable()
        {
            if (closeAction != null)
            {
                try
                {
                    closeAction.action.started += OnCloseStarted;
                    closeAction.action.Enable();
                }
                catch {}
            }
            if (pauseManager != null)
            {
                pauseManager.OnPauseChanged += OnPauseChanged;
            }
        }

        private void OnDisable()
        {
            if (closeAction != null)
            {
                try
                {
                    closeAction.action.started -= OnCloseStarted;
                    closeAction.action.Disable();
                }
                catch {}
            }
            if (pauseManager != null)
            {
                pauseManager.OnPauseChanged -= OnPauseChanged;
            }

            // Safety: if we get disabled mid-overlay, ensure we don't leave gameplay paused
            if (_active && pauseManager != null)
            {
                try { pauseManager.ReleasePause(PauseReason.Oniric); } catch { }
            }
        }

        private void OnCloseStarted(InputAction.CallbackContext ctx)
        {
            _closePressed = true;
        }

        private void OnPauseChanged(bool paused, System.Collections.Generic.IReadOnlyCollection<PauseReason> reasons)
        {
            _pausedExternally = paused; // UI/menu may pause over us; just pause our progression/audio
            if (audioSource != null)
            {
                if (paused && audioSource.isPlaying) audioSource.Pause();
                else if (!paused && audioSource.clip != null && audioSource.time > 0f && !audioSource.isPlaying) audioSource.UnPause();
            }
        }

        // Public API --------------------------------------------------------

        public bool IsActive => _active;

        /// <summary>
        /// Show an Oniric Thought. If id is provided and oneShot=true and was seen, it will ignore.
        /// </summary>
        public void Show(string text, AudioClip audio = null, float autoCloseOverride = -1f, string id = null, bool oneShot = true)
        {
            if (_active)
            {
                #if UNITY_EDITOR || DEVELOPMENT_BUILD
                if (showDebugLogs) Debug.LogWarning("[OniricOverlay] Already active. Ignored new request.");
                #endif
                return;
            }
            if (string.IsNullOrEmpty(text)) return;

            if (!string.IsNullOrEmpty(id) && oneShot && PlayerPrefs.GetInt(PP_OniricSeenPrefix + id, 0) == 1)
            {
                if (showDebugLogs)
                {
                    #if UNITY_EDITOR || DEVELOPMENT_BUILD
                    Debug.Log($"[OniricOverlay] Skipping one-shot id '{id}' (seen).");
                    #endif
                }
                return;
            }

            if (_runner != null) StopCoroutine(_runner);
            _runner = StartCoroutine(Run(text, audio, autoCloseOverride, id, oneShot));
        }

        // Internal ----------------------------------------------------------

        private IEnumerator Run(string full, AudioClip audio, float autoCloseOverride, string id, bool oneShot)
        {
            _active = true;
            _closePressed = false;

            if (overlayRoot != null) overlayRoot.SetActive(true);

            // Pause gameplay with Oniric reason
            if (pauseManager != null) pauseManager.RequestPause(PauseReason.Oniric);

            // Start audio
            if (audioSource != null)
            {
                audioSource.Stop();
                audioSource.clip = audio;
                if (audio != null) audioSource.Play();
            }

            // Typewriter
            oniricText.text = string.Empty;
            int total = full.Length;
            float cps = Mathf.Max(0.01f, charsPerSecond);
            float acc = 0f;
            int shown = 0;
            while (shown < total)
            {
                while (_pausedExternally) { yield return null; }
                acc += Time.unscaledDeltaTime * cps; // use unscaled so our own reason doesn't freeze us
                int toShow = Mathf.Min(total, Mathf.FloorToInt(acc));
                if (toShow != shown)
                {
                    oniricText.text = full.Substring(0, toShow);
                    shown = toShow;
                }
                yield return null;
            }
            oniricText.text = full;

            // Wait for audio to finish if present
            if (audioSource != null && audioSource.clip != null)
            {
                while (_pausedExternally) { yield return null; }
                while (audioSource.isPlaying)
                {
                    while (_pausedExternally) { yield return null; }
                    yield return null;
                }
            }

            // After completion, allow close (by input or timeout)
            float timeout = autoCloseOverride >= 0f ? autoCloseOverride : Mathf.Max(0f, autoCloseSeconds);
            _closePressed = false; // require fresh press

            if (!blockCloseUntilComplete)
            {
                // If not blocking until complete, we already reached here, so treat as allowed now.
            }

            bool closedByInput = false;
            if (timeout <= 0f)
            {
                // Wait for input only
                while (true)
                {
                    while (_pausedExternally) { yield return null; }
                    if (_closePressed) { closedByInput = true; break; }
                    yield return null;
                }
            }
            else
            {
                float t = 0f;
                while (t < timeout)
                {
                    while (_pausedExternally) { yield return null; }
                    if (_closePressed) { closedByInput = true; break; }
                    t += Time.unscaledDeltaTime;
                    yield return null;
                }
            }

            // Mark as seen
            if (!string.IsNullOrEmpty(id) && oneShot)
            {
                PlayerPrefs.SetInt(PP_OniricSeenPrefix + id, 1);
                PlayerPrefs.Save();
            }

            // Cleanup and resume gameplay
            if (audioSource != null)
            {
                audioSource.Stop();
                audioSource.clip = null;
            }
            if (overlayRoot != null) overlayRoot.SetActive(false);
            oniricText.text = string.Empty;

            if (pauseManager != null) pauseManager.ReleasePause(PauseReason.Oniric);

            _active = false;
            _runner = null;

            if (showDebugLogs)
            {
                #if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.Log($"[OniricOverlay] Closed by {(closedByInput ? "input" : "timeout")}.");
                #endif
            }
        }
    }
}

// ScriptRole | RelatedScripts | UsesSO | ReceivesFrom | SendsTo
// ScriptRole: Overlay de Pensamientos Oníricos. Bloquea juego (PauseReason.Oniric), typewriter + VO, cierre por input/timeout.
// RelatedScripts: Game.Core.PauseManager
// UsesSO: No (por ahora)
// ReceivesFrom: Triggers/guión (Show)
// SendsTo: TMP_Text, AudioSource, PauseManager (request/release)
