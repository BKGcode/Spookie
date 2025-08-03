using UnityEngine;

public class QuickSetup : MonoBehaviour
{
    [Header("Setup Configuration")]
    public bool setupOnStart = true;
    public bool createFogMaterial = true;
    public bool fixManagers = true;
    public bool assignReferences = true;
    
    [Header("References")]
    public FeedbackMessagesSO feedbackMessages;
    
    private void Start()
    {
        if (setupOnStart)
        {
            Debug.Log("QuickSetup: Starting automatic setup");
            SetupScene();
        }
    }
    
    [ContextMenu("Setup Scene")]
    public void SetupScene()
    {
        Debug.Log("QuickSetup: Setting up scene");
        
        if (fixManagers)
        {
            FixManagers();
        }
        
        if (createFogMaterial)
        {
            CreateFogMaterial();
        }
        
        if (assignReferences)
        {
            AssignReferences();
        }
        
        Debug.Log("QuickSetup: Scene setup complete");
    }
    
    private void FixManagers()
    {
        Debug.Log("QuickSetup: Fixing managers");
        
        // Ensure managers are root GameObjects
        GameManager gameManager = FindObjectOfType<GameManager>();
        if (gameManager != null && gameManager.transform.parent != null)
        {
            Debug.Log("QuickSetup: Moving GameManager to root");
            gameManager.transform.SetParent(null);
        }
        
        AudioManager audioManager = FindObjectOfType<AudioManager>();
        if (audioManager != null && audioManager.transform.parent != null)
        {
            Debug.Log("QuickSetup: Moving AudioManager to root");
            audioManager.transform.SetParent(null);
        }
        
        CutsceneManager cutsceneManager = FindObjectOfType<CutsceneManager>();
        if (cutsceneManager != null && cutsceneManager.transform.parent != null)
        {
            Debug.Log("QuickSetup: Moving CutsceneManager to root");
            cutsceneManager.transform.SetParent(null);
        }
        
        // Create managers if they don't exist
        if (gameManager == null)
        {
            Debug.Log("QuickSetup: Creating GameManager");
            GameObject gmObj = new GameObject("GameManager");
            gameManager = gmObj.AddComponent<GameManager>();
        }
        
        if (audioManager == null)
        {
            Debug.Log("QuickSetup: Creating AudioManager");
            GameObject amObj = new GameObject("AudioManager");
            audioManager = amObj.AddComponent<AudioManager>();
        }
        
        if (cutsceneManager == null)
        {
            Debug.Log("QuickSetup: Creating CutsceneManager");
            GameObject cmObj = new GameObject("CutsceneManager");
            cutsceneManager = cmObj.AddComponent<CutsceneManager>();
        }
        
        // Assign FeedbackMessagesSO
        if (feedbackMessages != null)
        {
            if (gameManager != null)
            {
                gameManager.feedbackMessages = feedbackMessages;
                Debug.Log("QuickSetup: Assigned FeedbackMessagesSO to GameManager");
            }
            
            if (audioManager != null)
            {
                audioManager.feedbackMessages = feedbackMessages;
                Debug.Log("QuickSetup: Assigned FeedbackMessagesSO to AudioManager");
            }
            
            if (cutsceneManager != null)
            {
                cutsceneManager.feedbackMessages = feedbackMessages;
                Debug.Log("QuickSetup: Assigned FeedbackMessagesSO to CutsceneManager");
            }
        }
    }
    
    private void CreateFogMaterial()
    {
        Debug.Log("QuickSetup: Creating fog material");
        
        // Create fog material
        Material fogMaterial = new Material(Shader.Find("Custom/FogShader"));
        if (fogMaterial != null)
        {
            fogMaterial.SetColor("_FogColor", new Color(0.1f, 0.1f, 0.2f, 0.8f));
            fogMaterial.SetFloat("_FogIntensity", 0f);
            fogMaterial.SetFloat("_FogDistance", 10f);
            fogMaterial.name = "FogMaterial";
            
            // Assign to DayNightCycle
            DayNightCycle dnc = FindObjectOfType<DayNightCycle>();
            if (dnc != null)
            {
                dnc.fogMaterial = fogMaterial;
                Debug.Log("QuickSetup: Assigned fog material to DayNightCycle");
            }
            
            // Assign to FogEffect
            FogEffect fogEffect = FindObjectOfType<FogEffect>();
            if (fogEffect != null)
            {
                fogEffect.fogMaterial = fogMaterial;
                Debug.Log("QuickSetup: Assigned fog material to FogEffect");
            }
        }
        else
        {
            Debug.LogWarning("QuickSetup: Custom/FogShader not found, using standard shader");
            // Create fallback material
            Material fallbackMaterial = new Material(Shader.Find("Standard"));
            fallbackMaterial.color = new Color(0.1f, 0.1f, 0.2f, 0.8f);
            fallbackMaterial.name = "FogMaterial_Fallback";
            
            // Assign to DayNightCycle
            DayNightCycle dnc = FindObjectOfType<DayNightCycle>();
            if (dnc != null)
            {
                dnc.fogMaterial = fallbackMaterial;
                Debug.Log("QuickSetup: Assigned fallback material to DayNightCycle");
            }
        }
    }
    
