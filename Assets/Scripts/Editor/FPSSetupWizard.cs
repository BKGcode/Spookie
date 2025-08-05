using UnityEngine;
using UnityEditor;
using UnityEngine.InputSystem;
using TMPro;

public class FPSSetupWizard : EditorWindow
{
    private int currentStep = 0;
    private const int totalSteps = 5;
    
    // Step 0: Welcome
    private bool hasReadWelcome = false;
    
    // Step 1: ScriptableObjects
    private PlayerSettingsSO playerSettings;
    private FeedbackMessagesSO feedbackMessages;
    
    // Step 2: Input System
    private InputActionAsset inputActions;
    
    // Step 3: Materials
    private Material normalMaterial;
    private Material highlightMaterial;
    
    // Step 4: Prefabs
    private bool playerPrefabCreated = false;
    private bool interactablePrefabCreated = false;
    
    // Step 5: Scene Setup
    private bool sceneSetup = false;
    
    [MenuItem("Tools/FPS System/Setup Wizard")]
    public static void ShowWindow()
    {
        GetWindow<FPSSetupWizard>("FPS Setup Wizard");
    }
    
    private void OnGUI()
    {
        GUILayout.Label("FPS System Setup Wizard", EditorStyles.boldLabel);
        GUILayout.Space(10);
        
        // Progress indicator
        GUILayout.Label($"Step {currentStep + 1} of {totalSteps + 1}", EditorStyles.boldLabel);
        GUILayout.Space(10);
        
        switch (currentStep)
        {
            case 0:
                DrawWelcomeStep();
                break;
            case 1:
                DrawScriptableObjectsStep();
                break;
            case 2:
                DrawInputSystemStep();
                break;
            case 3:
                DrawMaterialsStep();
                break;
            case 4:
                DrawPrefabsStep();
                break;
            case 5:
                DrawSceneSetupStep();
                break;
        }
        
        GUILayout.Space(20);
        
        // Navigation buttons
        EditorGUILayout.BeginHorizontal();
        
        if (currentStep > 0)
        {
            if (GUILayout.Button("Previous"))
            {
                currentStep--;
            }
        }
        
        GUILayout.FlexibleSpace();
        
        if (currentStep < totalSteps)
        {
            if (GUILayout.Button("Next"))
            {
                currentStep++;
            }
        }
        else
        {
            if (GUILayout.Button("Finish"))
            {
                Close();
            }
        }
        
        EditorGUILayout.EndHorizontal();
    }
    
    private void DrawWelcomeStep()
    {
        GUILayout.Label("Welcome to FPS System Setup!", EditorStyles.boldLabel);
        GUILayout.Space(10);
        
        GUILayout.Label("This wizard will help you set up a complete FPS system with:");
        GUILayout.Label("• First-person controller with movement and camera");
        GUILayout.Label("• Input System configuration");
        GUILayout.Label("• Interactable objects system");
        GUILayout.Label("• UI elements for interaction");
        GUILayout.Label("• ScriptableObjects for configuration");
        
        GUILayout.Space(10);
        
        GUILayout.Label("Requirements:");
        GUILayout.Label("• Unity 2022.3+ with URP");
        GUILayout.Label("• Input System package");
        GUILayout.Label("• TextMeshPro package");
        
        GUILayout.Space(10);
        
        hasReadWelcome = EditorGUILayout.Toggle("I understand and want to proceed", hasReadWelcome);
        
        if (!hasReadWelcome)
        {
            EditorGUILayout.HelpBox("Please check the box above to continue.", MessageType.Info);
        }
    }
    
