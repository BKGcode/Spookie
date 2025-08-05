using UnityEngine;
using UnityEditor;
using TMPro;

public class UICreator : EditorWindow
{
    [MenuItem("Tools/FPS System/Create UI Elements")]
    public static void ShowWindow()
    {
        GetWindow<UICreator>("UI Creator");
    }
    
    private void OnGUI()
    {
        GUILayout.Label("FPS System UI Creator", EditorStyles.boldLabel);
        GUILayout.Space(10);
        
        GUILayout.Label("This tool will create proper UI elements for the FPS system.");
        GUILayout.Space(10);
        
        if (GUILayout.Button("Create Complete UI System", GUILayout.Height(40)))
        {
            CreateCompleteUISystem();
        }
        
        GUILayout.Space(10);
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Create Canvas Only"))
        {
            CreateCanvasButton();
        }
        if (GUILayout.Button("Create Interaction UI"))
        {
            CreateInteractionUIButton();
        }
        EditorGUILayout.EndHorizontal();
        
        GUILayout.Space(5);
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Create Pause Menu"))
        {
            CreatePauseMenuButton();
        }
        if (GUILayout.Button("Create Game UI"))
        {
            CreateGameUIButton();
        }
        EditorGUILayout.EndHorizontal();
        
        GUILayout.Space(10);
        
        GUILayout.Label("UI Elements that will be created:");
        GUILayout.Label("• Canvas with proper UI components");
        GUILayout.Label("• GameUI container");
        GUILayout.Label("• InteractionPanel with CanvasGroup");
        GUILayout.Label("• InteractionText with TextMeshProUGUI");
        GUILayout.Label("• PauseMenu with background");
    }
    
    public void CreateCompleteUISystem()
    {
        Debug.Log("Creating complete UI system...");
        
        // Create Canvas
        GameObject canvas = CreateCanvas();
        
        // Create GameUI
        GameObject gameUI = CreateGameUI(canvas);
        
        // Create Interaction UI
        CreateInteractionUI(gameUI);
        
        // Create Pause Menu
        CreatePauseMenu(canvas);
        
        Debug.Log("Complete UI system created successfully!");
        EditorUtility.DisplayDialog("UI Creation Complete", 
            "Complete UI system has been created!\n\n" +
            "All elements have proper UI components and are ready to use.", "OK");
    }
    
    private GameObject CreateCanvas()
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
    
    private GameObject CreateGameUI(GameObject parent)
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
    
    private void CreateInteractionUI(GameObject parent)
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
    
    private void CreatePauseMenu(GameObject parent)
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
    
    // Overloaded methods for button calls
    private void CreateCanvasButton()
    {
        CreateCanvas();
    }
    
    private void CreateGameUIButton()
    {
        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas != null)
        {
            CreateGameUI(canvas.gameObject);
        }
        else
        {
            Debug.LogWarning("No Canvas found. Create Canvas first.");
        }
    }
    
    private void CreateInteractionUIButton()
    {
        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas != null)
        {
            Transform gameUI = canvas.transform.Find("GameUI");
            if (gameUI != null)
            {
                CreateInteractionUI(gameUI.gameObject);
            }
            else
            {
                Debug.LogWarning("No GameUI found. Create GameUI first.");
            }
        }
        else
        {
            Debug.LogWarning("No Canvas found. Create Canvas first.");
        }
    }
    
    private void CreatePauseMenuButton()
    {
        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas != null)
        {
            CreatePauseMenu(canvas.gameObject);
        }
        else
        {
            Debug.LogWarning("No Canvas found. Create Canvas first.");
        }
    }
}

// ScriptRole: Editor tool to create proper UI elements for FPS system
// Dependencies: Canvas, TextMeshProUGUI, UnityEngine.UI
// UsesSO: None 