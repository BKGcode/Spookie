using UnityEngine;

namespace Game.Config
{
    /// <summary>
    /// Game-wide configuration hub (Inspector-first). Aggregates references to core ScriptableObjects
    /// so designers can wire a single asset per project/scene. Immutable at runtime: do not modify.
    /// </summary>
    [CreateAssetMenu(fileName = "GameConfig", menuName = "Spookie/Game Config")]
    public class GameConfigSO : ScriptableObject
    {
        [Header("Player")]
        [Tooltip("Player movement/look/interaction settings.")]
        [SerializeField] private PlayerController.PlayerSettingsSO playerSettings;

        [Header("Day & Night")]
        [Tooltip("Day/Night timings, labels and penalties.")]
        [SerializeField] private Game.DayNight.DayNightConfigSO dayNightConfig;
        [Tooltip("Light preset for sun/ambient/fog gradients over the day.")]
        [SerializeField] private LightPreset sunLightPreset;
        [Tooltip("Optional light preset to tint lamps over the day.")]
        [SerializeField] private LightPreset lampLightPreset;

        [Header("Localization")]
        [Tooltip("Localization DB (keys -> text per language).")]
        [SerializeField] private Game.Localization.LocalizationDBSO localizationDB;
        [Tooltip("Locale defaults (do NOT mutate at runtime). Current language should live in a runtime owner.")]
        [SerializeField] private Game.Localization.LocaleSO localeDefaults;
        [Tooltip("Event channel raised when language changes (subscribed by LocalizedText & UI controllers).")]
        [SerializeField] private Game.Localization.LanguageChangedEventChannelSO languageChangedEvent;

        [Header("Messages")]
        [Tooltip("Catalog of messages (Lower/Oniric) with localization keys and VO per language.")]
        [SerializeField] private Game.Messages.MessageDBSO messageDB;

        // Public getters (read-only)
        public PlayerController.PlayerSettingsSO PlayerSettings => playerSettings;
        public Game.DayNight.DayNightConfigSO DayNightConfig => dayNightConfig;
        public LightPreset SunLightPreset => sunLightPreset;
        public LightPreset LampLightPreset => lampLightPreset;
        public Game.Localization.LocalizationDBSO LocalizationDB => localizationDB;
        public Game.Localization.LocaleSO LocaleDefaults => localeDefaults;
        public Game.Localization.LanguageChangedEventChannelSO LanguageChangedEvent => languageChangedEvent;
        public Game.Messages.MessageDBSO MessageDB => messageDB;

        private void OnValidate()
        {
            // Light warnings only to help wiring in the editor; no heavy work here.
            #if UNITY_EDITOR
            if (playerSettings == null) Debug.LogWarning("[GameConfigSO] PlayerSettings not assigned.");
            if (dayNightConfig == null) Debug.LogWarning("[GameConfigSO] DayNightConfig not assigned.");
            if (localizationDB == null) Debug.LogWarning("[GameConfigSO] LocalizationDB not assigned.");
            if (localeDefaults == null) Debug.LogWarning("[GameConfigSO] Locale defaults not assigned.");
            if (languageChangedEvent == null) Debug.LogWarning("[GameConfigSO] LanguageChanged EventChannel not assigned.");
            if (messageDB == null) Debug.LogWarning("[GameConfigSO] MessageDB not assigned.");
            if (sunLightPreset == null) Debug.LogWarning("[GameConfigSO] Sun LightPreset not assigned.");
            // lamp is optional
            #endif
        }
    }
}

// ScriptRole | RelatedScripts | UsesSO | ReceivesFrom | SendsTo
// ScriptRole: Hub de configuración global (solo referencias, inmutable en runtime).
// RelatedScripts: Game.Core.GameConfigProvider, PlayerMovement/MouseLook/PlayerInteraction, DayNightManager, Lower/Oniric controllers.
// UsesSO: PlayerSettingsSO, DayNightConfigSO, LocalizationDBSO, LocaleSO, LanguageChangedEventChannelSO, MessageDBSO, LightPreset(s).
// ReceivesFrom: — (asset edit por Inspector)
// SendsTo: Consumidores a través del Provider (futuro) o como panel de referencia para arrastrar.
// Adjuntar/Crear: Asset en Assets/SO/GameConfig (p.ej., GameConfig.asset). No mutar en runtime.
