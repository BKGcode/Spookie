using UnityEngine;
using UnityEngine.InputSystem;

namespace PlayerController
{
    public class PlayerInteraction : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerSettingsSO playerSettings;
        [SerializeField] private Camera playerCamera;
        [Header("Input (New Input System)")]
        [Tooltip("Interact action (e.g., 'E' key / gamepad button). Assign via Input System asset.")]
        [SerializeField] private InputActionReference interactAction;
        [Tooltip("Optional: Left Mouse / Gamepad South as an alternate interact.")]
        [SerializeField] private InputActionReference interactAltAction; // e.g., <Mouse>/leftButton
        [Tooltip("Tiempo necesario de mantener pulsado para completar la interacción. 0 = instantáneo (click). Si se usa PlayerSettingsSO, este valor se ignora.")]
        [SerializeField] private float holdToInteractSecondsOverride = -1f;
        [Header("Highlight (URP)")]
        [Tooltip("Enable highlighting via OutlineHighlighter on the aimed interactable.")]
        [SerializeField] private bool enableHighlight = true;
        [Tooltip("If true, only one object is highlighted at a time (the aimed one).")]
        [SerializeField] private bool singleHighlight = true;
        
        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;
        [SerializeField] private bool showInteractionRay = true;
        
        // Interaction variables
        private IInteractable currentInteractable;
        private RaycastHit lastHit;
        private bool isInteracting;
        private float interactHeldTime;
        private bool interactHeld;
        // Highlight cache
        private Game.Interaction.OutlineHighlighter currentHighlighter;
        
        // Input variables
        private bool interactPressed;
        
        // Events
        public System.Action<IInteractable> OnInteractableFound;
        public System.Action OnInteractableLost;
        public System.Action<IInteractable> OnInteractionStarted;
        public System.Action<IInteractable> OnInteractionCompleted;
        
        private void Start()
        {
            ValidateReferences();
            
            if (showDebugLogs)
                Debug.Log($"[PlayerInteraction] Initialized on {gameObject.name}");
        }
        
        private void Update()
        {
            HandleInput();
            CheckForInteractables();
            HandleInteraction();
        }
        
        private void OnEnable()
        {
            // Enable input action if assigned
            try { interactAction?.action.Enable(); } catch { }
            try { interactAltAction?.action.Enable(); } catch { }
            // Suscribir callbacks simples si se desea menos polling; mantenemos KISS con flags
            if (interactAction != null)
            {
                try
                {
                    interactAction.action.started += OnInteractStarted;
                    interactAction.action.canceled += OnInteractCanceled;
                }
                catch { }
            }
            if (interactAltAction != null)
            {
                try
                {
                    interactAltAction.action.started += OnInteractStarted;
                    interactAltAction.action.canceled += OnInteractCanceled;
                }
                catch { }
            }
        }

        private void OnDisable()
        {
            try { interactAction?.action.Disable(); } catch { }
            try { interactAltAction?.action.Disable(); } catch { }
            if (interactAction != null)
            {
                try
                {
                    interactAction.action.started -= OnInteractStarted;
                    interactAction.action.canceled -= OnInteractCanceled;
                }
                catch { }
            }
            if (interactAltAction != null)
            {
                try
                {
                    interactAltAction.action.started -= OnInteractStarted;
                    interactAltAction.action.canceled -= OnInteractCanceled;
                }
                catch { }
            }
            // Cancelar interacción en curso si el componente se deshabilita
            CancelInteraction();
            // Clear highlight on disable
            SetHighlight(null, false);
        }

