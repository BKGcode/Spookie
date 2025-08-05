using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "GameEvents", menuName = "Spookie/Game Events")]
public class GameEvents : ScriptableObject
{
    // Player Events
    public UnityEvent onPlayerJump;
    public UnityEvent onPlayerCrouch;
    public UnityEvent onPlayerRun;
    public UnityEvent onPlayerWalk;
    public UnityEvent onPlayerInteract;
    
    // Game State Events
    public UnityEvent onGameStart;
    public UnityEvent onGamePause;
    public UnityEvent onGameResume;
    public UnityEvent onGameOver;
    
    // Interaction Events
    public UnityEvent<IInteractable> onInteractableFound;
    public UnityEvent<IInteractable> onInteractableLost;
    public UnityEvent<IInteractable> onInteractableInteracted;
    
    // UI Events
    public UnityEvent<string> onShowMessage;
    public UnityEvent onHideMessage;
    
    private void OnEnable()
    {
        Debug.Log("GameEvents initialized");
    }
    
    // Helper methods for common events
    public void TriggerPlayerJump()
    {
        onPlayerJump?.Invoke();
        Debug.Log("Player jump event triggered");
    }
    
    public void TriggerPlayerCrouch()
    {
        onPlayerCrouch?.Invoke();
        Debug.Log("Player crouch event triggered");
    }
    
    public void TriggerPlayerRun()
    {
        onPlayerRun?.Invoke();
        Debug.Log("Player run event triggered");
    }
    
    public void TriggerPlayerWalk()
    {
        onPlayerWalk?.Invoke();
        Debug.Log("Player walk event triggered");
    }
    
    public void TriggerPlayerInteract()
    {
        onPlayerInteract?.Invoke();
        Debug.Log("Player interact event triggered");
    }
    
    public void TriggerInteractableFound(IInteractable interactable)
    {
        onInteractableFound?.Invoke(interactable);
        Debug.Log($"Interactable found: {interactable}");
    }
    
    public void TriggerInteractableLost(IInteractable interactable)
    {
        onInteractableLost?.Invoke(interactable);
        Debug.Log($"Interactable lost: {interactable}");
    }
    
    public void TriggerInteractableInteracted(IInteractable interactable)
    {
        onInteractableInteracted?.Invoke(interactable);
        Debug.Log($"Interactable interacted: {interactable}");
    }
    
    public void TriggerShowMessage(string message)
    {
        onShowMessage?.Invoke(message);
        Debug.Log($"Show message: {message}");
    }
    
    public void TriggerHideMessage()
    {
        onHideMessage?.Invoke();
        Debug.Log("Hide message");
    }
}

// ScriptRole: Centralized event system using ScriptableObjects
// Dependencies: None
// UsesSO: None (this is a SO)
// NeedsSetup: Assign event listeners in Inspector 