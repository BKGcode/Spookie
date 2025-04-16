using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [Header("Gamification UI")]
    public TextMeshProUGUI xpText;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI currencyText;

    [Header("Pause/Resume UI")]
    public GameObject pauseMenu;

    [Header("References")]
    public CustomizationSystem customizationSystem;

    private GameStateManager gameStateManager;

    private void Start()
    {
        gameStateManager = FindObjectOfType<GameStateManager>();

        UpdateUI();
    }

    private void OnEnable()
    {
        GamificationSystem.OnXPEarned += UpdateXPDisplay;
        GamificationSystem.OnLevelUp += UpdateLevelDisplay;
        GamificationSystem.OnCurrencyEarned += UpdateCurrencyDisplay;
    }

    private void OnDisable()
    {
        GamificationSystem.OnXPEarned -= UpdateXPDisplay;
        GamificationSystem.OnLevelUp -= UpdateLevelDisplay;
        GamificationSystem.OnCurrencyEarned -= UpdateCurrencyDisplay;
    }

    public void UpdateUI()
    {
        var gamificationSystem = FindObjectOfType<GamificationSystem>();
        UpdateXPDisplay(gamificationSystem.currentXP);
        UpdateLevelDisplay(gamificationSystem.currentLevel);
        UpdateCurrencyDisplay(gamificationSystem.currentCurrency);
    }

    public void UpdateXPDisplay(int xp)
    {
        xpText.text = $"XP: {xp}";
    }

    public void UpdateLevelDisplay(int level)
    {
        levelText.text = $"Level: {level}";
    }

    public void UpdateCurrencyDisplay(int currency)
    {
        currencyText.text = $"Credits: {currency}";
    }

    public void PauseGame()
    {
        gameStateManager.ChangeState(GameState.Paused);
        pauseMenu.SetActive(true);
        Debug.Log("Game Paused.");
    }

    public void ResumeGame()
    {
        gameStateManager.ChangeState(GameState.Working);
        pauseMenu.SetActive(false);
        Debug.Log("Game Resumed.");
    }

    public void ShowSummary()
    {
        gameStateManager.ChangeState(GameState.Summary);
        FindObjectOfType<SummaryState>().ShowSummary();
    }

    public void ReturnToMainMenu()
    {
        gameStateManager.ChangeState(GameState.Configuring);
    }
}
