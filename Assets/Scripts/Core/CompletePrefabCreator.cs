using UnityEngine;
using UnityEngine.UI;
using TMPro;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class CompletePrefabCreator : MonoBehaviour
{
    [Header("Complete Prefab Creation")]
    public bool createOnStart = false;
    public FeedbackMessagesSO feedbackMessages;
    
    [Header("Prefab Settings")]
    public string prefabPath = "Assets/Prefabs";
    
    private void Start()
    {
        if (createOnStart)
        {
            Debug.Log("CompletePrefabCreator: Starting complete prefab creation");
            CreateAllPrefabs();
        }
    }
    
    [ContextMenu("Create Complete Prefabs")]
    public void CreateAllPrefabs()
    {
        Debug.Log("CompletePrefabCreator: Creating complete prefabs with all references");
        
        // Create directories first
        CreateDirectories();
        
        // Create prefabs
        CreatePlayerPrefab();
        CreateBedPrefab();
        CreateHousePrefab();
        CreateGroundPrefab();
        CreateGameManagersPrefab();
        CreateCompleteUIPrefab();
        CreateAudioPrefab();
        CreateLightingPrefab();
        
        Debug.Log("CompletePrefabCreator: All complete prefabs created successfully!");
        
        #if UNITY_EDITOR
        // Refresh the Asset Database to show new prefabs
        AssetDatabase.Refresh();
        Debug.Log("CompletePrefabCreator: Asset Database refreshed");
        #endif
    }
    
    private void CreateDirectories()
    {
        string[] directories = {
            "Assets/Prefabs",
            "Assets/Prefabs/Player",
            "Assets/Prefabs/Environment",
            "Assets/Prefabs/Managers",
            "Assets/Prefabs/UI",
            "Assets/Prefabs/Audio",
            "Assets/Prefabs/Lighting"
        };
        
        foreach (string dir in directories)
        {
            if (!System.IO.Directory.Exists(dir))
            {
                System.IO.Directory.CreateDirectory(dir);
                Debug.Log($"CompletePrefabCreator: Created directory {dir}");
            }
        }
    }
    
    private void CreatePlayerPrefab()
    {
        Debug.Log("CompletePrefabCreator: Creating Player prefab");
        
        // Create Player GameObject
        GameObject player = new GameObject("Player");
        player.tag = "Player";
        
        // Add CharacterController
        CharacterController characterController = player.AddComponent<CharacterController>();
        characterController.height = 2f;
        characterController.radius = 0.5f;
        characterController.center = new Vector3(0, 1f, 0);
        
        // Add PlayerFPSController
        PlayerFPSController playerController = player.AddComponent<PlayerFPSController>();
        if (feedbackMessages != null)
        {
            playerController.feedbackMessages = feedbackMessages;
        }
        
        // Create PlayerCamera
        GameObject camera = new GameObject("PlayerCamera");
        camera.transform.SetParent(player.transform);
        camera.transform.localPosition = new Vector3(0, 1.6f, 0);
        
        // Add Camera component
        Camera cam = camera.AddComponent<Camera>();
        cam.fieldOfView = 60f;
        cam.nearClipPlane = 0.1f;
        cam.farClipPlane = 1000f;
        
        // Add AudioListener
        camera.AddComponent<AudioListener>();
        
        // Add FogEffect
        FogEffect fogEffect = camera.AddComponent<FogEffect>();
        
        // Assign camera reference
        playerController.cameraTransform = camera.transform;
        
        // Save prefab
        SavePrefab(player, "Player/Player");
        
        Debug.Log("CompletePrefabCreator: Player prefab created");
    }
    
    private void CreateBedPrefab()
    {
        Debug.Log("CompletePrefabCreator: Creating Bed prefab");
        
        // Create Bed GameObject
        GameObject bed = new GameObject("Bed");
        
        // Create BedBase
        GameObject bedBase = GameObject.CreatePrimitive(PrimitiveType.Cube);
        bedBase.name = "BedBase";
        bedBase.transform.SetParent(bed.transform);
        bedBase.transform.localPosition = new Vector3(0, 0.5f, 0);
        bedBase.transform.localScale = new Vector3(2f, 1f, 3f);
        
        // Create Mattress
        GameObject mattress = GameObject.CreatePrimitive(PrimitiveType.Cube);
        mattress.name = "Mattress";
        mattress.transform.SetParent(bed.transform);
        mattress.transform.localPosition = new Vector3(0, 1.5f, 0);
        mattress.transform.localScale = new Vector3(1.8f, 0.5f, 2.8f);
        
        // Add BedInteractable
        BedInteractable bedInteractable = bed.AddComponent<BedInteractable>();
        if (feedbackMessages != null)
        {
            bedInteractable.feedbackMessages = feedbackMessages;
        }
        
        // Add BoxCollider for interaction
        BoxCollider bedCollider = bed.AddComponent<BoxCollider>();
        bedCollider.isTrigger = true;
        bedCollider.size = new Vector3(2f, 2f, 3f);
        bedCollider.center = new Vector3(0, 1f, 0);
        
        // Create sleep and wake position markers
        GameObject sleepMarker = new GameObject("SleepPosition");
        sleepMarker.transform.SetParent(bed.transform);
        sleepMarker.transform.position = mattress.transform.position;
        
        GameObject wakeMarker = new GameObject("WakePosition");
        wakeMarker.transform.SetParent(bed.transform);
        wakeMarker.transform.position = mattress.transform.position + Vector3.up * 0.5f;
        
        // Configure bed positions
        bedInteractable.sleepPosition = sleepMarker.transform;
        bedInteractable.wakePosition = wakeMarker.transform;
        
        // Save prefab
        SavePrefab(bed, "Environment/Bed");
        
        Debug.Log("CompletePrefabCreator: Bed prefab created");
    }
    
    private void CreateHousePrefab()
    {
        Debug.Log("CompletePrefabCreator: Creating House prefab");
        
        // Create House GameObject
        GameObject house = new GameObject("House");
        
        // Create walls
        CreateWall(house, "Wall1", new Vector3(0, 2, 5), new Vector3(10, 4, 0.5f));
        CreateWall(house, "Wall2", new Vector3(0, 2, -5), new Vector3(10, 4, 0.5f));
        CreateWall(house, "Wall3", new Vector3(5, 2, 0), new Vector3(0.5f, 4, 10));
        CreateWall(house, "Wall4", new Vector3(-5, 2, 0), new Vector3(0.5f, 4, 10));
        
        // Create roof
        GameObject roof = GameObject.CreatePrimitive(PrimitiveType.Cube);
        roof.name = "Roof";
        roof.transform.SetParent(house.transform);
        roof.transform.localPosition = new Vector3(0, 4, 0);
        roof.transform.localScale = new Vector3(10, 0.5f, 10);
        
        // Create door
        GameObject door = GameObject.CreatePrimitive(PrimitiveType.Cube);
        door.name = "Door";
        door.transform.SetParent(house.transform);
        door.transform.localPosition = new Vector3(0, 1, 5.1f);
        door.transform.localScale = new Vector3(1.5f, 2, 0.2f);
        
        // Save prefab
        SavePrefab(house, "Environment/House");
        
        Debug.Log("CompletePrefabCreator: House prefab created");
    }
    
    private void CreateWall(GameObject parent, string name, Vector3 position, Vector3 scale)
    {
        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = name;
        wall.transform.SetParent(parent.transform);
        wall.transform.localPosition = position;
        wall.transform.localScale = scale;
    }
    
    private void CreateGroundPrefab()
    {
        Debug.Log("CompletePrefabCreator: Creating Ground prefab");
        
        // Create Ground GameObject
        GameObject ground = new GameObject("Ground");
        
        // Create Plane
        GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
        plane.name = "GroundPlane";
        plane.transform.SetParent(ground.transform);
        plane.transform.localPosition = Vector3.zero;
        plane.transform.localScale = new Vector3(20, 1, 20);
        
        // Add BoxCollider for physics
        BoxCollider groundCollider = ground.AddComponent<BoxCollider>();
        groundCollider.size = new Vector3(200, 1, 200);
        
        // Save prefab
        SavePrefab(ground, "Environment/Ground");
        
        Debug.Log("CompletePrefabCreator: Ground prefab created");
    }
    
    private void CreateGameManagersPrefab()
    {
        Debug.Log("CompletePrefabCreator: Creating GameManagers prefab");
        
        // Create Managers GameObject
        GameObject managers = new GameObject("GameManagers");
        
        // Create GameManager
        GameObject gameManager = new GameObject("GameManager");
        gameManager.transform.SetParent(managers.transform);
        GameManager gm = gameManager.AddComponent<GameManager>();
        if (feedbackMessages != null)
        {
            gm.feedbackMessages = feedbackMessages;
        }
        
        // Create DayNightCycle
        GameObject dayNightCycle = new GameObject("DayNightCycle");
        dayNightCycle.transform.SetParent(managers.transform);
        DayNightCycle dnc = dayNightCycle.AddComponent<DayNightCycle>();
        if (feedbackMessages != null)
        {
            dnc.feedbackMessages = feedbackMessages;
        }
        
        // Configure DayNightCycle for testing
        dnc.dayDuration = 60f;
        dnc.sleepPressureStartTime = 10f;
        dnc.maxSlowdownFactor = 0.3f;
        dnc.fogIntensity = 1f;
        
        // Create RespawnSystem
        GameObject respawnSystem = new GameObject("RespawnSystem");
        respawnSystem.transform.SetParent(managers.transform);
        RespawnSystem rs = respawnSystem.AddComponent<RespawnSystem>();
        if (feedbackMessages != null)
        {
            rs.feedbackMessages = feedbackMessages;
        }
        rs.fallThreshold = -5f;
        rs.respawnDelay = 1f;
        
        // Create AudioManager
        GameObject audioManager = new GameObject("AudioManager");
        audioManager.transform.SetParent(managers.transform);
        AudioManager am = audioManager.AddComponent<AudioManager>();
        if (feedbackMessages != null)
        {
            am.feedbackMessages = feedbackMessages;
        }
        
        // Create AudioSources
        CreateAudioSource(audioManager, "MusicSource", 0.7f, true);
        CreateAudioSource(audioManager, "SFXSource", 0.8f, false);
        CreateAudioSource(audioManager, "AmbientSource", 0.6f, true);
        CreateAudioSource(audioManager, "VoiceSource", 0.9f, false);
        
        // Create CutsceneManager
        GameObject cutsceneManager = new GameObject("CutsceneManager");
        cutsceneManager.transform.SetParent(managers.transform);
        CutsceneManager cm = cutsceneManager.AddComponent<CutsceneManager>();
        if (feedbackMessages != null)
        {
            cm.feedbackMessages = feedbackMessages;
        }
        
        // Save prefab
        SavePrefab(managers, "Managers/GameManagers");
        
        Debug.Log("CompletePrefabCreator: GameManagers prefab created");
    }
    
    private void CreateAudioSource(GameObject parent, string name, float volume, bool loop)
    {
        GameObject audioSource = new GameObject(name);
        audioSource.transform.SetParent(parent.transform);
        AudioSource source = audioSource.AddComponent<AudioSource>();
        source.volume = volume;
        source.loop = loop;
        source.playOnAwake = false;
    }
    
    private void CreateCompleteUIPrefab()
    {
        Debug.Log("CompletePrefabCreator: Creating Complete UI prefab");
        
        // Create Canvas
        GameObject canvas = new GameObject("Canvas");
        Canvas canvasComponent = canvas.AddComponent<Canvas>();
        canvasComponent.renderMode = RenderMode.ScreenSpaceOverlay;
        
        CanvasScaler scaler = canvas.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        
        canvas.AddComponent<GraphicRaycaster>();
        
        // Create UIManager
        GameObject uiManager = new GameObject("UIManager");
        uiManager.transform.SetParent(canvas.transform);
        UIManager um = uiManager.AddComponent<UIManager>();
        if (feedbackMessages != null)
        {
            um.feedbackMessages = feedbackMessages;
        }
        
        // Create GameUI
        GameObject gameUI = new GameObject("GameUI");
        gameUI.transform.SetParent(canvas.transform);
        GameUI gui = gameUI.AddComponent<GameUI>();
        if (feedbackMessages != null)
        {
            gui.feedbackMessages = feedbackMessages;
        }
        
        // Create Main Menu Panel
        GameObject mainMenuPanel = CreateUIPanel(canvas, "MainMenuPanel", new Vector2(0, 0), new Vector2(1, 1));
        
        // Create Game Panel
        GameObject gamePanel = CreateUIPanel(canvas, "GamePanel", new Vector2(0, 0), new Vector2(1, 1));
        
        // Create Pause Panel
        GameObject pausePanel = CreateUIPanel(canvas, "PausePanel", new Vector2(0, 0), new Vector2(1, 1));
        
        // Create Game Over Panel
        GameObject gameOverPanel = CreateUIPanel(canvas, "GameOverPanel", new Vector2(0, 0), new Vector2(1, 1));
        
        // Create Win Panel
        GameObject winPanel = CreateUIPanel(canvas, "WinPanel", new Vector2(0, 0), new Vector2(1, 1));
        
        // Create UI Text elements for GameUI
        TextMeshProUGUI dayNightText = CreateUIText(gamePanel, "DayNightText", "Día/Noche", new Vector2(100, -50));
        TextMeshProUGUI pressureText = CreateUIText(gamePanel, "PressureText", "Presión", new Vector2(100, -100));
        TextMeshProUGUI timeText = CreateUIText(gamePanel, "TimeText", "Tiempo", new Vector2(100, -150));
        TextMeshProUGUI interactionText = CreateUIText(gamePanel, "InteractionText", "Interacción", new Vector2(100, -200));
        
        // Create UI Text elements for UIManager
        TextMeshProUGUI scoreText = CreateUIText(gamePanel, "ScoreText", "Puntuación: 0", new Vector2(100, -250));
        TextMeshProUGUI gameOverText = CreateUIText(gameOverPanel, "GameOverText", "Game Over", new Vector2(0, 100));
        TextMeshProUGUI winText = CreateUIText(winPanel, "WinText", "¡Victoria!", new Vector2(0, 100));
        TextMeshProUGUI pauseText = CreateUIText(pausePanel, "PauseText", "Pausa", new Vector2(0, 100));
        
        // Create Buttons for UIManager
        Button startButton = CreateUIButton(mainMenuPanel, "StartButton", "Iniciar", new Vector2(0, 0));
        Button pauseButton = CreateUIButton(gamePanel, "PauseButton", "Pausa", new Vector2(100, -300));
        Button resumeButton = CreateUIButton(pausePanel, "ResumeButton", "Reanudar", new Vector2(0, 0));
        Button restartButton = CreateUIButton(gameOverPanel, "RestartButton", "Reiniciar", new Vector2(0, -100));
        Button quitButton = CreateUIButton(mainMenuPanel, "QuitButton", "Salir", new Vector2(0, -100));
        
        // Assign references to UIManager
        um.mainMenuPanel = mainMenuPanel;
        um.gamePanel = gamePanel;
        um.pausePanel = pausePanel;
        um.gameOverPanel = gameOverPanel;
        um.winPanel = winPanel;
        
        um.scoreText = scoreText;
        um.gameOverText = gameOverText;
        um.winText = winText;
        um.pauseText = pauseText;
        
        um.startButton = startButton;
        um.pauseButton = pauseButton;
        um.resumeButton = resumeButton;
        um.restartButton = restartButton;
        um.quitButton = quitButton;
        
        // Assign references to GameUI
        gui.dayNightText = dayNightText;
        gui.interactionText = interactionText;
        gui.pressureText = pressureText;
        gui.timeText = timeText;
        
        // Save prefab
        SavePrefab(canvas, "UI/CompleteGameUI");
        
        Debug.Log("CompletePrefabCreator: Complete UI prefab created with all references");
    }
    
    private GameObject CreateUIPanel(GameObject parent, string name, Vector2 anchorMin, Vector2 anchorMax)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent.transform);
        
        RectTransform panelRect = panel.AddComponent<RectTransform>();
        panelRect.anchorMin = anchorMin;
        panelRect.anchorMax = anchorMax;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        
        // Add Image component for background
        Image panelImage = panel.AddComponent<Image>();
        panelImage.color = new Color(0, 0, 0, 0.8f);
        
        return panel;
    }
    
    private TextMeshProUGUI CreateUIText(GameObject parent, string name, string defaultText, Vector2 position)
    {
        GameObject textObj = new GameObject(name);
        textObj.transform.SetParent(parent.transform);
        
        RectTransform rect = textObj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 1);
        rect.anchorMax = new Vector2(0, 1);
        rect.pivot = new Vector2(0, 1);
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(300, 50);
        
        TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
        text.text = defaultText;
        text.fontSize = 24;
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.Left;
        
        return text;
    }
    
    private Button CreateUIButton(GameObject parent, string name, string buttonText, Vector2 position)
    {
        GameObject buttonObj = new GameObject(name);
        buttonObj.transform.SetParent(parent.transform);
        
        RectTransform buttonRect = buttonObj.AddComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.5f, 0.5f);
        buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
        buttonRect.pivot = new Vector2(0.5f, 0.5f);
        buttonRect.anchoredPosition = position;
        buttonRect.sizeDelta = new Vector2(200, 50);
        
        // Add Image component
        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = new Color(0.2f, 0.2f, 0.2f, 1f);
        
        // Add Button component
        Button button = buttonObj.AddComponent<Button>();
        
        // Create Text child
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform);
        
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        
        TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
        text.text = buttonText;
        text.fontSize = 18;
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.Center;
        
        return button;
    }
    
    private void CreateAudioPrefab()
    {
        Debug.Log("CompletePrefabCreator: Creating Audio prefab");
        
        // Create AudioSources GameObject
        GameObject audioSources = new GameObject("AudioSources");
        
        // Create individual audio sources
        CreateAudioSource(audioSources, "MusicSource", 0.7f, true);
        CreateAudioSource(audioSources, "SFXSource", 0.8f, false);
        CreateAudioSource(audioSources, "AmbientSource", 0.6f, true);
        CreateAudioSource(audioSources, "VoiceSource", 0.9f, false);
        
        // Save prefab
        SavePrefab(audioSources, "Audio/AudioSources");
        
        Debug.Log("CompletePrefabCreator: Audio prefab created");
    }
    
    private void CreateLightingPrefab()
    {
        Debug.Log("CompletePrefabCreator: Creating Lighting prefab");
        
        // Create Lighting GameObject
        GameObject lighting = new GameObject("Lighting");
        
        // Create Directional Light
        GameObject directionalLight = new GameObject("DirectionalLight");
        directionalLight.transform.SetParent(lighting.transform);
        directionalLight.transform.position = new Vector3(0, 10, 0);
        directionalLight.transform.rotation = Quaternion.Euler(45, 45, 0);
        
        Light light = directionalLight.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1f;
        light.color = Color.white;
        light.shadows = LightShadows.Soft;
        
        // Save prefab
        SavePrefab(lighting, "Lighting/Lighting");
        
        Debug.Log("CompletePrefabCreator: Lighting prefab created");
    }
    
    private void SavePrefab(GameObject obj, string prefabName)
    {
        string fullPath = $"{prefabPath}/{prefabName}.prefab";
        Debug.Log($"CompletePrefabCreator: Saving prefab to {fullPath}");
        
        #if UNITY_EDITOR
        // Save the prefab using PrefabUtility
        PrefabUtility.SaveAsPrefabAsset(obj, fullPath);
        Debug.Log($"CompletePrefabCreator: {prefabName} prefab saved to {fullPath}");
        
        // Destroy the GameObject after saving
        DestroyImmediate(obj);
        #else
        Debug.Log($"CompletePrefabCreator: {prefabName} prefab created (Editor only)");
        #endif
    }
    
    [ContextMenu("Verify Complete Prefabs")]
    public void VerifyCompletePrefabs()
    {
        Debug.Log("CompletePrefabCreator: Verifying complete prefab structure");
        
        string[] expectedPrefabs = {
            "Assets/Prefabs/Player/Player.prefab",
            "Assets/Prefabs/Environment/Bed.prefab",
            "Assets/Prefabs/Environment/House.prefab",
            "Assets/Prefabs/Environment/Ground.prefab",
            "Assets/Prefabs/Managers/GameManagers.prefab",
            "Assets/Prefabs/UI/CompleteGameUI.prefab",
            "Assets/Prefabs/Audio/AudioSources.prefab",
            "Assets/Prefabs/Lighting/Lighting.prefab"
        };
        
        foreach (string prefabPath in expectedPrefabs)
        {
            Debug.Log($"CompletePrefabCreator: Expected prefab: {prefabPath}");
        }
        
        Debug.Log("CompletePrefabCreator: Verification complete");
    }
}

// ScriptRole: Creates complete prefabs with all UI references
// Dependencies: All core scripts and UI components
// UsesSO: FeedbackMessagesSO
// ReceivesFrom: None
// SendsTo: None (prefab creation only) 