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
        
        // Setup FirstPersonController
        SetupFirstPersonController();
        
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
    
    private void SetupFirstPersonController()
    {
        FirstPersonController controller = FindObjectOfType<FirstPersonController>();
        if (controller != null)
        {
            // Assign PlayerSettingsSO if found
            if (defaultPlayerSettings != null)
            {
                var playerSettingsField = typeof(FirstPersonController).GetField("playerSettings", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (playerSettingsField?.GetValue(controller) == null)
                {
                    playerSettingsField?.SetValue(controller, defaultPlayerSettings);
                    Debug.Log("Assigned PlayerSettingsSO to FirstPersonController");
                }
            }
            
            // Assign FeedbackMessagesSO if found
            if (defaultFeedbackMessages != null)
            {
                var feedbackField = typeof(FirstPersonController).GetField("feedbackMessages", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (feedbackField?.GetValue(controller) == null)
                {
                    feedbackField?.SetValue(controller, defaultFeedbackMessages);
                    Debug.Log("Assigned FeedbackMessagesSO to FirstPersonController");
                }
            }
            
            // Assign camera if not set
            if (controller.GetComponentInChildren<Camera>() != null)
            {
                var cameraField = typeof(FirstPersonController).GetField("cameraTransform", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (cameraField?.GetValue(controller) == null)
                {
                    cameraField?.SetValue(controller, controller.GetComponentInChildren<Camera>().transform);
                    Debug.Log("Assigned camera transform to FirstPersonController");
                }
            }
            
            Debug.Log("FirstPersonController auto setup completed");
        }
        else
        {
            Debug.LogWarning("No FirstPersonController found in scene");
        }
    }
    
    private void SetupInteractableObjects()
    {
        InteractableObject[] interactables = FindObjectsOfType<InteractableObject>();
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
        
        // Validate FirstPersonController
        FirstPersonController controller = FindObjectOfType<FirstPersonController>();
        if (controller != null)
        {
            DependencyValidator.ValidateFirstPersonController(controller);
        }
        
        // Validate InteractableObjects
        InteractableObject[] interactables = FindObjectsOfType<InteractableObject>();
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