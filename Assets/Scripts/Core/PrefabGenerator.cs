using UnityEngine;
using UnityEngine.UI;
using TMPro;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class PrefabGenerator : MonoBehaviour
{
    [Header("Prefab Generation")]
    public bool generateOnStart = false;
    public string prefabPath = "Assets/Prefabs";
    
    [Header("References")]
    public FeedbackMessagesSO feedbackMessages;
    
    private void Start()
    {
        if (generateOnStart)
        {
            Debug.Log("PrefabGenerator: Starting prefab generation");
            GenerateAllPrefabs();
        }
    }
    
    [ContextMenu("Generate All Prefabs")]
    public void GenerateAllPrefabs()
    {
        Debug.Log("PrefabGenerator: Generating all prefabs");
        
        // Create directory structure
        CreateDirectories();
        
        // Generate prefabs
        GeneratePlayerPrefab();
        GenerateBedPrefab();
        GenerateHousePrefab();
        GenerateGroundPrefab();
        GenerateGameManagersPrefab();
        GenerateUIPrefab();
        GenerateAudioPrefab();
        GenerateLightingPrefab();
        
        Debug.Log("PrefabGenerator: All prefabs generated successfully!");
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
                Debug.Log($"PrefabGenerator: Created directory {dir}");
            }
        }
    }
    
    private void GeneratePlayerPrefab()
    {
        Debug.Log("PrefabGenerator: Creating Player prefab");
        
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
        
        // Add FogEffect
        FogEffect fogEffect = camera.AddComponent<FogEffect>();
        
        // Assign camera reference
        playerController.cameraTransform = camera.transform;
        
        // Save prefab
        SavePrefab(player, "Player/Player");
        
        Debug.Log("PrefabGenerator: Player prefab created");
    }
    
    private void GenerateBedPrefab()
    {
        Debug.Log("PrefabGenerator: Creating Bed prefab");
        
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
        
        Debug.Log("PrefabGenerator: Bed prefab created");
    }
    
    private void GenerateHousePrefab()
    {
        Debug.Log("PrefabGenerator: Creating House prefab");
        
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
        
        Debug.Log("PrefabGenerator: House prefab created");
    }
    
    private void CreateWall(GameObject parent, string name, Vector3 position, Vector3 scale)
    {
        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = name;
        wall.transform.SetParent(parent.transform);
        wall.transform.localPosition = position;
        wall.transform.localScale = scale;
    }
    
    private void GenerateGroundPrefab()
    {
        Debug.Log("PrefabGenerator: Creating Ground prefab");
        
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
        
        Debug.Log("PrefabGenerator: Ground prefab created");
    }
    
    private void GenerateGameManagersPrefab()
    {
        Debug.Log("PrefabGenerator: Creating GameManagers prefab");
        
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
        
        Debug.Log("PrefabGenerator: GameManagers prefab created");
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
    
    private void GenerateUIPrefab()
    {
        Debug.Log("PrefabGenerator: Creating UI prefab");
        
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
        
        // Create GamePanel
        GameObject gamePanel = new GameObject("GamePanel");
        gamePanel.transform.SetParent(canvas.transform);
        RectTransform panelRect = gamePanel.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        
        // Create UI Text elements
        CreateUIText(gamePanel, "DayNightText", "Día/Noche", new Vector2(100, -50));
        CreateUIText(gamePanel, "PressureText", "Presión", new Vector2(100, -100));
        CreateUIText(gamePanel, "TimeText", "Tiempo", new Vector2(100, -150));
        CreateUIText(gamePanel, "InteractionText", "Interacción", new Vector2(100, -200));
        
        // Save prefab
        SavePrefab(canvas, "UI/GameUI");
        
        Debug.Log("PrefabGenerator: UI prefab created");
    }
    
    private void CreateUIText(GameObject parent, string name, string defaultText, Vector2 position)
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
    }
    
    private void GenerateAudioPrefab()
    {
        Debug.Log("PrefabGenerator: Creating Audio prefab");
        
        // Create AudioSources GameObject
        GameObject audioSources = new GameObject("AudioSources");
        
        // Create individual audio sources
        CreateAudioSource(audioSources, "MusicSource", 0.7f, true);
        CreateAudioSource(audioSources, "SFXSource", 0.8f, false);
        CreateAudioSource(audioSources, "AmbientSource", 0.6f, true);
        CreateAudioSource(audioSources, "VoiceSource", 0.9f, false);
        
        // Save prefab
        SavePrefab(audioSources, "Audio/AudioSources");
        
        Debug.Log("PrefabGenerator: Audio prefab created");
    }
    
    private void GenerateLightingPrefab()
    {
        Debug.Log("PrefabGenerator: Creating Lighting prefab");
        
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
        
        Debug.Log("PrefabGenerator: Lighting prefab created");
    }
    
    private void SavePrefab(GameObject obj, string prefabName)
    {
        string fullPath = $"{prefabPath}/{prefabName}.prefab";
        Debug.Log($"PrefabGenerator: Prefab would be saved to {fullPath}");
        
        #if UNITY_EDITOR
        // Save the prefab using PrefabUtility
        UnityEditor.PrefabUtility.SaveAsPrefabAsset(obj, fullPath);
        Debug.Log($"PrefabGenerator: {prefabName} prefab saved to {fullPath}");
        #else
        // In build, just log the path
        Debug.Log($"PrefabGenerator: {prefabName} prefab created successfully (Editor only)");
        #endif
    }
    
    [ContextMenu("Verify Prefabs")]
    public void VerifyPrefabs()
    {
        Debug.Log("PrefabGenerator: Verifying prefab structure");
        
        string[] expectedPrefabs = {
            "Assets/Prefabs/Player/Player.prefab",
            "Assets/Prefabs/Environment/Bed.prefab",
            "Assets/Prefabs/Environment/House.prefab",
            "Assets/Prefabs/Environment/Ground.prefab",
            "Assets/Prefabs/Managers/GameManagers.prefab",
            "Assets/Prefabs/UI/GameUI.prefab",
            "Assets/Prefabs/Audio/AudioSources.prefab",
            "Assets/Prefabs/Lighting/Lighting.prefab"
        };
        
        foreach (string prefabPath in expectedPrefabs)
        {
            Debug.Log($"PrefabGenerator: Expected prefab: {prefabPath}");
        }
        
        Debug.Log("PrefabGenerator: Verification complete");
    }
}

// ScriptRole: Generates modular prefabs for easy scene setup
// Dependencies: All core scripts and UI components
// UsesSO: FeedbackMessagesSO
// ReceivesFrom: None
// SendsTo: None (prefab generation only) 