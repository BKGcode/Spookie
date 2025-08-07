using UnityEngine;

namespace PlayerController
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMovement : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerSettingsSO playerSettings;
        [SerializeField] private Transform cameraTransform;
        
        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;
        
        // Components
        private CharacterController characterController;
        
        // Movement variables
        private Vector3 moveDirection;
        private float verticalVelocity;
        private bool isGrounded;
        private bool isCrouching;
        private bool isSprinting;
        
        // Penalty system
        private float speedMultiplier = 1f;
        private bool canSprint = true;
        
        // Input variables
        private Vector2 inputVector;
        private bool jumpPressed;
        private bool sprintPressed;
        private bool crouchPressed;
        
        // Events
        public System.Action<bool> OnGroundedChanged;
        public System.Action<bool> OnCrouchChanged;
        public System.Action<bool> OnSprintChanged;
        
        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
            
            if (showDebugLogs)
                Debug.Log($"[PlayerMovement] Initialized on {gameObject.name}");
        }
        
        private void Start()
        {
            ValidateReferences();
            LockCursor();
        }
        
        private void Update()
        {
            HandleInput();
            HandleMovement();
            HandleJump();
            HandleCrouch();
            HandleSprint();
        }
        
        private void HandleInput()
        {
            // Movement input
            inputVector.x = Input.GetAxis("Horizontal");
            inputVector.y = Input.GetAxis("Vertical");
            
            // Action inputs
            jumpPressed = Input.GetKeyDown(KeyCode.Space);
            sprintPressed = Input.GetKey(KeyCode.LeftShift);
            crouchPressed = Input.GetKey(KeyCode.LeftControl);
        }
        
        private void HandleMovement()
        {
            // Check if grounded
            bool wasGrounded = isGrounded;
            isGrounded = characterController.isGrounded;
            
            if (wasGrounded != isGrounded)
            {
                OnGroundedChanged?.Invoke(isGrounded);
                if (showDebugLogs)
                    Debug.Log($"[PlayerMovement] Grounded: {isGrounded}");
            }
            
            // Calculate movement direction based on camera orientation
            Vector3 cameraForward = cameraTransform != null ? cameraTransform.forward : transform.forward;
            Vector3 cameraRight = cameraTransform != null ? cameraTransform.right : transform.right;
            
            // Flatten the vectors to ignore Y component (camera pitch)
            cameraForward.y = 0f;
            cameraRight.y = 0f;
            cameraForward.Normalize();
            cameraRight.Normalize();
            
            // Calculate movement direction
            Vector3 forward = cameraForward * inputVector.y;
            Vector3 right = cameraRight * inputVector.x;
            moveDirection = (forward + right).normalized;
            
            // Apply speed based on state
            float currentSpeed = GetCurrentSpeed();
            moveDirection *= currentSpeed;
            
            // Apply gravity
            if (isGrounded && verticalVelocity < 0)
            {
                verticalVelocity = -2f; // Small downward force when grounded
            }
            else
            {
                // Clamp delta time to prevent huge jumps
                float deltaTime = Mathf.Clamp(Time.deltaTime, 0f, 0.1f);
                verticalVelocity += playerSettings.Gravity * deltaTime;
            }
            
            // Apply vertical velocity
            moveDirection.y = verticalVelocity;
            
            // Move character
            // Clamp delta time to prevent huge jumps
            float moveDeltaTime = Mathf.Clamp(Time.deltaTime, 0f, 0.1f);
            characterController.Move(moveDirection * moveDeltaTime);
        }
        
        private void HandleJump()
        {
            if (jumpPressed && isGrounded && !isCrouching)
            {
                verticalVelocity = playerSettings.JumpForce;
                
                if (showDebugLogs)
                    Debug.Log($"[PlayerMovement] Jump executed with force: {playerSettings.JumpForce}");
            }
        }
        
        private void HandleCrouch()
        {
            bool wasCrouching = isCrouching;
            isCrouching = crouchPressed;
            
            if (wasCrouching != isCrouching)
            {
                if (isCrouching)
                {
                    Crouch();
                }
                else
                {
                    StandUp();
                }
                
                OnCrouchChanged?.Invoke(isCrouching);
                
                if (showDebugLogs)
                    Debug.Log($"[PlayerMovement] Crouch state changed: {isCrouching}");
            }
        }
        
        private void HandleSprint()
        {
            bool wasSprinting = isSprinting;
            isSprinting = sprintPressed && !isCrouching && inputVector.magnitude > 0.1f && canSprint;
            
            if (wasSprinting != isSprinting)
            {
                OnSprintChanged?.Invoke(isSprinting);
                
                if (showDebugLogs)
                    Debug.Log($"[PlayerMovement] Sprint state changed: {isSprinting}");
            }
        }
        
        private void Crouch()
        {
            characterController.height = playerSettings.CrouchHeight;
            characterController.center = new Vector3(0, playerSettings.CrouchHeight / 2f, 0);
            
            // Lower camera if available
            if (cameraTransform != null)
            {
                Vector3 cameraPos = cameraTransform.localPosition;
                cameraPos.y = playerSettings.CrouchHeight - 0.5f;
                cameraTransform.localPosition = cameraPos;
            }
        }
        
        private void StandUp()
        {
            // Check if there's enough space to stand up
            if (Physics.CheckSphere(transform.position + Vector3.up * playerSettings.StandHeight, 0.1f))
            {
                if (showDebugLogs)
                    Debug.LogWarning("[PlayerMovement] Cannot stand up - obstacle detected above");
                return;
            }
            
            characterController.height = playerSettings.StandHeight;
            characterController.center = new Vector3(0, playerSettings.StandHeight / 2f, 0);
            
            // Restore camera position
            if (cameraTransform != null)
            {
                Vector3 cameraPos = cameraTransform.localPosition;
                cameraPos.y = playerSettings.StandHeight - 0.5f;
                cameraTransform.localPosition = cameraPos;
            }
        }
        
        private float GetCurrentSpeed()
        {
            float baseSpeed;
            if (isCrouching)
                baseSpeed = playerSettings.CrouchSpeed;
            else if (isSprinting)
                baseSpeed = playerSettings.SprintSpeed;
            else
                baseSpeed = playerSettings.WalkSpeed;
            
            // Apply penalty multiplier
            return baseSpeed * speedMultiplier;
        }
        
        private void ValidateReferences()
        {
            if (playerSettings == null)
            {
                Debug.LogError("[PlayerMovement] PlayerSettingsSO reference is missing!");
            }
            
            if (cameraTransform == null)
            {
                Debug.LogWarning("[PlayerMovement] Camera Transform reference is missing - camera won't move with crouch");
            }
        }
        
        private void LockCursor()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        
        // Public methods for external access
        public bool IsGrounded => isGrounded;
        public bool IsCrouching => isCrouching;
        public bool IsSprinting => isSprinting;
        public Vector3 GetMoveDirection() => moveDirection;
        public float GetCurrentSpeedValue() => GetCurrentSpeed();
        
        // Penalty system methods
        public void SetSpeedMultiplier(float multiplier)
        {
            speedMultiplier = Mathf.Clamp01(multiplier);
            if (showDebugLogs)
                Debug.Log($"[PlayerMovement] Speed multiplier set to: {speedMultiplier}");
        }
        
        public void SetCanSprint(bool canSprint)
        {
            this.canSprint = canSprint;
            if (!canSprint && isSprinting)
            {
                isSprinting = false;
                OnSprintChanged?.Invoke(false);
            }
            if (showDebugLogs)
                Debug.Log($"[PlayerMovement] Can sprint set to: {canSprint}");
        }
    }
}

// ScriptRole: Handles player movement, jumping, crouching, and sprinting
// RelatedScripts: MouseLook, PlayerInteraction
// UsesSO: PlayerSettingsSO
// ReceivesFrom: Input System
// SendsTo: CharacterController, Camera Transform
