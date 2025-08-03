using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneManager : MonoBehaviour
{
    [Header("Scene Settings")]
    public string[] levelScenes;
    public string mainMenuScene = "MainMenu";
    public float transitionDelay = 2f;
    
    [Header("Day/Night Cycle")]
    public float dayDuration = 300f; // 5 minutes per day
    public Light directionalLight;
    public Material skyboxMaterial;
    
    [Header("References")]
    public FeedbackMessagesSO feedbackMessages;
    
    private static SceneManager instance;
    public static SceneManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<SceneManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("SceneManager");
                    instance = go.AddComponent<SceneManager>();
                }
            }
            return instance;
        }
    }
    
    private int currentLevelIndex = 0;
    private float currentDayTime = 0f;
    private bool isTransitioning = false;

    private void Awake()
    {
        Debug.Log("SceneManager: Initializing");
        
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Debug.LogWarning("SceneManager: Duplicate instance found, destroying");
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        Debug.Log("SceneManager: Starting scene manager");
        StartDayNightCycle();
    }

    private void Update()
    {
        if (!GameManager.Instance.IsGamePlaying() || isTransitioning)
            return;

        UpdateDayNightCycle();
    }

    private void UpdateDayNightCycle()
    {
        currentDayTime += Time.deltaTime;
        
        // Update lighting
        if (directionalLight != null)
        {
            float dayProgress = currentDayTime / dayDuration;
            float sunAngle = Mathf.Lerp(0f, 360f, dayProgress);
            directionalLight.transform.rotation = Quaternion.Euler(sunAngle, 170f, 0f);
            
            // Adjust light intensity based on time of day
            float intensity = Mathf.Sin(dayProgress * Mathf.PI) * 0.5f + 0.5f;
            directionalLight.intensity = intensity;
        }
        
        // Check if day cycle is complete
        if (currentDayTime >= dayDuration)
        {
            Debug.Log("SceneManager: Day cycle complete, transitioning to next level");
            StartCoroutine(TransitionToNextLevel());
        }
    }

    private void StartDayNightCycle()
    {
        currentDayTime = 0f;
        Debug.Log("SceneManager: Starting new day cycle");
    }

    public void LoadLevel(int levelIndex)
    {
        if (levelIndex >= 0 && levelIndex < levelScenes.Length)
        {
            Debug.Log($"SceneManager: Loading level {levelIndex}");
            currentLevelIndex = levelIndex;
            StartCoroutine(LoadSceneAsync(levelScenes[levelIndex]));
        }
        else
        {
            Debug.LogError($"SceneManager: Invalid level index {levelIndex}");
        }
    }

    public void LoadNextLevel()
    {
        int nextLevel = currentLevelIndex + 1;
        if (nextLevel < levelScenes.Length)
        {
            LoadLevel(nextLevel);
        }
        else
        {
            Debug.Log("SceneManager: All levels completed, returning to main menu");
            LoadMainMenu();
        }
    }

    public void LoadMainMenu()
    {
        Debug.Log("SceneManager: Loading main menu");
        StartCoroutine(LoadSceneAsync(mainMenuScene));
    }

    public void RestartCurrentLevel()
    {
        Debug.Log("SceneManager: Restarting current level");
        LoadLevel(currentLevelIndex);
    }

    private IEnumerator TransitionToNextLevel()
    {
        isTransitioning = true;
        
        // Show transition message
        if (feedbackMessages != null)
        {
            string message = feedbackMessages.GetMessage("day_complete_message");
            Debug.Log($"SceneManager: {message}");
        }
        
        // Wait for transition delay
        yield return new WaitForSeconds(transitionDelay);
        
        // Load next level
        LoadNextLevel();
        
        isTransitioning = false;
    }

    private IEnumerator LoadSceneAsync(string sceneName)
    {
        Debug.Log($"SceneManager: Loading scene {sceneName}");
        
        // Show loading screen if available
        if (feedbackMessages != null)
        {
            string message = feedbackMessages.GetMessage("loading_message");
            Debug.Log($"SceneManager: {message}");
        }
        
        // Load scene asynchronously
        AsyncOperation asyncLoad = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName);
        
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
        
        Debug.Log($"SceneManager: Scene {sceneName} loaded successfully");
        
        // Reset day cycle for new level
        StartDayNightCycle();
    }

    public float GetDayProgress()
    {
        return currentDayTime / dayDuration;
    }

    public bool IsDayTime()
    {
        float progress = GetDayProgress();
        return progress > 0.25f && progress < 0.75f;
    }

    public bool IsNightTime()
    {
        return !IsDayTime();
    }

    public int GetCurrentLevel()
    {
        return currentLevelIndex;
    }

    public int GetTotalLevels()
    {
        return levelScenes.Length;
    }

    public void SetDayTime(float time)
    {
        currentDayTime = Mathf.Clamp(time, 0f, dayDuration);
    }

    public void SkipToNight()
    {
        currentDayTime = dayDuration * 0.75f;
        Debug.Log("SceneManager: Skipped to night time");
    }

    public void SkipToDay()
    {
        currentDayTime = dayDuration * 0.25f;
        Debug.Log("SceneManager: Skipped to day time");
    }
}

// ScriptRole: Manages scene transitions and day/night cycle
// RelatedScripts: GameManager, PlayerFPSController
// UsesSO: FeedbackMessagesSO
// ReceivesFrom: GameManager, UI
// SendsTo: GameManager via scene changes 