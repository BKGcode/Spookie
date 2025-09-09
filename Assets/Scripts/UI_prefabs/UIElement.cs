
using UnityEngine;

namespace Brainamics.UI.Prototypes
{
    /// <summary>
    /// Clase base para componentes de UI, incluye un toggle para logs.
    /// </summary>
    public abstract class UIElement : MonoBehaviour
    {
        [Header("Debugging")]
        [Tooltip("Habilita o deshabilita los logs de este componente.")]
        [SerializeField] protected bool showLogs = false;
    }

    // ScriptRole | RelatedScripts | UsesSO | ReceivesFrom | SendsTo
    // Role: Clase base para estandarizar componentes de UI con opciones de debug.
    // Related: Todos los componentes de UI que hereden de él.
    // UsesSO: N/A
    // ReceivesFrom: N/A
    // SendsTo: N/A
}
