using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Localization
{
    [CreateAssetMenu(fileName = "LocalizationDB", menuName = "Spookie/Localization/DB")]
    public class LocalizationDBSO : ScriptableObject
    {
        [Serializable]
        public class Entry
        {
            [Tooltip("Unique key, e.g., 'ui.play', 'msg.welcome'")]
            public string key;
            [Tooltip("Per-language texts for this key (language code -> text)")]
            public List<LocalizedValue> values = new List<LocalizedValue>();
        }

        [Serializable]
        public class LocalizedValue
        {
            [Tooltip("Language code, e.g., 'en', 'es'")]
            public string language;
            [TextArea(1, 6)] public string text;
        }

        [Header("Languages")]
        [Tooltip("Declared languages (codes) available in this DB. First is default.")]
        [SerializeField] private List<string> languages = new List<string> { "en", "es" };

        [Header("Entries")]
        [SerializeField] private List<Entry> entries = new List<Entry>();

        public IReadOnlyList<string> Languages => languages;

        public string Get(string key, string language, string fallbackLanguage = null)
        {
            if (string.IsNullOrEmpty(key)) return string.Empty;
            // Find entry
            for (int i = 0; i < entries.Count; i++)
            {
                var e = entries[i];
                if (!string.Equals(e.key, key, StringComparison.Ordinal)) continue;
                // Try preferred language
                string t = FindIn(e, language);
                if (!string.IsNullOrEmpty(t)) return t;
                // Try fallback
                if (!string.IsNullOrEmpty(fallbackLanguage))
                {
                    t = FindIn(e, fallbackLanguage);
                    if (!string.IsNullOrEmpty(t)) return t;
                }
                // Try first available
                if (e.values != null && e.values.Count > 0) return e.values[0].text ?? string.Empty;
                return string.Empty;
            }
            return string.Empty;
        }

        private string FindIn(Entry e, string language)
        {
            if (e == null || e.values == null || string.IsNullOrEmpty(language)) return null;
            for (int j = 0; j < e.values.Count; j++)
            {
                var v = e.values[j];
                if (string.Equals(v.language, language, StringComparison.OrdinalIgnoreCase)) return v.text;
            }
            return null;
        }

        private void OnValidate()
        {
            // Light dedupe/trim
            for (int i = entries.Count - 1; i >= 0; i--)
            {
                if (entries[i] == null) { entries.RemoveAt(i); continue; }
                if (entries[i].values == null) continue;
                // Remove nulls and trim languages
                for (int j = entries[i].values.Count - 1; j >= 0; j--)
                {
                    var v = entries[i].values[j];
                    if (v == null) { entries[i].values.RemoveAt(j); continue; }
                    if (v.language != null) v.language = v.language.Trim();
                }
            }
            for (int i = 0; i < languages.Count; i++)
            {
                if (languages[i] != null) languages[i] = languages[i].Trim();
            }

            // Simple validators: empty keys and missing default-language text
            #if UNITY_EDITOR
            string defaultLang = (languages != null && languages.Count > 0) ? (languages[0] ?? string.Empty) : string.Empty;
            for (int i = 0; i < entries.Count; i++)
            {
                var e = entries[i];
                if (e == null) continue;
                if (string.IsNullOrWhiteSpace(e.key))
                {
                    Debug.LogWarning($"[LocalizationDB] Entry at index {i} has empty key");
                    continue;
                }
                if (!string.IsNullOrEmpty(defaultLang))
                {
                    string t = FindIn(e, defaultLang);
                    if (string.IsNullOrEmpty(t))
                    {
                        Debug.LogWarning($"[LocalizationDB] Key '{e.key}' has no text for default language '{defaultLang}'");
                    }
                }
            }
            #endif
        }
    }
}

// ScriptRole: Base de datos de localización (clave -> textos por idioma)
// RelatedScripts: Game.Localization.LocalizedText, Game.Localization.LanguageChangedEventChannelSO
// UsesSO: —
// ReceivesFrom: LocalizedText.Get()
// SendsTo: —
// Adjuntar: Asset en carpeta SO. Rellenar idiomas y entradas por Inspector.
