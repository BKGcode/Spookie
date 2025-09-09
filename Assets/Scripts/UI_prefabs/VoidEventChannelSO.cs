
using UnityEngine;

namespace Brainamics.UI.Prototypes
{
    /// <summary>
    /// Crea un asset de menú para un canal de eventos que no transmite datos (void).
    /// </summary>
    [CreateAssetMenu(menuName = "Brainamics/UI Prototypes/Event Channels/Void Event Channel", fileName = "VoidEventChannel_")]
    public class VoidEventChannelSO : EventChannelSO<object>
    {
        // Sobrescribe RaiseEvent para no requerir un parámetro, facilitando su uso con UnityEvents.
        public void RaiseEvent()
        {
            base.RaiseEvent(null);
        }
    }

    // ScriptRole | RelatedScripts | UsesSO | ReceivesFrom | SendsTo
    // Role: Un canal de eventos específico para señales que no necesitan pasar datos.
    // Related: EventChannelSO.cs, CustomButton.cs
    // UsesSO: N/A
    // ReceivesFrom: Quienes llaman a RaiseEvent().
    // SendsTo: Quienes están suscritos a OnEventRaised.
}
