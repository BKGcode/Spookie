using UnityEngine;
using TMPro;
using DayNightSystem.Core;

namespace DayNightSystem.UI.TimeDisplay
{
    public class TimeDisplayController : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI timeDisplay;
        
        [Header("Settings")]
        [SerializeField] private string timeFormat = "HH:mm";
        [SerializeField] private bool showSeconds = false;
        [SerializeField] private bool use24HourFormat = true;
        
        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;
        
        // References
        private DayNightTimeController timeController;
        
        // Events
        public System.Action<string> OnTimeDisplayUpdated;
        
        private void Awake()
        {
            FindReferences();
            ValidateReferences();
            
            if (showDebugLogs)
                Debug.Log("[TimeDisplayController] Initialized");
        }
        
        private void OnEnable()
        {
            if (timeController != null)
            {
                timeController.OnTimeChanged += OnTimeChanged;
                // Optional: react to sunset warning if present
                timeController.OnSunsetWarning += OnSunsetWarning;
            }
        }
        
        private void OnDisable()
        {
            if (timeController != null)
            {
                timeController.OnTimeChanged -= OnTimeChanged;
                timeController.OnSunsetWarning -= OnSunsetWarning;
            }
        }
        
        private void OnTimeChanged(float time)
        {
            // Convert to normalized (0..1 per phase) using controller's normalized time
            float normalized = timeController != null ? (timeController.CurrentTimeNormalized % 1f) : 0f;
            UpdateTimeDisplayNormalized(normalized);
        }

        private void OnSunsetWarning(float secondsRemaining)
        {
            // hook point for future UI highlighting if desired
            if (showDebugLogs)
                Debug.Log($"[TimeDisplayController] Sunset warning: {secondsRemaining:F0}s to night");
        }
        
        private void UpdateTimeDisplayNormalized(float timeNormalized)
        {
            if (timeDisplay == null) return;
            
            string formattedTime = FormatTimeFromNormalized(timeNormalized);
            timeDisplay.text = formattedTime;
            
            OnTimeDisplayUpdated?.Invoke(formattedTime);
            
            if (showDebugLogs)
                Debug.Log($"[TimeDisplayController] Time updated: {formattedTime}");
        }
        
        private string FormatTimeFromNormalized(float timeNormalized)
        {
            // Convert normalized time (0..1) to a 24h representation
            float totalMinutes = Mathf.Clamp01(timeNormalized) * 24f * 60f;
            int hours = Mathf.FloorToInt(totalMinutes / 60f);
            int minutes = Mathf.FloorToInt(totalMinutes % 60f);
            int seconds = showSeconds ? Mathf.FloorToInt((totalMinutes % 1f) * 60f) : 0;
            
            if (use24HourFormat)
            {
                if (showSeconds)
                    return $"{hours:D2}:{minutes:D2}:{seconds:D2}";
                else
                    return $"{hours:D2}:{minutes:D2}";
            }
            else
            {
                int displayHours = hours % 12;
                if (displayHours == 0) displayHours = 12;
                string ampm = hours < 12 ? "AM" : "PM";
                
                if (showSeconds)
                    return $"{displayHours:D2}:{minutes:D2}:{seconds:D2} {ampm}";
                else
                    return $"{displayHours:D2}:{minutes:D2} {ampm}";
            }
        }
        
        public void ForceUpdate()
        {
            if (timeController != null)
            {
                float normalized = timeController.CurrentTimeNormalized % 1f;
                UpdateTimeDisplayNormalized(normalized);
            }
        }
        
        public void SetTimeFormat(string format)
        {
            timeFormat = format;
            ForceUpdate();
        }
        
        public void SetShowSeconds(bool show)
        {
            showSeconds = show;
            ForceUpdate();
        }
        
        public void Set24HourFormat(bool use24Hour)
        {
            use24HourFormat = use24Hour;
            ForceUpdate();
        }
        
        public string GetCurrentTimeString()
        {
            if (timeController != null)
            {
                return FormatTimeFromNormalized(timeController.CurrentTimeNormalized % 1f);
            }
            return "00:00";
        }
        
        public float GetCurrentTimeNormalized()
        {
            return timeController != null ? timeController.CurrentTimeNormalized : 0f;
        }
        
        private void FindReferences()
        {
            timeController = GetComponent<DayNightTimeController>();
            if (timeController == null)
            {
                timeController = FindObjectOfType<DayNightTimeController>();
            }
        }
        
        private void ValidateReferences()
        {
            if (timeDisplay == null)
            {
                Debug.LogError("[TimeDisplayController] TimeDisplay reference is missing!");
            }
            
            if (timeController == null)
            {
                Debug.LogError("[TimeDisplayController] DayNightTimeController reference is missing!");
            }
        }
        
        [ContextMenu("Force Update Time Display")]
        public void ForceUpdateContext()
        {
            ForceUpdate();
        }
        
        [ContextMenu("Show Current Time")]
        public void ShowCurrentTime()
        {
            Debug.Log($"[TimeDisplayController] Current Time: {GetCurrentTimeString()} (Normalized: {GetCurrentTimeNormalized():F3})");
        }
        
        [ContextMenu("Toggle 24 Hour Format")]
        public void Toggle24HourFormat()
        {
            Set24HourFormat(!use24HourFormat);
        }
        
        [ContextMenu("Toggle Show Seconds")]
        public void ToggleShowSeconds()
        {
            SetShowSeconds(!showSeconds);
        }
    }
}
