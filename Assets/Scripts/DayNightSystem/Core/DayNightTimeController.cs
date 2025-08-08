using UnityEngine;

namespace DayNightSystem.Core
{
    public class DayNightTimeController : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private DayNightConfig config;
        
        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;
        
        // Private fields
        private float currentTime;
        private bool isPaused = false;
        
        // Public properties
        public float CurrentTime => currentTime;
        public float CurrentTimeNormalized => config != null ? currentTime / config.DayDurationSeconds : 0f;
        public bool IsPaused => isPaused;
        
        // Events
        public System.Action<float> OnTimeChanged;
        public System.Action OnDayTimeReached;
        public System.Action OnNightTimeReached;
        public System.Action<float> OnSunsetWarning; // seconds remaining to night
        
        private void Awake()
        {
            ValidateReferences();
            
            if (showDebugLogs)
                Debug.Log($"[DayNightTimeController] Initialized - Current Time: {currentTime:F1}s");
        }
        
        private void Update()
        {
            if (isPaused) return;
            
            // Validate config is available
            if (config == null)
            {
                Debug.LogError("[DayNightTimeController] Config is null! Cannot update time.");
                return;
            }
            
            // Clamp delta time to prevent huge jumps
            float deltaTime = Mathf.Clamp(Time.deltaTime, 0f, 0.1f);
            float previousTime = currentTime;
            currentTime += deltaTime;
            
            // Notify time change
            if (Mathf.Abs(currentTime - previousTime) > 0.01f)
            {
                OnTimeChanged?.Invoke(currentTime);
            }
            
            // Debug log every 10 seconds
            if (Mathf.FloorToInt(currentTime) % 10 == 0 && Mathf.FloorToInt(currentTime) != Mathf.FloorToInt(previousTime))
            {
                if (showDebugLogs)
                    Debug.Log($"[DayNightTimeController] Current Time: {currentTime:F1}s / {config.DayDurationSeconds:F1}s ({(currentTime/config.DayDurationSeconds)*100:F1}%)");
            }
            
            // Check for day/night transitions robustly
            CheckDayNightTransition(previousTime);
        }
        
        private bool dayEventFiredThisCycle = false;
        private bool nightEventFiredThisCycle = false;
        [Header("Sunset Warning")]
        [SerializeField] private bool enableSunsetWarning = true;
        [SerializeField] private float sunsetWarningLeadSeconds = 30f;
        private bool sunsetWarningFired = false;

        private void CheckDayNightTransition(float previousTime)
        {
            if (config == null) return;
            
            float dayDuration = config.DayDurationSeconds;
            float cycleDuration = dayDuration * 2f;
            if (cycleDuration <= 0f) return;
            float prevCycle = previousTime % cycleDuration;
            float cycleTime = currentTime % cycleDuration;
            // Sunset pre-warning when day is about to end
            if (enableSunsetWarning && dayDuration > 1f)
            {
                float lead = Mathf.Clamp(sunsetWarningLeadSeconds, 1f, dayDuration - 0.5f);
                // Reset flag at start of new day segment
                if (cycleTime < (dayDuration - lead))
                {
                    sunsetWarningFired = false;
                }
                // Fire once near end of day, before night boundary
                if (!sunsetWarningFired && cycleTime >= (dayDuration - lead) && cycleTime < dayDuration)
                {
                    float secondsRemainingToNight = dayDuration - cycleTime;
                    OnSunsetWarning?.Invoke(secondsRemainingToNight);
                    sunsetWarningFired = true;
                    if (showDebugLogs)
                        Debug.Log($"[DayNightTimeController] Sunset warning: {secondsRemainingToNight:F1}s to night");
                }
            }
            
            // Night boundary: cross from day segment (< dayDuration) to night segment (>= dayDuration)
            if (prevCycle < dayDuration && cycleTime >= dayDuration)
            {
                OnNightTimeReached?.Invoke();
                if (showDebugLogs)
                    Debug.Log("[DayNightTimeController] Night time reached (boundary crossed)");
            }

            // Day boundary: wrap from night segment (>= dayDuration) to day segment (< dayDuration)
            if (prevCycle >= dayDuration && cycleTime < dayDuration)
            {
                OnDayTimeReached?.Invoke();
                if (showDebugLogs)
                    Debug.Log("[DayNightTimeController] Day time reached (boundary crossed)");
            }
        }
        
        public void SetTime(float time)
        {
            currentTime = Mathf.Clamp(time, 0f, config != null ? config.DayDurationSeconds * 2f : 0f);
            OnTimeChanged?.Invoke(currentTime);
            
            if (showDebugLogs)
                Debug.Log($"[DayNightTimeController] Time set to: {currentTime:F1}s");
        }
        
        public void AddTime(float minutes)
        {
            float seconds = minutes * 60f;
            currentTime += seconds;
            
            if (showDebugLogs)
                Debug.Log($"[DayNightTimeController] Added {minutes} minutes ({seconds}s) - New time: {currentTime:F1}s");
        }
        
        public void PauseTime(bool pause)
        {
            isPaused = pause;
            
            if (showDebugLogs)
                Debug.Log($"[DayNightTimeController] Time {(pause ? "paused" : "resumed")}");
        }
        
        public void ResetTime()
        {
            currentTime = 0f;
            OnTimeChanged?.Invoke(currentTime);
            
            if (showDebugLogs)
                Debug.Log("[DayNightTimeController] Time reset to 0");
        }
        
        public float GetTimeProgress()
        {
            if (config == null) return 0f;
            return (currentTime % config.DayDurationSeconds) / config.DayDurationSeconds;
        }
        
        public bool IsDayTime()
        {
            if (config == null) return true;
            return currentTime < config.DayDurationSeconds;
        }
        
        public bool IsNightTime()
        {
            if (config == null) return false;
            return currentTime >= config.DayDurationSeconds && currentTime < config.DayDurationSeconds * 2f;
        }
        
        private void ValidateReferences()
        {
            if (config == null)
            {
                Debug.LogError("[DayNightTimeController] DayNightConfig reference is missing!");
            }
        }
        
        [ContextMenu("Add 1 Minute")]
        public void AddOneMinute()
        {
            AddTime(1f);
        }
        
        [ContextMenu("Add 5 Minutes")]
        public void AddFiveMinutes()
        {
            AddTime(5f);
        }
        
        [ContextMenu("Reset Time")]
        public void ResetTimeContext()
        {
            ResetTime();
        }
        
        [ContextMenu("Show Current Time")]
        public void ShowCurrentTime()
        {
            Debug.Log($"[DayNightTimeController] Current Time: {currentTime:F1}s, Progress: {GetTimeProgress():P1}, IsDay: {IsDayTime()}");
        }
    }
}
