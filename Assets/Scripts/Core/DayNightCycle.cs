using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using System.Collections;

public class DayNightCycle : MonoBehaviour
{
    [Header("Timeline")]
    public PlayableDirector dayNightTimeline;
    public TimelineAsset dayNightTimelineAsset;
    
    [Header("Day/Night Settings")]
    public float dayDuration = 300f; // 5 minutes per day
    public float nightDuration = 60f; // 1 minute sleep transition
    public bool isDayTime = true;
    
    [Header("Sleep Pressure")]
    public float sleepPressureStartTime = 60f; // Time after night starts when pressure begins
    public float maxSlowdownFactor = 0.3f; // Minimum speed multiplier
    public float fogIntensity = 0.8f; // Maximum fog density
    public float pressureBuildRate = 0.5f; // How fast pressure builds
    
    [Header("References")]
    public Light directionalLight;
    public Transform playerSpawnPoint;
    public Transform bedTransform;
    public FeedbackMessagesSO feedbackMessages;
    
    [Header("Fog System")]
    public Material fogMaterial;
    public Color dayFogColor = new Color(0.5f, 0.5f, 0.5f, 0.1f);
    public Color nightFogColor = new Color(0.1f, 0.1f, 0.2f, 0.8f);
    public float fogTransitionSpeed = 2f;
    
    [Header("Events")]
    public UnityEngine.Events.UnityEvent OnDayStart;
    public UnityEngine.Events.UnityEvent OnNightStart;
    public UnityEngine.Events.UnityEvent OnSleepStart;
    public UnityEngine.Events.UnityEvent OnSleepEnd;
    
    private float currentTime = 0f;
    private bool isSleeping = false;
    private bool cycleComplete = false;
    private PlayerFPSController playerController;
    private Vector3 originalPlayerPosition;
    private float sleepPressure = 0f;
    private float currentFogIntensity = 0f;

    private void Start()
    {
        Debug.Log("DayNightCycle: Starting day/night cycle");
        
        // Find player controller
        playerController = FindObjectOfType<PlayerFPSController>();
        
        if (playerController != null)
        {
            originalPlayerPosition = playerController.transform.position;
        }
        
        // Initialize fog
        if (fogMaterial != null)
        {
            fogMaterial.SetColor("_FogColor", dayFogColor);
            fogMaterial.SetFloat("_FogIntensity", 0f);
        }
        
        // Start day cycle
        StartDay();
    }

    private void Update()
    {
        if (!GameManager.Instance.IsGamePlaying() || isSleeping)
            return;

        UpdateCycle();
        UpdateSleepPressure();
        UpdateFog();
    }

    private void UpdateCycle()
    {
        currentTime += Time.deltaTime;
        
        if (isDayTime)
        {
            // Day cycle
            if (currentTime >= dayDuration)
            {
                Debug.Log("DayNightCycle: Day cycle complete, starting night");
                StartNight();
            }
        }
        else
        {
            // Night cycle (sleep time)
            if (currentTime >= nightDuration)
            {
                Debug.Log("DayNightCycle: Night cycle complete, starting new day");
                StartNewDay();
            }
        }
    }

    private void UpdateSleepPressure()
    {
        if (!cycleComplete || isSleeping) return;
        
        // Start building pressure after sleepPressureStartTime
        if (currentTime > sleepPressureStartTime)
        {
            sleepPressure += Time.deltaTime * pressureBuildRate;
            sleepPressure = Mathf.Clamp01(sleepPressure);
            
            // Apply slowdown to player
            if (playerController != null)
            {
                float slowdownFactor = Mathf.Lerp(1f, maxSlowdownFactor, sleepPressure);
                playerController.SetSpeedMultiplier(slowdownFactor);
            }
            
            // Show pressure message
            if (sleepPressure > 0.5f && feedbackMessages != null)
            {
                string message = feedbackMessages.GetMessage("sleep_pressure_message");
                Debug.Log($"DayNightCycle: {message} (Pressure: {sleepPressure:F2})");
            }
        }
    }

    private void UpdateFog()
    {
        if (fogMaterial == null) return;
        
        Color targetFogColor = isDayTime ? dayFogColor : nightFogColor;
        float targetIntensity = isDayTime ? 0f : (cycleComplete ? fogIntensity * sleepPressure : 0.3f);
        
        // Smooth fog transition
        currentFogIntensity = Mathf.Lerp(currentFogIntensity, targetIntensity, Time.deltaTime * fogTransitionSpeed);
        Color currentFogColor = Color.Lerp(fogMaterial.GetColor("_FogColor"), targetFogColor, Time.deltaTime * fogTransitionSpeed);
        
        fogMaterial.SetColor("_FogColor", currentFogColor);
        fogMaterial.SetFloat("_FogIntensity", currentFogIntensity);
    }

