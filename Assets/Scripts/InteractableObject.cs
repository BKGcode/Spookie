using UnityEngine;
using TMPro;

public class InteractableObject : MonoBehaviour, IInteractable
{
    [Header("Interaction Settings")]
    [SerializeField] private string interactionMessage = "Press E to interact";
    [SerializeField] private string interactionFeedback = "Object interacted!";
    [SerializeField] private bool canInteractMultipleTimes = true;
    
    [Header("Visual Feedback")]
    [SerializeField] private Material normalMaterial;
    [SerializeField] private Material highlightMaterial;
    [SerializeField] private Renderer objectRenderer;
    
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI interactionText;
    [SerializeField] private FeedbackMessagesSO feedbackMessages;
    
    private bool isHighlighted = false;
    private bool hasBeenInteracted = false;
    private bool wasLookingAt = false;
    
    private void Awake()
    {
        if (objectRenderer == null)
            objectRenderer = GetComponent<Renderer>();
            
        if (normalMaterial == null)
            normalMaterial = objectRenderer.material;
            
        Debug.Log($"InteractableObject '{gameObject.name}' initialized");
    }
    
    private void Start()
    {
        // Validate dependencies
        DependencyValidator.ValidateInteractableObject(this);
    }
    
    public void OnLookAt()
    {
        if (!isHighlighted)
        {
            HighlightObject(true);
            ShowInteractionText();
            
            if (!wasLookingAt)
            {
                Debug.Log($"Player looking at {gameObject.name}");
                wasLookingAt = true;
            }
        }
    }
    
    public void OnLookAway()
    {
        if (isHighlighted)
        {
            HighlightObject(false);
            HideInteractionText();
            
            if (wasLookingAt)
            {
                Debug.Log($"Player stopped looking at {gameObject.name}");
                wasLookingAt = false;
            }
        }
    }
    
    public void Interact()
    {
        if (!canInteractMultipleTimes && hasBeenInteracted)
            return;
            
        hasBeenInteracted = true;
        
        string message = feedbackMessages != null ? 
            feedbackMessages.GetMessage("interaction_success") : 
            interactionFeedback;
            
        Debug.Log($"Interacted with {gameObject.name}: {message}");
        
        // Here you can add specific interaction logic
        // For example: open doors, collect items, trigger events, etc.
    }
    
    private void HighlightObject(bool highlight)
    {
        isHighlighted = highlight;
        
        if (objectRenderer != null && highlightMaterial != null)
        {
            objectRenderer.material = highlight ? highlightMaterial : normalMaterial;
        }
    }
    
    private void ShowInteractionText()
    {
        if (interactionText != null)
        {
            string message = feedbackMessages != null ? 
                feedbackMessages.GetMessage("press_to_interact") : 
                interactionMessage;
                
            interactionText.text = message;
            interactionText.gameObject.SetActive(true);
        }
    }
    
    private void HideInteractionText()
    {
        if (interactionText != null)
        {
            interactionText.gameObject.SetActive(false);
        }
    }
}

// ScriptRole: Example interactable object with visual feedback and UI integration
// Dependencies: Renderer, TextMeshProUGUI
// UsesSO: FeedbackMessagesSO
// NeedsSetup: objectRenderer, highlightMaterial, interactionText, feedbackMessages 