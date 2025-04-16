using System; // Asegúrate de agregar esta línea para usar "Action".
using UnityEngine;

public class TimerSystem : MonoBehaviour
{
    public static event Action<float> OnTimerTick;  // Notifica el tiempo restante.
    public static event Action OnTimerComplete;    // Notifica el fin del temporizador.

    private bool isRunning;
    private float remainingTime;

    [Header("Rewards")]
    public int workXPRate = 10;  // XP por sesión completada
    public int breakCurrencyRate = 5;  // Créditos por descanso

    public void StartTimer(float duration, bool isWorkSession)
    {
        if (isRunning)
        {
            Debug.LogWarning("Timer is already running!");
            return;
        }

        remainingTime = duration;
        isRunning = true;

        Debug.Log($"Timer started for {(isWorkSession ? "Work" : "Break")} session ({duration} seconds).");
    }

    public void StopTimer()
    {
        isRunning = false;
        Debug.Log("Timer stopped.");
    }

    private void Update()
    {
        if (!isRunning) return;

        remainingTime -= Time.deltaTime;

        OnTimerTick?.Invoke(remainingTime);

        if (remainingTime <= 0f)
        {
            isRunning = false;
            remainingTime = 0f;

            // Otorgar recompensas al finalizar
            if (FindObjectOfType<GameStateManager>().GetCurrentState() == GameState.Working)
            {
                FindObjectOfType<GamificationSystem>().EarnXP(workXPRate);
            }
            else if (FindObjectOfType<GameStateManager>().GetCurrentState() == GameState.Break)
            {
                FindObjectOfType<GamificationSystem>().EarnCurrency(breakCurrencyRate);
            }

            OnTimerComplete?.Invoke();
            Debug.Log("Timer complete!");
        }
    }
}