    private void DrawScriptableObjectsStep()
    {
        GUILayout.Label("Step 1: ScriptableObjects", EditorStyles.boldLabel);
        GUILayout.Space(10);
        
        GUILayout.Label("We need to create or assign the required ScriptableObjects:");
        
        GUILayout.Space(10);
        
        // Player Settings
        EditorGUILayout.BeginHorizontal();
        playerSettings = (PlayerSettingsSO)EditorGUILayout.ObjectField("Player Settings", playerSettings, typeof(PlayerSettingsSO), false);
        if (GUILayout.Button("Create New", GUILayout.Width(100)))
        {
            playerSettings = CreatePlayerSettings();
        }
        EditorGUILayout.EndHorizontal();
        
        // Feedback Messages
        EditorGUILayout.BeginHorizontal();
        feedbackMessages = (FeedbackMessagesSO)EditorGUILayout.ObjectField("Feedback Messages", feedbackMessages, typeof(FeedbackMessagesSO), false);
        if (GUILayout.Button("Create New", GUILayout.Width(100)))
        {
            feedbackMessages = CreateFeedbackMessages();
        }
        EditorGUILayout.EndHorizontal();
        
        GUILayout.Space(10);
        
        if (playerSettings != null && feedbackMessages != null)
        {
            EditorGUILayout.HelpBox("✓ ScriptableObjects ready!", MessageType.Info);
        }
        else
        {
            EditorGUILayout.HelpBox("Please create or assign both ScriptableObjects.", MessageType.Warning);
        }
    }
    
    private void DrawInputSystemStep()
    {
        GUILayout.Label("Step 2: Input System", EditorStyles.boldLabel);
        GUILayout.Space(10);
        
        GUILayout.Label("We need to use the existing Input Actions Asset:");
        
        GUILayout.Space(10);
        
        EditorGUILayout.BeginHorizontal();
        inputActions = (InputActionAsset)EditorGUILayout.ObjectField("Input Actions", inputActions, typeof(InputActionAsset), false);
        if (GUILayout.Button("Load Existing", GUILayout.Width(100)))
        {
            inputActions = CreateInputActions();
        }
        EditorGUILayout.EndHorizontal();
        
        GUILayout.Space(10);
        
        if (inputActions != null)
        {
            EditorGUILayout.HelpBox("✓ Input System ready!", MessageType.Info);
        }
        else
        {
            EditorGUILayout.HelpBox("Please load the existing Input Actions Asset.", MessageType.Warning);
        }
    }
    
