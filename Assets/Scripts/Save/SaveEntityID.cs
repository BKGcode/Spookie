using UnityEngine;

namespace Game.Save
{
    /// <summary>
    /// Identificador persistente para objetos interactuables / minería.
    /// Debe ser único por escena. Validado por herramientas editor (futuro).
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Spookie/Save/Entity ID")] 
    public class SaveEntityID : MonoBehaviour
    {
        [Tooltip("Identificador único dentro de la escena (ej: 'msg.lab_table_01')")] public string id;
        [Tooltip("Categoría opcional (msg, mine, puzzle, etc.)")] public string category;

        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(id)) id = name; // fallback nombre objeto
        }
    }
}

// ScriptRole: Marca objetos con IDs persistentes para progresión.
// RelatedScripts: ISaveParticipant (mensajes/minería)
// UsesSO: No
// ReceivesFrom: — (usado por otros scripts)
// SendsTo: SaveGameManager (indirecto: registros de progreso)
// Adjuntar: Objetos interactuables / nodos de minería.