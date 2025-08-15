using UnityEngine;
using UnityEngine.InputSystem;
using Game.Core; // GameConfigProvider

namespace PlayerController
{
    [RequireComponent(typeof(CharacterController))]
    [AddComponentMenu("Spookie/Player Movement")]
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
        
    [Header("Sprint Settings")]
    [Tooltip("Si está activo, debes mantener pulsado Sprint para correr. Si se desactiva, Sprint funciona como toggle (pulsar para activar/desactivar).")]
    [SerializeField] private bool holdToSprint = true;
        
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

    // Transition and speed easing now come from PlayerSettingsSO

    // Crouch state progress 0..1 (0 = standing, 1 = crouching)
    private float crouchProgress = 0f;
        
        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
            
            if (showDebugLogs)
                Debug.Log($"[PlayerMovement] Initialized on {gameObject.name}");
        }
        
        private void Start()
        {
            ValidateReferences();
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
            // Procesar estados antes del movimiento para que surtan efecto en el mismo frame
            HandleCrouch();
            UpdateCrouchTransition();
            HandleSprint();
            HandleJump();
            HandleMovement();
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
                try
                {
                    var action = sprintAction.action;
                    if (holdToSprint)
                    {
                        sprintPressed = action.IsPressed();
                    }
                    else
                    {
                        // Toggle sprint on press
                        if (action.WasPressedThisFrame())
                        {
                            sprintPressed = !sprintPressed;
                        }
                        // Optional: if player releases while not moving, keep state; no-op on release
                    }
                }
                catch { sprintPressed = false; }
            }
            else
            {
                if (holdToSprint)
                {
                    sprintPressed = Input.GetKey(KeyCode.LeftShift);
                }
                else
                {
                    if (Input.GetKeyDown(KeyCode.LeftShift))
                        sprintPressed = !sprintPressed;
                }
            }

            if (crouchAction != null)
            {
                bool cp = false;
                try
                {
                    var action = crouchAction.action;
                    cp = action.IsPressed();
                    if (!cp)
                    {
                        float v = 0f;
                        try { v = action.ReadValue<float>(); } catch { v = 0f; }
                        cp = v > 0.5f;
                    }
                }
                catch { cp = false; }
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
            // Target crouch; transition handled in UpdateCrouchTransition
        }
        
        private void StandUp()
        {
            // Check if there's enough space to stand up usando cápsula del CharacterController
            if (!HasSpaceToStand())
            {
                if (showDebugLogs)
                    Debug.LogWarning("[PlayerMovement] Cannot stand up - obstacle detected above");
                // Revert intended state to keep logic consistent (still crouching)
                isCrouching = true;
                return;
            }
            // Target stand; transition handled in UpdateCrouchTransition
        }

        // Smoothly updates crouchProgress and applies CC height/center and camera Y
        private void UpdateCrouchTransition()
        {
            if (playerSettings == null || characterController == null) return;

            float target = isCrouching ? 1f : 0f;
            float crouchTransitionDuration = playerSettings != null ? playerSettings.CrouchTransitionDuration : 0f;
            if (crouchTransitionDuration <= 0f)
            {
                crouchProgress = target;
            }
            else
            {
                float step = Time.deltaTime / Mathf.Max(0.0001f, crouchTransitionDuration);
                crouchProgress = Mathf.MoveTowards(crouchProgress, target, step);
            }

            float t = Mathf.Clamp01(crouchProgress);
            var curve = playerSettings != null ? playerSettings.CrouchEaseCurve : AnimationCurve.Linear(0, 0, 1, 1);
            float shaped = Mathf.Clamp01(curve.Evaluate(t));

            // Apply CharacterController height and center
            float height = Mathf.Lerp(playerSettings.StandHeight, playerSettings.CrouchHeight, shaped);
            characterController.height = height;
            characterController.center = new Vector3(0f, height * 0.5f, 0f);

            // Apply camera local Y if assigned
            if (cameraTransform != null)
            {
                Vector3 camLocal = cameraTransform.localPosition;
                camLocal.y = Mathf.Lerp(playerSettings.StandHeight - 0.5f, playerSettings.CrouchHeight - 0.5f, shaped);
                cameraTransform.localPosition = camLocal;
            }
        }

        // Verifica con una cápsula si hay espacio suficiente para ponerse de pie
        private bool HasSpaceToStand()
        {
            if (playerSettings == null || characterController == null) return false;

            float radius = Mathf.Max(0f, characterController.radius - 0.05f);
            float standHeight = Mathf.Max(playerSettings.StandHeight, radius * 2f + 0.1f);

            // Calculate feet plane from current capsule to anchor the stand capsule to the same feet
            Vector3 up = transform.up;
            Vector3 currentWorldCenter = transform.TransformPoint(characterController.center);
            float currentHalfHeight = characterController.height * 0.5f;
            float feetPlaneY = currentWorldCenter.y - currentHalfHeight; // lowest point of current capsule
            Vector3 feet = new Vector3(currentWorldCenter.x, feetPlaneY, currentWorldCenter.z);

            // Build the would-be standing capsule: pass sphere centers for bottom/top
            Vector3 bottom = feet + up * radius;                 // bottom sphere center
            Vector3 top = feet + up * (standHeight - radius);    // top sphere center

            // Ignorar triggers y usar capas por defecto
            int mask = Physics.DefaultRaycastLayers & ~(1 << gameObject.layer); // exclude player's own layer
            bool blocked = Physics.CheckCapsule(bottom, top, radius, mask, QueryTriggerInteraction.Ignore);
            return !blocked;
        }
        
        private float GetCurrentSpeed()
        {
            if (playerSettings == null)
                return 0f;

            float standBase = isSprinting ? playerSettings.SprintSpeed : playerSettings.WalkSpeed;
            float crouchBase = playerSettings.CrouchSpeed;
            float t = Mathf.Clamp01(crouchProgress);
            var curve = playerSettings != null ? playerSettings.CrouchEaseCurve : AnimationCurve.Linear(0,0,1,1);
            float shaped = Mathf.Clamp01(curve.Evaluate(t));
            float blended = Mathf.Lerp(standBase, crouchBase, shaped);

            // Edge slowdown near start/end of crouch transition
            float edge = 1f - 4f * t * (1f - t); // 1 at edges, 0 at middle
            float potency = playerSettings != null ? playerSettings.SpeedEdgeSlowPotency : 0.3f;
            float edgeMultiplier = 1f - (potency * Mathf.Clamp01(edge));

            return blended * Mathf.Clamp(edgeMultiplier, 0.1f, 1f) * speedMultiplier;
        }
        
        private void ValidateReferences()
        {
            // Optional: auto-wire from GameConfigProvider if left unassigned
            if (playerSettings == null)
            {
                var provider = FindObjectOfType<GameConfigProvider>();
                var cfg = provider != null ? provider.Config : null;
                if (cfg != null) playerSettings = cfg.PlayerSettings;
            }

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
            // Obsoleto: el cursor se gestiona en MouseLook si es necesario.
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
