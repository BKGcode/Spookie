using UnityEngine;
using Game.Core; // GameConfigProvider
using Game.Localization; // LocaleSO, LanguageChangedEventChannelSO, LocalizationDBSO

namespace Game.Localization
{
    /// <summary>
    /// Runtime owner of current language. Initializes from LocaleSO (defaults) and PlayerPrefs,
    /// emits LanguageChangedEventChannel on start and when SetLanguage is called. Does not mutate the LocaleSO asset.
    /// KISS: no UI here; wire buttons to SetLanguage(lang) or call from a settings menu.
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Spookie/Locale Runtime")]
    public class LocaleRuntime : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Locale defaults (do NOT mutate). If left empty, will try to read from GameConfigProvider.")]
        [SerializeField] private LocaleSO localeDefaults;
        [Tooltip("Event channel to notify language changes to LocalizedText and UI controllers.")]
        [SerializeField] private LanguageChangedEventChannelSO languageChanged;
        [Tooltip("Optional Localization DB to validate language codes (first language used as fallback).")]
        [SerializeField] private LocalizationDBSO localizationDB;

        [Header("Persistence")]
        [Tooltip("PlayerPrefs key to store the last selected language code.")]
        [SerializeField] private string prefsKey = "Spookie.Language";

        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;

        public string CurrentLanguage { get; private set; }

        private void Awake()
        {
            // Auto-wire from GameConfigProvider if left unassigned
            if (localeDefaults == null || languageChanged == null || localizationDB == null)
            {
                var provider = FindObjectOfType<GameConfigProvider>();
                var cfg = provider != null ? provider.Config : null;
                if (cfg != null)
                {
                    if (localeDefaults == null) localeDefaults = cfg.LocaleDefaults;
                    if (languageChanged == null) languageChanged = cfg.LanguageChangedEvent;
                    if (localizationDB == null) localizationDB = cfg.LocalizationDB;
                }
            }

            // Resolve startup language: PlayerPrefs -> LocaleSO default -> DB first -> "en"
            string lang = null;
            try { lang = PlayerPrefs.GetString(prefsKey, null); } catch {}
            if (string.IsNullOrEmpty(lang) && localeDefaults != null) lang = localeDefaults.GetCurrentOrDefault();
            if (string.IsNullOrEmpty(lang) && localizationDB != null && localizationDB.Languages.Count > 0) lang = localizationDB.Languages[0];
            if (string.IsNullOrEmpty(lang)) lang = "en";

            SetLanguageInternal(lang, persist:false, raise:true);
        }

        public void SetLanguage(string languageCode)
        {
            if (string.IsNullOrWhiteSpace(languageCode)) return;
            SetLanguageInternal(languageCode.Trim(), persist:true, raise:true);
        }

        private void SetLanguageInternal(string lang, bool persist, bool raise)
        {
            // Optionally validate against DB languages
            if (localizationDB != null && localizationDB.Languages != null && localizationDB.Languages.Count > 0)
            {
                bool exists = false;
                for (int i = 0; i < localizationDB.Languages.Count; i++)
                {
                    var l = localizationDB.Languages[i];
                    if (!string.IsNullOrEmpty(l) && string.Equals(l, lang, System.StringComparison.OrdinalIgnoreCase))
                    {
                        exists = true; break;
                    }
                }
                if (!exists)
                {
                    // fallback to first language in DB
                    lang = localizationDB.Languages[0];
                }
            }

            CurrentLanguage = lang;
            if (persist)
            {
                try { PlayerPrefs.SetString(prefsKey, lang); PlayerPrefs.Save(); } catch {}
            }

            if (raise && languageChanged != null)
            {
                try { languageChanged.Raise(lang); } catch {}
            }

            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (showDebugLogs) Debug.Log($"[LocaleRuntime] Language set to '{CurrentLanguage}'");
            #endif
        }

        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(prefsKey)) prefsKey = "Spookie.Language";
        }
    }
}

// ScriptRole | RelatedScripts | UsesSO | ReceivesFrom | SendsTo
// ScriptRole: Dueño del idioma en runtime; persiste en PlayerPrefs y emite el evento de cambio.
// RelatedScripts: Game.Config.GameConfigSO (via Provider), LocalizedText, LowerMessageController, OniricOverlayController.
// UsesSO: LocaleSO (solo lectura), LanguageChangedEventChannelSO, LocalizationDBSO (validación opcional)
// ReceivesFrom: UI de Settings (SetLanguage), PlayerPrefs
// SendsTo: LanguageChangedEventChannelSO (Raise)
// Adjuntar a: GameObject raíz "Systems" o "Config". Asignar referencias o dejar que las obtenga del Provider.
