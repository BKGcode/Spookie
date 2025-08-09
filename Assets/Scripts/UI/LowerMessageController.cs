using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using Game.Core;

namespace Game.UI
{
    /// <summary>
    /// Lower screen message banner with a small queue, typewriter animation, and audio gating.
    /// KISS: Inspector-first, no localization in this slice. Pauses via PauseManager reasons (UI/Oniric/etc.).
    /// </summary>
    [DisallowMultipleComponent]
    public class LowerMessageController : MonoBehaviour
    {
        [Header("UI")]
        [Tooltip("Root container of the lower message banner (enable/disable).")]
        [SerializeField] private GameObject bannerRoot;
        [Tooltip("CanvasGroup for simple fades (optional). If null, no fade is applied.")]
        [SerializeField] private CanvasGroup canvasGroup;
        [Tooltip("TMP text field to display the message.")]
        [SerializeField] private TMP_Text messageText;

        [Header("Audio (optional)")]
        [Tooltip("AudioSource dedicated to message VO/SFX. If null, one will be added at runtime.")]
        [SerializeField] private AudioSource audioSource;

        [Header("Input (New Input System)")]
        [Tooltip("Action to advance/close message after animation+audio (e.g., Interact).")]
        [SerializeField] private InputActionReference advanceAction;

        [Header("Behavior")]
        [Tooltip("Characters per second for the typewriter.")]
        [SerializeField, Min(0.01f)] private float charsPerSecond = 30f;
        [Tooltip("Seconds to wait after animation+audio complete before auto-close.")]
        [SerializeField, Min(0f)] private float autoCloseSeconds = 2.0f;
        [Tooltip("Maximum queue length (older items will be dropped if exceeded).")]
        [SerializeField, Min(1)] private int maxQueue = 4;

        [Header("Integration")]
        [Tooltip("Optional PauseManager to pause messages when game/UI/Oniric is paused.")]
        [SerializeField] private PauseManager pauseManager;

        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;

        private readonly Queue<MessageItem> _queue = new Queue<MessageItem>();
        private Coroutine _runner;
        private bool _pausedExternally;
        private bool _advancePressed;
        private bool _subscribed;

        private struct MessageItem
        {
            public string Text;
            public AudioClip Audio;
            public float AutoCloseOverride; // < 0 => use default
        }

        private void Awake()
        {
            if (bannerRoot != null) bannerRoot.SetActive(false);
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
            }
        }

        private void OnEnable()
        {
            if (advanceAction != null)
            {
                try
                {
                    advanceAction.action.started += OnAdvanceStarted;
                    advanceAction.action.Enable();
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
            if (advanceAction != null)
            {
                try
                {
                    advanceAction.action.started -= OnAdvanceStarted;
                    advanceAction.action.Disable();
                }
                catch {}
            }

            if (pauseManager != null && _subscribed)
            {
                pauseManager.OnPauseChanged -= OnPauseChanged;
                _subscribed = false;
            }
        }

        private void OnAdvanceStarted(InputAction.CallbackContext ctx)
        {
            _advancePressed = true;
        }

        private void OnPauseChanged(bool paused, System.Collections.Generic.IReadOnlyCollection<PauseReason> reasons)
        {
            _pausedExternally = paused; // Pause by any reason pauses messages
            if (audioSource != null)
            {
                if (paused && audioSource.isPlaying) audioSource.Pause();
                else if (!paused && audioSource.clip != null && audioSource.time > 0f && !audioSource.isPlaying) audioSource.UnPause();
            }
        }

        // Public API --------------------------------------------------------

        public void EnqueueText(string text, AudioClip audio = null, float autoCloseOverride = -1f)
        {
            if (string.IsNullOrEmpty(text)) return;
            if (_queue.Count >= Mathf.Max(1, maxQueue))
            {
                // Drop oldest to keep things moving
                _queue.Dequeue();
            }
            _queue.Enqueue(new MessageItem { Text = text, Audio = audio, AutoCloseOverride = autoCloseOverride });
            TryRun();
        }

        public void ClearQueue()
        {
            _queue.Clear();
        }

        public bool IsBusy => _runner != null || (_queue.Count > 0);

        // Internal ----------------------------------------------------------

        private void TryRun()
        {
            if (_runner == null && _queue.Count > 0)
            {
                _runner = StartCoroutine(RunQueue());
            }
        }

        private IEnumerator RunQueue()
        {
            while (_queue.Count > 0)
            {
                var item = _queue.Dequeue();
                yield return ShowOne(item);
            }
            _runner = null;
            yield break;
        }

        private IEnumerator ShowOne(MessageItem item)
        {
            _advancePressed = false;
            if (bannerRoot != null) bannerRoot.SetActive(true);
            if (canvasGroup != null) canvasGroup.alpha = 1f;

            // Start audio (if any) immediately so the banner lifetime matches VO length
            if (audioSource != null)
            {
                audioSource.Stop();
                audioSource.clip = item.Audio;
                if (item.Audio != null) audioSource.Play();
            }

            // Typewriter
            messageText.text = string.Empty;
            var full = item.Text;
            int total = full.Length;
            float cps = Mathf.Max(0.01f, charsPerSecond);
            float acc = 0f;
            int shown = 0;
            while (shown < total)
            {
                // Pause gating
                while (_pausedExternally) { yield return null; }

                acc += Time.deltaTime * cps;
                int toShow = Mathf.Min(total, Mathf.FloorToInt(acc));
                if (toShow != shown)
                {
                    messageText.text = full.Substring(0, toShow);
                    shown = toShow;
                }
                yield return null;
            }
            // Ensure full text
            messageText.text = full;

            // Wait until audio (if any) finishes before allowing close
            if (audioSource != null && audioSource.clip != null)
            {
                while (_pausedExternally) { yield return null; }
                while (audioSource.isPlaying)
                {
                    while (_pausedExternally) { yield return null; }
                    yield return null;
                }
            }

            // Allow close: either by input or by timeout
            float timeout = item.AutoCloseOverride >= 0f ? item.AutoCloseOverride : Mathf.Max(0f, autoCloseSeconds);
            float t = 0f;
            _advancePressed = false; // require a press after completion
            bool closedByInput = false;
            while (t < timeout)
            {
                while (_pausedExternally) { yield return null; }
                if (_advancePressed)
                {
                    closedByInput = true;
                    break;
                }
                t += Time.deltaTime;
                yield return null;
            }

            if (showDebugLogs)
            {
                #if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.Log($"[LowerMessage] Done. Closed by {(closedByInput ? "input" : "timeout")}");
                #endif
            }

            // Hide banner (no fancy fade in this slice)
            if (bannerRoot != null) bannerRoot.SetActive(false);
            messageText.text = string.Empty;
            if (audioSource != null)
            {
                audioSource.Stop();
                audioSource.clip = null;
            }
        }
    }
}

// ScriptRole | RelatedScripts | UsesSO | ReceivesFrom | SendsTo
// ScriptRole: Banner de mensajes inferior con cola, typewriter y gating por audio; pausable por PauseManager.
// RelatedScripts: Game.Core.PauseManager, PlayerController.PlayerInteraction (input)
// UsesSO: No (por ahora)
// ReceivesFrom: Triggers/otros scripts (EnqueueText), InputActionReference (advance)
// SendsTo: TMP_Text, AudioSource, GameObject (UI)
