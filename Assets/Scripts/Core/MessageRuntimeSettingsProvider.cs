using UnityEngine;

namespace Game.Core
{
    [AddComponentMenu("Spookie/Message Runtime Settings Provider")]
    public class MessageRuntimeSettingsProvider : MonoBehaviour
    {
        [Tooltip("Asset con la configuración global de mensajes. Si no se asigna, se intentará localizar uno en Resources o en la escena (no se creará en runtime).")]
        public MessageRuntimeSettingsSO settings;

        private static MessageRuntimeSettingsSO _cached;

        private void Awake()
        {
            if (settings != null) _cached = settings;
        }

        public static MessageRuntimeSettingsSO Get()
        {
            if (_cached != null) return _cached;
            var prov = FindObjectOfType<MessageRuntimeSettingsProvider>();
            if (prov != null) _cached = prov.settings;
            return _cached;
        }
    }
}

// ScriptRole | RelatedScripts | UsesSO | ReceivesFrom | SendsTo
// ScriptRole: Proveedor en escena de ajustes globales de mensajes.
// RelatedScripts: Game.Interaction.DBTextInteractable
// UsesSO: MessageRuntimeSettingsSO
// ReceivesFrom: N/A
// SendsTo: DBTextInteractable (Get())
