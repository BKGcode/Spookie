using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class SceneSetupFixer : MonoBehaviour
{
    [Header("Setup Configuration")]
    public bool fixOnStart = true;
    public bool createFogMaterial = true;
    public bool setupButtonListeners = true;
    public bool fixManagerReferences = true;
    
    [Header("References")]
    public FeedbackMessagesSO feedbackMessages;
    
    private void Start()
    {
        if (fixOnStart)
        {
            Debug.Log("SceneSetupFixer: Starting scene setup fix");
            FixSceneSetup();
        }
    }
    
    [ContextMenu("Fix Scene Setup")]
    public void FixSceneSetup()
    {
        Debug.Log("SceneSetupFixer: Fixing scene setup");
        
        if (createFogMaterial)
        {
            CreateFogMaterial();
        }
        
        if (setupButtonListeners)
        {
            SetupButtonListeners();
        }
        
        if (fixManagerReferences)
        {
            FixManagerReferences();
        }
        
        Debug.Log("SceneSetupFixer: Scene setup fix complete");
    }
    
    private void CreateFogMaterial()
    {
        Debug.Log("SceneSetupFixer: Creating fog material");
        
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
                Debug.Log("SceneSetupFixer: Assigned fog material to DayNightCycle");
            }
            
            // Assign to FogEffect
            FogEffect fogEffect = FindObjectOfType<FogEffect>();
            if (fogEffect != null)
            {
                fogEffect.fogMaterial = fogMaterial;
                Debug.Log("SceneSetupFixer: Assigned fog material to FogEffect");
            }
        }
        else
        {
            Debug.LogWarning("SceneSetupFixer: Custom/FogShader not found, using standard shader");
            // Create fallback material
            Material fallbackMaterial = new Material(Shader.Find("Standard"));
            fallbackMaterial.color = new Color(0.1f, 0.1f, 0.2f, 0.8f);
            fallbackMaterial.name = "FogMaterial_Fallback";
            
            // Assign to DayNightCycle
            DayNightCycle dnc = FindObjectOfType<DayNightCycle>();
            if (dnc != null)
            {
                dnc.fogMaterial = fallbackMaterial;
                Debug.Log("SceneSetupFixer: Assigned fallback material to DayNightCycle");
            }
        }
    }
    
    private void SetupButtonListeners()
    {
        Debug.Log("SceneSetupFixer: Setting up button listeners");
        
        // Find UIManager
        UIManager uiManager = FindObjectOfType<UIManager>();
        if (uiManager == null)
        {
            Debug.LogWarning("SceneSetupFixer: UIManager not found");
            return;
        }
        
        // The UIManager already has its own SetupButtonListeners method
        // We just need to ensure EventSystem exists
        if (FindObjectOfType<EventSystem>() == null)
        {
            Debug.Log("SceneSetupFixer: Creating EventSystem");
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();
            Debug.Log("SceneSetupFixer: EventSystem created");
        }
        else
        {
            Debug.Log("SceneSetupFixer: EventSystem already exists");
        }
        
        // Verify that buttons have listeners
        if (uiManager.startButton != null && uiManager.startButton.onClick.GetPersistentEventCount() == 0)
        {
            Debug.LogWarning("SceneSetupFixer: Start button has no listeners - UIManager may not have initialized properly");
        }
        
        Debug.Log("SceneSetupFixer: Button listeners setup complete (UIManager handles the actual listeners)");
    }
    
    private void FixManagerReferences()
    {
        Debug.Log("SceneSetupFixer: Fixing manager references");
        
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
                Debug.Log("SceneSetupFixer: Assigned directional light to DayNightCycle");
            }
            
            // Configure for quick testing
            dayNightCycle.dayDuration = 60f; // 1 minute for testing
            dayNightCycle.sleepPressureStartTime = 10f; // Pressure in 10 seconds
            dayNightCycle.maxSlowdownFactor = 0.3f;
            dayNightCycle.fogIntensity = 1f;
            Debug.Log("SceneSetupFixer: Configured DayNightCycle for quick testing");
        }
        
        // Assign GameUI references
        if (gameUI != null)
        {
            if (dayNightCycle != null)
            {
                gameUI.dayNightCycle = dayNightCycle;
                Debug.Log("SceneSetupFixer: Assigned DayNightCycle to GameUI");
            }
            
            if (playerController != null)
            {
                gameUI.playerController = playerController;
                Debug.Log("SceneSetupFixer: Assigned PlayerController to GameUI");
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
                Debug.Log("SceneSetupFixer: Assigned audio sources to AudioManager");
            }
        }
        
        // Assign RespawnSystem references
        RespawnSystem respawnSystem = FindObjectOfType<RespawnSystem>();
        if (respawnSystem != null)
        {
            if (playerController != null)
            {
                respawnSystem.SetPlayerController(playerController);
                Debug.Log("SceneSetupFixer: Assigned PlayerController to RespawnSystem");
            }
            
            // Configure respawn
            respawnSystem.fallThreshold = -5f;
            respawnSystem.respawnDelay = 1f;
            Debug.Log("SceneSetupFixer: Configured RespawnSystem");
        }
        
        // Assign FeedbackMessagesSO to all managers
        if (feedbackMessages != null)
        {
            GameManager gameManager = FindObjectOfType<GameManager>();
            if (gameManager != null)
            {
                gameManager.feedbackMessages = feedbackMessages;
                Debug.Log("SceneSetupFixer: Assigned FeedbackMessagesSO to GameManager");
            }
            
            if (audioManager != null)
            {
                audioManager.feedbackMessages = feedbackMessages;
                Debug.Log("SceneSetupFixer: Assigned FeedbackMessagesSO to AudioManager");
            }
            
            CutsceneManager cutsceneManager = FindObjectOfType<CutsceneManager>();
            if (cutsceneManager != null)
            {
                cutsceneManager.feedbackMessages = feedbackMessages;
                Debug.Log("SceneSetupFixer: Assigned FeedbackMessagesSO to CutsceneManager");
            }
            
            UIManager uiManager = FindObjectOfType<UIManager>();
            if (uiManager != null)
            {
                uiManager.feedbackMessages = feedbackMessages;
                Debug.Log("SceneSetupFixer: Assigned FeedbackMessagesSO to UIManager");
            }
            
            if (gameUI != null)
            {
                gameUI.feedbackMessages = feedbackMessages;
                Debug.Log("SceneSetupFixer: Assigned FeedbackMessagesSO to GameUI");
            }
        }
    }
    
    [ContextMenu("Verify Setup")]
    public void VerifySetup()
    {
        Debug.Log("SceneSetupFixer: Verifying setup");
        
        // Check managers
        GameManager gm = FindObjectOfType<GameManager>();
        AudioManager am = FindObjectOfType<AudioManager>();
        CutsceneManager cm = FindObjectOfType<CutsceneManager>();
        UIManager um = FindObjectOfType<UIManager>();
        
        Debug.Log($"GameManager: {(gm != null ? "OK" : "MISSING")}");
        Debug.Log($"AudioManager: {(am != null ? "OK" : "MISSING")}");
        Debug.Log($"CutsceneManager: {(cm != null ? "OK" : "MISSING")}");
        Debug.Log($"UIManager: {(um != null ? "OK" : "MISSING")}");
        
        // Check FeedbackMessagesSO
        if (gm != null) Debug.Log($"GameManager FeedbackMessagesSO: {(gm.feedbackMessages != null ? "OK" : "MISSING")}");
        if (am != null) Debug.Log($"AudioManager FeedbackMessagesSO: {(am.feedbackMessages != null ? "OK" : "MISSING")}");
        if (cm != null) Debug.Log($"CutsceneManager FeedbackMessagesSO: {(cm.feedbackMessages != null ? "OK" : "MISSING")}");
        if (um != null) Debug.Log($"UIManager FeedbackMessagesSO: {(um.feedbackMessages != null ? "OK" : "MISSING")}");
        
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
        
        // Check EventSystem
        EventSystem eventSystem = FindObjectOfType<EventSystem>();
        Debug.Log($"EventSystem: {(eventSystem != null ? "OK" : "MISSING")}");
        
        // Check Button Listeners
        if (um != null)
        {
            Debug.Log($"StartButton Listeners: {(um.startButton != null && um.startButton.onClick.GetPersistentEventCount() > 0 ? "OK" : "MISSING")}");
            Debug.Log($"PauseButton Listeners: {(um.pauseButton != null && um.pauseButton.onClick.GetPersistentEventCount() > 0 ? "OK" : "MISSING")}");
            Debug.Log($"ResumeButton Listeners: {(um.resumeButton != null && um.resumeButton.onClick.GetPersistentEventCount() > 0 ? "OK" : "MISSING")}");
            Debug.Log($"RestartButton Listeners: {(um.restartButton != null && um.restartButton.onClick.GetPersistentEventCount() > 0 ? "OK" : "MISSING")}");
            Debug.Log($"QuitButton Listeners: {(um.quitButton != null && um.quitButton.onClick.GetPersistentEventCount() > 0 ? "OK" : "MISSING")}");
        }
        
        Debug.Log("SceneSetupFixer: Verification complete");
    }
}

// ScriptRole: Automatically fixes scene setup issues and configures button listeners
// Dependencies: All manager scripts and UI components
// UsesSO: FeedbackMessagesSO
// ReceivesFrom: None
// SendsTo: None (setup only) 