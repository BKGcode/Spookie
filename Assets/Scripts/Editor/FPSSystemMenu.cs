using UnityEngine;
using UnityEditor;
using TMPro;

public static class FPSSystemMenu
{
    [MenuItem("Tools/FPS System/📋 Setup ScriptableObjects", priority = 1)]
    public static void SetupScriptableObjects()
    {
        // Open the FeedbackMessagesSetup window which now handles both SOs
        FeedbackMessagesSetup.ShowWindow();
    }
    
    [MenuItem("Tools/FPS System/🎮 Setup Wizard", priority = 2)]
    public static void OpenSetupWizard()
    {
        FPSSetupWizard.ShowWindow();
    }
    
    [MenuItem("Tools/FPS System/⚙️ Generate Complete System", priority = 3)]
    public static void OpenSystemGenerator()
    {
        FPSSystemTools.ShowWindow();
    }
    
    [MenuItem("Tools/FPS System/🎨 Create UI Elements", priority = 4)]
    public static void OpenUICreator()
    {
        UICreator.ShowWindow();
    }
    
    [MenuItem("Tools/FPS System/🐛 Debug Tools", priority = 5)]
    public static void OpenDebugTools()
    {
        FPSDebugTool.ShowWindow();
    }
    
    [MenuItem("Tools/FPS System/📖 Documentation", priority = 6)]
    public static void OpenDocumentation()
    {
        // Open the README file
        string readmePath = "Assets/Scripts/README_FPS_Features.md";
        if (System.IO.File.Exists(readmePath))
        {
            AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath<TextAsset>(readmePath));
        }
        else
        {
            Debug.LogWarning("README_FPS_Features.md not found. Please check the Scripts folder.");
        }
    }
    
    [MenuItem("Tools/FPS System/🔧 Quick Setup", priority = 0)]
    public static void QuickSetup()
    {
        Debug.Log("Starting FPS System Quick Setup...");
        
        // Setup ScriptableObjects
        SetupAllScriptableObjects();
        
        // Create UI Elements
        CreateUIElements();
        
        // Validate scene components
        ValidateSceneComponents();
        
        Debug.Log("Quick Setup completed! Check the console for details.");
        EditorUtility.DisplayDialog("Quick Setup Complete", 
            "FPS System has been quickly configured!\n\n" +
            "• ScriptableObjects created/updated\n" +
            "• UI Elements created with proper components\n" +
            "• Scene components validated\n" +
            "• Check console for any warnings", "OK");
    }
    
    private static void SetupAllScriptableObjects()
    {
        // Setup FeedbackMessagesSO
        FeedbackMessagesSO feedbackMessages = AssetDatabase.LoadAssetAtPath<FeedbackMessagesSO>("Assets/Scripts/SO/FeedbackMessages.asset");
        if (feedbackMessages == null)
        {
            feedbackMessages = ScriptableObject.CreateInstance<FeedbackMessagesSO>();
            AssetDatabase.CreateAsset(feedbackMessages, "Assets/Scripts/SO/FeedbackMessages.asset");
            Debug.Log("Created FeedbackMessagesSO");
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
        
        // Setup PlayerSettingsSO
        PlayerSettingsSO playerSettings = AssetDatabase.LoadAssetAtPath<PlayerSettingsSO>("Assets/Scripts/SO/PlayerSettings.asset");
        if (playerSettings == null)
        {
            playerSettings = ScriptableObject.CreateInstance<PlayerSettingsSO>();
            AssetDatabase.CreateAsset(playerSettings, "Assets/Scripts/SO/PlayerSettings.asset");
            Debug.Log("Created PlayerSettingsSO");
        }
        
        EditorUtility.SetDirty(feedbackMessages);
        EditorUtility.SetDirty(playerSettings);
        AssetDatabase.SaveAssets();
        
        Debug.Log("ScriptableObjects setup completed");
    }
    
    private static void CreateUIElements()
    {
        Debug.Log("Creating UI elements...");
        
        // Create Canvas if it doesn't exist
        Canvas existingCanvas = Object.FindFirstObjectByType<Canvas>();
        if (existingCanvas == null)
        {
            GameObject canvas = new GameObject("Canvas");
            Canvas canvasComponent = canvas.AddComponent<Canvas>();
            canvasComponent.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvas.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            Debug.Log("Created Canvas with proper UI components");
        }
        else
        {
            Debug.Log("Canvas already exists");
        }
        
        // Create UI elements using static methods
        CreateCompleteUISystemStatic();
        
        Debug.Log("UI elements created successfully");
    }
    
    private static void CreateCompleteUISystemStatic()
    {
        Debug.Log("Creating complete UI system...");
        
        // Create Canvas
        GameObject canvas = CreateCanvasStatic();
        
        // Create GameUI
        GameObject gameUI = CreateGameUIStatic(canvas);
        
        // Create Interaction UI
        CreateInteractionUIStatic(gameUI);
        
        // Create Pause Menu
        CreatePauseMenuStatic(canvas);
        
        Debug.Log("Complete UI system created successfully!");
    }
    
    private static GameObject CreateCanvasStatic()
    {
        // Check if Canvas already exists
        Canvas existingCanvas = Object.FindFirstObjectByType<Canvas>();
        if (existingCanvas != null)
        {
            Debug.Log("Canvas already exists, using existing one");
            return existingCanvas.gameObject;
        }
        
        // Create new Canvas
        GameObject canvas = new GameObject("Canvas");
        Canvas canvasComponent = canvas.AddComponent<Canvas>();
        canvasComponent.renderMode = RenderMode.ScreenSpaceOverlay;
        
        // Add CanvasScaler
        UnityEngine.UI.CanvasScaler scaler = canvas.AddComponent<UnityEngine.UI.CanvasScaler>();
        scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = UnityEngine.UI.CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        
        // Add GraphicRaycaster
        canvas.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        
        Debug.Log("Canvas created with proper UI components");
        return canvas;
    }
    
    private static GameObject CreateGameUIStatic(GameObject parent)
    {
        // Check if GameUI already exists
        Transform existingGameUI = parent.transform.Find("GameUI");
        if (existingGameUI != null)
        {
            Debug.Log("GameUI already exists, using existing one");
            return existingGameUI.gameObject;
        }
        
        // Create GameUI
        GameObject gameUI = new GameObject("GameUI");
        gameUI.transform.SetParent(parent.transform);
        
        // Add RectTransform with proper settings
        RectTransform gameUIRect = gameUI.AddComponent<RectTransform>();
        gameUIRect.anchorMin = Vector2.zero;
        gameUIRect.anchorMax = Vector2.one;
        gameUIRect.sizeDelta = Vector2.zero;
        gameUIRect.anchoredPosition = Vector2.zero;
        
        Debug.Log("GameUI created with proper UI components");
        return gameUI;
    }
    
    private static void CreateInteractionUIStatic(GameObject parent)
    {
        // Check if InteractionPanel already exists
        Transform existingPanel = parent.transform.Find("InteractionPanel");
        if (existingPanel != null)
        {
            Debug.Log("InteractionPanel already exists, using existing one");
            return;
        }
        
        // Create InteractionPanel
        GameObject interactionPanel = new GameObject("InteractionPanel");
        interactionPanel.transform.SetParent(parent.transform);
        
        // Add RectTransform
        RectTransform panelRect = interactionPanel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.1f);
        panelRect.anchorMax = new Vector2(0.5f, 0.1f);
        panelRect.sizeDelta = new Vector2(400, 50);
        panelRect.anchoredPosition = Vector2.zero;
        
        // Add CanvasGroup for fade effects
        CanvasGroup panelCanvasGroup = interactionPanel.AddComponent<CanvasGroup>();
        panelCanvasGroup.alpha = 0f; // Start hidden
        
        // Create InteractionText
        GameObject interactionText = new GameObject("InteractionText");
        interactionText.transform.SetParent(interactionPanel.transform);
        
        // Add TextMeshProUGUI
        TextMeshProUGUI textComponent = interactionText.AddComponent<TextMeshProUGUI>();
        textComponent.text = "Press E to interact";
        textComponent.fontSize = 24;
        textComponent.color = Color.white;
        textComponent.alignment = TextAlignmentOptions.Center;
        
        // Add RectTransform for text
        RectTransform textRect = interactionText.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        textRect.anchoredPosition = Vector2.zero;
        
        Debug.Log("Interaction UI created with proper UI components");
    }
    
    private static void CreatePauseMenuStatic(GameObject parent)
    {
        // Check if PauseMenu already exists
        Transform existingPauseMenu = parent.transform.Find("PauseMenu");
        if (existingPauseMenu != null)
        {
            Debug.Log("PauseMenu already exists, using existing one");
            return;
        }
        
        // Create PauseMenu
        GameObject pauseMenu = new GameObject("PauseMenu");
        pauseMenu.transform.SetParent(parent.transform);
        
        // Add RectTransform
        RectTransform pauseRect = pauseMenu.AddComponent<RectTransform>();
        pauseRect.anchorMin = Vector2.zero;
        pauseRect.anchorMax = Vector2.one;
        pauseRect.sizeDelta = Vector2.zero;
        pauseRect.anchoredPosition = Vector2.zero;
        pauseMenu.SetActive(false);
        
        // Add background
        GameObject pauseBackground = new GameObject("Background");
        pauseBackground.transform.SetParent(pauseMenu.transform);
        UnityEngine.UI.Image backgroundImage = pauseBackground.AddComponent<UnityEngine.UI.Image>();
        backgroundImage.color = new Color(0, 0, 0, 0.8f);
        RectTransform bgRect = pauseBackground.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;
        bgRect.anchoredPosition = Vector2.zero;
        
        Debug.Log("PauseMenu created with proper UI components");
    }
    
    private static void AddMessageIfMissing(FeedbackMessagesSO feedbackMessages, string key, string message)
    {
        string existingMessage = feedbackMessages.GetMessage(key);
        if (existingMessage == "Message not found")
        {
            feedbackMessages.AddMessage(key, message);
            Debug.Log($"Added message: {key}");
        }
    }
    
    private static void ValidateSceneComponents()
    {
        Debug.Log("Validating scene components...");
        
        // Check for FirstPersonController
        FirstPersonController controller = Object.FindFirstObjectByType<FirstPersonController>();
        if (controller == null)
        {
            Debug.LogWarning("No FirstPersonController found in scene. Add one to test the system.");
        }
        else
        {
            Debug.Log("✓ FirstPersonController found");
        }
        
        // Check for InteractableObjects
        InteractableObject[] interactables = Object.FindObjectsByType<InteractableObject>(FindObjectsSortMode.None);
        if (interactables.Length == 0)
        {
            Debug.LogWarning("No InteractableObjects found in scene. Add some to test interactions.");
        }
        else
        {
            Debug.Log($"✓ Found {interactables.Length} InteractableObjects");
        }
        
        // Check for InteractionUI
        InteractionUI[] interactionUIs = Object.FindObjectsByType<InteractionUI>(FindObjectsSortMode.None);
        if (interactionUIs.Length == 0)
        {
            Debug.LogWarning("No InteractionUI found in scene. Add one for interaction feedback.");
        }
        else
        {
            Debug.Log($"✓ Found {interactionUIs.Length} InteractionUIs");
        }
        
        // Check for UI elements
        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogWarning("No Canvas found in scene. UI elements may not work properly.");
        }
        else
        {
            Debug.Log("✓ Canvas found");
        }
        
        Debug.Log("Scene validation completed");
    }
}

// ScriptRole: Unified menu system for FPS tools
// Dependencies: All FPS Editor tools
// UsesSO: None 