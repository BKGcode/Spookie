using UnityEngine;
using UnityEngine.Events;

public class InteractableObject : MonoBehaviour, IInteractable
{
    [Header("Interaction")]
    public string interactionPrompt = "Press E to interact";
    public float interactionDistance = 3f;
    public bool canInteractMultipleTimes = true;
    public bool destroyAfterInteraction = false;
    
    [Header("Audio")]
    public AudioClip interactionSound;
    public AudioClip voiceClip;
    
    [Header("Events")]
    public UnityEvent OnInteract;
    public UnityEvent OnInteractStart;
    public UnityEvent OnInteractEnd;
    
    [Header("References")]
    public FeedbackMessagesSO feedbackMessages;
    
    protected bool hasBeenInteracted = false;
    protected bool isInRange = false;
    protected PlayerFPSController playerController;

    private void Start()
    {
        Debug.Log($"InteractableObject: Initialized {gameObject.name}");
        
        // Find player controller
        playerController = FindObjectOfType<PlayerFPSController>();
    }

    private void Update()
    {
        if (playerController == null) return;
        
        // Check if player is in range
        float distance = Vector3.Distance(transform.position, playerController.transform.position);
        bool wasInRange = isInRange;
        isInRange = distance <= interactionDistance;
        
        // Show/hide interaction prompt
        if (isInRange && !wasInRange)
        {
            OnPlayerEnterRange();
        }
        else if (!isInRange && wasInRange)
        {
            OnPlayerExitRange();
        }
    }

    public virtual void Interact()
    {
        if (!CanInteract()) return;
        
        Debug.Log($"InteractableObject: Player interacted with {gameObject.name}");
        
        // Play interaction sound
        if (interactionSound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySoundAtPosition("interaction", transform.position);
        }
        
        // Play voice clip
        if (voiceClip != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayVoice(voiceClip);
        }
        
        // Trigger events
        OnInteractStart?.Invoke();
        OnInteract?.Invoke();
        
        // Mark as interacted
        hasBeenInteracted = true;
        
        // Destroy if needed
        if (destroyAfterInteraction)
        {
            Destroy(gameObject);
        }
        
        OnInteractEnd?.Invoke();
    }

    protected virtual bool CanInteract()
    {
        if (!isInRange) return false;
        if (hasBeenInteracted && !canInteractMultipleTimes) return false;
        return true;
    }

    protected virtual void OnPlayerEnterRange()
    {
        Debug.Log($"InteractableObject: Player entered range of {gameObject.name}");
        
        // Show interaction prompt
        if (feedbackMessages != null)
        {
            string prompt = feedbackMessages.GetMessage("interaction_prompt");
            if (string.IsNullOrEmpty(prompt))
            {
                prompt = interactionPrompt;
            }
            Debug.Log($"InteractableObject: {prompt}");
        }
    }

    protected virtual void OnPlayerExitRange()
    {
        Debug.Log($"InteractableObject: Player exited range of {gameObject.name}");
    }

    private void OnDrawGizmosSelected()
    {
        // Draw interaction range
        Gizmos.color = isInRange ? Color.green : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionDistance);
    }
}

// ScriptRole: Base class for all interactable objects in the game
// Dependencies: Collider component
// UsesSO: FeedbackMessagesSO
// ReceivesFrom: PlayerFPSController
// SendsTo: AudioManager, GameManager via events 