using UnityEngine;
using System.Collections.Generic;

public static class DependencyValidator
{
    public static bool ValidateFirstPersonController(FirstPersonController controller)
    {
        List<string> missingDependencies = new List<string>();
        
        if (controller == null)
        {
            Debug.LogError("FirstPersonController is null!");
            return false;
        }
        
        // Check PlayerSettingsSO
        var playerSettingsField = typeof(FirstPersonController).GetField("playerSettings", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var playerSettings = playerSettingsField?.GetValue(controller) as PlayerSettingsSO;
        if (playerSettings == null)
        {
            missingDependencies.Add("PlayerSettingsSO");
        }
        
        // Check FeedbackMessagesSO
        var feedbackField = typeof(FirstPersonController).GetField("feedbackMessages", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var feedbackMessages = feedbackField?.GetValue(controller) as FeedbackMessagesSO;
        if (feedbackMessages == null)
        {
            missingDependencies.Add("FeedbackMessagesSO");
        }
        
        // Check Camera Transform
        var cameraField = typeof(FirstPersonController).GetField("cameraTransform", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var cameraTransform = cameraField?.GetValue(controller) as Transform;
        if (cameraTransform == null)
        {
            missingDependencies.Add("Camera Transform");
        }
        
        if (missingDependencies.Count > 0)
        {
            Debug.LogError($"FirstPersonController missing dependencies: {string.Join(", ", missingDependencies)}");
            return false;
        }
        
        Debug.Log("FirstPersonController dependencies validated successfully");
        return true;
    }
    
    public static bool ValidateInteractableObject(InteractableObject interactable)
    {
        List<string> missingDependencies = new List<string>();
        
        if (interactable == null)
        {
            Debug.LogError("InteractableObject is null!");
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
            Debug.LogError($"InteractableObject missing dependencies: {string.Join(", ", missingDependencies)}");
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
            Debug.LogError("InteractionUI is null!");
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
            Debug.LogError($"InteractionUI missing dependencies: {string.Join(", ", missingDependencies)}");
            return false;
        }
        
        Debug.Log("InteractionUI dependencies validated successfully");
        return true;
    }
}

// ScriptRole: Static utility class for validating script dependencies
// Dependencies: None
// UsesSO: None 