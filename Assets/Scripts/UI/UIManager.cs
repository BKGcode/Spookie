using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject mainMenuPanel;
    public GameObject gamePanel;
    public GameObject pausePanel;
    public GameObject gameOverPanel;
    public GameObject winPanel;
    
    [Header("Text Elements")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI gameOverText;
    public TextMeshProUGUI winText;
    public TextMeshProUGUI pauseText;
    
    [Header("Buttons")]
    public Button startButton;
    public Button pauseButton;
    public Button resumeButton;
    public Button restartButton;
    public Button quitButton;
    
    [Header("References")]
    public FeedbackMessagesSO feedbackMessages;
    
    private int currentScore = 0;

    private void Awake()
    {
        Debug.Log("UIManager: Initializing");
        SetupButtonListeners();
    }

    private void Start()
    {
        Debug.Log("UIManager: Starting UI manager");
        ShowMainMenu();
    }

    private void OnEnable()
    {
        // Subscribe to game events
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStart.AddListener(ShowGameUI);
            GameManager.Instance.OnGamePause.AddListener(ShowPauseUI);
            GameManager.Instance.OnGameResume.AddListener(ShowGameUI);
            GameManager.Instance.OnGameOver.AddListener(ShowGameOverUI);
            GameManager.Instance.OnGameWin.AddListener(ShowWinUI);
        }
    }

    private void OnDisable()
    {
        // Unsubscribe from game events
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStart.RemoveListener(ShowGameUI);
            GameManager.Instance.OnGamePause.RemoveListener(ShowPauseUI);
            GameManager.Instance.OnGameResume.RemoveListener(ShowGameUI);
            GameManager.Instance.OnGameOver.RemoveListener(ShowGameOverUI);
            GameManager.Instance.OnGameWin.RemoveListener(ShowWinUI);
        }
    }

    private void SetupButtonListeners()
    {
        if (startButton != null)
            startButton.onClick.AddListener(OnStartButtonClicked);
        
        if (pauseButton != null)
            pauseButton.onClick.AddListener(OnPauseButtonClicked);
        
        if (resumeButton != null)
            resumeButton.onClick.AddListener(OnResumeButtonClicked);
        
        if (restartButton != null)
            restartButton.onClick.AddListener(OnRestartButtonClicked);
        
        if (quitButton != null)
            quitButton.onClick.AddListener(OnQuitButtonClicked);
    }

    private void ShowMainMenu()
    {
        Debug.Log("UIManager: Showing main menu");
        SetPanelActive(mainMenuPanel, true);
        SetPanelActive(gamePanel, false);
        SetPanelActive(pausePanel, false);
        SetPanelActive(gameOverPanel, false);
        SetPanelActive(winPanel, false);
    }

    private void ShowGameUI()
    {
        Debug.Log("UIManager: Showing game UI");
        SetPanelActive(mainMenuPanel, false);
        SetPanelActive(gamePanel, true);
        SetPanelActive(pausePanel, false);
        SetPanelActive(gameOverPanel, false);
        SetPanelActive(winPanel, false);
        
        UpdateScoreText();
    }

    private void ShowPauseUI()
    {
        Debug.Log("UIManager: Showing pause UI");
        SetPanelActive(pausePanel, true);
        
        if (pauseText != null && feedbackMessages != null)
        {
            pauseText.text = feedbackMessages.GetMessage("pause_message");
        }
    }

    private void ShowGameOverUI()
    {
        Debug.Log("UIManager: Showing game over UI");
        SetPanelActive(gamePanel, false);
        SetPanelActive(pausePanel, false);
        SetPanelActive(gameOverPanel, true);
        
        if (gameOverText != null && feedbackMessages != null)
        {
            gameOverText.text = feedbackMessages.GetMessage("game_over_message");
        }
    }

    private void ShowWinUI()
    {
        Debug.Log("UIManager: Showing win UI");
        SetPanelActive(gamePanel, false);
        SetPanelActive(pausePanel, false);
        SetPanelActive(winPanel, true);
        
        if (winText != null && feedbackMessages != null)
        {
            winText.text = feedbackMessages.GetMessage("win_message");
        }
    }

    private void SetPanelActive(GameObject panel, bool active)
    {
        if (panel != null)
        {
            panel.SetActive(active);
        }
    }

    public void AddScore(int points)
    {
        currentScore += points;
        Debug.Log($"UIManager: Score updated to {currentScore}");
        UpdateScoreText();
    }

    private void UpdateScoreText()
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {currentScore}";
        }
    }

    // Button event handlers
    private void OnStartButtonClicked()
    {
        Debug.Log("UIManager: Start button clicked");
        GameManager.Instance.StartGame();
    }

    private void OnPauseButtonClicked()
    {
        Debug.Log("UIManager: Pause button clicked");
        GameManager.Instance.PauseGame();
    }

    private void OnResumeButtonClicked()
    {
        Debug.Log("UIManager: Resume button clicked");
        GameManager.Instance.ResumeGame();
    }

    private void OnRestartButtonClicked()
    {
        Debug.Log("UIManager: Restart button clicked");
        currentScore = 0;
        GameManager.Instance.StartGame();
    }

    private void OnQuitButtonClicked()
    {
        Debug.Log("UIManager: Quit button clicked");
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}

// ScriptRole: Manages all UI elements and their state changes
// Dependencies: TextMeshProUGUI, Button components
// UsesSO: FeedbackMessagesSO
// ReceivesFrom: GameManager events
// SendsTo: GameManager via button clicks 