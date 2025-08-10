using UnityEngine;
using Game.Messages;
using Game.Localization;
using Game.Core; // GameConfigProvider

namespace Game.UI
{
    /// <summary>
    /// Utility to enqueue messages by ID from a MessageDBSO into LowerMessageController or OniricOverlayController.
    /// Inspector-first: wire DB and controllers, then call EnqueueById(id).
    /// </summary>
    [AddComponentMenu("Spookie/Message DB Enqueuer")]
    public class MessageDBEnqueuer : MonoBehaviour
    {
        [Header("DB & Locale")]
        [SerializeField] private MessageDBSO db;
        [SerializeField] private LocalizationDBSO localizationDB;
        [SerializeField] private LocaleSO locale;
        [SerializeField] private string fallbackLanguage = "en";

        [Header("Controllers")]
        [SerializeField] private LowerMessageController lower;
        [SerializeField] private OniricOverlayController oniric;

        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;

        public void EnqueueById(string id)
        {
            // Optional: auto-wire from GameConfigProvider if left unassigned
            if ((db == null) || (localizationDB == null) || (locale == null))
            {
                var provider = FindObjectOfType<GameConfigProvider>();
                var cfg = provider != null ? provider.Config : null;
                if (cfg != null)
                {
                    if (db == null) db = cfg.MessageDB;
                    if (localizationDB == null) localizationDB = cfg.LocalizationDB;
                    if (locale == null) locale = cfg.LocaleDefaults;
                }
            }

            if (db == null || string.IsNullOrEmpty(id)) return;
            var e = db.Find(id);
            if (e == null) { LogWarn($"ID not found: {id}"); return; }

            // Resolve language for VO (if any)
            string lang = locale != null ? locale.GetCurrentOrDefault() : (localizationDB != null && localizationDB.Languages.Count > 0 ? localizationDB.Languages[0] : fallbackLanguage);
            string fb = string.IsNullOrEmpty(fallbackLanguage) ? "en" : fallbackLanguage;
            var audio = e.ResolveAudio(lang, fb);

            switch (e.type)
            {
                case MessageType.Lower:
                    if (lower == null) { LogWarn("Lower controller not assigned"); return; }
                    if (!string.IsNullOrEmpty(e.key))
                    {
                        lower.EnqueueKey(e.key, audio, e.autoCloseSeconds, e.repeatable, e.typewriterCps);
                    }
                    else
                    {
                        // Fallback: no key -> nothing to show
                        LogWarn($"Lower entry '{id}' has no key");
                    }
                    break;
                case MessageType.Oniric:
                    if (oniric == null) { LogWarn("Oniric controller not assigned"); return; }
                    if (!string.IsNullOrEmpty(e.key))
                    {
                        oniric.ShowKey(e.key, audio, e.autoCloseSeconds, string.IsNullOrEmpty(e.persistentIdOverride)? id : e.persistentIdOverride, e.oneShot, e.typewriterCps);
                    }
                    else
                    {
                        LogWarn($"Oniric entry '{id}' has no key");
                    }
                    break;
            }
        }

        private void LogWarn(string msg)
        {
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (showDebugLogs) Debug.LogWarning($"[MessageDBEnqueuer] {msg}");
            #endif
        }
    }
}

// ScriptRole: Encolador por ID desde MessageDBSO hacia Lower/Oniric
// RelatedScripts: Game.Messages.MessageDBSO, Game.UI.LowerMessageController, Game.UI.OniricOverlayController
// UsesSO: MessageDBSO, LocalizationDBSO (indirecto vía controllers)
// ReceivesFrom: Triggers/UI/guion (EnqueueById)
// SendsTo: LowerMessageController.EnqueueKey, OniricOverlayController.ShowKey