        private void HandleInput()
        {
            bool down = false, held = false, up = false;
            if (interactAction != null)
            {
                try
                {
                    held |= interactAction.action.IsPressed();
                    down |= interactAction.action.WasPressedThisFrame();
                    up   |= interactAction.action.WasReleasedThisFrame();
                }
                catch { }
            }
            if (interactAltAction != null)
            {
                try
                {
                    held |= interactAltAction.action.IsPressed();
                    down |= interactAltAction.action.WasPressedThisFrame();
                    up   |= interactAltAction.action.WasReleasedThisFrame();
                }
                catch { }
            }
            // Legacy fallback only if no actions
            if (interactAction == null && interactAltAction == null)
            {
                down |= Input.GetKeyDown(KeyCode.E) || Input.GetMouseButtonDown(0);
                held |= Input.GetKey(KeyCode.E) || Input.GetMouseButton(0);
                up   |= Input.GetKeyUp(KeyCode.E) || Input.GetMouseButtonUp(0);
            }

            if (down) interactHeld = true;
            if (up)   interactHeld = false;
            interactPressed = down; // use as a click when hold time is 0
        }
        
        private void CheckForInteractables()
        {
            Vector3 rayOrigin = playerCamera.transform.position;
            Vector3 rayDirection = playerCamera.transform.forward;
            
            // Cast ray to find interactable objects
            float range = playerSettings != null ? playerSettings.InteractionRange : 3f;
            LayerMask mask = playerSettings != null ? playerSettings.InteractableLayers : Physics.DefaultRaycastLayers;
            if (Physics.Raycast(rayOrigin, rayDirection, out RaycastHit hit, range, mask, QueryTriggerInteraction.Ignore))
            {
                lastHit = hit;
                
                // Check if the hit object has an IInteractable component
                IInteractable interactable = null;
                if (!hit.collider.TryGetComponent<IInteractable>(out interactable))
                {
                    interactable = hit.collider.GetComponentInParent<IInteractable>();
                }
                
                if (interactable != null)
                {
                    if (currentInteractable != interactable)
                    {
                        // Unhighlight previous
                        if (enableHighlight)
                        {
                            SetHighlight(currentInteractable as Component, false);
                        }
                        // New interactable found
                        if (currentInteractable != null)
                        {
                            OnInteractableLost?.Invoke();
                            if (showDebugLogs)
                                Debug.Log($"[PlayerInteraction] Lost interactable: {currentInteractable.GetType().Name}");
                        }
                        
                        currentInteractable = interactable;
                        OnInteractableFound?.Invoke(interactable);
                        
                        if (enableHighlight)
                        {
                            SetHighlight(interactable as Component, true);
                        }
                        
                        if (showDebugLogs)
                            Debug.Log($"[PlayerInteraction] Found interactable: {interactable.GetType().Name} at distance: {hit.distance:F2}");
                    }
                }
                else
                {
                    // Hit object doesn't have IInteractable component
                    if (currentInteractable != null)
                    {
                        if (enableHighlight)
                        {
                            SetHighlight(currentInteractable as Component, false);
                        }
                        OnInteractableLost?.Invoke();
                        if (showDebugLogs)
                            Debug.Log($"[PlayerInteraction] Lost interactable: {currentInteractable.GetType().Name}");
                        currentInteractable = null;
                    }
                }
            }
            else
            {
                // No hit detected
                if (currentInteractable != null)
                {
                    if (enableHighlight)
                    {
                        SetHighlight(currentInteractable as Component, false);
                    }
                    OnInteractableLost?.Invoke();
                    if (showDebugLogs)
                        Debug.Log($"[PlayerInteraction] Lost interactable: {currentInteractable.GetType().Name}");
                    currentInteractable = null;
                }
            }
        }
        
        private void HandleInteraction()
        {
            if (currentInteractable == null) { interactHeldTime = 0f; return; }

            float required = GetRequiredHoldSeconds();

            // Instant interaction
            if (required <= 0f)
            {
                if (interactPressed && !isInteracting)
                {
                    StartInteraction();
                    CompleteInteraction();
                }
                return;
            }

            // Hold interaction
            if (interactHeld)
            {
                if (!isInteracting)
                {
                    StartInteraction();
                }
                interactHeldTime += Time.deltaTime;
                if (interactHeldTime >= required)
                {
                    CompleteInteraction();
                }
            }
            else
            {
                if (isInteracting)
                {
                    CancelInteraction();
                }
                interactHeldTime = 0f;
            }
        }
        
