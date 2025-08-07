using UnityEngine;

namespace PlayerController
{
    public class PlayerInteraction : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerSettingsSO playerSettings;
        [SerializeField] private Camera playerCamera;
        
        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;
        [SerializeField] private bool showInteractionRay = true;
        
        // Interaction variables
        private IInteractable currentInteractable;
        private RaycastHit lastHit;
        private bool isInteracting;
        
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
        
        private void HandleInput()
        {
            interactPressed = Input.GetKeyDown(KeyCode.E);
        }
        
        private void CheckForInteractables()
        {
            Vector3 rayOrigin = playerCamera.transform.position;
            Vector3 rayDirection = playerCamera.transform.forward;
            
            // Cast ray to find interactable objects
            if (Physics.Raycast(rayOrigin, rayDirection, out RaycastHit hit, playerSettings.InteractionRange, playerSettings.InteractableLayers))
            {
                lastHit = hit;
                
                // Check if the hit object has an IInteractable component
                IInteractable interactable = hit.collider.GetComponent<IInteractable>();
                
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
            if (interactPressed && currentInteractable != null && !isInteracting)
            {
                StartInteraction();
            }
        }
        
        private void StartInteraction()
        {
            isInteracting = true;
            OnInteractionStarted?.Invoke(currentInteractable);
            
            if (showDebugLogs)
                Debug.Log($"[PlayerInteraction] Starting interaction with: {currentInteractable.GetType().Name}");
            
            // Call the interactable's Interact method
            currentInteractable.Interact(gameObject);
            
            // For now, we'll assume interaction is immediate
            // In a more complex system, you might want to handle async interactions
            CompleteInteraction();
        }
        
        private void CompleteInteraction()
        {
            isInteracting = false;
            OnInteractionCompleted?.Invoke(currentInteractable);
            
            if (showDebugLogs)
                Debug.Log($"[PlayerInteraction] Completed interaction with: {currentInteractable.GetType().Name}");
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
        
        private void OnDrawGizmos()
        {
            if (showInteractionRay && playerCamera != null)
            {
                Gizmos.color = currentInteractable != null ? Color.green : Color.red;
                Vector3 rayOrigin = playerCamera.transform.position;
                Vector3 rayDirection = playerCamera.transform.forward * playerSettings.InteractionRange;
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
