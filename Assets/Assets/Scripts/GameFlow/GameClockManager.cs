using UnityEngine;

public class GameClockManager : MonoBehaviour
{
    [SerializeField] private GameSettingsSO gameSettings;

    private float cycleTimer;
    private bool isDay = true;
    private float currentCycleDuration;

    private void Start()
    {
        if (gameSettings == null)
        {
            Debug.LogError("GameSettingsSO is not assigned!");
            this.enabled = false;
            return;
        }
        StartDay();
    }

    private void Update()
    {
        cycleTimer -= Time.deltaTime;
        
        GameEvents.ReportTimeUpdated(cycleTimer, currentCycleDuration);

        if (cycleTimer <= 0)
        {
            if (isDay)
            {
                StartNight();
            }
            else
            {
                StartDay();
            }
        }
    }

    private void StartDay()
    {
        isDay = true;
        currentCycleDuration = gameSettings.dayDurationSeconds;
        cycleTimer = currentCycleDuration;
        GameEvents.ReportDayStart();
        Debug.Log("A new day has started!");
    }

    private void StartNight()
    {
        isDay = false;
        currentCycleDuration = gameSettings.nightDurationSeconds;
        cycleTimer = currentCycleDuration;
        GameEvents.ReportNightStart();
        Debug.Log("Night has fallen.");
    }
}

// ScriptRole: Manages the in-game clock and the day/night cycle.
// Dependencies: None
// HandlesEvents: None
// TriggersEvents: GameEvents.OnDayStart, GameEvents.OnNightStart, GameEvents.OnTimeUpdated
// UsesSO: GameSettingsSO
// NeedsSetup: Assign the GameSettingsSO asset in the Inspector. 