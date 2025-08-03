using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SceneSetup : MonoBehaviour
{
    [Header("Scene Configuration")]
    public bool autoSetup = true;
    public bool createHouse = true;
    public bool createBed = true;
    public bool createPlayer = true;
    
    [Header("Prefabs")]
    public GameObject playerPrefab;
    public GameObject housePrefab;
    public GameObject bedPrefab;
    public GameObject respawnPointPrefab;
    
    [Header("Materials")]
    public Material fogMaterial;
    public Material groundMaterial;
    
    [Header("References")]
    public FeedbackMessagesSO feedbackMessages;
    
    private void Start()
    {
        if (autoSetup)
        {
            Debug.Log("SceneSetup: Starting automatic scene setup");
            SetupScene();
        }
    }
    
    [ContextMenu("Setup Scene")]
    public void SetupScene()
    {
        Debug.Log("SceneSetup: Setting up scene");
        
        // Create ground
        CreateGround();
        
        // Create house
        if (createHouse)
        {
            CreateHouse();
        }
        
        // Create bed
        if (createBed)
        {
            CreateBed();
        }
        
        // Create player
        if (createPlayer)
        {
            CreatePlayer();
        }
        
        // Setup lighting
        SetupLighting();
        
        // Setup camera
        SetupCamera();
        
        // Setup managers
        SetupManagers();
        
        // Setup UI
        SetupUI();
        
        Debug.Log("SceneSetup: Scene setup complete");
    }
    
    private void CreateGround()
    {
        Debug.Log("SceneSetup: Creating ground");
        
        // Create ground plane
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.transform.position = Vector3.zero;
        ground.transform.localScale = new Vector3(20f, 1f, 20f);
        
        // Add material
        if (groundMaterial != null)
        {
            ground.GetComponent<Renderer>().material = groundMaterial;
        }
        
        // Add collider for respawn detection
        ground.AddComponent<BoxCollider>();
    }
    
    private void CreateHouse()
    {
        Debug.Log("SceneSetup: Creating house");
        
        // Create house structure
        GameObject house = new GameObject("House");
        house.transform.position = new Vector3(0f, 0f, 0f);
        
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
        
        // Create door
        GameObject door = GameObject.CreatePrimitive(PrimitiveType.Cube);
        door.name = "Door";
        door.transform.SetParent(house.transform);
        door.transform.position = new Vector3(0f, 1f, 5f);
        door.transform.localScale = new Vector3(1.5f, 2f, 0.1f);
        door.GetComponent<Renderer>().material.color = Color.brown;
    }
    
    private void CreateWall(GameObject parent, string name, Vector3 position, Vector3 scale)
    {
        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = name;
        wall.transform.SetParent(parent.transform);
        wall.transform.position = position;
        wall.transform.localScale = scale;
        wall.GetComponent<Renderer>().material.color = Color.gray;
    }
    
    private void CreateBed()
    {
        Debug.Log("SceneSetup: Creating bed");
        
        // Create bed frame
        GameObject bed = new GameObject("Bed");
        bed.transform.position = new Vector3(0f, 0.5f, 2f);
        
        // Bed base
        GameObject bedBase = GameObject.CreatePrimitive(PrimitiveType.Cube);
        bedBase.name = "BedBase";
        bedBase.transform.SetParent(bed.transform);
        bedBase.transform.position = new Vector3(0f, 0.25f, 0f);
        bedBase.transform.localScale = new Vector3(2f, 0.5f, 3f);
        bedBase.GetComponent<Renderer>().material.color = Color.brown;
        
        // Bed mattress
        GameObject mattress = GameObject.CreatePrimitive(PrimitiveType.Cube);
        mattress.name = "Mattress";
        mattress.transform.SetParent(bed.transform);
        mattress.transform.position = new Vector3(0f, 0.75f, 0f);
        mattress.transform.localScale = new Vector3(1.8f, 0.2f, 2.8f);
        mattress.GetComponent<Renderer>().material.color = Color.white;
        
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
    }
    
    private void CreatePlayer()
    {
        Debug.Log("SceneSetup: Creating player");
        
        GameObject player;
        
        if (playerPrefab != null)
        {
            player = Instantiate(playerPrefab);
        }
        else
        {
            // Create basic player
            player = new GameObject("Player");
            
            // Add CharacterController
            CharacterController controller = player.AddComponent<CharacterController>();
            controller.height = 2f;
            controller.radius = 0.5f;
            
            // Add PlayerFPSController
            PlayerFPSController fpsController = player.AddComponent<PlayerFPSController>();
            fpsController.feedbackMessages = feedbackMessages;
            
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
            fogEffect.fogMaterial = fogMaterial;
            
            // Assign camera to player controller
            fpsController.cameraTransform = camera.transform;
        }
        
        // Set player position
        player.transform.position = new Vector3(0f, 1f, 0f);
        
        // Add player tag
        player.tag = "Player";
    }
    
    private void SetupLighting()
    {
        Debug.Log("SceneSetup: Setting up lighting");
        
        // Create directional light
        GameObject light = new GameObject("DirectionalLight");
        Light directionalLight = light.AddComponent<Light>();
        directionalLight.type = LightType.Directional;
        directionalLight.intensity = 1f;
        directionalLight.color = Color.white;
        light.transform.rotation = Quaternion.Euler(45f, 45f, 0f);
        
        // Create ambient light
        RenderSettings.ambientLight = new Color(0.3f, 0.3f, 0.3f);
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
    }
    
    private void SetupCamera()
    {
        Debug.Log("SceneSetup: Setting up camera");
        
        // Set main camera if it exists
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            // Add FogEffect to main camera if no player camera
            if (mainCamera.GetComponent<FogEffect>() == null)
            {
                FogEffect fogEffect = mainCamera.gameObject.AddComponent<FogEffect>();
                fogEffect.fogMaterial = fogMaterial;
            }
        }
    }
    
    private void SetupManagers()
    {
        Debug.Log("SceneSetup: Setting up managers");
        
        // Create GameManager
        GameObject gameManager = new GameObject("GameManager");
        GameManager gm = gameManager.AddComponent<GameManager>();
        gm.feedbackMessages = feedbackMessages;
        
        // Create DayNightCycle
        GameObject dayNightCycle = new GameObject("DayNightCycle");
        DayNightCycle dnc = dayNightCycle.AddComponent<DayNightCycle>();
        dnc.feedbackMessages = feedbackMessages;
        dnc.directionalLight = FindObjectOfType<Light>();
        dnc.fogMaterial = fogMaterial;
        
        // Create RespawnSystem
        GameObject respawnSystem = new GameObject("RespawnSystem");
        RespawnSystem rs = respawnSystem.AddComponent<RespawnSystem>();
        rs.feedbackMessages = feedbackMessages;
        rs.fallThreshold = -5f;
        
        // Create AudioManager
        GameObject audioManager = new GameObject("AudioManager");
        AudioManager am = audioManager.AddComponent<AudioManager>();
        am.feedbackMessages = feedbackMessages;
        
        // Create CutsceneManager
        GameObject cutsceneManager = new GameObject("CutsceneManager");
        CutsceneManager cm = cutsceneManager.AddComponent<CutsceneManager>();
        cm.feedbackMessages = feedbackMessages;
    }
    
    private void SetupUI()
    {
        Debug.Log("SceneSetup: Setting up UI");
        
        // Create Canvas
        GameObject canvas = new GameObject("Canvas");
        Canvas canvasComponent = canvas.AddComponent<Canvas>();
        canvasComponent.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.AddComponent<CanvasScaler>();
        canvas.AddComponent<GraphicRaycaster>();
        
        // Create UI Manager
        GameObject uiManager = new GameObject("UIManager");
        UIManager ui = uiManager.AddComponent<UIManager>();
        ui.feedbackMessages = feedbackMessages;
        
        // Create basic UI elements
        CreateBasicUI(canvas);
    }
    
    private void CreateBasicUI(GameObject canvas)
    {
        // Create game panel
        GameObject gamePanel = new GameObject("GamePanel");
        gamePanel.transform.SetParent(canvas.transform, false);
        RectTransform gamePanelRect = gamePanel.AddComponent<RectTransform>();
        gamePanelRect.anchorMin = Vector2.zero;
        gamePanelRect.anchorMax = Vector2.one;
        gamePanelRect.offsetMin = Vector2.zero;
        gamePanelRect.offsetMax = Vector2.zero;
        
        // Create interaction text
        GameObject interactionText = new GameObject("InteractionText");
        interactionText.transform.SetParent(gamePanel.transform, false);
        RectTransform interactionRect = interactionText.AddComponent<RectTransform>();
        interactionRect.anchorMin = new Vector2(0.5f, 0f);
        interactionRect.anchorMax = new Vector2(0.5f, 0f);
        interactionRect.anchoredPosition = new Vector2(0f, 100f);
        interactionRect.sizeDelta = new Vector2(400f, 50f);
        
        TextMeshProUGUI interactionTMP = interactionText.AddComponent<TextMeshProUGUI>();
        interactionTMP.text = "Press E to interact";
        interactionTMP.fontSize = 24f;
        interactionTMP.color = Color.white;
        interactionTMP.alignment = TextAlignmentOptions.Center;
        
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
    }
}

// ScriptRole: Automatically sets up the complete scene with all components
// Dependencies: All core scripts and UI components
// UsesSO: FeedbackMessagesSO
// ReceivesFrom: None
// SendsTo: None (setup only) 