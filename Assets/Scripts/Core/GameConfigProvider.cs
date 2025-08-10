using UnityEngine;

namespace Game.Core
{
    /// <summary>
    /// Scene-level provider for the GameConfigSO hub. Keep one per scene (e.g., on a root "Config" GameObject).
    /// This component doesn't push values; it's a convenient anchor to fetch shared ScriptableObjects.
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Spookie/Game Config Provider")]
    public class GameConfigProvider : MonoBehaviour
    {
        [Header("References")] 
        [Tooltip("Reference to the global GameConfigSO hub.")]
        [SerializeField] private Game.Config.GameConfigSO gameConfig;

        public Game.Config.GameConfigSO Config => gameConfig;

        private void OnValidate()
        {
            if (gameConfig == null)
            {
                #if UNITY_EDITOR
                Debug.LogWarning("[GameConfigProvider] GameConfigSO not assigned.");
                #endif
            }
        }
    }
}

// ScriptRole | RelatedScripts | UsesSO | ReceivesFrom | SendsTo
// ScriptRole: Proveedor de escena para acceder al GameConfigSO.
// RelatedScripts: Game.Config.GameConfigSO, varios consumidores opcionales.
// UsesSO: GameConfigSO
// ReceivesFrom: —
// SendsTo: Consumidores que lo consulten (opcional)
// Adjuntar a: GameObject raíz "Config" en la escena. Asignar GameConfigSO por Inspector.
