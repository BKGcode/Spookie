using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using Game.Core;
using Game.Localization; // LocalizationDBSO, LocaleSO
using Game.Core; // GameConfigProvider

namespace Game.UI
{
    /// <summary>
    /// Lower screen message banner with a small queue, typewriter animation, and audio gating.
    /// KISS: Inspector-first, no localization in this slice. Pauses via PauseManager reasons (UI/Oniric/etc.).
    /// Priority policy (gating): UI > Transition > Oniric > Banner. Banner should reject while Transition is active.
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Spookie/Lower Message Controller")]
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
    [Tooltip("Optional: Action to instantly reveal all text (does not close)")]
    [SerializeField] private InputActionReference revealAllAction;

        [Header("Behavior")]
        [Tooltip("Characters per second for the typewriter.")]
        [SerializeField, Min(0.01f)] private float charsPerSecond = 30f;
        [Tooltip("Seconds to wait after animation+audio complete before auto-close.")]
        [SerializeField, Min(0f)] private float autoCloseSeconds = 2.0f;
        [Tooltip("Maximum queue length (older items will be dropped if exceeded).")]
    [SerializeField, Min(1)] private int maxQueue = 2;

    [Header("Visual (Fade)")]
    [Tooltip("Fade-in seconds for banner (requires CanvasGroup). 0 = instant.")]
    [SerializeField, Min(0f)] private float fadeInSeconds = 0.15f;
    [Tooltip("Fade-out seconds for banner (requires CanvasGroup). 0 = instant.")]
    [SerializeField, Min(0f)] private float fadeOutSeconds = 0.15f;

    [Header("Cooldown")]
    [Tooltip("Global cooldown to accept new (non-repeatable) messages after a banner closes.")]
    [SerializeField, Min(0f)] private float globalCooldownSeconds = 3f;
    [Tooltip("Default repeatable flag for messages enqueued via the 3-arg API.")]
    [SerializeField] private bool repeatableByDefault = false;

        [Header("Integration")]
        [Tooltip("Optional PauseManager to pause messages when game/UI/Oniric is paused.")]
        [SerializeField] private PauseManager pauseManager;
    [Header("Localization (optional)")]
    [Tooltip("Localization DB to resolve keys into texts. If null, key enqueue is ignored.")]
    [SerializeField] private LocalizationDBSO localizationDB;
    [Tooltip("Locale provider for current language code. If null, uses DB first language or fallbackLanguage.")]
    [SerializeField] private LocaleSO locale;
    [Tooltip("Fallback language code when not found.")]
    [SerializeField] private string fallbackLanguage = "en";

        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;

        private readonly Queue<MessageItem> _queue = new Queue<MessageItem>();
        private Coroutine _runner;
        private bool _pausedExternally;
        private bool _advancePressed;
    private bool _revealPressed;
        private bool _subscribed;
    private float _lastCloseTime = -999f;

        private struct MessageItem
        {
            public string Text;
            public string Key;
            public AudioClip Audio;
            public float AutoCloseOverride; // < 0 => use default
            public bool Repeatable; // used only for enqueue rules
            public bool IsKey;
            public float CpsOverride; // <= 0 => use controller default
        }

        private void Awake()
        {
            if (bannerRoot != null) bannerRoot.SetActive(false);
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
            }

