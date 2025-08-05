using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    [Header("Game State")]
    [SerializeField] private bool isGamePaused = false;
    [SerializeField] private bool isGameOver = false;
    
    [Header("References")]
    [SerializeField] private FeedbackMessagesSO feedbackMessages;
    [SerializeField] private InputManager inputManager;
    
    [Header("Events")]
    [SerializeField] private UnityEvent onGameStart;
    [SerializeField] private UnityEvent onGamePause;
    [SerializeField] private UnityEvent onGameResume;
    [SerializeField] private UnityEvent onGameOver;
    
    public static GameManager Instance { get; private set; }
    
    public bool IsGamePaused => isGamePaused;
    public bool IsGameOver => isGameOver;
    
    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        Debug.Log("GameManager initialized");
    }
    
    private void Start()
    {
        StartGame();
    }
    
    public void StartGame()
    {
        isGamePaused = false;
        isGameOver = false;
        
        if (inputManager != null)
        {
            inputManager.EnableInput();
        }
        
        Time.timeScale = 1f;
        onGameStart?.Invoke();
        
        Debug.Log("Game started");
    }
    
    public void PauseGame()
    {
        if (isGameOver) return;
        
        isGamePaused = true;
        Time.timeScale = 0f;
        
        if (inputManager != null)
        {
            inputManager.DisableInput();
        }
        
        onGamePause?.Invoke();
        
        Debug.Log("Game paused");
    }
    
    public void ResumeGame()
    {
        if (isGameOver) return;
        
        isGamePaused = false;
        Time.timeScale = 1f;
        
        if (inputManager != null)
        {
            inputManager.EnableInput();
        }
        
        onGameResume?.Invoke();
        
        Debug.Log("Game resumed");
    }
    
    public void GameOver()
    {
        isGameOver = true;
        isGamePaused = false;
        
        if (inputManager != null)
        {
            inputManager.DisableInput();
        }
        
        onGameOver?.Invoke();
        
        Debug.Log("Game over");
    }
    
    public void RestartGame()
    {
        // Reset game state
        isGamePaused = false;
        isGameOver = false;
        
        if (inputManager != null)
        {
            inputManager.EnableInput();
        }
        
        Time.timeScale = 1f;
        
        Debug.Log("Game restarted");
    }
    
    public void QuitGame()
    {
        Debug.Log("Quitting game");
        Application.Quit();
    }
    
    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus && !isGamePaused && !isGameOver)
        {
            PauseGame();
        }
    }
}

// ScriptRole: Manages global game state and events
// Dependencies: InputManager
// UsesSO: FeedbackMessagesSO
// NeedsSetup: feedbackMessages, inputManager, UnityEvents 