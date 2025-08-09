using UnityEngine;
using PlayerController;

namespace Game.Interaction
{
    /// <summary>
    /// Ejemplo mínimo de interactuable compatible con PlayerInteraction e IInteractable.
    /// Adjunta también un OutlineHighlighter y asigna el material de outline para ver el efecto.
    /// </summary>
    [DisallowMultipleComponent]
    public class ExampleInteractable : MonoBehaviour, PlayerController.IInteractable
    {
        [Header("Feedback simple")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip onInteractClip;

        public void Interact(GameObject interactor)
        {
            if (audioSource != null && onInteractClip != null)
            {
                audioSource.PlayOneShot(onInteractClip);
            }
            // Aquí iría la lógica del objeto (abrir puerta, recoger ítem, etc.)
            Debug.Log($"[ExampleInteractable] Interact -> {gameObject.name} por {interactor.name}");
        }

        public string GetInteractionPrompt()
        {
            return "Interact";
        }

        private void Reset()
        {
            // Intento de auto-configuración suave
            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
            }
        }
    }
}

// ScriptRole | RelatedScripts | UsesSO | ReceivesFrom | SendsTo
// ScriptRole: Ejemplo de objeto interactuable sencillo, reproduce un sonido al interactuar.
// RelatedScripts: PlayerController.PlayerInteraction, Game.Interaction.OutlineHighlighter
// UsesSO: No
// ReceivesFrom: PlayerInteraction.Interact
// SendsTo: AudioSource (opcional)
// Adjuntar a: Prefab/objeto interactuable. Añadir OutlineHighlighter y asignar el material URP de silueta.
