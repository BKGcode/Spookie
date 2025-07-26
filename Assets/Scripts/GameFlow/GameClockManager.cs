using UnityEngine;

public class GameClockManager : MonoBehaviour
{
    public static GameClockManager Instance { get; private set; }
    
    [SerializeField] private GameSettingsSO gameSettings;

    private float cycleTimer;
    private bool isDay = true;
    private float currentCycleDuration;

    public float CurrentTime { get; private set; }
    
    // Time block properties (based on a 24-hour cycle)
    public bool IsWorkingHours => CurrentTime < 0.5f; // First 12 hours (0.0 to 0.5)
    public bool IsWanderingHours => CurrentTime >= 0.5f && CurrentTime < 0.666f; // Next 4 hours (0.5 to 0.666)
    public bool IsSleepingHours => CurrentTime >= 0.666f; // Last 8 hours (0.666 to 1.0)

    private float timeScale;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (gameSettings == null)
        {
            Debug.LogError("GameSettingsSO is not assigned!");
            this.enabled = false;
            return;
        }
        CurrentTime = 0;
        timeScale = 1f / gameSettings.dayDurationSeconds;
    }

    private void Start()
    {
        StartDay();
    }

    private void Update()
    {
        CurrentTime += Time.deltaTime * timeScale;

        if (CurrentTime >= 1f)
        {
            CurrentTime = 0f; // Start a new day
            Debug.Log("A new day cycle starts.");
        }
        
        // Report the normalized time
        GameEvents.ReportTimeUpdated(CurrentTime, 1f);

        // Check for transitions
        bool wasDay = isDay;
        isDay = CurrentTime < 0.5f; // Day is the first half of the cycle

        if (isDay && !wasDay)
        {
            StartDay();
        }
        else if (!isDay && wasDay)
        {
            StartNight();
        }
    }

    private void StartDay()
    {
        isDay = true;
        Debug.Log("A new day has started!");
        GameEvents.ReportDayStart();
    }

    private void StartNight()
    {
        isDay = false;
        Debug.Log("Night has fallen.");
        GameEvents.ReportNightStart();
    }
}

// ScriptRole: Manages the in-game clock and the day/night cycle.
// Dependencies: None
// HandlesEvents: None
// TriggersEvents: GameEvents.OnDayStart, GameEvents.OnNightStart, GameEvents.OnTimeUpdated
// UsesSO: GameSettingsSO
// NeedsSetup: Assign the GameSettingsSO asset in the Inspector. 