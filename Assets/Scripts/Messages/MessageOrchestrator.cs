using UnityEngine;
using Game.Core; // PauseManager, MessageRuntimeSettingsProvider
using Game.Messages; // MessageDBSO, MessageType
using Game.UI; // LowerMessageController, OniricOverlayController
using Game.Localization; // LocalizationDBSO, LocaleSO

namespace Game.Messages
{
    /// <summary>
    /// Centralizes message dispatch for Lower (banner) and Oniric overlay.
    /// Applies gating (UI/Transition), localization for VO, defaults, and one-shot persistence.
    /// KISS: Inspector-first; consumers call ShowById/ShowLowerKey/ShowOniricKey.
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Spookie/Message Orchestrator")]
    public class MessageOrchestrator : MonoBehaviour
    {
    [Header("DB & Locale")]
    [SerializeField] private MessageDBSO messageDB;
    [SerializeField] private LocalizationDBSO localizationDB;
    [SerializeField] private LocaleSO locale;
    [SerializeField] private string fallbackLanguage = "en";

        [Header("Controllers")]
        [SerializeField] private LowerMessageController lower;
        [SerializeField] private OniricOverlayController oniric;
        [SerializeField] private PauseManager pauseManager;

    [Header("Debug")] [SerializeField] private bool showDebugLogs = false;

        // Unified persistence prefixes
        private const string PREF_LOWER_SEEN = "spk.lower.seen.";
        private const string PREF_ONIRIC_SEEN = "spk.oniric.seen.";
        // Legacy prefixes (migration-soft read)
        private const string LEGACY_LOWER = "Spookie.MsgSeen.";
        private const string LEGACY_ONIRIC = "Spookie.PO.";

        private void Awake()
        {
            if (localizationDB == null || locale == null || messageDB == null)
            {
                var provider = FindObjectOfType<GameConfigProvider>();
                var cfg = provider != null ? provider.Config : null;
                if (cfg != null)
                {
                    if (messageDB == null) messageDB = cfg.MessageDB;
                    if (localizationDB == null) localizationDB = cfg.LocalizationDB;
                    if (locale == null) locale = cfg.LocaleDefaults;
                }
            }
        }

    // Public API

        public void ShowById(string id)
        {
            if (string.IsNullOrWhiteSpace(id) || messageDB == null) return;

            var e = messageDB.Find(id);
            if (e == null)
            {
                LogWarn($"ID not found: {id}");
                return;
            }

            string lang = GetCurrentLanguage();
            string fb = string.IsNullOrEmpty(fallbackLanguage) ? "en" : fallbackLanguage;
            var audio = e.ResolveAudio(lang, fb);

            var settings = MessageRuntimeSettingsProvider.Get();
            float autoClose = (e.autoCloseSeconds >= 0f)
                ? e.autoCloseSeconds
                : (settings != null ? settings.defaultAutoCloseSeconds : -1f);
            float cps = (e.typewriterCps > 0f)
                ? e.typewriterCps
                : (settings != null ? settings.defaultTypewriterCps : -1f);

            string persistKey = string.IsNullOrEmpty(e.persistentIdOverride) ? id : e.persistentIdOverride;

            if (e.type == MessageType.Oniric)
            {
                if (IsBlockedForOniric()) return;
                if (e.oneShot && WasSeen(true, persistKey)) return;
                if (!string.IsNullOrEmpty(e.key))
                {
                    oniric?.ShowKey(e.key, audio, autoClose, null, false, cps);
                    if (e.oneShot) MarkSeen(true, persistKey);
                }
            }
            else // Lower
            {
                if (IsBlockedForLower()) return;
                if (e.oneShot && WasSeen(false, persistKey)) return;
                if (!string.IsNullOrEmpty(e.key))
                {
                    if (cps > 0f) lower?.EnqueueKey(e.key, audio, autoClose, e.repeatable, cps);
                    else lower?.EnqueueKey(e.key, audio, autoClose, e.repeatable);
                    if (e.oneShot) MarkSeen(false, persistKey);
                }
            }
        }

        public void ShowLowerKey(string key, AudioClip audio = null, float autoCloseOverride = -1f, bool repeatable = false, float cpsOverride = -1f)
        {
            if (string.IsNullOrEmpty(key)) return;
            if (IsBlockedForLower()) return;
            var settings = MessageRuntimeSettingsProvider.Get();
            float autoClose = (autoCloseOverride >= 0f) ? autoCloseOverride : (settings != null ? settings.defaultAutoCloseSeconds : -1f);
            float cps = (cpsOverride > 0f) ? cpsOverride : (settings != null ? settings.defaultTypewriterCps : -1f);
            if (cps > 0f) lower?.EnqueueKey(key, audio, autoClose, repeatable, cps);
            else lower?.EnqueueKey(key, audio, autoClose, repeatable);
        }

