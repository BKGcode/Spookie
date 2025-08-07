using UnityEngine;

namespace PlayerController
{
    public class ExampleInteractable : MonoBehaviour, IInteractable
    {
        [Header("Interaction Settings")]
        [SerializeField] private string interactionPrompt = "Press E to interact";
        [SerializeField] private bool showDebugLogs = true;
        
        [Header("Visual Feedback")]
        [SerializeField] private Material normalMaterial;
        [SerializeField] private Material highlightMaterial;
        
        private Renderer objectRenderer;
        private bool isHighlighted = false;
        
        private void Start()
        {
            objectRenderer = GetComponent<Renderer>();
            
            if (objectRenderer == null)
            {
                Debug.LogWarning($"[ExampleInteractable] No Renderer found on {gameObject.name} - visual feedback disabled");
            }
            
            if (showDebugLogs)
                Debug.Log($"[ExampleInteractable] Initialized on {gameObject.name}");
        }
        
        public void Interact(GameObject interactor)
        {
            if (showDebugLogs)
                Debug.Log($"[ExampleInteractable] Interacted by: {interactor.name}");
            
            // Example interaction behavior
            // You can replace this with your specific interaction logic
            transform.Rotate(0f, 90f, 0f);
            
            // Optional: Play sound effect
            // AudioSource.PlayClipAtPoint(interactionSound, transform.position);
        }
        
        public string GetInteractionPrompt()
        {
            return interactionPrompt;
        }
        
        // Called when player looks at this object
        public void OnPlayerLookAt()
        {
            if (!isHighlighted && objectRenderer != null && highlightMaterial != null)
            {
                objectRenderer.material = highlightMaterial;
                isHighlighted = true;
            }
        }
        
        // Called when player stops looking at this object
        public void OnPlayerLookAway()
        {
            if (isHighlighted && objectRenderer != null && normalMaterial != null)
            {
                objectRenderer.material = normalMaterial;
                isHighlighted = false;
            }
        }
    }
}

// ScriptRole: Example implementation of IInteractable interface
// RelatedScripts: PlayerInteraction
// UsesSO: None
// ReceivesFrom: PlayerInteraction
// SendsTo: None
