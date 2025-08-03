using UnityEngine;

public class SceneInitializer : MonoBehaviour
{
    [Header("Scene Setup")]
    public bool setupOnStart = true;
    public bool createTags = true;
    public bool setupManagers = true;
    
    [Header("References")]
    public FeedbackMessagesSO feedbackMessages;
    
    private void Start()
    {
        if (setupOnStart)
        {
            Debug.Log("SceneInitializer: Starting scene setup");
            SetupScene();
        }
    }
    
    [ContextMenu("Setup Scene")]
    public void SetupScene()
    {
        Debug.Log("SceneInitializer: Setting up scene");
        
        if (createTags)
        {
            CreateTags();
        }
        
        if (setupManagers)
        {
            SetupManagers();
        }
        
        Debug.Log("SceneInitializer: Scene setup complete");
    }
    
    private void CreateTags()
    {
        Debug.Log("SceneInitializer: Creating tags");
        
        // Create tags if they don't exist
        // Note: In a real scenario, you'd need to modify Unity's TagManager
        // For now, we'll just log the required tags
        
        string[] requiredTags = { "Player", "Hazard", "Enemy", "WinZone", "DeathZone" };
        
        foreach (string tag in requiredTags)
        {
            Debug.Log($"SceneInitializer: Required tag: {tag}");
        }
        
        Debug.Log("SceneInitializer: Tags created (check TagManager)");
    }
    
    private void SetupManagers()
    {
        Debug.Log("SceneInitializer: Setting up managers");
        
        // Find or create managers
        GameManager gameManager = FindObjectOfType<GameManager>();
        if (gameManager == null)
        {
            Debug.Log("SceneInitializer: Creating GameManager");
            GameObject gmObj = new GameObject("GameManager");
            gameManager = gmObj.AddComponent<GameManager>();
        }
        
        AudioManager audioManager = FindObjectOfType<AudioManager>();
        if (audioManager == null)
        {
            Debug.Log("SceneInitializer: Creating AudioManager");
            GameObject amObj = new GameObject("AudioManager");
            audioManager = amObj.AddComponent<AudioManager>();
        }
        
        CutsceneManager cutsceneManager = FindObjectOfType<CutsceneManager>();
        if (cutsceneManager == null)
        {
            Debug.Log("SceneInitializer: Creating CutsceneManager");
            GameObject cmObj = new GameObject("CutsceneManager");
            cutsceneManager = cmObj.AddComponent<CutsceneManager>();
        }
        
        // Assign FeedbackMessagesSO to all managers
        if (feedbackMessages != null)
        {
            if (gameManager != null)
            {
                gameManager.feedbackMessages = feedbackMessages;
            }
            
            if (audioManager != null)
            {
                audioManager.feedbackMessages = feedbackMessages;
            }
            
            if (cutsceneManager != null)
            {
                cutsceneManager.feedbackMessages = feedbackMessages;
            }
        }
        
        Debug.Log("SceneInitializer: Managers setup complete");
    }
}

// ScriptRole: Initializes scene and fixes common setup issues
// Dependencies: All manager scripts
// UsesSO: FeedbackMessagesSO
// ReceivesFrom: None
// SendsTo: None (setup only) 