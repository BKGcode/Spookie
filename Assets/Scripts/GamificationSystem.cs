using UnityEngine;
using System;

public class GamificationSystem : MonoBehaviour
{
    public static event Action<int> OnXPEarned;           // XP ganado
    public static event Action<int> OnCurrencyEarned;     // Créditos ganados
    public static event Action<int> OnLevelUp;           // Nivel alcanzado

    [Header("Player Stats")]
    public int currentXP = 0;
    public int currentLevel = 1;
    public int currentCurrency = 0;

    [Header("XP per Level")]
    public int xpPerLevel = 100;

    public void EarnXP(int amount)
    {
        currentXP += amount;
        OnXPEarned?.Invoke(amount);
        Debug.Log($"Earned {amount} XP. Total XP: {currentXP}");

        // Verifica si se sube de nivel
        if (currentXP >= currentLevel * xpPerLevel)
        {
            currentXP -= currentLevel * xpPerLevel;
            currentLevel++;
            OnLevelUp?.Invoke(currentLevel);
            Debug.Log($"Level Up! New Level: {currentLevel}");
        }
    }

    public void EarnCurrency(int amount)
    {
        currentCurrency += amount;
        OnCurrencyEarned?.Invoke(amount);
        Debug.Log($"Earned {amount} currency. Total: {currentCurrency}");
    }
}
