using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class FPSPlayerMovement : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private PlayerSettingsSO playerSettings;
    
    [Header("Events")]
    [SerializeField] private GameEvents gameEvents;
    
    private CharacterController characterController;
    private Vector3 velocity;
    private Vector2 moveInput;
    private bool isJumping;
    private bool isRunning;
    private bool isCrouching;
    private float currentSpeed;
    private float originalHeight;
    private Vector3 originalCenter;
    
    public bool IsGrounded => characterController.isGrounded;
    public bool IsRunning => isRunning;
    public bool IsCrouching => isCrouching;
    public float CurrentSpeed => currentSpeed;
    
    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        
        // Store original character controller settings
        originalHeight = characterController.height;
        originalCenter = characterController.center;
        
        // Use settings if available, otherwise use defaults
        if (playerSettings != null)
        {
            currentSpeed = playerSettings.WalkSpeed;
        }
        else
        {
            currentSpeed = 5f; // Default walk speed
            Debug.LogWarning("PlayerSettings not assigned to FPSPlayerMovement, using default values");
        }
        
        Debug.Log("FPSPlayerMovement initialized");
    }
    
    private void Update()
    {
        HandleMovement();
        HandleGravity();
    }
    
    private void HandleMovement()
    {
        // Calculate movement direction
        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;
        
        // Determine current speed based on state
        float targetSpeed = playerSettings != null ? playerSettings.WalkSpeed : 5f;
        
        if (isRunning && !isCrouching)
        {
            targetSpeed = playerSettings != null ? playerSettings.RunSpeed : 8f;
        }
        else if (isCrouching)
        {
            targetSpeed = playerSettings != null ? playerSettings.CrouchSpeed : 2.5f;
        }
        
        // Smoothly adjust current speed
        currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, Time.deltaTime * 5f);
        
        // Apply movement
        characterController.Move(move * currentSpeed * Time.deltaTime);
    }
    
    private void HandleGravity()
    {
        float gravity = playerSettings != null ? playerSettings.Gravity : -9.81f;
        
        if (characterController.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
            isJumping = false;
        }
        
        velocity.y += gravity * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);
    }
    
    public void SetMoveInput(Vector2 input)
    {
        moveInput = input;
    }
    
    public void Jump()
    {
        if (characterController.isGrounded && !isJumping)
        {
            float jumpHeight = playerSettings != null ? playerSettings.JumpHeight : 2f;
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * (playerSettings != null ? playerSettings.Gravity : -9.81f));
            isJumping = true;
            
            Debug.Log("Player jumped");
            
            // Trigger game events
            if (gameEvents != null)
            {
                gameEvents.TriggerPlayerJump();
            }
        }
    }
    
    public void SetRunning(bool running)
    {
        isRunning = running;
        Debug.Log($"Player running: {running}");
        
        // Trigger game events
        if (gameEvents != null)
        {
            if (running)
            {
                gameEvents.TriggerPlayerRun();
            }
            else
            {
                gameEvents.TriggerPlayerWalk();
            }
        }
    }
    
    public void ToggleCrouch()
    {
        isCrouching = !isCrouching;
        
        if (isCrouching)
        {
            // Crouch down
            characterController.height = originalHeight * 0.5f;
            characterController.center = originalCenter * 0.5f;
            Debug.Log("Player crouched");
        }
        else
        {
            // Stand up
            characterController.height = originalHeight;
            characterController.center = originalCenter;
            Debug.Log("Player stood up");
        }
        
        // Trigger game events
        if (gameEvents != null)
        {
            gameEvents.TriggerPlayerCrouch();
        }
    }
    
    public void SetCrouching(bool crouching)
    {
        if (crouching != isCrouching)
        {
            ToggleCrouch();
        }
    }
}

// ScriptRole: Handles player movement, jumping, running and crouching
// Dependencies: CharacterController
// UsesSO: PlayerSettingsSO, GameEvents
// NeedsSetup: playerSettings, gameEvents 