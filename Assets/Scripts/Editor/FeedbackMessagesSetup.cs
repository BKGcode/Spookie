using UnityEngine;
using UnityEditor;

public class FeedbackMessagesSetup : EditorWindow
{
    [MenuItem("Tools/FPS System/Setup Feedback Messages")]
    public static void ShowWindow()
    {
        GetWindow<FeedbackMessagesSetup>("Feedback Messages Setup");
    }
    
    private void OnGUI()
    {
        GUILayout.Label("FPS System Setup", EditorStyles.boldLabel);
        GUILayout.Space(10);
        
        GUILayout.Label("This tool will create or update the ScriptableObjects required for the FPS system.");
        GUILayout.Space(10);
        
        if (GUILayout.Button("Setup Feedback Messages", GUILayout.Height(30)))
        {
            SetupFeedbackMessages();
        }
        
        GUILayout.Space(5);
        
        if (GUILayout.Button("Setup Player Settings", GUILayout.Height(30)))
        {
            SetupPlayerSettings();
        }
        
        GUILayout.Space(5);
        
        if (GUILayout.Button("Setup All ScriptableObjects", GUILayout.Height(40)))
        {
            SetupAllScriptableObjects();
        }
        
        GUILayout.Space(10);
        
        GUILayout.Label("Required Messages:");
        GUILayout.Label("• press_to_interact");
        GUILayout.Label("• interaction_success");
        GUILayout.Label("• jump_message");
        GUILayout.Label("• crouch_message");
        GUILayout.Label("• run_message");
        GUILayout.Label("• walk_message");
        GUILayout.Label("• cursor_locked");
        GUILayout.Label("• cursor_unlocked");
        GUILayout.Label("• no_interactable");
    }
    
    private void SetupAllScriptableObjects()
    {
        Debug.Log("Setting up all ScriptableObjects...");
        
        SetupFeedbackMessages();
        SetupPlayerSettings();
        
        // Refresh AssetDatabase
        AssetDatabase.Refresh();
        
        Debug.Log("All ScriptableObjects setup completed!");
        EditorUtility.DisplayDialog("Setup Complete", 
            "All ScriptableObjects have been configured successfully!\n\n" +
            "The FPS system should now work properly with auto-setup.", "OK");
    }
    
    private void SetupFeedbackMessages()
    {
        // Try to find existing FeedbackMessagesSO
        FeedbackMessagesSO feedbackMessages = AssetDatabase.LoadAssetAtPath<FeedbackMessagesSO>("Assets/Scripts/SO/FeedbackMessages.asset");
        
        if (feedbackMessages == null)
        {
            // Create new FeedbackMessagesSO
            feedbackMessages = ScriptableObject.CreateInstance<FeedbackMessagesSO>();
            AssetDatabase.CreateAsset(feedbackMessages, "Assets/Scripts/SO/FeedbackMessages.asset");
            Debug.Log("Created new FeedbackMessagesSO at Assets/Scripts/SO/FeedbackMessages.asset");
        }
        
        // Add required messages
        AddMessageIfMissing(feedbackMessages, "press_to_interact", "Press E to interact");
        AddMessageIfMissing(feedbackMessages, "interaction_success", "Object interacted!");
        AddMessageIfMissing(feedbackMessages, "jump_message", "Jump!");
        AddMessageIfMissing(feedbackMessages, "crouch_message", "Crouching");
        AddMessageIfMissing(feedbackMessages, "run_message", "Running");
        AddMessageIfMissing(feedbackMessages, "walk_message", "Walking");
        AddMessageIfMissing(feedbackMessages, "cursor_locked", "Cursor locked");
        AddMessageIfMissing(feedbackMessages, "cursor_unlocked", "Cursor unlocked");
        AddMessageIfMissing(feedbackMessages, "no_interactable", "No interactable object in range");
        
        // Mark as dirty and save
        EditorUtility.SetDirty(feedbackMessages);
        AssetDatabase.SaveAssets();
        
        Debug.Log("FeedbackMessagesSO setup completed!");
        Debug.Log("Messages added/verified:");
        Debug.Log("- press_to_interact: Press E to interact");
        Debug.Log("- interaction_success: Object interacted!");
        Debug.Log("- jump_message: Jump!");
        Debug.Log("- crouch_message: Crouching");
        Debug.Log("- run_message: Running");
        Debug.Log("- walk_message: Walking");
        Debug.Log("- cursor_locked: Cursor locked");
        Debug.Log("- cursor_unlocked: Cursor unlocked");
        Debug.Log("- no_interactable: No interactable object in range");
        
        EditorUtility.DisplayDialog("Feedback Messages Setup Complete", 
            "FeedbackMessagesSO has been setup with all required messages!\n\n" +
            "You can now use the FPS system with proper text feedback.", "OK");
    }
    
    private void SetupPlayerSettings()
    {
        // Try to find existing PlayerSettingsSO
        PlayerSettingsSO playerSettings = AssetDatabase.LoadAssetAtPath<PlayerSettingsSO>("Assets/Scripts/SO/PlayerSettings.asset");
        
        if (playerSettings == null)
        {
            // Create new PlayerSettingsSO
            playerSettings = ScriptableObject.CreateInstance<PlayerSettingsSO>();
            AssetDatabase.CreateAsset(playerSettings, "Assets/Scripts/SO/PlayerSettings.asset");
            Debug.Log("Created new PlayerSettingsSO at Assets/Scripts/SO/PlayerSettings.asset");
        }
        
        // Mark as dirty and save
        EditorUtility.SetDirty(playerSettings);
        AssetDatabase.SaveAssets();
        
        Debug.Log("PlayerSettingsSO setup completed!");
        EditorUtility.DisplayDialog("Player Settings Setup Complete", 
            "PlayerSettingsSO has been configured successfully!\n\n" +
            "Default settings are applied. You can adjust them in the Inspector.", "OK");
    }
    
    private void AddMessageIfMissing(FeedbackMessagesSO feedbackMessages, string key, string message)
    {
        // Check if message already exists
        string existingMessage = feedbackMessages.GetMessage(key);
        if (existingMessage == "Message not found")
        {
            feedbackMessages.AddMessage(key, message);
            Debug.Log($"Added message: {key} = {message}");
        }
        else
        {
            Debug.Log($"Message already exists: {key}");
        }
    }
}

// ScriptRole: Editor tool to setup ScriptableObjects for FPS system
// Dependencies: FeedbackMessagesSO, PlayerSettingsSO
// UsesSO: None 