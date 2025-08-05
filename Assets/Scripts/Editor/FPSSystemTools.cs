using UnityEngine;
using UnityEditor;
using UnityEngine.InputSystem;
using TMPro;

public class FPSSystemTools : EditorWindow
{
    [Header("Required Assets")]
    private PlayerSettingsSO playerSettings;
    private FeedbackMessagesSO feedbackMessages;
    private GameEvents gameEvents;
    private InputActionAsset inputActions;
    
    [Header("Prefab Settings")]
    private string playerPrefabName = "Player_FPS";
    private string interactablePrefabName = "InteractableObject";
    
    [Header("Material Settings")]
    private Material normalMaterial;
    private Material highlightMaterial;
    
    [Header("Generation Options")]
    private bool generateInputActions = true;
    private bool generatePlayerPrefab = true;
    private bool generateInteractablePrefab = true;
    private bool createDefaultMaterials = true;
    private bool setupCompleteScene = false;
    
    [MenuItem("Tools/FPS System/Generate Complete FPS System")]
    public static void ShowWindow()
    {
        GetWindow<FPSSystemTools>("FPS System Generator");
    }
    
    private void OnGUI()
    {
        GUILayout.Label("FPS System Generator", EditorStyles.boldLabel);
        GUILayout.Space(10);
        
        // Required Assets Section
        GUILayout.Label("Required Assets", EditorStyles.boldLabel);
        playerSettings = (PlayerSettingsSO)EditorGUILayout.ObjectField("Player Settings", playerSettings, typeof(PlayerSettingsSO), false);
        feedbackMessages = (FeedbackMessagesSO)EditorGUILayout.ObjectField("Feedback Messages", feedbackMessages, typeof(FeedbackMessagesSO), false);
        gameEvents = (GameEvents)EditorGUILayout.ObjectField("Game Events", gameEvents, typeof(GameEvents), false);
        inputActions = (InputActionAsset)EditorGUILayout.ObjectField("Input Actions (Uses existing)", inputActions, typeof(InputActionAsset), false);
        
        GUILayout.Space(10);
        
        // Generation Options
        GUILayout.Label("Generation Options", EditorStyles.boldLabel);
        generateInputActions = EditorGUILayout.Toggle("Load Input Actions", generateInputActions);
        generatePlayerPrefab = EditorGUILayout.Toggle("Generate Player Prefab", generatePlayerPrefab);
        generateInteractablePrefab = EditorGUILayout.Toggle("Generate Interactable Prefab", generateInteractablePrefab);
        createDefaultMaterials = EditorGUILayout.Toggle("Create Default Materials", createDefaultMaterials);
        setupCompleteScene = EditorGUILayout.Toggle("Setup Complete Scene", setupCompleteScene);
        
        GUILayout.Space(10);
        
        // Material Settings
        GUILayout.Label("Material Settings", EditorStyles.boldLabel);
        normalMaterial = (Material)EditorGUILayout.ObjectField("Normal Material", normalMaterial, typeof(Material), false);
        highlightMaterial = (Material)EditorGUILayout.ObjectField("Highlight Material", highlightMaterial, typeof(Material), false);
        
        GUILayout.Space(10);
        
        // Prefab Names
        GUILayout.Label("Prefab Names", EditorStyles.boldLabel);
        playerPrefabName = EditorGUILayout.TextField("Player Prefab Name", playerPrefabName);
        interactablePrefabName = EditorGUILayout.TextField("Interactable Prefab Name", interactablePrefabName);
        
        GUILayout.Space(20);
        
        // Generate Button
        if (GUILayout.Button("Generate Complete FPS System", GUILayout.Height(40)))
        {
            GenerateCompleteSystem();
        }
        
        GUILayout.Space(10);
        
        // Individual Generation Buttons
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Load Input Actions"))
        {
            GenerateInputActions();
        }
        if (GUILayout.Button("Generate Player Prefab"))
        {
            GeneratePlayerPrefab();
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Generate Interactable Prefab"))
        {
            GenerateInteractablePrefab();
        }
        if (GUILayout.Button("Create Default Materials"))
        {
            CreateDefaultMaterials();
        }
        EditorGUILayout.EndHorizontal();
        
        if (GUILayout.Button("Setup Complete Scene"))
        {
            SetupCompleteScene();
        }
    }
    
    private void GenerateCompleteSystem()
    {
        Debug.Log("=== GENERATING COMPLETE FPS SYSTEM ===");
        
        // Step 1: Create default materials if needed
        if (createDefaultMaterials)
        {
            CreateDefaultMaterials();
        }
        
        // Step 2: Generate Input Actions Asset
        if (generateInputActions)
        {
            GenerateInputActions();
        }
        
        // Step 3: Generate Player Prefab
        if (generatePlayerPrefab)
        {
            GeneratePlayerPrefab();
        }
        
        // Step 4: Generate Interactable Prefab
        if (generateInteractablePrefab)
        {
            GenerateInteractablePrefab();
        }
        
        // Step 5: Setup Scene if requested
        if (setupCompleteScene)
        {
            SetupCompleteScene();
        }
        
        Debug.Log("=== FPS SYSTEM GENERATION COMPLETED ===");
        Debug.Log("Next steps:");
        Debug.Log("1. Drag Player_FPS prefab to your scene");
        Debug.Log("2. Drag InteractableObject prefab to your scene");
        Debug.Log("3. Test the system in Play Mode");
        
        EditorUtility.DisplayDialog("FPS System Generated", 
            "FPS System has been generated successfully!\n\n" +
            "Next steps:\n" +
            "1. Drag Player_FPS prefab to your scene\n" +
            "2. Drag InteractableObject prefab to your scene\n" +
            "3. Test the system in Play Mode", "OK");
    }
    
    private void GenerateInputActions()
    {
        Debug.Log("Loading existing Input Actions Asset...");
        
        // Use the existing Input Actions Asset
        InputActionAsset inputActions = AssetDatabase.LoadAssetAtPath<InputActionAsset>("Assets/InputSystem_Actions.inputactions");
        
        if (inputActions == null)
        {
            Debug.LogError("InputSystem_Actions.inputactions not found! Please ensure it exists in the Assets folder.");
            return;
        }
        
        Debug.Log("Using existing Input Actions Asset: InputSystem_Actions");
        
        // Assign to this tool
        this.inputActions = inputActions;
    }
    
    public void GeneratePlayerPrefab()
    {
        Debug.Log("Generating Player Prefab...");
        
        // Create Player GameObject
        GameObject playerObj = new GameObject(playerPrefabName);
        
        // Add CharacterController
        CharacterController characterController = playerObj.AddComponent<CharacterController>();
        characterController.height = 2f;
        characterController.radius = 0.5f;
        characterController.center = new Vector3(0, 1f, 0);
        
        // Create Camera child
        GameObject cameraObj = new GameObject("Camera");
        cameraObj.transform.SetParent(playerObj.transform);
        cameraObj.transform.localPosition = new Vector3(0, 1.6f, 0);
        cameraObj.transform.localRotation = Quaternion.identity;
        
        Camera camera = cameraObj.AddComponent<Camera>();
        camera.nearClipPlane = 0.1f;
        camera.farClipPlane = 1000f;
        camera.fieldOfView = 60f;
        camera.tag = "MainCamera";
        
        // Add all required components
        FPSPlayerMovement playerMovement = playerObj.AddComponent<FPSPlayerMovement>();
        PlayerCamera playerCamera = cameraObj.AddComponent<PlayerCamera>();
        PlayerInteraction playerInteraction = playerObj.AddComponent<PlayerInteraction>();
        PlayerInput playerInput = playerObj.AddComponent<PlayerInput>();
        
        // Add Unity's Input System PlayerInput component
        UnityEngine.InputSystem.PlayerInput unityPlayerInput = playerObj.AddComponent<UnityEngine.InputSystem.PlayerInput>();
        
        InputManager inputManager = playerObj.AddComponent<InputManager>();
        CursorManager cursorManager = playerObj.AddComponent<CursorManager>();
        InteractionUI interactionUI = playerObj.AddComponent<InteractionUI>();
        
        // Assign references using reflection
        AssignReferencesToPlayerComponents(playerMovement, playerCamera, playerInteraction);
        AssignReferencesToPlayerInput(playerInput, unityPlayerInput);
        AssignReferencesToInputManager(inputManager);
        
        // Create prefab
        string prefabPath = "Assets/Prefabs/" + playerPrefabName + ".prefab";
        
        // Ensure directory exists
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
        {
            AssetDatabase.CreateFolder("Assets", "Prefabs");
        }
        
        // Create prefab
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(playerObj, prefabPath);
        
        if (prefab != null)
        {
            Debug.Log($"Player prefab created successfully at: {prefabPath}");
            
            // Clean up the temporary GameObject
            DestroyImmediate(playerObj);
        }
        else
        {
            Debug.LogError("Failed to create player prefab!");
        }
    }
    
    public void GenerateInteractablePrefab()
    {
        Debug.Log("Generating Interactable Prefab...");
        
        // Create GameObject
        GameObject interactableObj = new GameObject(interactablePrefabName);
        
        // Add MeshRenderer and MeshFilter
        MeshRenderer renderer = interactableObj.AddComponent<MeshRenderer>();
        MeshFilter meshFilter = interactableObj.AddComponent<MeshFilter>();
        
        // Create a simple cube mesh
        Mesh cubeMesh = CreateCubeMesh();
        meshFilter.mesh = cubeMesh;
        
        // Set material
        if (normalMaterial != null)
        {
            renderer.material = normalMaterial;
        }
        else
        {
            // Create default material
            Material defaultMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            defaultMaterial.color = Color.white;
            renderer.material = defaultMaterial;
        }
        
        // Add Collider
        BoxCollider collider = interactableObj.AddComponent<BoxCollider>();
        collider.size = new Vector3(1f, 1f, 1f);
        
        // Add InteractableObject component
        InteractableObject interactable = interactableObj.AddComponent<InteractableObject>();
        
        // Set layer to Interactable
        interactableObj.layer = LayerMask.NameToLayer("Interactable");
        if (interactableObj.layer == -1)
        {
            // Create Interactable layer if it doesn't exist
            Debug.LogWarning("Interactable layer not found. Creating it...");
            CreateInteractableLayer();
            interactableObj.layer = LayerMask.NameToLayer("Interactable");
        }
        
        // Assign references to InteractableObject using reflection
        AssignReferencesToInteractableObject(interactable, renderer);
        
        // Create prefab
        string prefabPath = "Assets/Prefabs/" + interactablePrefabName + ".prefab";
        
        // Ensure directory exists
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
        {
            AssetDatabase.CreateFolder("Assets", "Prefabs");
        }
        
        // Create prefab
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(interactableObj, prefabPath);
        
        if (prefab != null)
        {
            Debug.Log($"Interactable prefab created successfully at: {prefabPath}");
            
            // Clean up the temporary GameObject
            DestroyImmediate(interactableObj);
        }
        else
        {
            Debug.LogError("Failed to create interactable prefab!");
        }
    }
    
    private void CreateDefaultMaterials()
    {
        Debug.Log("Creating default materials...");
        
        // Create normal material
        Material normalMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        normalMat.name = "NormalMaterial";
        normalMat.color = Color.white;
        AssetDatabase.CreateAsset(normalMat, "Assets/Materials/NormalMaterial.mat");
        
        // Create highlight material
        Material highlightMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        highlightMat.name = "HighlightMaterial";
        highlightMat.color = Color.yellow;
        highlightMat.SetFloat("_EmissionIntensity", 0.5f);
        AssetDatabase.CreateAsset(highlightMat, "Assets/Materials/HighlightMaterial.mat");
        
        // Assign to this tool
        normalMaterial = normalMat;
        highlightMaterial = highlightMat;
        
        AssetDatabase.SaveAssets();
        Debug.Log("Default materials created successfully");
    }
    
    public void SetupCompleteScene()
    {
        Debug.Log("Setting up complete scene...");
        
        // Create Canvas
        GameObject canvas = new GameObject("Canvas");
        Canvas canvasComponent = canvas.AddComponent<Canvas>();
        canvasComponent.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.AddComponent<UnityEngine.UI.CanvasScaler>();
        canvas.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        
        // Create GameUI with proper UI components
        GameObject gameUI = new GameObject("GameUI");
        gameUI.transform.SetParent(canvas.transform);
        RectTransform gameUIRect = gameUI.AddComponent<RectTransform>();
        gameUIRect.anchorMin = Vector2.zero;
        gameUIRect.anchorMax = Vector2.one;
        gameUIRect.sizeDelta = Vector2.zero;
        gameUIRect.anchoredPosition = Vector2.zero;
        
        // Create InteractionPanel with proper UI components
        GameObject interactionPanel = new GameObject("InteractionPanel");
        interactionPanel.transform.SetParent(gameUI.transform);
        RectTransform panelRect = interactionPanel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.1f);
        panelRect.anchorMax = new Vector2(0.5f, 0.1f);
        panelRect.sizeDelta = new Vector2(400, 50);
        panelRect.anchoredPosition = Vector2.zero;
        
        // Add CanvasGroup for fade effects
        CanvasGroup panelCanvasGroup = interactionPanel.AddComponent<CanvasGroup>();
        panelCanvasGroup.alpha = 0f; // Start hidden
        
        // Create InteractionText with proper UI components
        GameObject interactionText = new GameObject("InteractionText");
        interactionText.transform.SetParent(interactionPanel.transform);
        TextMeshProUGUI textComponent = interactionText.AddComponent<TextMeshProUGUI>();
        textComponent.text = "Press E to interact";
        textComponent.fontSize = 24;
        textComponent.color = Color.white;
        textComponent.alignment = TextAlignmentOptions.Center;
        
        // Position the text
        RectTransform textRect = interactionText.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        textRect.anchoredPosition = Vector2.zero;
        
        // Create PauseMenu with proper UI components
        GameObject pauseMenu = new GameObject("PauseMenu");
        pauseMenu.transform.SetParent(canvas.transform);
        RectTransform pauseRect = pauseMenu.AddComponent<RectTransform>();
        pauseRect.anchorMin = Vector2.zero;
        pauseRect.anchorMax = Vector2.one;
        pauseRect.sizeDelta = Vector2.zero;
        pauseRect.anchoredPosition = Vector2.zero;
        pauseMenu.SetActive(false);
        
        // Add background to pause menu
        GameObject pauseBackground = new GameObject("Background");
        pauseBackground.transform.SetParent(pauseMenu.transform);
        UnityEngine.UI.Image backgroundImage = pauseBackground.AddComponent<UnityEngine.UI.Image>();
        backgroundImage.color = new Color(0, 0, 0, 0.8f);
        RectTransform bgRect = pauseBackground.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;
        bgRect.anchoredPosition = Vector2.zero;
        
        Debug.Log("Scene setup completed with proper UI components!");
        Debug.Log("Remember to:");
        Debug.Log("1. Assign the InteractionText to your InteractionUI component");
        Debug.Log("2. Assign the InteractionPanel to your InteractionUI component");
        Debug.Log("3. Assign the PauseMenu to your CursorManager component");
        Debug.Log("4. Assign the GameUI to your CursorManager component");
    }
    
