using UnityEngine;
using UnityEditor;
using UnityEngine.InputSystem;

public class FPSDebugTool : EditorWindow
{
    [MenuItem("Tools/FPS System/Debug Tool")]
    public static void ShowWindow()
    {
        GetWindow<FPSDebugTool>("FPS Debug Tool");
    }
    
    private void OnGUI()
    {
        GUILayout.Label("FPS System Debug Tool", EditorStyles.boldLabel);
        GUILayout.Space(10);
        
        if (GUILayout.Button("Check Input System", GUILayout.Height(30)))
        {
            CheckInputSystem();
        }
        
        if (GUILayout.Button("Check ScriptableObjects", GUILayout.Height(30)))
        {
            CheckScriptableObjects();
        }
        
        if (GUILayout.Button("Check Scene Setup", GUILayout.Height(30)))
        {
            CheckSceneSetup();
        }
        
        if (GUILayout.Button("Setup Feedback Messages", GUILayout.Height(30)))
        {
            SetupFeedbackMessages();
        }
        
        GUILayout.Space(10);
        
        GUILayout.Label("Debug Information:", EditorStyles.boldLabel);
        GUILayout.Label("• Check Input System: Verifies Input Actions Asset");
        GUILayout.Label("• Check ScriptableObjects: Verifies PlayerSettings and FeedbackMessages");
        GUILayout.Label("• Check Scene Setup: Verifies Player and Interactable objects");
        GUILayout.Label("• Setup Feedback Messages: Adds required messages to FeedbackMessagesSO");
    }
    
    private void CheckInputSystem()
    {
        Debug.Log("=== CHECKING INPUT SYSTEM ===");
        
        // Check Input Actions Asset
        InputActionAsset inputActions = AssetDatabase.LoadAssetAtPath<InputActionAsset>("Assets/InputSystem_Actions.inputactions");
        if (inputActions != null)
        {
            Debug.Log("✓ Input Actions Asset found");
            
            // Check Player Action Map
            var playerMap = inputActions.FindActionMap("Player");
            if (playerMap != null)
            {
                Debug.Log("✓ Player Action Map found");
                
                // Check specific actions
                CheckAction(playerMap, "Move");
                CheckAction(playerMap, "Look");
                CheckAction(playerMap, "Jump");
                CheckAction(playerMap, "Run");
                CheckAction(playerMap, "Crouch");
                CheckAction(playerMap, "Interact");
                CheckAction(playerMap, "ToggleCursor");
            }
            else
            {
                Debug.LogError("✗ Player Action Map not found!");
            }
        }
        else
        {
            Debug.LogError("✗ Input Actions Asset not found at Assets/InputSystem_Actions.inputactions");
        }
    }
    
    private void CheckAction(InputActionMap actionMap, string actionName)
    {
        var action = actionMap.FindAction(actionName);
        if (action != null)
        {
            Debug.Log($"✓ Action '{actionName}' found with {action.bindings.Count} bindings");
        }
        else
        {
            Debug.LogError($"✗ Action '{actionName}' not found!");
        }
    }
    
    private void CheckScriptableObjects()
    {
        Debug.Log("=== CHECKING SCRIPTABLEOBJECTS ===");
        
        // Check PlayerSettingsSO
        PlayerSettingsSO playerSettings = AssetDatabase.LoadAssetAtPath<PlayerSettingsSO>("Assets/PlayerSettings.asset");
        if (playerSettings != null)
        {
            Debug.Log("✓ PlayerSettingsSO found");
            Debug.Log($"  - Walk Speed: {playerSettings.WalkSpeed}");
            Debug.Log($"  - Run Speed: {playerSettings.RunSpeed}");
            Debug.Log($"  - Crouch Speed: {playerSettings.CrouchSpeed}");
            Debug.Log($"  - Jump Height: {playerSettings.JumpHeight}");
            Debug.Log($"  - Mouse Sensitivity: {playerSettings.MouseSensitivity}");
        }
        else
        {
            Debug.LogError("✗ PlayerSettingsSO not found at Assets/PlayerSettings.asset");
        }
        
        // Check FeedbackMessagesSO
        FeedbackMessagesSO feedbackMessages = AssetDatabase.LoadAssetAtPath<FeedbackMessagesSO>("Assets/FeedbackMessages.asset");
        if (feedbackMessages != null)
        {
            Debug.Log("✓ FeedbackMessagesSO found");
            
            // Check required messages
            string[] requiredMessages = { "press_to_interact", "interaction_success" };
            foreach (string message in requiredMessages)
            {
                string result = feedbackMessages.GetMessage(message);
                if (result != "Message not found")
                {
                    Debug.Log($"✓ Message '{message}' found");
                }
                else
                {
                    Debug.LogWarning($"⚠ Message '{message}' not found");
                }
            }
        }
        else
        {
            Debug.LogError("✗ FeedbackMessagesSO not found at Assets/FeedbackMessages.asset");
        }
    }
    
