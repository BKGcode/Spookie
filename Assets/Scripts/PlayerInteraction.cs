using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private PlayerSettingsSO playerSettings;
    [SerializeField] private LayerMask interactableLayer = -1;
    
    [Header("References")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private FeedbackMessagesSO feedbackMessages;
    
    [Header("Events")]
    [SerializeField] private GameEvents gameEvents;
    
    private IInteractable currentInteractable;
    private bool wasInteractableInRange = false;
    
    private void Awake()
    {
        if (cameraTransform == null)
        {
            cameraTransform = Camera.main.transform;
            Debug.LogWarning("Camera transform not assigned to PlayerInteraction, using main camera");
        }
        
        Debug.Log("PlayerInteraction initialized");
    }
    
    private void Update()
    {
        HandleInteraction();
    }
    
    private void HandleInteraction()
    {
        float interactionRange = playerSettings != null ? playerSettings.InteractionRange : 3f;
        
        // Cast ray from camera
        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);
        RaycastHit hit;
        
        bool hitInteractable = Physics.Raycast(ray, out hit, interactionRange, interactableLayer);
        
        if (hitInteractable)
        {
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();
            
            if (interactable != null)
            {
                if (currentInteractable != interactable)
                {
                    // New interactable found
                    if (currentInteractable != null)
                    {
                        currentInteractable.OnLookAway();
                    }
                    
                    currentInteractable = interactable;
                    currentInteractable.OnLookAt();
                    
                    if (!wasInteractableInRange)
                    {
                        Debug.Log($"Found interactable: {hit.collider.gameObject.name}");
                        wasInteractableInRange = true;
                        
                        // Trigger game events
                        if (gameEvents != null)
                        {
                            gameEvents.TriggerInteractableFound(interactable);
                        }
                    }
                }
            }
            else
            {
                ClearCurrentInteractable();
            }
        }
        else
        {
            ClearCurrentInteractable();
        }
    }
    
    private void ClearCurrentInteractable()
    {
        if (currentInteractable != null)
        {
            currentInteractable.OnLookAway();
            currentInteractable = null;
            
            if (wasInteractableInRange)
            {
                Debug.Log("No interactable in range");
                wasInteractableInRange = false;
                
                // Trigger game events
                if (gameEvents != null)
                {
                    gameEvents.TriggerInteractableLost(currentInteractable);
                }
            }
        }
    }
    
    public void Interact()
    {
        if (currentInteractable != null)
        {
            currentInteractable.Interact();
            Debug.Log("Player interacted with object");
            
            // Trigger game events
            if (gameEvents != null)
            {
                gameEvents.TriggerPlayerInteract();
            }
        }
        else
        {
            string message = feedbackMessages != null ? 
                feedbackMessages.GetMessage("no_interactable") : 
                "No interactable object in range";
            Debug.Log(message);
        }
    }
    
    public IInteractable GetCurrentInteractable()
    {
        return currentInteractable;
    }
    
    public bool HasInteractableInRange()
    {
        return currentInteractable != null;
    }
}

// ScriptRole: Handles player interaction with objects
// Dependencies: Camera
// UsesSO: PlayerSettingsSO, FeedbackMessagesSO, GameEvents
// NeedsSetup: playerSettings, cameraTransform, feedbackMessages, gameEvents, interactableLayer 