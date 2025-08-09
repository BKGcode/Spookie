using UnityEngine;

namespace Game.Localization
{
    [CreateAssetMenu(fileName = "Locale", menuName = "Spookie/Localization/Locale")]
    public class LocaleSO : ScriptableObject
    {
        [SerializeField] private string currentLanguage = "en";
        [SerializeField] private string defaultLanguage = "en";

        public string CurrentLanguage => string.IsNullOrEmpty(currentLanguage) ? defaultLanguage : currentLanguage;
        public string DefaultLanguage => string.IsNullOrEmpty(defaultLanguage) ? "en" : defaultLanguage;

        public void SetLanguage(string lang)
        {
            currentLanguage = string.IsNullOrEmpty(lang) ? defaultLanguage : lang;
        }

        public string GetCurrentOrDefault()
        {
            return string.IsNullOrEmpty(currentLanguage) ? DefaultLanguage : currentLanguage;
        }
    }
}

// ScriptRole: Proveedor simple del idioma actual
// RelatedScripts: Game.Localization.LocalizationDBSO, LocalizedText
// UsesSO: —
// ReceivesFrom: UI de settings
// SendsTo: Consumidores de idioma (banner/P.O./LocalizedText)