    private void AssignReferences()
    {
        Debug.Log("QuickSetup: Assigning references");
        
        // Find components
        DayNightCycle dayNightCycle = FindObjectOfType<DayNightCycle>();
        PlayerFPSController playerController = FindObjectOfType<PlayerFPSController>();
        GameUI gameUI = FindObjectOfType<GameUI>();
        Light directionalLight = FindObjectOfType<Light>();
        
        // Assign DayNightCycle references
        if (dayNightCycle != null)
        {
            if (directionalLight != null)
            {
                dayNightCycle.directionalLight = directionalLight;
                Debug.Log("QuickSetup: Assigned directional light to DayNightCycle");
            }
            
            // Configure for quick testing
            dayNightCycle.dayDuration = 60f; // 1 minute for testing
            dayNightCycle.sleepPressureStartTime = 10f; // Pressure in 10 seconds
            dayNightCycle.maxSlowdownFactor = 0.3f;
            dayNightCycle.fogIntensity = 1f;
            Debug.Log("QuickSetup: Configured DayNightCycle for quick testing");
        }
        
        // Assign GameUI references
        if (gameUI != null)
        {
            if (dayNightCycle != null)
            {
                gameUI.dayNightCycle = dayNightCycle;
                Debug.Log("QuickSetup: Assigned DayNightCycle to GameUI");
            }
            
            if (playerController != null)
            {
                gameUI.playerController = playerController;
                Debug.Log("QuickSetup: Assigned PlayerController to GameUI");
            }
        }
        
        // Assign AudioManager references
        AudioManager audioManager = FindObjectOfType<AudioManager>();
        if (audioManager != null)
        {
            // Find audio sources
            AudioSource[] audioSources = FindObjectsOfType<AudioSource>();
            if (audioSources.Length >= 4)
            {
                audioManager.musicSource = audioSources[0];
                audioManager.sfxSource = audioSources[1];
                audioManager.ambientSource = audioSources[2];
                audioManager.voiceSource = audioSources[3];
                Debug.Log("QuickSetup: Assigned audio sources to AudioManager");
            }
        }
        
        // Assign RespawnSystem references
        RespawnSystem respawnSystem = FindObjectOfType<RespawnSystem>();
        if (respawnSystem != null)
        {
            if (playerController != null)
            {
                respawnSystem.SetPlayerController(playerController);
                Debug.Log("QuickSetup: Assigned PlayerController to RespawnSystem");
            }
            
            // Configure respawn
            respawnSystem.fallThreshold = -5f;
            respawnSystem.respawnDelay = 1f;
            Debug.Log("QuickSetup: Configured RespawnSystem");
        }
    }
    
    [ContextMenu("Verify Setup")]
    public void VerifySetup()
    {
        Debug.Log("QuickSetup: Verifying setup");
        
        // Check managers
        GameManager gm = FindObjectOfType<GameManager>();
        AudioManager am = FindObjectOfType<AudioManager>();
        CutsceneManager cm = FindObjectOfType<CutsceneManager>();
        
        Debug.Log($"GameManager: {(gm != null ? "OK" : "MISSING")}");
        Debug.Log($"AudioManager: {(am != null ? "OK" : "MISSING")}");
        Debug.Log($"CutsceneManager: {(cm != null ? "OK" : "MISSING")}");
        
        // Check FeedbackMessagesSO
        if (gm != null) Debug.Log($"GameManager FeedbackMessagesSO: {(gm.feedbackMessages != null ? "OK" : "MISSING")}");
        if (am != null) Debug.Log($"AudioManager FeedbackMessagesSO: {(am.feedbackMessages != null ? "OK" : "MISSING")}");
        if (cm != null) Debug.Log($"CutsceneManager FeedbackMessagesSO: {(cm.feedbackMessages != null ? "OK" : "MISSING")}");
        
        // Check DayNightCycle
        DayNightCycle dnc = FindObjectOfType<DayNightCycle>();
        if (dnc != null)
        {
            Debug.Log($"DayNightCycle FogMaterial: {(dnc.fogMaterial != null ? "OK" : "MISSING")}");
            Debug.Log($"DayNightCycle DirectionalLight: {(dnc.directionalLight != null ? "OK" : "MISSING")}");
        }
        
        // Check GameUI
        GameUI gameUI = FindObjectOfType<GameUI>();
        if (gameUI != null)
        {
            Debug.Log($"GameUI DayNightCycle: {(gameUI.dayNightCycle != null ? "OK" : "MISSING")}");
            Debug.Log($"GameUI PlayerController: {(gameUI.playerController != null ? "OK" : "MISSING")}");
        }
        
        Debug.Log("QuickSetup: Verification complete");
    }
}

// ScriptRole: Automatically fixes common setup issues and configures the scene
// Dependencies: All manager scripts and components
// UsesSO: FeedbackMessagesSO
// ReceivesFrom: None
// SendsTo: None (setup only) 