using UnityEngine;

namespace Game.Mine
{
    /// <summary>
    /// Tipos básicos de recursos. Extender según necesidad. Mantener valores estables para persistencia.
    /// </summary>
    public enum ResourceType
    {
        None = 0,
        Stone = 1,
        Iron = 2,
        Crystal = 3,
        Coal = 4,
    }
}

// ScriptRole | RelatedScripts | UsesSO | ReceivesFrom | SendsTo
// ScriptRole: Enum simple de tipos de recurso para minado.
// RelatedScripts: RockNode, MiningCluster
// UsesSO: No (posible futuro para configuraciones de recursos).
// ReceivesFrom: —
// SendsTo: —
