using UnityEngine;
using TMPro;

namespace Game.Localization
{
    /// <summary>
    /// Simple localized text binder for TMP_Text. Pulls from LocalizationDBSO using a key and language code.
    /// Subscribes to a LanguageChanged event channel to update at runtime.
    /// </summary>
    [RequireComponent(typeof(TMP_Text))]
    [AddComponentMenu("Spookie/Localized Text")]
    public class LocalizedText : MonoBehaviour
    {
        [Header("DB & Key")]
        [SerializeField] private LocalizationDBSO localizationDB;
        [SerializeField] private string key;

        [Header("Language")]
        [Tooltip("Current language code, e.g., 'en', 'es'. If empty, uses defaultLanguage.")]
        [SerializeField] private string currentLanguage;
        [Tooltip("Fallback language if current not found")]
        [SerializeField] private string fallbackLanguage = "en";
        [Tooltip("Event Channel to auto-update when language changes")]
        [SerializeField] private LanguageChangedEventChannelSO languageChanged;

        [Header("Preview")]
        [SerializeField] private bool updateOnEnable = true;

        private TMP_Text _text;
        private bool _subscribed;

        private void Awake()
        {
            _text = GetComponent<TMP_Text>();
        }

        private void OnEnable()
        {
            if (languageChanged != null && !_subscribed)
            {
                languageChanged.OnLanguageChanged += OnLanguageChanged;
                _subscribed = true;
            }
            if (updateOnEnable) Refresh();
        }

        private void OnDisable()
        {
            if (languageChanged != null && _subscribed)
            {
                languageChanged.OnLanguageChanged -= OnLanguageChanged;
                _subscribed = false;
            }
        }

        private void OnLanguageChanged(string lang)
        {
            currentLanguage = lang;
            Refresh();
        }

        [ContextMenu("Refresh Now")]
        public void Refresh()
        {
            if (_text == null || localizationDB == null || string.IsNullOrEmpty(key)) return;
            string lang = string.IsNullOrEmpty(currentLanguage) && localizationDB.Languages.Count > 0
                ? localizationDB.Languages[0]
                : currentLanguage;
            _text.text = localizationDB.Get(key, lang, fallbackLanguage);
        }

        public void SetKey(string newKey)
        {
            key = newKey;
            Refresh();
        }

        public void SetLanguage(string lang)
        {
            currentLanguage = lang;
            Refresh();
        }

        private void OnValidate()
        {
            if (_text == null) _text = GetComponent<TMP_Text>();
        }
    }
}

// ScriptRole: Binder simple de localización para TMP_Text
// RelatedScripts: Game.Localization.LocalizationDBSO, LanguageChangedEventChannelSO
// UsesSO: LocalizationDBSO, LanguageChangedEventChannelSO
// ReceivesFrom: LanguageChangedEventChannelSO (Raise), Inspector
// SendsTo: TMP_Text (text)
// Adjuntar: UI con TMP_Text. Asignar DB, clave y canal de evento.
