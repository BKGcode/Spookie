using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    [Header("Input Actions Asset")]
    [SerializeField] private InputActionAsset inputActions;
    
    [Header("Auto Setup")]
    [SerializeField] private bool autoFindInputActions = true;
    
    private PlayerInput playerInput;
    private bool isInputEnabled = true;
    
    private void Awake()
    {
        // Try to find Input Actions Asset automatically
        if (autoFindInputActions && inputActions == null)
        {
            inputActions = Resources.Load<InputActionAsset>("InputSystem_Actions");
            if (inputActions == null)
            {
                // Try to find in Assets folder
                inputActions = FindInputActionAsset();
            }
        }
        
        // Ensure Input System is enabled
        if (inputActions == null)
        {
            Debug.LogError("Input Actions Asset not assigned to InputManager! Please assign an Input Actions Asset in the Inspector.");
            return;
        }
        
        // Enable the action map
        inputActions.Enable();
        
        Debug.Log("InputManager initialized");
    }
    
    private void Start()
    {
        ValidateInputSetup();
    }
    
    private InputActionAsset FindInputActionAsset()
    {
        // Try to find by name using Resources
        InputActionAsset[] allInputAssets = Resources.FindObjectsOfTypeAll<InputActionAsset>();
        foreach (InputActionAsset asset in allInputAssets)
        {
            if (asset != null && (asset.name.Contains("Input") || asset.name.Contains("Actions")))
            {
                Debug.Log($"Found Input Action Asset: {asset.name}");
                return asset;
            }
        }
        
        Debug.LogWarning("No Input Action Asset found automatically. Please assign one manually.");
        return null;
    }
    
    private void ValidateInputSetup()
    {
        if (inputActions == null)
        {
            Debug.LogError("Input Actions Asset is not assigned!");
            return;
        }
        
        // Check if action maps exist
        var actionMaps = inputActions.actionMaps;
        if (actionMaps.Count == 0)
        {
            Debug.LogError("Input Actions Asset has no action maps!");
            return;
        }
        
        Debug.Log($"Input Actions Asset validated. Found {actionMaps.Count} action maps.");
        
        // Log available actions for debugging
        foreach (var actionMap in actionMaps)
        {
            Debug.Log($"Action Map: {actionMap.name} with {actionMap.actions.Count} actions");
        }
    }
    
    private void OnDestroy()
    {
        if (inputActions != null)
        {
            inputActions.Disable();
        }
    }
    
    public void EnableInput()
    {
        if (inputActions != null)
        {
            inputActions.Enable();
            isInputEnabled = true;
            Debug.Log("Input enabled");
        }
        else
        {
            Debug.LogWarning("Cannot enable input - Input Actions Asset is null");
        }
    }
    
    public void DisableInput()
    {
        if (inputActions != null)
        {
            inputActions.Disable();
            isInputEnabled = false;
            Debug.Log("Input disabled");
        }
        else
        {
            Debug.LogWarning("Cannot disable input - Input Actions Asset is null");
        }
    }
    
    public bool IsInputEnabled => isInputEnabled;
    
    public InputActionAsset GetInputActions()
    {
        return inputActions;
    }
}

// ScriptRole: Manages Input System configuration and state
// Dependencies: InputActionAsset
// NeedsSetup: inputActions (auto-assigned if available) 