    private void AssignReferencesToPlayerComponents(FPSPlayerMovement playerMovement, PlayerCamera playerCamera, PlayerInteraction playerInteraction)
    {
        // Assign PlayerSettingsSO to FPSPlayerMovement
        if (playerSettings != null)
        {
            var playerSettingsField = typeof(FPSPlayerMovement).GetField("playerSettings", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (playerSettingsField != null)
            {
                playerSettingsField.SetValue(playerMovement, playerSettings);
            }
        }
        
        // Assign PlayerSettingsSO to PlayerCamera
        if (playerSettings != null)
        {
            var playerSettingsField = typeof(PlayerCamera).GetField("playerSettings", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (playerSettingsField != null)
            {
                playerSettingsField.SetValue(playerCamera, playerSettings);
            }
        }
        
        // Assign PlayerSettingsSO and FeedbackMessagesSO to PlayerInteraction
        if (playerSettings != null)
        {
            var playerSettingsField = typeof(PlayerInteraction).GetField("playerSettings", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (playerSettingsField != null)
            {
                playerSettingsField.SetValue(playerInteraction, playerSettings);
            }
        }
        
        if (feedbackMessages != null)
        {
            var feedbackField = typeof(PlayerInteraction).GetField("feedbackMessages", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (feedbackField != null)
            {
                feedbackField.SetValue(playerInteraction, feedbackMessages);
            }
        }
        
        // Assign GameEvents to FPSPlayerMovement and PlayerInteraction
        if (gameEvents != null)
        {
            var gameEventsField = typeof(FPSPlayerMovement).GetField("gameEvents", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (gameEventsField != null)
            {
                gameEventsField.SetValue(playerMovement, gameEvents);
            }
            
            var gameEventsField2 = typeof(PlayerInteraction).GetField("gameEvents", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (gameEventsField2 != null)
            {
                gameEventsField2.SetValue(playerInteraction, gameEvents);
            }
        }
        
        // Assign Camera Transform to PlayerInteraction
        var cameraField = typeof(PlayerInteraction).GetField("cameraTransform", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (cameraField != null)
        {
            cameraField.SetValue(playerInteraction, playerCamera.transform);
        }
        
        // Assign Player Body Transform to PlayerCamera
        var playerBodyField = typeof(PlayerCamera).GetField("playerBody", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (playerBodyField != null)
        {
            playerBodyField.SetValue(playerCamera, playerMovement.transform);
        }
        
        Debug.Log("Assigned references to Player Components");
    }
    
    private void AssignReferencesToPlayerInput(PlayerInput playerInput, UnityEngine.InputSystem.PlayerInput unityPlayerInput)
    {
        // Configure Unity's Input System PlayerInput component
        if (inputActions != null)
        {
            unityPlayerInput.actions = inputActions;
            unityPlayerInput.defaultActionMap = "Player";
            unityPlayerInput.notificationBehavior = UnityEngine.InputSystem.PlayerNotifications.InvokeCSharpEvents;
        }
        
        // Configure our custom PlayerInput script references
        if (playerInput != null)
        {
            // Assign references to our custom PlayerInput script using reflection
            var playerMovementField = typeof(PlayerInput).GetField("playerMovement", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (playerMovementField != null)
            {
                var playerMovement = playerInput.GetComponent<FPSPlayerMovement>();
                playerMovementField.SetValue(playerInput, playerMovement);
            }
            
            var playerCameraField = typeof(PlayerInput).GetField("playerCamera", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (playerCameraField != null)
            {
                var playerCamera = playerInput.GetComponentInChildren<PlayerCamera>();
                playerCameraField.SetValue(playerInput, playerCamera);
            }
            
            var playerInteractionField = typeof(PlayerInput).GetField("playerInteraction", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (playerInteractionField != null)
            {
                var playerInteraction = playerInput.GetComponent<PlayerInteraction>();
                playerInteractionField.SetValue(playerInput, playerInteraction);
            }
        }
    }
    
    private void AssignReferencesToInputManager(InputManager inputManager)
    {
        if (inputActions != null)
        {
            var inputActionsField = typeof(InputManager).GetField("inputActions", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (inputActionsField != null)
            {
                inputActionsField.SetValue(inputManager, inputActions);
            }
        }
    }
    
    private void AssignReferencesToInteractableObject(InteractableObject interactable, MeshRenderer renderer)
    {
        var rendererField = typeof(InteractableObject).GetField("objectRenderer", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (rendererField != null)
        {
            rendererField.SetValue(interactable, renderer);
        }
        
        if (normalMaterial != null)
        {
            var normalMaterialField = typeof(InteractableObject).GetField("normalMaterial", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (normalMaterialField != null)
            {
                normalMaterialField.SetValue(interactable, normalMaterial);
            }
        }
        
        if (highlightMaterial != null)
        {
            var highlightMaterialField = typeof(InteractableObject).GetField("highlightMaterial", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (highlightMaterialField != null)
            {
                highlightMaterialField.SetValue(interactable, highlightMaterial);
            }
        }
        
        if (feedbackMessages != null)
        {
            var feedbackMessagesField = typeof(InteractableObject).GetField("feedbackMessages", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (feedbackMessagesField != null)
            {
                feedbackMessagesField.SetValue(interactable, feedbackMessages);
            }
        }
        
        // Assign GameEvents
        if (gameEvents != null)
        {
            var gameEventsField = typeof(InteractableObject).GetField("gameEvents", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (gameEventsField != null)
            {
                gameEventsField.SetValue(interactable, gameEvents);
            }
        }
    }
    
    private Mesh CreateCubeMesh()
    {
        Mesh mesh = new Mesh();
        
        // Vertices
        Vector3[] vertices = {
            new Vector3(-0.5f, -0.5f, -0.5f),
            new Vector3(0.5f, -0.5f, -0.5f),
            new Vector3(0.5f, 0.5f, -0.5f),
            new Vector3(-0.5f, 0.5f, -0.5f),
            new Vector3(-0.5f, -0.5f, 0.5f),
            new Vector3(0.5f, -0.5f, 0.5f),
            new Vector3(0.5f, 0.5f, 0.5f),
            new Vector3(-0.5f, 0.5f, 0.5f)
        };
        
        // Triangles
        int[] triangles = {
            0, 2, 1, 0, 3, 2, // Front
            1, 6, 5, 1, 2, 6, // Right
            5, 7, 4, 5, 6, 7, // Back
            4, 3, 0, 4, 7, 3, // Left
            3, 6, 2, 3, 7, 6, // Top
            0, 5, 4, 0, 1, 5  // Bottom
        };
        
        // UVs
        Vector2[] uvs = {
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(1, 1),
            new Vector2(0, 1),
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(1, 1),
            new Vector2(0, 1)
        };
        
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateNormals();
        
        return mesh;
    }
    
    private void CreateInteractableLayer()
    {
        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty layers = tagManager.FindProperty("layers");
        
        // Find first empty layer
        for (int i = 8; i < layers.arraySize; i++)
        {
            SerializedProperty layerSP = layers.GetArrayElementAtIndex(i);
            if (layerSP.stringValue == "")
            {
                layerSP.stringValue = "Interactable";
                tagManager.ApplyModifiedProperties();
                Debug.Log("Created Interactable layer at index " + i);
                return;
            }
        }
        
        Debug.LogError("No empty layer slots available!");
    }
}

// ScriptRole: Editor tool to generate complete FPS system automatically
// Dependencies: All FPS system components
// UsesSO: PlayerSettingsSO, FeedbackMessagesSO, InputActionAsset
// NeedsSetup: Assign ScriptableObjects in tool window before generating 
// NeedsSetup: Assign ScriptableObjects in tool window before generating 