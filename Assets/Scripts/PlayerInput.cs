using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(FPSPlayerMovement))]
[RequireComponent(typeof(PlayerCamera))]
[RequireComponent(typeof(PlayerInteraction))]
public class PlayerInput : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private FPSPlayerMovement playerMovement;
    [SerializeField] private PlayerCamera playerCamera;
    [SerializeField] private PlayerInteraction playerInteraction;
    
    private void Awake()
    {
        // Auto-assign references if not set
        if (playerMovement == null)
            playerMovement = GetComponent<FPSPlayerMovement>();
        if (playerCamera == null)
            playerCamera = GetComponentInChildren<PlayerCamera>();
        if (playerInteraction == null)
            playerInteraction = GetComponent<PlayerInteraction>();
            
        Debug.Log("PlayerInput initialized");
    }
    
    private void Start()
    {
        // Validate dependencies
        if (playerMovement == null || playerCamera == null || playerInteraction == null)
        {
            Debug.LogError("PlayerInput: Missing required components!");
        }
    }
    
    // Input System callbacks
    public void OnMove(InputValue value)
    {
        Vector2 input = value.Get<Vector2>();
        if (playerMovement != null)
        {
            playerMovement.SetMoveInput(input);
        }
    }
    
    public void OnLook(InputValue value)
    {
        Vector2 input = value.Get<Vector2>();
        if (playerCamera != null)
        {
            playerCamera.SetLookInput(input);
        }
    }
    
    public void OnJump(InputValue value)
    {
        if (value.isPressed)
        {
            if (playerMovement != null)
            {
                playerMovement.Jump();
            }
        }
    }
    
    public void OnRun(InputValue value)
    {
        bool running = value.isPressed;
        if (playerMovement != null)
        {
            playerMovement.SetRunning(running);
        }
    }
    
    public void OnCrouch(InputValue value)
    {
        if (value.isPressed)
        {
            if (playerMovement != null)
            {
                playerMovement.ToggleCrouch();
            }
        }
    }
    
    public void OnInteract(InputValue value)
    {
        if (value.isPressed)
        {
            if (playerInteraction != null)
            {
                playerInteraction.Interact();
            }
        }
    }
    
    public void OnToggleCursor(InputValue value)
    {
        if (value.isPressed)
        {
            if (playerCamera != null)
            {
                playerCamera.ToggleCursorLock();
            }
        }
    }
}

// ScriptRole: Handles player input using Input System
// Dependencies: PlayerMovement, PlayerCamera, PlayerInteraction
// UsesSO: None
// NeedsSetup: playerMovement, playerCamera, playerInteraction (auto-assigned) 