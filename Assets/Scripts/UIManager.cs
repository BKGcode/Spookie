using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [Header("Gamification UI")]
    public TextMeshProUGUI xpText;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI currencyText;

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

    private void Start()
    {
        var gamificationSystem = FindObjectOfType<GamificationSystem>();
        UpdateXPDisplay(gamificationSystem.currentXP);
        UpdateLevelDisplay(gamificationSystem.currentLevel);
        UpdateCurrencyDisplay(gamificationSystem.currentCurrency);
    }

    private void UpdateXPDisplay(int xp)
    {
        xpText.text = $"XP: {xp}";
    }

    private void UpdateLevelDisplay(int level)
    {
        levelText.text = $"Level: {level}";
    }

    private void UpdateCurrencyDisplay(int currency)
    {
        currencyText.text = $"Credits: {currency}";
    }
}
