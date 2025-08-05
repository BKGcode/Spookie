using UnityEngine;
using TMPro;

public class InteractionUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI interactionText;
    [SerializeField] private GameObject interactionPanel;
    [SerializeField] private FeedbackMessagesSO feedbackMessages;
    
    [Header("Animation Settings")]
    [SerializeField] private float fadeInDuration = 0.2f;
    [SerializeField] private float fadeOutDuration = 0.3f;
    
    private CanvasGroup canvasGroup;
    private bool isVisible = false;
    
    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        
        // Start hidden
        SetVisibility(false);
        
        Debug.Log("InteractionUI initialized");
    }
    
    private void Start()
    {
        // Validate dependencies
        DependencyValidator.ValidateInteractionUI(this);
    }
    
    public void ShowInteractionMessage(string message)
    {
        if (interactionText != null)
        {
            interactionText.text = message;
        }
        
        SetVisibility(true);
        Debug.Log($"Showing interaction message: {message}");
    }
    
    public void HideInteractionMessage()
    {
        SetVisibility(false);
        Debug.Log("Hiding interaction message");
    }
    
    public void ShowDefaultMessage()
    {
        string message = feedbackMessages != null ? 
            feedbackMessages.GetMessage("press_to_interact") : 
            "Press E to interact";
            
        ShowInteractionMessage(message);
    }
    
    private void SetVisibility(bool visible)
    {
        if (isVisible == visible) return;
        
        isVisible = visible;
        
        if (canvasGroup != null)
        {
            canvasGroup.alpha = visible ? 1f : 0f;
            canvasGroup.interactable = visible;
            canvasGroup.blocksRaycasts = visible;
        }
        
        if (interactionPanel != null)
        {
            interactionPanel.SetActive(visible);
        }
    }
    
    private void OnValidate()
    {
        // Ensure we have a CanvasGroup component
        if (GetComponent<CanvasGroup>() == null)
        {
            gameObject.AddComponent<CanvasGroup>();
        }
    }
}

// ScriptRole: Manages interaction UI display and animations
// Dependencies: CanvasGroup, TextMeshProUGUI
// UsesSO: FeedbackMessagesSO
// NeedsSetup: interactionText, interactionPanel, feedbackMessages 