        public void ShowOniricKey(string key, AudioClip audio = null, float autoCloseOverride = -1f, string id = null, bool oneShot = true, float cpsOverride = -1f)
        {
            if (string.IsNullOrEmpty(key)) return;
            if (IsBlockedForOniric()) return;
            if (oneShot && !string.IsNullOrEmpty(id) && WasSeen(true, id)) return;
            var settings = MessageRuntimeSettingsProvider.Get();
            float autoClose = (autoCloseOverride >= 0f) ? autoCloseOverride : (settings != null ? settings.defaultAutoCloseSeconds : -1f);
            float cps = (cpsOverride > 0f) ? cpsOverride : (settings != null ? settings.defaultTypewriterCps : -1f);
            oniric?.ShowKey(key, audio, autoClose, null, false, cps); // orchestrator handles persistence
            if (oneShot && !string.IsNullOrEmpty(id)) MarkSeen(true, id);
        }

        // Convenience: show Oniric with literal text (no localization)
        public void ShowOniricText(string text, AudioClip audio = null, float autoCloseOverride = -1f, string id = null, bool oneShot = true, float cpsOverride = -1f)
        {
            if (string.IsNullOrEmpty(text)) return;
            if (IsBlockedForOniric()) return;
            if (oneShot && !string.IsNullOrEmpty(id) && WasSeen(true, id)) return;
            var settings = MessageRuntimeSettingsProvider.Get();
            float autoClose = (autoCloseOverride >= 0f) ? autoCloseOverride : (settings != null ? settings.defaultAutoCloseSeconds : -1f);
            float cps = (cpsOverride > 0f) ? cpsOverride : (settings != null ? settings.defaultTypewriterCps : -1f);
            oniric?.Show(text, audio, autoClose, null, false, cps);
            if (oneShot && !string.IsNullOrEmpty(id)) MarkSeen(true, id);
        }

    // Helpers

        private bool IsBlockedForLower()
        {
            if (pauseManager != null && pauseManager.IsPaused)
            {
                var reasons = pauseManager.GetActiveReasons();
                if (ContainsReason(reasons, PauseReason.Transition) || ContainsReason(reasons, PauseReason.UI))
                {
                    LogInfo("Lower gated by Transition/UI");
                    return true;
                }
            }
            return false;
        }

        private bool IsBlockedForOniric()
        {
            if (pauseManager != null && pauseManager.IsPaused)
            {
                var reasons = pauseManager.GetActiveReasons();
                if (ContainsReason(reasons, PauseReason.Transition) || ContainsReason(reasons, PauseReason.UI))
                {
                    LogInfo("Oniric gated by Transition/UI");
                    return true;
                }
            }
            // Avoid stacking multiple Onirics
            if (oniric != null && oniric.IsActive)
            {
                LogInfo("Oniric already active");
                return true;
            }
            return false;
        }

        private string GetCurrentLanguage()
        {
            if (locale != null) return locale.GetCurrentOrDefault();
            if (localizationDB != null && localizationDB.Languages.Count > 0) return localizationDB.Languages[0];
            return string.IsNullOrEmpty(fallbackLanguage) ? "en" : fallbackLanguage;
        }

        private bool WasSeen(bool oniricChannel, string id)
        {
            if (string.IsNullOrEmpty(id)) return false;
            string key = (oniricChannel ? PREF_ONIRIC_SEEN : PREF_LOWER_SEEN) + id;
            int seen = PlayerPrefs.GetInt(key, -1);
            if (seen == 1) return true;
            // Soft-migrate legacy keys (read-only)
            string legacy = (oniricChannel ? LEGACY_ONIRIC : LEGACY_LOWER) + id;
            return PlayerPrefs.GetInt(legacy, 0) == 1;
        }

        private void MarkSeen(bool oniricChannel, string id)
        {
            if (string.IsNullOrEmpty(id)) return;
            string key = (oniricChannel ? PREF_ONIRIC_SEEN : PREF_LOWER_SEEN) + id;
            PlayerPrefs.SetInt(key, 1);
            PlayerPrefs.Save();
        }

        private static bool ContainsReason(System.Collections.Generic.IReadOnlyCollection<PauseReason> set, PauseReason r)
        {
            if (set == null) return false; foreach (var it in set) { if (it == r) return true; } return false;
        }

        private void LogWarn(string msg)
        {
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (showDebugLogs) Debug.LogWarning($"[MessageOrchestrator] {msg}");
            #endif
        }

        private void LogInfo(string msg)
        {
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (showDebugLogs) Debug.Log($"[MessageOrchestrator] {msg}");
            #endif
        }
    }
}

// ScriptRole: Orquestador unificado para mensajes Lower y Oníricos (gating, defaults, VO, persistencia)
// RelatedScripts: Game.UI.LowerMessageController, Game.UI.OniricOverlayController, Game.Messages.MessageDBSO
// UsesSO: MessageDBSO, LocalizationDBSO, LocaleSO, MessageRuntimeSettingsSO (via Provider)
// ReceivesFrom: DBTextInteractable, guiones y triggers (ShowById/ShowLowerKey/ShowOniricKey)
// SendsTo: LowerMessageController.EnqueueKey, OniricOverlayController.ShowKey
