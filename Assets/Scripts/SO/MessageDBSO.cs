using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Messages
{
    public enum MessageType { Lower, Oniric }

    [CreateAssetMenu(fileName = "MessageDB", menuName = "Spookie/Messages/DB")]
    public class MessageDBSO : ScriptableObject
    {
    [Header("Defaults")]
    [Tooltip("Default language code to validate VO presence (e.g., 'en'). Empty to skip this specific warning.")]
    [SerializeField] private string defaultLanguageCode = "en";

        [Serializable]
        public class Entry
        {
            [Tooltip("Unique ID for this message entry, e.g., 'intro.welcome'")]
            public string id;
            [Tooltip("Lower (banner) or Oniric overlay")]
            public MessageType type = MessageType.Lower;
            [Header("Localization")]
            [Tooltip("Localization key used to resolve text")] public string key;
            [Header("Behavior Overrides (optional)")]
            [Tooltip("<=0 uses controller default")] public float autoCloseSeconds = -1f;
            [Tooltip("Typewriter speed override (<=0 uses default)")] public float typewriterCps = -1f;
            [Tooltip("Repeatable flag (if false, banner cooldown applies by default)")] public bool repeatable = false;
            [Header("Audio (optional)")]
            [Tooltip("Default/fallback audio clip if no per-language clip is set")]
            public AudioClip audio;
            [Tooltip("Optional per-language audio overrides (language code -> clip)")]
            public List<LocalizedAudio> localizedAudio = new List<LocalizedAudio>();
            [Header("One-Shot")]
            [Tooltip("If set, mark as seen and skip next time (PlayerPrefs)")] public bool oneShot = false;
            [Tooltip("Persistent key override for one-shot (defaults to id)")] public string persistentIdOverride;

            public AudioClip ResolveAudio(string language, string fallbackLanguage = null)
            {
                // Try preferred language
                var clip = FindAudio(language);
                if (clip != null) return clip;
                // Try fallback
                if (!string.IsNullOrEmpty(fallbackLanguage))
                {
                    clip = FindAudio(fallbackLanguage);
                    if (clip != null) return clip;
                }
                // Return default
                return audio;
            }

            private AudioClip FindAudio(string language)
            {
                if (string.IsNullOrEmpty(language) || localizedAudio == null) return null;
                for (int i = 0; i < localizedAudio.Count; i++)
                {
                    var la = localizedAudio[i];
                    if (la == null || string.IsNullOrEmpty(la.language)) continue;
                    if (string.Equals(la.language, language, StringComparison.OrdinalIgnoreCase))
                    {
                        return la.clip;
                    }
                }
                return null;
            }
        }

        [Serializable]
        public class LocalizedAudio
        {
            [Tooltip("Language code, e.g., 'en', 'es'")] public string language;
            public AudioClip clip;
        }

        [SerializeField] private List<Entry> entries = new List<Entry>();

        public Entry Find(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            for (int i = 0; i < entries.Count; i++)
            {
                var e = entries[i];
                if (string.Equals(e.id, id, StringComparison.Ordinal)) return e;
            }
            return null;
        }

        private void OnValidate()
        {
            // trim ids, ensure uniqueness warning
            var seen = new HashSet<string>(StringComparer.Ordinal);
            for (int i = 0; i < entries.Count; i++)
            {
                if (entries[i] == null) continue;
                if (!string.IsNullOrEmpty(entries[i].id)) entries[i].id = entries[i].id.Trim();
                if (!string.IsNullOrEmpty(entries[i].id) && !seen.Add(entries[i].id))
                {
                    #if UNITY_EDITOR
                    Debug.LogWarning($"[MessageDB] Duplicate id '{entries[i].id}' at index {i}");
                    #endif
                }

                // Clean localized audio list (dedupe languages, trim codes)
                var e = entries[i];
                if (e.localizedAudio != null)
                {
                    var langSeen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    for (int j = e.localizedAudio.Count - 1; j >= 0; j--)
                    {
                        var la = e.localizedAudio[j];
                        if (la == null) { e.localizedAudio.RemoveAt(j); continue; }
                        if (!string.IsNullOrEmpty(la.language)) la.language = la.language.Trim();
                        var code = la.language ?? string.Empty;
                        if (!string.IsNullOrEmpty(code))
                        {
                            if (!langSeen.Add(code))
                            {
                                #if UNITY_EDITOR
                                Debug.LogWarning($"[MessageDB] Duplicate audio language '{code}' in entry '{e.id}'");
                                #endif
                            }
                            if (la.clip == null)
                            {
                                #if UNITY_EDITOR
                                Debug.LogWarning($"[MessageDB] Missing audio clip for language '{code}' in entry '{e.id}'");
                                #endif
                            }
                        }
                    }
                }

                // Warn if no audio at all (default or any localized), only as info
                #if UNITY_EDITOR
                bool hasAnyAudio = e.audio != null;
                if (!hasAnyAudio && e.localizedAudio != null)
                {
                    for (int j = 0; j < e.localizedAudio.Count; j++)
                    {
                        if (e.localizedAudio[j] != null && e.localizedAudio[j].clip != null) { hasAnyAudio = true; break; }
                    }
                }
                if (!hasAnyAudio)
                {
                    Debug.LogWarning($"[MessageDB] Entry '{e.id}' has no audio set (default nor localized). This is fine if text-only.");
                }

                // New: warn if localized VO list exists but lacks the default language clip
                string def = string.IsNullOrWhiteSpace(defaultLanguageCode) ? null : defaultLanguageCode.Trim();
                if (!string.IsNullOrEmpty(def) && e.localizedAudio != null && e.localizedAudio.Count > 0)
                {
                    bool found = false;
                    for (int j = 0; j < e.localizedAudio.Count; j++)
                    {
                        var la = e.localizedAudio[j];
                        if (la == null || string.IsNullOrEmpty(la.language)) continue;
                        if (string.Equals(la.language, def, StringComparison.OrdinalIgnoreCase))
                        {
                            found = la.clip != null; break;
                        }
                    }
                    if (!found)
                    {
                        Debug.LogWarning($"[MessageDB] Entry '{e.id}' has no VO clip for default language '{def}'.");
                    }
                }
                #endif
            }
        }
    }
}

// ScriptRole: Catálogo de mensajes (Lower/Oniric) con clave de localización, audio y flags
// RelatedScripts: Game.UI.LowerMessageController, Game.UI.OniricOverlayController
// UsesSO: LocalizationDBSO (indirecto)
// ReceivesFrom: MessageDBEnqueuer
// SendsTo: Controladores Lower/Oniric
