using UnityEngine;
using TMPro;

public class AutoSetupManager : MonoBehaviour
{
    [Header("Auto Setup")]
    [SerializeField] private bool autoSetupOnStart = true;
    [SerializeField] private bool validateOnStart = true;
    
    [Header("Default References")]
    [SerializeField] private PlayerSettingsSO defaultPlayerSettings;
    [SerializeField] private FeedbackMessagesSO defaultFeedbackMessages;
    
    private void Start()
    {
        if (autoSetupOnStart)
        {
            AutoSetup();
        }
        
        if (validateOnStart)
        {
            ValidateAllComponents();
        }
    }
    
    private void AutoSetup()
    {
        Debug.Log("Starting auto setup...");
        
        // Find ScriptableObjects first
        FindScriptableObjects();
        
        // Setup Player Components
        SetupPlayerComponents();
        
        // Setup InteractableObjects
        SetupInteractableObjects();
        
        // Setup InteractionUI
        SetupInteractionUI();
        
        Debug.Log("Auto setup completed!");
    }
    
    private void FindScriptableObjects()
    {
        // Find PlayerSettingsSO
        if (defaultPlayerSettings == null)
        {
            defaultPlayerSettings = FindScriptableObject<PlayerSettingsSO>();
            if (defaultPlayerSettings != null)
            {
                Debug.Log($"Found PlayerSettingsSO: {defaultPlayerSettings.name}");
            }
        }
        
        // Find FeedbackMessagesSO
        if (defaultFeedbackMessages == null)
        {
            defaultFeedbackMessages = FindScriptableObject<FeedbackMessagesSO>();
            if (defaultFeedbackMessages != null)
            {
                Debug.Log($"Found FeedbackMessagesSO: {defaultFeedbackMessages.name}");
            }
        }
    }
    
