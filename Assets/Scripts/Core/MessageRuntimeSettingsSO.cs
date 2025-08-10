using UnityEngine;

namespace Game.Core
{
    [CreateAssetMenu(fileName = "MessageRuntimeSettings", menuName = "Spookie/Message Runtime Settings", order = 10)]
    public class MessageRuntimeSettingsSO : ScriptableObject
    {
        [Header("Global Defaults (apply when local/DB have no value)")]
        [Tooltip("Cooldown global por defecto para interacciones de texto (segundos). 0 = sin cooldown global.")]
        public float globalCooldownSeconds = 0f;

        [Tooltip("Typewriter CPS por defecto para Lower/Oníric si no hay valor en override ni en el DB. ≤0 = usar el del controlador.")]
        public float defaultTypewriterCps = -1f;

        [Tooltip("AutoClose por defecto si no hay valor en override ni en el DB. <0 = sin valor global.")]
        public float defaultAutoCloseSeconds = -1f;
    }
}

// ScriptRole | RelatedScripts | UsesSO | ReceivesFrom | SendsTo
// ScriptRole: Ajustes globales para mensajes (cooldown, cps, autocierre por defecto).
// RelatedScripts: Game.Interaction.DBTextInteractable, Game.UI.LowerMessageController, Game.UI.OniricOverlayController
// UsesSO: Sí (es un SO de configuración)
// ReceivesFrom: MessageRuntimeSettingsProvider
// SendsTo: Leído por DBTextInteractable como fallback global