            // Optional: auto-wire from GameConfigProvider if left unassigned
            if (localizationDB == null || locale == null)
            {
                var provider = FindObjectOfType<GameConfigProvider>();
                var cfg = provider != null ? provider.Config : null;
                if (cfg != null)
                {
                    if (localizationDB == null) localizationDB = cfg.LocalizationDB;
                    if (locale == null) locale = cfg.LocaleDefaults;
                }
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
            if (revealAllAction != null)
            {
                try
                {
                    revealAllAction.action.started += OnRevealStarted;
                    revealAllAction.action.Enable();
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
            if (revealAllAction != null)
            {
                try
                {
                    revealAllAction.action.started -= OnRevealStarted;
                    revealAllAction.action.Disable();
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

        private void OnRevealStarted(InputAction.CallbackContext ctx)
        {
            _revealPressed = true;
        }

        private void OnPauseChanged(bool paused, System.Collections.Generic.IReadOnlyCollection<PauseReason> reasons)
        {
            _pausedExternally = paused; // Pause by any reason pauses messages
            if (audioSource != null)
            {
                if (paused && audioSource.isPlaying) audioSource.Pause();
                else if (!paused && audioSource.clip != null && audioSource.time > 0f && !audioSource.isPlaying) audioSource.UnPause();
            }
            // If we just resumed and there's pending items but no runner, try to run now
            if (!paused && _runner == null && _queue.Count > 0)
            {
                TryRun();
            }
        }

        // Public API --------------------------------------------------------

        public void EnqueueText(string text, AudioClip audio = null, float autoCloseOverride = -1f)
        {
            // Backwards-compatible API: uses default repeatable flag
            EnqueueText(text, audio, autoCloseOverride, repeatableByDefault);
        }

        // New overload with repeatable flag
        public void EnqueueText(string text, AudioClip audio, float autoCloseOverride, bool repeatable)
        {
            if (string.IsNullOrEmpty(text)) return;
            // Gate during Transition to enforce priority
            if (pauseManager != null && pauseManager.IsPaused)
            {
                var reasons = pauseManager.GetActiveReasons();
                if (ContainsReason(reasons, PauseReason.Transition))
                {
                    #if UNITY_EDITOR || DEVELOPMENT_BUILD
                    if (showDebugLogs) Debug.Log($"[LowerMessage] Gated by Transition. Rejected: '{text}'");
                    #endif
                    return;
                }
            }
            // Global cooldown gate for non-repeatable messages
            if (!repeatable && (Time.time - _lastCloseTime) < Mathf.Max(0f, globalCooldownSeconds))
            {
                #if UNITY_EDITOR || DEVELOPMENT_BUILD
                if (showDebugLogs) Debug.Log($"[LowerMessage] Rejected by cooldown ({globalCooldownSeconds:F2}s): '{text}'");
                #endif
                return;
            }
            if (_queue.Count >= Mathf.Max(1, maxQueue))
            {
                // Drop oldest to keep things moving
                _queue.Dequeue();
            }
            _queue.Enqueue(new MessageItem { Text = text, Key = null, Audio = audio, AutoCloseOverride = autoCloseOverride, Repeatable = repeatable, IsKey = false, CpsOverride = -1f });
            TryRun();
        }

        // Overload with per-message typewriter cps override
        public void EnqueueText(string text, AudioClip audio, float autoCloseOverride, bool repeatable, float cpsOverride)
        {
            if (string.IsNullOrEmpty(text)) return;
            if (pauseManager != null && pauseManager.IsPaused)
            {
                var reasons = pauseManager.GetActiveReasons();
                if (ContainsReason(reasons, PauseReason.Transition))
                {
                    #if UNITY_EDITOR || DEVELOPMENT_BUILD
                    if (showDebugLogs) Debug.Log($"[LowerMessage] Gated by Transition. Rejected: '{text}'");
                    #endif
                    return;
                }
            }
            if (!repeatable && (Time.time - _lastCloseTime) < Mathf.Max(0f, globalCooldownSeconds))
            {
                #if UNITY_EDITOR || DEVELOPMENT_BUILD
                if (showDebugLogs) Debug.Log($"[LowerMessage] Rejected by cooldown ({globalCooldownSeconds:F2}s): '{text}'");
                #endif
                return;
            }
            if (_queue.Count >= Mathf.Max(1, maxQueue))
            {
                _queue.Dequeue();
            }
            _queue.Enqueue(new MessageItem { Text = text, Key = null, Audio = audio, AutoCloseOverride = autoCloseOverride, Repeatable = repeatable, IsKey = false, CpsOverride = cpsOverride });
            TryRun();
        }

        // New: enqueue a localization key (text is resolved just-in-time at display)
        public void EnqueueKey(string key, AudioClip audio = null, float autoCloseOverride = -1f, bool repeatable = false)
        {
            if (string.IsNullOrEmpty(key)) return;
            if (pauseManager != null && pauseManager.IsPaused)
            {
                var reasons = pauseManager.GetActiveReasons();
                if (ContainsReason(reasons, PauseReason.Transition))
                {
                    #if UNITY_EDITOR || DEVELOPMENT_BUILD
                    if (showDebugLogs) Debug.Log($"[LowerMessage] Gated by Transition. Rejected key: '{key}'");
                    #endif
                    return;
                }
            }
            // Global cooldown gate for non-repeatable messages
            if (!repeatable && (Time.time - _lastCloseTime) < Mathf.Max(0f, globalCooldownSeconds))
            {
                #if UNITY_EDITOR || DEVELOPMENT_BUILD
                if (showDebugLogs) Debug.Log($"[LowerMessage] Rejected by cooldown (key='{key}')");
                #endif
                return;
            }
            if (_queue.Count >= Mathf.Max(1, maxQueue))
            {
                _queue.Dequeue();
            }
            _queue.Enqueue(new MessageItem { Text = null, Key = key, Audio = audio, AutoCloseOverride = autoCloseOverride, Repeatable = repeatable, IsKey = true, CpsOverride = -1f });
            TryRun();
        }

        // Overload with per-message typewriter cps override for keys
        public void EnqueueKey(string key, AudioClip audio, float autoCloseOverride, bool repeatable, float cpsOverride)
        {
            if (string.IsNullOrEmpty(key)) return;
            if (pauseManager != null && pauseManager.IsPaused)
            {
                var reasons = pauseManager.GetActiveReasons();
                if (ContainsReason(reasons, PauseReason.Transition))
                {
                    #if UNITY_EDITOR || DEVELOPMENT_BUILD
                    if (showDebugLogs) Debug.Log($"[LowerMessage] Gated by Transition. Rejected key: '{key}'");
                    #endif
                    return;
                }
            }
            if (!repeatable && (Time.time - _lastCloseTime) < Mathf.Max(0f, globalCooldownSeconds))
            {
                #if UNITY_EDITOR || DEVELOPMENT_BUILD
                if (showDebugLogs) Debug.Log($"[LowerMessage] Rejected by cooldown (key='{key}')");
                #endif
                return;
            }
            if (_queue.Count >= Mathf.Max(1, maxQueue))
            {
                _queue.Dequeue();
            }
            _queue.Enqueue(new MessageItem { Text = null, Key = key, Audio = audio, AutoCloseOverride = autoCloseOverride, Repeatable = repeatable, IsKey = true, CpsOverride = cpsOverride });
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
            // Don't start while paused; keep items enqueued (priority gate)
            if (_pausedExternally) return;
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

            // Resolve text first; if empty, do nothing to avoid leaving UI/audio in inconsistent state
            string full = item.IsKey ? ResolveKey(item.Key) : item.Text;
            if (string.IsNullOrEmpty(full)) yield break;

            if (bannerRoot != null)
            {
                // Prepare fade-in
                if (canvasGroup != null) canvasGroup.alpha = 0f;
                bannerRoot.SetActive(true);
                // Run fade-in if configured
                if (canvasGroup != null && fadeInSeconds > 0f)
                {
                    yield return FadeCanvas(0f, 1f, fadeInSeconds);
                }
                else if (canvasGroup != null)
                {
                    canvasGroup.alpha = 1f;
                }
            }

            // Start audio (if any) after confirming we have text to display
            if (audioSource != null)
            {
                audioSource.Stop();
                audioSource.clip = item.Audio;
                if (item.Audio != null) audioSource.Play();
            }

            // Typewriter
            messageText.text = string.Empty;
            int total = full.Length;
            float cps = (item.CpsOverride > 0f) ? item.CpsOverride : Mathf.Max(0.01f, charsPerSecond);
            float acc = 0f;
            int shown = 0;
            while (shown < total)
            {
                // Pause gating
                while (_pausedExternally) { yield return null; }

                acc += Time.deltaTime * cps;
                int toShow = Mathf.Min(total, Mathf.FloorToInt(acc));
                if (_revealPressed)
                {
                    toShow = total;
                    _revealPressed = false; // consume reveal
                }
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
            _revealPressed = false;  // reset
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

            // Fade-out then hide
            if (bannerRoot != null)
            {
                if (canvasGroup != null && fadeOutSeconds > 0f)
                {
                    yield return FadeCanvas(canvasGroup != null ? canvasGroup.alpha : 1f, 0f, fadeOutSeconds);
                }
                if (canvasGroup != null) canvasGroup.alpha = 0f;
                bannerRoot.SetActive(false);
            }
            messageText.text = string.Empty;
            if (audioSource != null)
            {
                audioSource.Stop();
                audioSource.clip = null;
            }

            // Mark last close for cooldown budget
            _lastCloseTime = Time.time;
        }

        private string ResolveKey(string key)
        {
            if (string.IsNullOrEmpty(key)) return string.Empty;
            if (localizationDB == null)
            {
                #if UNITY_EDITOR || DEVELOPMENT_BUILD
                if (showDebugLogs) Debug.LogWarning($"[LowerMessage] No LocalizationDB set; key '{key}' ignored");
                #endif
                return string.Empty;
            }
            string lang = locale != null ? locale.GetCurrentOrDefault() : (localizationDB.Languages.Count > 0 ? localizationDB.Languages[0] : fallbackLanguage);
            return localizationDB.Get(key, lang, fallbackLanguage);
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

        private IEnumerator FadeCanvas(float from, float to, float duration)
        {
            if (canvasGroup == null || duration <= 0f)
            {
                if (canvasGroup != null) canvasGroup.alpha = to;
                yield break;
            }
            canvasGroup.alpha = from;
            float t = 0f;
            while (t < duration)
            {
                while (_pausedExternally) { yield return null; }
                t += Time.deltaTime;
                float k = Mathf.Clamp01(t / duration);
                canvasGroup.alpha = Mathf.Lerp(from, to, k);
                yield return null;
            }
            canvasGroup.alpha = to;
        }
    }
}

// ScriptRole | RelatedScripts | UsesSO | ReceivesFrom | SendsTo
// ScriptRole: Banner de mensajes inferior con cola, typewriter y gating por audio; pausable por PauseManager.
// RelatedScripts: Game.Core.PauseManager, PlayerController.PlayerInteraction (input)
// UsesSO: No (por ahora)
// ReceivesFrom: Triggers/otros scripts (EnqueueText), InputActionReference (advance)
// SendsTo: TMP_Text, AudioSource, GameObject (UI)
