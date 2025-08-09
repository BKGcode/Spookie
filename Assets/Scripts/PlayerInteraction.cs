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
    [Tooltip("Tiempo necesario de mantener pulsado para completar la interacción. 0 = instantáneo (click). Si se usa PlayerSettingsSO, este valor se ignora.")]
    [SerializeField] private float holdToInteractSecondsOverride = -1f;
        
        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;
        [SerializeField] private bool showInteractionRay = true;
        
        // Interaction variables
        private IInteractable currentInteractable;
        private RaycastHit lastHit;
        private bool isInteracting;
    private float interactHeldTime;
    private bool interactHeld;
        
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
        }

        private void OnDisable()
        {
            try { interactAction?.action.Disable(); } catch { }
            if (interactAction != null)
            {
                try
                {
                    interactAction.action.started -= OnInteractStarted;
                    interactAction.action.canceled -= OnInteractCanceled;
                }
                catch { }
            }
            // Cancelar interacción en curso si el componente se deshabilita
            CancelInteraction();
        }

        private void HandleInput()
        {
            if (interactAction != null)
            {
                // Usamos flags de callbacks para soporte de "mantener pulsado"
                bool down = false;
                bool held = false;
                bool up = false;
                try
                {
                    // Por si el dispositivo no emite started/canceled correctamente, reforzamos con estado
                    held = interactAction.action.IsPressed();
                    down = interactAction.action.WasPressedThisFrame();
                    up = interactAction.action.WasReleasedThisFrame();
                }
                catch { }
                if (down) interactHeld = true;
                if (up) interactHeld = false;
                interactPressed = down; // compat: un "click" si hold time es 0
            }
            else
            {
                // Legacy fallback
                bool down = Input.GetKeyDown(KeyCode.E);
                bool held = Input.GetKey(KeyCode.E);
                bool up = Input.GetKeyUp(KeyCode.E);
                if (down) interactHeld = true;
                if (up) interactHeld = false;
                interactPressed = down;
            }
        }
        
        private void CheckForInteractables()
        {
            Vector3 rayOrigin = playerCamera.transform.position;
            Vector3 rayDirection = playerCamera.transform.forward;
            
            // Cast ray to find interactable objects
            float range = playerSettings != null ? playerSettings.InteractionRange : 3f;
            LayerMask mask = playerSettings != null ? playerSettings.InteractableLayers : Physics.DefaultRaycastLayers;
            if (Physics.Raycast(rayOrigin, rayDirection, out RaycastHit hit, range, mask))
            {
                lastHit = hit;
                
                // Check if the hit object has an IInteractable component
                IInteractable interactable = null;
                // Try on collider first, then on parent chain to be robust with child colliders
                if (!hit.collider.TryGetComponent<IInteractable>(out interactable))
                {
                    interactable = hit.collider.GetComponentInParent<IInteractable>();
                }
                
                if (interactable != null)
                {
                    if (currentInteractable != interactable)
                    {
                        // New interactable found
                        if (currentInteractable != null)
                        {
                            OnInteractableLost?.Invoke();
                            if (showDebugLogs)
                                Debug.Log($"[PlayerInteraction] Lost interactable: {currentInteractable.GetType().Name}");
                        }
                        
                        currentInteractable = interactable;
                        OnInteractableFound?.Invoke(interactable);
                        
                        if (showDebugLogs)
                            Debug.Log($"[PlayerInteraction] Found interactable: {interactable.GetType().Name} at distance: {hit.distance:F2}");
                    }
                }
                else
                {
                    // Hit object doesn't have IInteractable component
                    if (currentInteractable != null)
                    {
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

            // Interacción instantánea
            if (required <= 0f)
            {
                if (interactPressed && !isInteracting)
                {
                    StartInteraction();
                    CompleteInteraction();
                }
                return;
            }

            // Interacción mantenida (lenta)
            if (interactHeld)
            {
                if (!isInteracting)
                {
                    // Marcar inicio para eventos/feedbacks
                    StartInteraction();
                    // No completamos aún; medimos tiempo
                }
                interactHeldTime += Time.deltaTime;
                if (interactHeldTime >= required)
                {
                    CompleteInteraction();
                }
            }
            else
            {
                // Soltó antes de tiempo: cancelar si había empezado
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
            
            // En modo mantenido, la ejecución se hace al completar; en instantáneo, se completa en HandleInteraction
        }
        
        private void CompleteInteraction()
        {
            isInteracting = false;
            // Ejecutar la acción del interactuable al completar
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
            // No notificamos Completed; el emisor puede escuchar OnInteractionStarted y gestionar cancelación si fuese necesario
        }

        private float GetRequiredHoldSeconds()
        {
            if (playerSettings != null && playerSettings.InteractionHoldSeconds >= 0f)
                return playerSettings.InteractionHoldSeconds;
            if (holdToInteractSecondsOverride >= 0f)
                return holdToInteractSecondsOverride;
            return 0f; // por defecto instantáneo
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
            // Si estábamos en una interacción mantenida, cancelarla al soltar
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
// ReceivesFrom: Input System (E key), Camera
// SendsTo: IInteractable objects