    private void DrawMaterialsStep()
    {
        GUILayout.Label("Step 3: Materials", EditorStyles.boldLabel);
        GUILayout.Space(10);
        
        GUILayout.Label("We need materials for the interactable objects:");
        
        GUILayout.Space(10);
        
        EditorGUILayout.BeginHorizontal();
        normalMaterial = (Material)EditorGUILayout.ObjectField("Normal Material", normalMaterial, typeof(Material), false);
        if (GUILayout.Button("Create New", GUILayout.Width(100)))
        {
            normalMaterial = CreateNormalMaterial();
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        highlightMaterial = (Material)EditorGUILayout.ObjectField("Highlight Material", highlightMaterial, typeof(Material), false);
        if (GUILayout.Button("Create New", GUILayout.Width(100)))
        {
            highlightMaterial = CreateHighlightMaterial();
        }
        EditorGUILayout.EndHorizontal();
        
        GUILayout.Space(10);
        
        if (normalMaterial != null && highlightMaterial != null)
        {
            EditorGUILayout.HelpBox("✓ Materials ready!", MessageType.Info);
        }
        else
        {
            EditorGUILayout.HelpBox("Please create both materials.", MessageType.Warning);
        }
    }
    
    private void DrawPrefabsStep()
    {
        GUILayout.Label("Step 4: Prefabs", EditorStyles.boldLabel);
        GUILayout.Space(10);
        
        GUILayout.Label("We need to create the prefabs:");
        
        GUILayout.Space(10);
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Create Player Prefab"))
        {
            CreatePlayerPrefab();
            playerPrefabCreated = true;
        }
        if (playerPrefabCreated)
        {
            EditorGUILayout.HelpBox("✓ Created", MessageType.Info);
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Create Interactable Prefab"))
        {
            CreateInteractablePrefab();
            interactablePrefabCreated = true;
        }
        if (interactablePrefabCreated)
        {
            EditorGUILayout.HelpBox("✓ Created", MessageType.Info);
        }
        EditorGUILayout.EndHorizontal();
        
        GUILayout.Space(10);
        
        if (playerPrefabCreated && interactablePrefabCreated)
        {
            EditorGUILayout.HelpBox("✓ Prefabs ready!", MessageType.Info);
        }
        else
        {
            EditorGUILayout.HelpBox("Please create both prefabs.", MessageType.Warning);
        }
    }
    
    private void DrawSceneSetupStep()
    {
        GUILayout.Label("Step 5: Scene Setup", EditorStyles.boldLabel);
        GUILayout.Space(10);
        
        GUILayout.Label("Final step - setup the scene:");
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("Setup Complete Scene"))
        {
            SetupCompleteScene();
            sceneSetup = true;
        }
        
        GUILayout.Space(10);
        
        if (sceneSetup)
        {
            EditorGUILayout.HelpBox("✓ Scene setup complete!", MessageType.Info);
            GUILayout.Space(10);
            GUILayout.Label("Next steps:");
            GUILayout.Label("1. Drag Player_FPS prefab to your scene");
            GUILayout.Label("2. Drag InteractableObject prefab to your scene");
            GUILayout.Label("3. Assign UI references to the Player");
            GUILayout.Label("4. Test in Play Mode!");
        }
        else
        {
            EditorGUILayout.HelpBox("Please setup the scene.", MessageType.Warning);
        }
    }
    
    private PlayerSettingsSO CreatePlayerSettings()
    {
        PlayerSettingsSO asset = ScriptableObject.CreateInstance<PlayerSettingsSO>();
        AssetDatabase.CreateAsset(asset, "Assets/PlayerSettings.asset");
        AssetDatabase.SaveAssets();
        return asset;
    }
    
    private FeedbackMessagesSO CreateFeedbackMessages()
    {
        FeedbackMessagesSO asset = ScriptableObject.CreateInstance<FeedbackMessagesSO>();
        AssetDatabase.CreateAsset(asset, "Assets/FeedbackMessages.asset");
        AssetDatabase.SaveAssets();
        return asset;
    }
    
    private InputActionAsset CreateInputActions()
    {
        // Use the existing Input Actions Asset
        InputActionAsset asset = AssetDatabase.LoadAssetAtPath<InputActionAsset>("Assets/InputSystem_Actions.inputactions");
        
        if (asset == null)
        {
            Debug.LogError("InputSystem_Actions.inputactions not found! Please ensure it exists in the Assets folder.");
            return null;
        }
        
        Debug.Log("Using existing Input Actions Asset: InputSystem_Actions");
        return asset;
    }
    
    private Material CreateNormalMaterial()
    {
        Material material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        material.name = "NormalMaterial";
        material.color = Color.white;
        AssetDatabase.CreateAsset(material, "Assets/Materials/NormalMaterial.mat");
        AssetDatabase.SaveAssets();
        return material;
    }
    
    private Material CreateHighlightMaterial()
    {
        Material material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        material.name = "HighlightMaterial";
        material.color = Color.yellow;
        material.SetFloat("_EmissionIntensity", 0.5f);
        AssetDatabase.CreateAsset(material, "Assets/Materials/HighlightMaterial.mat");
        AssetDatabase.SaveAssets();
        return material;
    }
    
    private void CreatePlayerPrefab()
    {
        // Use the existing FPSSystemTools to create the prefab
        FPSSystemTools tools = CreateInstance<FPSSystemTools>();
        tools.GeneratePlayerPrefab();
        DestroyImmediate(tools);
    }
    
    private void CreateInteractablePrefab()
    {
        // Use the existing FPSSystemTools to create the prefab
        FPSSystemTools tools = CreateInstance<FPSSystemTools>();
        tools.GenerateInteractablePrefab();
        DestroyImmediate(tools);
    }
    
    private void SetupCompleteScene()
    {
        // Use the existing FPSSystemTools to setup the scene
        FPSSystemTools tools = CreateInstance<FPSSystemTools>();
        tools.SetupCompleteScene();
        DestroyImmediate(tools);
    }
}

// ScriptRole: Editor wizard to guide users through FPS system setup
// Dependencies: All FPS system components and FPSSystemTools
// UsesSO: PlayerSettingsSO, FeedbackMessagesSO, InputActionAsset
// NeedsSetup: Follow wizard steps to configure system 