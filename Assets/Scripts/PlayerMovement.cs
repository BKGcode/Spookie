using UnityEngine;
using UnityEngine.InputSystem;

namespace PlayerController
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMovement : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerSettingsSO playerSettings;
        [SerializeField] private Transform cameraTransform;
        
    [Header("Input (New Input System)")]
    [Tooltip("Acción Vector2 de movimiento (WASD/Stick Izquierdo). Asignar desde el asset Input System.")]
    [SerializeField] private InputActionReference moveAction;
    [Tooltip("Acción de salto (Space/ButtonSouth).")]
    [SerializeField] private InputActionReference jumpAction;
    [Tooltip("Acción de sprint (LeftShift/StickPress). Mantenida para correr.")]
    [SerializeField] private InputActionReference sprintAction;
    [Tooltip("Acción de agacharse (C/Botón). Mantenida para agacharse.")]
    [SerializeField] private InputActionReference crouchAction;
        
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
        
        private void OnEnable()
        {
            // Habilitar acciones si están asignadas (si las gestiona PlayerInput no pasa nada)
            try { moveAction?.action.Enable(); } catch { }
            try { jumpAction?.action.Enable(); } catch { }
            try { sprintAction?.action.Enable(); } catch { }
            try { crouchAction?.action.Enable(); } catch { }
        }
        
        private void OnDisable()
        {
            // Deshabilitar acciones para liberar dispositivos
            try { moveAction?.action.Disable(); } catch { }
            try { jumpAction?.action.Disable(); } catch { }
            try { sprintAction?.action.Disable(); } catch { }
            try { crouchAction?.action.Disable(); } catch { }
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
            // Movement input (New Input System, con fallback a legacy si no está asignado)
            if (moveAction != null)
            {
                Vector2 move = Vector2.zero;
                try { move = moveAction.action.ReadValue<Vector2>(); } catch { move = Vector2.zero; }
                inputVector = move;
            }
            else
            {
                inputVector.x = Input.GetAxis("Horizontal");
                inputVector.y = Input.GetAxis("Vertical");
            }

            // Action inputs
            if (jumpAction != null)
            {
                bool jp = false; try { jp = jumpAction.action.WasPressedThisFrame(); } catch { jp = false; }
                jumpPressed = jp;
            }
            else
            {
                jumpPressed = Input.GetKeyDown(KeyCode.Space);
            }

            if (sprintAction != null)
            {
                bool sp = false; try { sp = sprintAction.action.IsPressed(); } catch { sp = false; }
                sprintPressed = sp;
            }
            else
            {
                sprintPressed = Input.GetKey(KeyCode.LeftShift);
            }

            if (crouchAction != null)
            {
                bool cp = false; try { cp = crouchAction.action.IsPressed(); } catch { cp = false; }
                crouchPressed = cp;
            }
            else
            {
                crouchPressed = Input.GetKey(KeyCode.LeftControl);
            }
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
            
            // Avisar si no hay acciones asignadas (seguirán funcionando con fallback legacy)
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (moveAction == null || jumpAction == null || sprintAction == null || crouchAction == null)
            {
                if (showDebugLogs)
                {
                    Debug.Log("[PlayerMovement] Falta asignar alguna InputActionReference (Move/Jump/Sprint/Crouch). Usando fallback legacy temporal.");
                }
            }
            #endif
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
// ReceivesFrom: Input System (Move/Jump/Sprint/Crouch)
// SendsTo: CharacterController, Camera Transform
