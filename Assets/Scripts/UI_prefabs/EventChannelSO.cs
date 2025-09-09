
using UnityEngine;
using UnityEngine.Events;

namespace Brainamics.UI.Prototypes
{
    /// <summary>
    /// ScriptableObject genérico que actúa como un canal de eventos para un tipo de dato específico.
    /// </summary>
    /// <typeparam name="T">El tipo de dato que este canal transmitirá.</typeparam>
    public class EventChannelSO<T> : ScriptableObject
    {
        [Tooltip("Acción que se invoca cuando el evento es disparado. Se pueden suscribir listeners desde código.")]
        public UnityAction<T> OnEventRaised;

        public void RaiseEvent(T value)
        {
            OnEventRaised?.Invoke(value);
        }
    }

    // ScriptRole | RelatedScripts | UsesSO | ReceivesFrom | SendsTo
    // Role: Actúa como un bus de eventos genérico para desacoplar sistemas.
    // Related: VoidEventChannelSO.cs, cualquier script que lo use para emitir o recibir.
    // UsesSO: N/A
    // ReceivesFrom: Cualquier sistema que llame a RaiseEvent.
    // SendsTo: Cualquier sistema suscrito a OnEventRaised.
}