        private void StartInteraction()
        {
            isInteracting = true;
            OnInteractionStarted?.Invoke(currentInteractable);
            
            if (showDebugLogs)
                Debug.Log($"[PlayerInteraction] Starting interaction with: {currentInteractable.GetType().Name}");
        }
        
        private void CompleteInteraction()
        {
            isInteracting = false;
            currentInteractable?.Interact(gameObject);
            OnInteractionCompleted?.Invoke(currentInteractable);
            
            if (showDebugLogs)
                Debug.Log($"[PlayerInteraction] Completed interaction with: {currentInteractable.GetType().Name}");
        }

        private void CancelInteraction()
        {
            if (!isInteracting) return;
            isInteracting = false;
            interactHeldTime = 0f;
        }

        private float GetRequiredHoldSeconds()
        {
            if (playerSettings != null && playerSettings.InteractionHoldSeconds >= 0f)
                return playerSettings.InteractionHoldSeconds;
            if (holdToInteractSecondsOverride >= 0f)
                return holdToInteractSecondsOverride;
            return 0f; // instant by default
        }
        
        private void ValidateReferences()
        {
            if (playerSettings == null)
            {
                Debug.LogError("[PlayerInteraction] PlayerSettingsSO reference is missing!");
            }
            
            if (playerCamera == null)
            {
                playerCamera = Camera.main;
                if (playerCamera == null)
                {
                    Debug.LogError("[PlayerInteraction] No camera found! Please assign a camera reference.");
                }
            }
        }

        // Input callbacks (New Input System)
        private void OnInteractStarted(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
        {
            interactHeld = true;
        }

        private void OnInteractCanceled(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
        {
            interactHeld = false;
            if (isInteracting)
            {
                CancelInteraction();
            }
        }
        
        private void OnDrawGizmos()
        {
            if (showInteractionRay && playerCamera != null)
            {
                Gizmos.color = currentInteractable != null ? Color.green : Color.red;
                Vector3 rayOrigin = playerCamera.transform.position;
                float range = playerSettings != null ? playerSettings.InteractionRange : 3f;
                Vector3 rayDirection = playerCamera.transform.forward * range;
                Gizmos.DrawRay(rayOrigin, rayDirection);
                
                if (currentInteractable != null)
                {
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawWireSphere(lastHit.point, 0.1f);
                }
            }
        }
        
        // Public methods for external access
        public IInteractable GetCurrentInteractable() => currentInteractable;
        public bool HasInteractable() => currentInteractable != null;
        public bool IsInteracting() => isInteracting;
        public float GetDistanceToInteractable() => lastHit.distance;

        // Highlight helper
        private void SetHighlight(Component comp, bool state)
        {
            if (!enableHighlight) return;
            if (singleHighlight)
            {
                if (currentHighlighter != null && currentHighlighter != null)
                {
                    currentHighlighter.SetHighlighted(false);
                }
                currentHighlighter = null;
            }

            if (comp == null)
            {
                return;
            }
            var highlighter = comp.GetComponentInParent<Game.Interaction.OutlineHighlighter>();
            if (highlighter != null)
            {
                highlighter.SetHighlighted(state);
                if (state) currentHighlighter = highlighter;
            }
        }
    }
    
    // Interface for interactable objects
    public interface IInteractable
    {
        void Interact(GameObject interactor);
        string GetInteractionPrompt();
    }
}

// ScriptRole: Handles player interaction with objects using raycast detection
// RelatedScripts: PlayerMovement
// UsesSO: PlayerSettingsSO
// ReceivesFrom: Input System (E key + LeftMouse), Camera
// SendsTo: IInteractable objects; toggles OutlineHighlighter on focused object