    private void SetupPlayerComponents()
    {
        // Setup FPSPlayerMovement
        FPSPlayerMovement playerMovement = FindFirstObjectByType<FPSPlayerMovement>();
        if (playerMovement != null)
        {
            if (defaultPlayerSettings != null)
            {
                var playerSettingsField = typeof(FPSPlayerMovement).GetField("playerSettings", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (playerSettingsField?.GetValue(playerMovement) == null)
                {
                    playerSettingsField?.SetValue(playerMovement, defaultPlayerSettings);
                    Debug.Log("Assigned PlayerSettingsSO to FPSPlayerMovement");
                }
            }
        }
        
        // Setup PlayerCamera
        PlayerCamera playerCamera = FindObjectOfType<PlayerCamera>();
        if (playerCamera != null)
        {
            if (defaultPlayerSettings != null)
            {
                var playerSettingsField = typeof(PlayerCamera).GetField("playerSettings", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (playerSettingsField?.GetValue(playerCamera) == null)
                {
                    playerSettingsField?.SetValue(playerCamera, defaultPlayerSettings);
                    Debug.Log("Assigned PlayerSettingsSO to PlayerCamera");
                }
            }
        }
        
        // Setup PlayerInteraction
        PlayerInteraction playerInteraction = FindObjectOfType<PlayerInteraction>();
        if (playerInteraction != null)
        {
            if (defaultPlayerSettings != null)
            {
                var playerSettingsField = typeof(PlayerInteraction).GetField("playerSettings", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (playerSettingsField?.GetValue(playerInteraction) == null)
                {
                    playerSettingsField?.SetValue(playerInteraction, defaultPlayerSettings);
                    Debug.Log("Assigned PlayerSettingsSO to PlayerInteraction");
                }
            }
            
            if (defaultFeedbackMessages != null)
            {
                var feedbackField = typeof(PlayerInteraction).GetField("feedbackMessages", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (feedbackField?.GetValue(playerInteraction) == null)
                {
                    feedbackField?.SetValue(playerInteraction, defaultFeedbackMessages);
                    Debug.Log("Assigned FeedbackMessagesSO to PlayerInteraction");
                }
            }
                }
        
        Debug.Log("Player components auto setup completed");
    }
    
    private void SetupInteractableObjects()
    {
        InteractableObject[] interactables = FindObjectsByType<InteractableObject>(FindObjectsSortMode.None);
        foreach (var interactable in interactables)
        {
            // Assign renderer if not set
            var rendererField = typeof(InteractableObject).GetField("objectRenderer", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (rendererField?.GetValue(interactable) == null)
            {
                rendererField?.SetValue(interactable, interactable.GetComponent<Renderer>());
            }
            
            // Assign FeedbackMessagesSO if not set
            if (defaultFeedbackMessages != null)
            {
                var feedbackField = typeof(InteractableObject).GetField("feedbackMessages", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (feedbackField?.GetValue(interactable) == null)
                {
                    feedbackField?.SetValue(interactable, defaultFeedbackMessages);
                }
            }
        }
        
        Debug.Log($"Setup {interactables.Length} InteractableObjects");
    }
    
    private void SetupInteractionUI()
    {
        InteractionUI[] interactionUIs = FindObjectsOfType<InteractionUI>();
        foreach (var interactionUI in interactionUIs)
        {
            // Assign FeedbackMessagesSO if not set
            if (defaultFeedbackMessages != null)
            {
                var feedbackField = typeof(InteractionUI).GetField("feedbackMessages", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (feedbackField?.GetValue(interactionUI) == null)
                {
                    feedbackField?.SetValue(interactionUI, defaultFeedbackMessages);
                }
            }
            
            // Find TextMeshProUGUI if not set
            var textField = typeof(InteractionUI).GetField("interactionText", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (textField?.GetValue(interactionUI) == null)
            {
                TextMeshProUGUI text = interactionUI.GetComponentInChildren<TextMeshProUGUI>();
                if (text != null)
                {
                    textField?.SetValue(interactionUI, text);
                }
            }
        }
        
        Debug.Log($"Setup {interactionUIs.Length} InteractionUIs");
    }
    
    private T FindScriptableObject<T>() where T : ScriptableObject
    {
        // Try to find in Resources folder first
        T[] allScriptableObjects = Resources.FindObjectsOfTypeAll<T>();
        if (allScriptableObjects.Length > 0)
        {
            Debug.Log($"Found {typeof(T).Name} in Resources: {allScriptableObjects[0].name}");
            return allScriptableObjects[0];
        }
        
        // Try to find in scene (if any are instantiated)
        T[] sceneObjects = FindObjectsOfType<T>();
        if (sceneObjects.Length > 0)
        {
            Debug.Log($"Found {typeof(T).Name} in scene: {sceneObjects[0].name}");
            return sceneObjects[0];
        }
        
        Debug.LogWarning($"Could not find {typeof(T).Name} automatically. Please assign manually in Inspector.");
        return null;
    }
    
    private void ValidateAllComponents()
    {
        Debug.Log("Validating all components...");
        
        // Validate Player Components
        FPSPlayerMovement playerMovement = FindFirstObjectByType<FPSPlayerMovement>();
        if (playerMovement != null)
        {
            DependencyValidator.ValidatePlayerMovement(playerMovement);
        }
        
        PlayerCamera playerCamera = FindFirstObjectByType<PlayerCamera>();
        if (playerCamera != null)
        {
            DependencyValidator.ValidatePlayerCamera(playerCamera);
        }
        
        PlayerInteraction playerInteraction = FindFirstObjectByType<PlayerInteraction>();
        if (playerInteraction != null)
        {
            DependencyValidator.ValidatePlayerInteraction(playerInteraction);
        }
        
        // Validate InteractableObjects
        InteractableObject[] interactables = FindObjectsByType<InteractableObject>(FindObjectsSortMode.None);
        foreach (var interactable in interactables)
        {
            DependencyValidator.ValidateInteractableObject(interactable);
        }
        
        // Validate InteractionUI
        InteractionUI[] interactionUIs = FindObjectsOfType<InteractionUI>();
        foreach (var interactionUI in interactionUIs)
        {
            DependencyValidator.ValidateInteractionUI(interactionUI);
        }
        
        Debug.Log("Component validation completed!");
    }
}

// ScriptRole: Automates setup and validation of FPS system components
// Dependencies: None
// UsesSO: PlayerSettingsSO, FeedbackMessagesSO
// NeedsSetup: defaultPlayerSettings, defaultFeedbackMessages 