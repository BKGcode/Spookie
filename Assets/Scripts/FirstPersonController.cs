using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private PlayerSettingsSO playerSettings;
    [SerializeField] private LayerMask interactableLayer = -1;
    
    [Header("References")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private FeedbackMessagesSO feedbackMessages;
    
    private CharacterController characterController;
    private Vector3 velocity;
    private Vector2 moveInput;
    private Vector2 lookInput;
    private bool isJumping;
    private bool isRunning;
    private bool isCrouching;
    private float currentSpeed;
    private float originalHeight;
    private Vector3 originalCenter;
    
    private IInteractable currentInteractable;
    private bool wasInteractableInRange = false;
    
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
            Debug.LogWarning("PlayerSettings not assigned, using default values");
        }
        
        // Lock cursor for FPS experience
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        Debug.Log("FirstPersonController initialized");
    }
    
    private void Start()
    {
        if (cameraTransform == null)
        {
            cameraTransform = Camera.main.transform;
            Debug.LogWarning("Camera transform not assigned, using main camera");
        }
        
        // Validate dependencies
        DependencyValidator.ValidateFirstPersonController(this);
    }
    
    private void Update()
    {
        HandleMovement();
        HandleMouseLook();
        HandleInteraction();
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
        
        // Handle gravity and jumping
        if (characterController.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
            isJumping = false;
        }
        
        if (isJumping && characterController.isGrounded)
        {
            float jumpHeight = playerSettings != null ? playerSettings.JumpHeight : 2f;
            float jumpGravity = playerSettings != null ? playerSettings.Gravity : -9.81f;
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * jumpGravity);
        }
        
        float gravity = playerSettings != null ? playerSettings.Gravity : -9.81f;
        velocity.y += gravity * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);
    }
    
    private void HandleMouseLook()
    {
        float mouseSensitivity = playerSettings != null ? playerSettings.MouseSensitivity : 2f;
        float maxLookAngle = playerSettings != null ? playerSettings.MaxLookAngle : 80f;
        
        // Adjust sensitivity when crouching
        if (isCrouching && playerSettings != null)
        {
            mouseSensitivity = playerSettings.CrouchMouseSensitivity;
        }
        
        // Rotate player body horizontally
        transform.Rotate(Vector3.up * lookInput.x * mouseSensitivity);
        
        // Rotate camera vertically
        float currentRotation = cameraTransform.localEulerAngles.x;
        float newRotation = currentRotation - lookInput.y * mouseSensitivity;
        
        // Clamp vertical rotation
        if (newRotation > 180f)
            newRotation -= 360f;
        
        newRotation = Mathf.Clamp(newRotation, -maxLookAngle, maxLookAngle);
        cameraTransform.localEulerAngles = new Vector3(newRotation, 0f, 0f);
    }
    
    private void HandleInteraction()
    {
        // Cast ray from camera center
        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);
        RaycastHit hit;
        
        float interactionRange = playerSettings != null ? playerSettings.InteractionRange : 3f;
        bool hasHit = Physics.Raycast(ray, out hit, interactionRange, interactableLayer);
        
        if (hasHit)
        {
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();
            
            if (interactable != null)
            {
                if (currentInteractable != interactable)
                {
                    currentInteractable = interactable;
                    interactable.OnLookAt();
                    
                    if (!wasInteractableInRange)
                    {
                        Debug.Log($"Looking at interactable: {hit.collider.name}");
                        wasInteractableInRange = true;
                    }
                }
            }
            else
            {
                if (wasInteractableInRange)
                {
                    Debug.Log($"Hit object '{hit.collider.name}' but it doesn't have IInteractable component");
                    wasInteractableInRange = false;
                }
                ClearCurrentInteractable();
            }
        }
        else
        {
            if (wasInteractableInRange)
            {
                Debug.Log("No interactable object in range");
                wasInteractableInRange = false;
            }
            ClearCurrentInteractable();
        }
    }
    
    private void ClearCurrentInteractable()
    {
        if (currentInteractable != null)
        {
            currentInteractable.OnLookAway();
            currentInteractable = null;
        }
    }
    
    // Input System callbacks
    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }
    
    public void OnLook(InputValue value)
    {
        lookInput = value.Get<Vector2>();
    }
    
    public void OnJump(InputValue value)
    {
        if (value.isPressed && characterController.isGrounded && !isJumping)
        {
            isJumping = true;
            Debug.Log("Jump initiated");
        }
    }
    
    public void OnRun(InputValue value)
    {
        isRunning = value.isPressed;
        Debug.Log($"Running: {isRunning}");
    }
    
    public void OnCrouch(InputValue value)
    {
        if (value.isPressed)
        {
            ToggleCrouch();
        }
        Debug.Log($"Crouching: {isCrouching}");
    }
    
    private void ToggleCrouch()
    {
        isCrouching = !isCrouching;
        
        if (isCrouching)
        {
            // Crouch down
            characterController.height = originalHeight * 0.6f;
            characterController.center = originalCenter * 0.6f;
            
            // Adjust camera position
            if (cameraTransform != null)
            {
                cameraTransform.localPosition = new Vector3(0, 1f, 0);
            }
        }
        else
        {
            // Stand up
            characterController.height = originalHeight;
            characterController.center = originalCenter;
            
            // Restore camera position
            if (cameraTransform != null)
            {
                cameraTransform.localPosition = new Vector3(0, 1.6f, 0);
            }
        }
    }
    
    public void OnInteract(InputValue value)
    {
        if (value.isPressed && currentInteractable != null)
        {
            currentInteractable.Interact();
            Debug.Log("Interaction triggered");
        }
        else if (value.isPressed && currentInteractable == null)
        {
            Debug.LogWarning("Tried to interact but no interactable object is currently being looked at");
        }
    }
    
    public void OnToggleCursor(InputValue value)
    {
        if (value.isPressed)
        {
            Cursor.lockState = Cursor.lockState == CursorLockMode.Locked ? 
                CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = !Cursor.visible;
            Debug.Log($"Cursor locked: {Cursor.lockState == CursorLockMode.Locked}");
        }
    }
}

// ScriptRole: First-person character controller with movement, camera control, and interaction system
// Dependencies: CharacterController, Transform (camera)
// UsesSO: FeedbackMessagesSO, PlayerSettingsSO
// NeedsSetup: cameraTransform, feedbackMessages, playerSettings, interactableLayer 