    private void StartDay()
    {
        Debug.Log("DayNightCycle: Starting day");
        isDayTime = true;
        currentTime = 0f;
        cycleComplete = false;
        sleepPressure = 0f;
        
        // Reset player speed
        if (playerController != null)
        {
            playerController.SetSpeedMultiplier(1f);
        }
        
        // Set day lighting
        if (directionalLight != null)
        {
            directionalLight.intensity = 1f;
            directionalLight.color = Color.white;
        }
        
        // Play day ambient
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayDayAmbient();
        }
        
        OnDayStart?.Invoke();
    }

    private void StartNight()
    {
        Debug.Log("DayNightCycle: Starting night");
        isDayTime = false;
        currentTime = 0f;
        cycleComplete = true;
        sleepPressure = 0f;
        
        // Set night lighting
        if (directionalLight != null)
        {
            directionalLight.intensity = 0.1f;
            directionalLight.color = new Color(0.2f, 0.2f, 0.5f);
        }
        
        // Play night ambient
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayNightAmbient();
        }
        
        OnNightStart?.Invoke();
        
        // Show message to go to bed
        if (feedbackMessages != null)
        {
            string message = feedbackMessages.GetMessage("go_to_bed_message");
            Debug.Log($"DayNightCycle: {message}");
        }
    }

    private void StartNewDay()
    {
        Debug.Log("DayNightCycle: Starting new day");
        
        // Start sleep sequence
        StartCoroutine(SleepSequence());
    }

    private IEnumerator SleepSequence()
    {
        Debug.Log("DayNightCycle: Starting sleep sequence");
        isSleeping = true;
        
        // Disable player movement
        if (playerController != null)
        {
            playerController.SetMovementEnabled(false);
        }
        
        // Show sleep message
        if (feedbackMessages != null)
        {
            string message = feedbackMessages.GetMessage("sleeping_message");
            Debug.Log($"DayNightCycle: {message}");
        }
        
        OnSleepStart?.Invoke();
        
        // Fade to black (simple implementation)
        if (CutsceneManager.Instance != null)
        {
            CutsceneManager.Instance.PlayCutsceneWithMessage("sleep", "sleeping_message");
        }
        
        // Wait for sleep duration
        yield return new WaitForSeconds(nightDuration);
        
        // Fade back in
        if (CutsceneManager.Instance != null)
        {
            CutsceneManager.Instance.SkipCurrentCutscene();
        }
        
        // Re-enable player movement
        if (playerController != null)
        {
            playerController.SetMovementEnabled(true);
            playerController.SetSpeedMultiplier(1f);
        }
        
        OnSleepEnd?.Invoke();
        isSleeping = false;
        
        // Start new day
        StartDay();
    }

    public void ForceSleep()
    {
        if (!isSleeping && cycleComplete)
        {
            Debug.Log("DayNightCycle: Force sleep triggered");
            StartNewDay();
        }
    }

    public float GetDayProgress()
    {
        if (isDayTime)
        {
            return currentTime / dayDuration;
        }
        else
        {
            return currentTime / nightDuration;
        }
    }

    public float GetSleepPressure()
    {
        return sleepPressure;
    }

    public bool IsDayTime()
    {
        return isDayTime && !isSleeping;
    }

    public bool IsNightTime()
    {
        return !isDayTime && !isSleeping;
    }

    public bool IsSleeping()
    {
        return isSleeping;
    }

    public bool IsCycleComplete()
    {
        return cycleComplete;
    }

    public void SetDayTime(float time)
    {
        currentTime = Mathf.Clamp(time, 0f, isDayTime ? dayDuration : nightDuration);
    }

    public void SkipToNight()
    {
        if (isDayTime)
        {
            currentTime = dayDuration;
            StartNight();
        }
    }

    public void SkipToDay()
    {
        if (!isDayTime)
        {
            currentTime = nightDuration;
            StartNewDay();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if player entered bed trigger
        if (other.CompareTag("Player") && cycleComplete && !isSleeping)
        {
            Debug.Log("DayNightCycle: Player entered bed, starting sleep");
            ForceSleep();
        }
    }
}

// ScriptRole: Manages day/night cycle with gradual sleep pressure and fog
// Dependencies: PlayableDirector, TimelineAsset, Light, Material
// UsesSO: FeedbackMessagesSO
// ReceivesFrom: PlayerFPSController, GameManager
// SendsTo: AudioManager, CutsceneManager via events 