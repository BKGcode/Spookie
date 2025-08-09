using System;
using UnityEngine;

namespace Game.Localization
{
    [CreateAssetMenu(fileName = "LanguageChanged", menuName = "Spookie/Localization/LanguageChanged EventChannel")]
    public class LanguageChangedEventChannelSO : ScriptableObject
    {
        public event Action<string> OnLanguageChanged; // lang code

        public void Raise(string language)
        {
            try { OnLanguageChanged?.Invoke(language); } catch { }
        }
    }
}

// ScriptRole: EventChannel para cambio de idioma actual
// RelatedScripts: Game.Localization.LocalizedText
// UsesSO: —
// ReceivesFrom: UI de settings/localización
// SendsTo: LocalizedText (suscriptores)
