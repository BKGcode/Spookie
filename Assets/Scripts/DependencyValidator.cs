using UnityEngine;
using System.Collections.Generic;

public static class DependencyValidator
{
    public static bool ValidatePlayerMovement(FPSPlayerMovement movement)
    {
        List<string> missingDependencies = new List<string>();
        
        if (movement == null)
        {
            ErrorHandler.LogError("FPSPlayerMovement is null!");
            return false;
        }
        
        // Check CharacterController
        var characterController = movement.GetComponent<CharacterController>();
        if (characterController == null)
        {
            missingDependencies.Add("CharacterController");
        }
        
        // Check PlayerSettingsSO using reflection (temporary until we add public property)
        var playerSettingsField = typeof(FPSPlayerMovement).GetField("playerSettings", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var playerSettings = playerSettingsField?.GetValue(movement) as PlayerSettingsSO;
        if (playerSettings == null)
        {
            missingDependencies.Add("PlayerSettingsSO");
        }
        
        if (missingDependencies.Count > 0)
        {
            ErrorHandler.LogError($"FPSPlayerMovement missing dependencies: {string.Join(", ", missingDependencies)}", movement);
            return false;
        }
        
        Debug.Log("FPSPlayerMovement dependencies validated successfully");
        return true;
    }
    
    public static bool ValidatePlayerCamera(PlayerCamera camera)
    {
        List<string> missingDependencies = new List<string>();
        
        if (camera == null)
        {
            ErrorHandler.LogError("PlayerCamera is null!");
            return false;
        }
        
        // Check PlayerSettingsSO
        var playerSettingsField = typeof(PlayerCamera).GetField("playerSettings", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var playerSettings = playerSettingsField?.GetValue(camera) as PlayerSettingsSO;
        if (playerSettings == null)
        {
            missingDependencies.Add("PlayerSettingsSO");
        }
        
        // Check Player Body Transform
        var playerBodyField = typeof(PlayerCamera).GetField("playerBody", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var playerBody = playerBodyField?.GetValue(camera) as Transform;
        if (playerBody == null)
        {
            missingDependencies.Add("Player Body Transform");
        }
        
        if (missingDependencies.Count > 0)
        {
            ErrorHandler.LogError($"PlayerCamera missing dependencies: {string.Join(", ", missingDependencies)}", camera);
            return false;
        }
        
        Debug.Log("PlayerCamera dependencies validated successfully");
        return true;
    }
    
    public static bool ValidatePlayerInteraction(PlayerInteraction interaction)
    {
        List<string> missingDependencies = new List<string>();
        
        if (interaction == null)
        {
            ErrorHandler.LogError("PlayerInteraction is null!");
            return false;
        }
        
        // Check PlayerSettingsSO
        var playerSettingsField = typeof(PlayerInteraction).GetField("playerSettings", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var playerSettings = playerSettingsField?.GetValue(interaction) as PlayerSettingsSO;
        if (playerSettings == null)
        {
            missingDependencies.Add("PlayerSettingsSO");
        }
        
        // Check FeedbackMessagesSO
        var feedbackField = typeof(PlayerInteraction).GetField("feedbackMessages", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var feedbackMessages = feedbackField?.GetValue(interaction) as FeedbackMessagesSO;
        if (feedbackMessages == null)
        {
            missingDependencies.Add("FeedbackMessagesSO");
        }
        
        // Check Camera Transform
        var cameraField = typeof(PlayerInteraction).GetField("cameraTransform", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var cameraTransform = cameraField?.GetValue(interaction) as Transform;
        if (cameraTransform == null)
        {
            missingDependencies.Add("Camera Transform");
        }
        
        if (missingDependencies.Count > 0)
        {
            ErrorHandler.LogError($"PlayerInteraction missing dependencies: {string.Join(", ", missingDependencies)}", interaction);
            return false;
        }
        
        Debug.Log("PlayerInteraction dependencies validated successfully");
        return true;
    }
    
    public static bool ValidateInteractableObject(InteractableObject interactable)
    {
        List<string> missingDependencies = new List<string>();
        
        if (interactable == null)
        {
            ErrorHandler.LogError("InteractableObject is null!");
            return false;
        }
        
        // Check Renderer
        var rendererField = typeof(InteractableObject).GetField("objectRenderer", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var renderer = rendererField?.GetValue(interactable) as Renderer;
        if (renderer == null)
        {
            missingDependencies.Add("Renderer");
        }
        
        // Check FeedbackMessagesSO
        var feedbackField = typeof(InteractableObject).GetField("feedbackMessages", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var feedbackMessages = feedbackField?.GetValue(interactable) as FeedbackMessagesSO;
        if (feedbackMessages == null)
        {
            missingDependencies.Add("FeedbackMessagesSO");
        }
        
        if (missingDependencies.Count > 0)
        {
            ErrorHandler.LogError($"InteractableObject missing dependencies: {string.Join(", ", missingDependencies)}", interactable);
            return false;
        }
        
        Debug.Log("InteractableObject dependencies validated successfully");
        return true;
    }
    
    public static bool ValidateInteractionUI(InteractionUI interactionUI)
    {
        List<string> missingDependencies = new List<string>();
        
        if (interactionUI == null)
        {
            ErrorHandler.LogError("InteractionUI is null!");
            return false;
        }
        
        // Check TextMeshProUGUI
        var textField = typeof(InteractionUI).GetField("interactionText", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var text = textField?.GetValue(interactionUI) as TMPro.TextMeshProUGUI;
        if (text == null)
        {
            missingDependencies.Add("TextMeshProUGUI");
        }
        
        // Check FeedbackMessagesSO
        var feedbackField = typeof(InteractionUI).GetField("feedbackMessages", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var feedbackMessages = feedbackField?.GetValue(interactionUI) as FeedbackMessagesSO;
        if (feedbackMessages == null)
        {
            missingDependencies.Add("FeedbackMessagesSO");
        }
        
        if (missingDependencies.Count > 0)
        {
            ErrorHandler.LogError($"InteractionUI missing dependencies: {string.Join(", ", missingDependencies)}", interactionUI);
            return false;
        }
        
        Debug.Log("InteractionUI dependencies validated successfully");
        return true;
    }
}

// ScriptRole: Static utility class for validating script dependencies
// Dependencies: ErrorHandler
// UsesSO: None 