    private void CheckSceneSetup()
    {
        Debug.Log("=== CHECKING SCENE SETUP ===");
        
        // Check for Player Components
        FPSPlayerMovement playerMovement = Object.FindFirstObjectByType<FPSPlayerMovement>();
        PlayerCamera playerCamera = Object.FindFirstObjectByType<PlayerCamera>();
        PlayerInteraction playerInteraction = Object.FindFirstObjectByType<PlayerInteraction>();
        PlayerInput playerInput = Object.FindFirstObjectByType<PlayerInput>();
        
        if (playerMovement != null && playerCamera != null && playerInteraction != null && playerInput != null)
        {
            Debug.Log("✓ All player components found in scene");
            
            // Check components
            CharacterController characterController = playerMovement.GetComponent<CharacterController>();
            if (characterController != null)
                Debug.Log("✓ CharacterController found");
            else
                Debug.LogError("✗ CharacterController missing!");
                
            if (playerInput != null)
                Debug.Log("✓ PlayerInput found");
            else
                Debug.LogError("✗ PlayerInput missing!");
        }
        else
        {
            Debug.LogError("✗ Missing player components in scene");
        }
        
        // Check for InteractableObjects
        InteractableObject[] interactables = Object.FindObjectsByType<InteractableObject>(FindObjectsSortMode.None);
        if (interactables.Length > 0)
        {
            Debug.Log($"✓ Found {interactables.Length} InteractableObject(s) in scene");
            foreach (var interactable in interactables)
            {
                Debug.Log($"  - {interactable.name}");
            }
        }
        else
        {
            Debug.LogWarning("⚠ No InteractableObject found in scene");
        }
    }
    
    private void SetupFeedbackMessages()
    {
        Debug.Log("=== SETTING UP FEEDBACK MESSAGES ===");
        
        // Try to find existing FeedbackMessagesSO
        FeedbackMessagesSO feedbackMessages = AssetDatabase.LoadAssetAtPath<FeedbackMessagesSO>("Assets/FeedbackMessages.asset");
        
        if (feedbackMessages == null)
        {
            // Create new FeedbackMessagesSO
            feedbackMessages = ScriptableObject.CreateInstance<FeedbackMessagesSO>();
            AssetDatabase.CreateAsset(feedbackMessages, "Assets/FeedbackMessages.asset");
            Debug.Log("Created new FeedbackMessagesSO at Assets/FeedbackMessages.asset");
        }
        
        // Add required messages
        feedbackMessages.AddMessage("press_to_interact", "Press E to interact");
        feedbackMessages.AddMessage("interaction_success", "Object interacted!");
        feedbackMessages.AddMessage("jump_message", "Jump!");
        feedbackMessages.AddMessage("crouch_message", "Crouching");
        feedbackMessages.AddMessage("run_message", "Running");
        feedbackMessages.AddMessage("walk_message", "Walking");
        feedbackMessages.AddMessage("cursor_locked", "Cursor locked");
        feedbackMessages.AddMessage("cursor_unlocked", "Cursor unlocked");
        
        // Mark as dirty and save
        EditorUtility.SetDirty(feedbackMessages);
        AssetDatabase.SaveAssets();
        
        Debug.Log("✓ FeedbackMessagesSO setup completed!");
    }
}

// ScriptRole: Editor debug tool for FPS system verification
// Dependencies: All FPS system components
// UsesSO: PlayerSettingsSO, FeedbackMessagesSO, InputActionAsset
// NeedsSetup: Run from Tools menu to debug system 