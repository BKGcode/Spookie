using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    [Header("Game State")]
    public GameState currentGameState = GameState.MainMenu;
    
    [Header("Events")]
    public UnityEvent OnGameStart;
    public UnityEvent OnGamePause;
    public UnityEvent OnGameResume;
    public UnityEvent OnGameOver;
    public UnityEvent OnGameWin;
    
    [Header("References")]
    public FeedbackMessagesSO feedbackMessages;
    
    private static GameManager instance;
    public static GameManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<GameManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("GameManager");
                    instance = go.AddComponent<GameManager>();
                }
            }
            return instance;
        }
    }

    private void Awake()
    {
        Debug.Log("GameManager: Initializing");
        
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Debug.LogWarning("GameManager: Duplicate instance found, destroying");
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        Debug.Log("GameManager: Starting with state " + currentGameState);
        InitializeGame();
    }

    private void InitializeGame()
    {
        if (feedbackMessages == null)
        {
            Debug.LogError("GameManager: FeedbackMessagesSO not assigned!");
        }
        
        // Set initial state
        SetGameState(GameState.MainMenu);
    }

    public void StartGame()
    {
        Debug.Log("GameManager: Starting game");
        SetGameState(GameState.Playing);
        OnGameStart?.Invoke();
    }

    public void PauseGame()
    {
        Debug.Log("GameManager: Pausing game");
        SetGameState(GameState.Paused);
        Time.timeScale = 0f;
        OnGamePause?.Invoke();
    }

    public void ResumeGame()
    {
        Debug.Log("GameManager: Resuming game");
        SetGameState(GameState.Playing);
        Time.timeScale = 1f;
        OnGameResume?.Invoke();
    }

    public void GameOver()
    {
        Debug.Log("GameManager: Game Over");
        SetGameState(GameState.GameOver);
        OnGameOver?.Invoke();
    }

    public void GameWin()
    {
        Debug.Log("GameManager: Game Won");
        SetGameState(GameState.GameWin);
        OnGameWin?.Invoke();
    }

    private void SetGameState(GameState newState)
    {
        currentGameState = newState;
        Debug.Log($"GameManager: State changed to {newState}");
    }

    public bool IsGamePlaying()
    {
        return currentGameState == GameState.Playing;
    }

    public bool IsGamePaused()
    {
        return currentGameState == GameState.Paused;
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus && IsGamePlaying())
        {
            PauseGame();
        }
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus && IsGamePlaying())
        {
            PauseGame();
        }
    }
}

public enum GameState
{
    MainMenu,
    Playing,
    Paused,
    GameOver,
    GameWin
}

// ScriptRole: Main game controller managing game states and coordination
// RelatedScripts: All game systems
// UsesSO: FeedbackMessagesSO
// ReceivesFrom: UI, Player, Enemies
// SendsTo: All game systems via events 