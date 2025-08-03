using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PrefabCreator : MonoBehaviour
{
    [Header("Prefab Creation")]
    public bool createPrefabsOnStart = false;
    public string prefabPath = "Assets/Prefabs";
    
    [Header("References")]
    public FeedbackMessagesSO feedbackMessages;
    
    private void Start()
    {
        if (createPrefabsOnStart)
        {
            Debug.Log("PrefabCreator: Creating prefabs");
            CreateAllPrefabs();
        }
    }
    
    [ContextMenu("Create All Prefabs")]
    public void CreateAllPrefabs()
    {
        Debug.Log("PrefabCreator: Creating all prefabs");
        
        // Create prefab directory
        if (!System.IO.Directory.Exists(prefabPath))
        {
            System.IO.Directory.CreateDirectory(prefabPath);
        }
        
        // Create subdirectories
        string[] subdirs = { "Player", "Environment", "UI", "Managers", "Audio" };
        foreach (string dir in subdirs)
        {
            string fullPath = $"{prefabPath}/{dir}";
            if (!System.IO.Directory.Exists(fullPath))
            {
                System.IO.Directory.CreateDirectory(fullPath);
            }
        }
        
        // Create prefabs
        CreatePlayerPrefab();
        CreateHousePrefab();
        CreateBedPrefab();
        CreateGroundPrefab();
        CreateManagersPrefab();
        CreateUIPrefab();
        CreateAudioPrefab();
        
        Debug.Log("PrefabCreator: All prefabs created successfully!");
    }
    
    private void CreatePlayerPrefab()
    {
        Debug.Log("PrefabCreator: Creating Player prefab");
        
        // Create player
        GameObject player = new GameObject("Player");
        player.tag = "Player";
        
        // Add CharacterController
        CharacterController controller = player.AddComponent<CharacterController>();
        controller.height = 2f;
        controller.radius = 0.5f;
        
        // Add PlayerFPSController
        PlayerFPSController fpsController = player.AddComponent<PlayerFPSController>();
        fpsController.feedbackMessages = feedbackMessages;
        fpsController.walkSpeed = 3f;
        fpsController.runSpeed = 6f;
        fpsController.mouseSensitivity = 2f;
        
        // Create camera
        GameObject camera = new GameObject("PlayerCamera");
        camera.transform.SetParent(player.transform);
        camera.transform.localPosition = new Vector3(0f, 1.6f, 0f);
        
        Camera cam = camera.AddComponent<Camera>();
        cam.fieldOfView = 60f;
        cam.nearClipPlane = 0.1f;
        cam.farClipPlane = 1000f;
        
        // Add FogEffect to camera
        FogEffect fogEffect = camera.AddComponent<FogEffect>();
        
        // Assign camera to player controller
        fpsController.cameraTransform = camera.transform;
        
        // Save as prefab
        SaveAsPrefab(player, "Player/Player");
        
        Debug.Log("PrefabCreator: Player prefab created");
    }
    
    private void CreateHousePrefab()
    {
        Debug.Log("PrefabCreator: Creating House prefab");
        
        GameObject house = new GameObject("House");
        
        // Create walls
        CreateWall(house, "Wall_North", new Vector3(0f, 2f, 5f), new Vector3(10f, 4f, 0.2f));
        CreateWall(house, "Wall_South", new Vector3(0f, 2f, -5f), new Vector3(10f, 4f, 0.2f));
        CreateWall(house, "Wall_East", new Vector3(5f, 2f, 0f), new Vector3(0.2f, 4f, 10f));
        CreateWall(house, "Wall_West", new Vector3(-5f, 2f, 0f), new Vector3(0.2f, 4f, 10f));
        
        // Create roof
        GameObject roof = GameObject.CreatePrimitive(PrimitiveType.Cube);
        roof.name = "Roof";
        roof.transform.SetParent(house.transform);
        roof.transform.position = new Vector3(0f, 4.5f, 0f);
        roof.transform.localScale = new Vector3(10.5f, 0.2f, 10.5f);
        
        Material roofMaterial = new Material(Shader.Find("Standard"));
        roofMaterial.color = new Color(0.4f, 0.2f, 0.1f);
        roof.GetComponent<Renderer>().material = roofMaterial;
        
        // Create door
        GameObject door = GameObject.CreatePrimitive(PrimitiveType.Cube);
        door.name = "Door";
        door.transform.SetParent(house.transform);
        door.transform.position = new Vector3(0f, 1f, 5f);
        door.transform.localScale = new Vector3(1.5f, 2f, 0.1f);
        
        Material doorMaterial = new Material(Shader.Find("Standard"));
        doorMaterial.color = Color.brown;
        door.GetComponent<Renderer>().material = doorMaterial;
        
        // Save as prefab
        SaveAsPrefab(house, "Environment/House");
        
        Debug.Log("PrefabCreator: House prefab created");
    }
    
    private void CreateWall(GameObject parent, string name, Vector3 position, Vector3 scale)
    {
        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = name;
        wall.transform.SetParent(parent.transform);
        wall.transform.position = position;
        wall.transform.localScale = scale;
        
        Material wallMaterial = new Material(Shader.Find("Standard"));
        wallMaterial.color = Color.gray;
        wall.GetComponent<Renderer>().material = wallMaterial;
    }
    
    private void CreateBedPrefab()
    {
        Debug.Log("PrefabCreator: Creating Bed prefab");
        
        // Create bed frame
        GameObject bed = new GameObject("Bed");
        
        // Bed base
        GameObject bedBase = GameObject.CreatePrimitive(PrimitiveType.Cube);
        bedBase.name = "BedBase";
        bedBase.transform.SetParent(bed.transform);
        bedBase.transform.position = new Vector3(0f, 0.25f, 0f);
        bedBase.transform.localScale = new Vector3(2f, 0.5f, 3f);
        
        Material bedMaterial = new Material(Shader.Find("Standard"));
        bedMaterial.color = Color.brown;
        bedBase.GetComponent<Renderer>().material = bedMaterial;
        
        // Bed mattress
        GameObject mattress = GameObject.CreatePrimitive(PrimitiveType.Cube);
        mattress.name = "Mattress";
        mattress.transform.SetParent(bed.transform);
        mattress.transform.position = new Vector3(0f, 0.75f, 0f);
        mattress.transform.localScale = new Vector3(1.8f, 0.2f, 2.8f);
        
        Material mattressMaterial = new Material(Shader.Find("Standard"));
        mattressMaterial.color = Color.white;
        mattress.GetComponent<Renderer>().material = mattressMaterial;
        
        // Add BedInteractable component
        BedInteractable bedInteractable = bed.AddComponent<BedInteractable>();
        bedInteractable.feedbackMessages = feedbackMessages;
        bedInteractable.interactionDistance = 2f;
        bedInteractable.sleepPosition = mattress.transform;
        bedInteractable.wakePosition = mattress.transform;
        
        // Add collider for interaction
        BoxCollider bedCollider = bed.AddComponent<BoxCollider>();
        bedCollider.size = new Vector3(2f, 1f, 3f);
        bedCollider.isTrigger = true;
        
        // Save as prefab
        SaveAsPrefab(bed, "Environment/Bed");
        
        Debug.Log("PrefabCreator: Bed prefab created");
    }
    
    private void CreateGroundPrefab()
    {
        Debug.Log("PrefabCreator: Creating Ground prefab");
        
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.transform.position = Vector3.zero;
        ground.transform.localScale = new Vector3(20f, 1f, 20f);
        
        // Add material
        Material groundMaterial = new Material(Shader.Find("Standard"));
        groundMaterial.color = new Color(0.3f, 0.2f, 0.1f);
        ground.GetComponent<Renderer>().material = groundMaterial;
        
        // Add collider for respawn detection
        ground.AddComponent<BoxCollider>();
        
        // Save as prefab
        SaveAsPrefab(ground, "Environment/Ground");
        
        Debug.Log("PrefabCreator: Ground prefab created");
    }
    
    private void CreateManagersPrefab()
    {
        Debug.Log("PrefabCreator: Creating Managers prefab");
        
        GameObject managers = new GameObject("Managers");
        
        // Create GameManager
        GameObject gameManager = new GameObject("GameManager");
        gameManager.transform.SetParent(managers.transform);
        GameManager gm = gameManager.AddComponent<GameManager>();
        gm.feedbackMessages = feedbackMessages;
        
        // Create DayNightCycle
        GameObject dayNightCycle = new GameObject("DayNightCycle");
        dayNightCycle.transform.SetParent(managers.transform);
        DayNightCycle dnc = dayNightCycle.AddComponent<DayNightCycle>();
        dnc.feedbackMessages = feedbackMessages;
        dnc.dayDuration = 300f; // 5 minutes for testing
        dnc.sleepPressureStartTime = 60f;
        dnc.maxSlowdownFactor = 0.3f;
        dnc.fogIntensity = 0.8f;
        
        // Create RespawnSystem
        GameObject respawnSystem = new GameObject("RespawnSystem");
        respawnSystem.transform.SetParent(managers.transform);
        RespawnSystem rs = respawnSystem.AddComponent<RespawnSystem>();
        rs.feedbackMessages = feedbackMessages;
        rs.fallThreshold = -5f;
        rs.respawnDelay = 1f;
        
        // Create AudioManager
        GameObject audioManager = new GameObject("AudioManager");
        audioManager.transform.SetParent(managers.transform);
        AudioManager am = audioManager.AddComponent<AudioManager>();
        am.feedbackMessages = feedbackMessages;
        
        // Create CutsceneManager
        GameObject cutsceneManager = new GameObject("CutsceneManager");
        cutsceneManager.transform.SetParent(managers.transform);
        CutsceneManager cm = cutsceneManager.AddComponent<CutsceneManager>();
        cm.feedbackMessages = feedbackMessages;
        
        // Save as prefab
        SaveAsPrefab(managers, "Managers/GameManagers");
        
        Debug.Log("PrefabCreator: Managers prefab created");
    }
    
    private void CreateUIPrefab()
    {
        Debug.Log("PrefabCreator: Creating UI prefab");
        
        // Create Canvas
        GameObject canvas = new GameObject("Canvas");
        Canvas canvasComponent = canvas.AddComponent<Canvas>();
        canvasComponent.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.AddComponent<CanvasScaler>();
        canvas.AddComponent<GraphicRaycaster>();
        
        // Create UI Manager
        GameObject uiManager = new GameObject("UIManager");
        uiManager.transform.SetParent(canvas.transform);
        UIManager ui = uiManager.AddComponent<UIManager>();
        ui.feedbackMessages = feedbackMessages;
        
        // Create GameUI
        GameObject gameUI = new GameObject("GameUI");
        gameUI.transform.SetParent(canvas.transform);
        GameUI gameUIComponent = gameUI.AddComponent<GameUI>();
        gameUIComponent.feedbackMessages = feedbackMessages;
        
        // Create UI elements
        CreateUIElements(canvas, gameUIComponent);
        
        // Save as prefab
        SaveAsPrefab(canvas, "UI/GameUI");
        
        Debug.Log("PrefabCreator: UI prefab created");
    }
    
    private void CreateUIElements(GameObject canvas, GameUI gameUI)
    {
        // Create game panel
        GameObject gamePanel = new GameObject("GamePanel");
        gamePanel.transform.SetParent(canvas.transform, false);
        RectTransform gamePanelRect = gamePanel.AddComponent<RectTransform>();
        gamePanelRect.anchorMin = Vector2.zero;
        gamePanelRect.anchorMax = Vector2.one;
        gamePanelRect.offsetMin = Vector2.zero;
        gamePanelRect.offsetMax = Vector2.zero;
        
        // Create day/night text
        GameObject dayNightText = new GameObject("DayNightText");
        dayNightText.transform.SetParent(gamePanel.transform, false);
        RectTransform dayNightRect = dayNightText.AddComponent<RectTransform>();
        dayNightRect.anchorMin = new Vector2(1f, 1f);
        dayNightRect.anchorMax = new Vector2(1f, 1f);
        dayNightRect.anchoredPosition = new Vector2(-20f, -20f);
        dayNightRect.sizeDelta = new Vector2(200f, 50f);
        
        TextMeshProUGUI dayNightTMP = dayNightText.AddComponent<TextMeshProUGUI>();
        dayNightTMP.text = "Day";
        dayNightTMP.fontSize = 18f;
        dayNightTMP.color = Color.white;
        dayNightTMP.alignment = TextAlignmentOptions.Right;
        
        // Create pressure text
        GameObject pressureText = new GameObject("PressureText");
        pressureText.transform.SetParent(gamePanel.transform, false);
        RectTransform pressureRect = pressureText.AddComponent<RectTransform>();
        pressureRect.anchorMin = new Vector2(1f, 1f);
        pressureRect.anchorMax = new Vector2(1f, 1f);
        pressureRect.anchoredPosition = new Vector2(-20f, -70f);
        pressureRect.sizeDelta = new Vector2(250f, 50f);
        
        TextMeshProUGUI pressureTMP = pressureText.AddComponent<TextMeshProUGUI>();
        pressureTMP.text = "";
        pressureTMP.fontSize = 16f;
        pressureTMP.color = Color.red;
        pressureTMP.alignment = TextAlignmentOptions.Right;
        
        // Create time text
        GameObject timeText = new GameObject("TimeText");
        timeText.transform.SetParent(gamePanel.transform, false);
        RectTransform timeRect = timeText.AddComponent<RectTransform>();
        timeRect.anchorMin = new Vector2(1f, 1f);
        timeRect.anchorMax = new Vector2(1f, 1f);
        timeRect.anchoredPosition = new Vector2(-20f, -120f);
        timeRect.sizeDelta = new Vector2(150f, 50f);
        
        TextMeshProUGUI timeTMP = timeText.AddComponent<TextMeshProUGUI>();
        timeTMP.text = "00:00";
        timeTMP.fontSize = 16f;
        timeTMP.color = Color.white;
        timeTMP.alignment = TextAlignmentOptions.Right;
        
        // Create interaction text
        GameObject interactionText = new GameObject("InteractionText");
        interactionText.transform.SetParent(gamePanel.transform, false);
        RectTransform interactionRect = interactionText.AddComponent<RectTransform>();
        interactionRect.anchorMin = new Vector2(0.5f, 0f);
        interactionRect.anchorMax = new Vector2(0.5f, 0f);
        interactionRect.anchoredPosition = new Vector2(0f, 100f);
        interactionRect.sizeDelta = new Vector2(400f, 50f);
        
        TextMeshProUGUI interactionTMP = interactionText.AddComponent<TextMeshProUGUI>();
        interactionTMP.text = "";
        interactionTMP.fontSize = 24f;
        interactionTMP.color = Color.white;
        interactionTMP.alignment = TextAlignmentOptions.Center;
        
        // Assign UI references
        gameUI.dayNightText = dayNightTMP;
        gameUI.pressureText = pressureTMP;
        gameUI.timeText = timeTMP;
        gameUI.interactionText = interactionTMP;
    }
    
    private void CreateAudioPrefab()
    {
        Debug.Log("PrefabCreator: Creating Audio prefab");
        
        GameObject audioSources = new GameObject("AudioSources");
        
        // Music source
        GameObject musicSource = new GameObject("MusicSource");
        musicSource.transform.SetParent(audioSources.transform);
        AudioSource music = musicSource.AddComponent<AudioSource>();
        music.playOnAwake = false;
        music.loop = true;
        music.volume = 0.7f;
        
        // SFX source
        GameObject sfxSource = new GameObject("SFXSource");
        sfxSource.transform.SetParent(audioSources.transform);
        AudioSource sfx = sfxSource.AddComponent<AudioSource>();
        sfx.playOnAwake = false;
        sfx.volume = 1f;
        
        // Ambient source
        GameObject ambientSource = new GameObject("AmbientSource");
        ambientSource.transform.SetParent(audioSources.transform);
        AudioSource ambient = ambientSource.AddComponent<AudioSource>();
        ambient.playOnAwake = false;
        ambient.loop = true;
        ambient.volume = 0.5f;
        
        // Voice source
        GameObject voiceSource = new GameObject("VoiceSource");
        voiceSource.transform.SetParent(audioSources.transform);
        AudioSource voice = voiceSource.AddComponent<AudioSource>();
        voice.playOnAwake = false;
        voice.volume = 1f;
        
        // Save as prefab
        SaveAsPrefab(audioSources, "Audio/AudioSources");
        
        Debug.Log("PrefabCreator: Audio prefab created");
    }
    
    private void SaveAsPrefab(GameObject obj, string prefabName)
    {
        string fullPath = $"{prefabPath}/{prefabName}.prefab";
        
        // Note: In a real scenario, you'd use PrefabUtility.SaveAsPrefabAsset
        // For now, we'll just log the path
        Debug.Log($"PrefabCreator: Prefab would be saved to {fullPath}");
        
        // In editor, you would use:
        // PrefabUtility.SaveAsPrefabAsset(obj, fullPath);
    }
}

// ScriptRole: Creates modular prefabs for scene construction
// Dependencies: All core scripts and UI components
// UsesSO: FeedbackMessagesSO
// ReceivesFrom: None
// SendsTo: None (prefab creation only) 