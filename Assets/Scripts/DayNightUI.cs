using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace DayNightSystem
{
    public class DayNightUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI timeDisplay;
        [SerializeField] private TextMeshProUGUI statusDisplay;
        [SerializeField] private Image warningIcon;
        [SerializeField] private Image spawnProximityIcon;
        [SerializeField] private FeedbackMessagesSO feedbackMessages;
        
        [Header("UI Settings")]
        [SerializeField] private bool showWarningIcon = true;
        [SerializeField] private bool showSpawnProximity = true;
        [SerializeField] private float warningBlinkRate = 0.5f;
        [SerializeField] private float proximityUpdateRate = 0.5f;
        
        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;
        
        // Private fields
        private DayNightManager dayNightManager;
        private PlayerPenalty playerPenalty;
        private Coroutine warningBlinkCoroutine;
        private Coroutine proximityUpdateCoroutine;
        private bool isWarningActive = false;
        private bool isProximityActive = false;
        
        private void Awake()
        {
            ValidateReferences();
            FindDayNightManager();
            FindPlayerPenalty();
        }
        
        private void OnEnable()
        {
            if (dayNightManager != null)
            {
                dayNightManager.OnTimeChanged += OnTimeChanged;
                dayNightManager.OnExhaustionWarning += OnExhaustionWarning;
                dayNightManager.OnDayStart += OnDayStart;
                dayNightManager.OnExhaustionStarted += OnExhaustionStarted;
                dayNightManager.OnPlayerSlept += OnPlayerSlept;
                dayNightManager.OnPlayerFainted += OnPlayerFainted;
            }
            
            if (SpawnPoint.Current != null)
            {
                SpawnPoint.Current.OnPlayerEnteredSafeArea += OnPlayerEnteredSafeArea;
                SpawnPoint.Current.OnPlayerLeftSafeArea += OnPlayerLeftSafeArea;
            }
        }
        
        private void OnDisable()
        {
            if (dayNightManager != null)
            {
                dayNightManager.OnTimeChanged -= OnTimeChanged;
                dayNightManager.OnExhaustionWarning -= OnExhaustionWarning;
                dayNightManager.OnDayStart -= OnDayStart;
                dayNightManager.OnExhaustionStarted -= OnExhaustionStarted;
                dayNightManager.OnPlayerSlept -= OnPlayerSlept;
                dayNightManager.OnPlayerFainted -= OnPlayerFainted;
            }
            
            if (SpawnPoint.Current != null)
            {
                SpawnPoint.Current.OnPlayerEnteredSafeArea -= OnPlayerEnteredSafeArea;
                SpawnPoint.Current.OnPlayerLeftSafeArea -= OnPlayerLeftSafeArea;
            }
            
            StopWarningBlink();
            StopProximityUpdate();
        }
        
        private void ValidateReferences()
        {
            if (timeDisplay == null)
            {
                Debug.LogError("[DayNightUI] Time Display reference is missing!");
            }
            
            if (statusDisplay == null)
            {
                Debug.LogWarning("[DayNightUI] Status Display reference is missing - status messages disabled");
            }
            
            if (warningIcon == null)
            {
                Debug.LogWarning("[DayNightUI] Warning Icon reference is missing - exhaustion warnings disabled");
            }
            
            if (spawnProximityIcon == null)
            {
                Debug.LogWarning("[DayNightUI] Spawn Proximity Icon reference is missing - proximity indicator disabled");
            }
            
            if (feedbackMessages == null)
            {
                Debug.LogWarning("[DayNightUI] Feedback Messages reference is missing - using default messages");
            }
        }
        
        private void FindDayNightManager()
        {
            dayNightManager = FindObjectOfType<DayNightManager>();
            if (dayNightManager == null)
            {
                Debug.LogError("[DayNightUI] No DayNightManager found in scene!");
            }
        }
        
        private void FindPlayerPenalty()
        {
            playerPenalty = FindObjectOfType<PlayerPenalty>();
            if (playerPenalty == null)
            {
                Debug.LogWarning("[DayNightUI] No PlayerPenalty found in scene!");
            }
        }
        
        private void OnTimeChanged(float timeNormalized)
        {
            UpdateTimeDisplay(timeNormalized);
            UpdateStatusDisplay();
        }
        
        private void OnExhaustionWarning(float secondsRemaining)
        {
            ShowExhaustionWarning(secondsRemaining);
        }
        
        private void OnExhaustionStarted()
        {
            ShowExhaustionWarning(0f);
            UpdateStatusDisplay();
        }
        
        private void OnDayStart()
        {
            HideExhaustionWarning();
            HideProximityIndicator();
            UpdateStatusDisplay();
            
            if (showDebugLogs)
                Debug.Log("[DayNightUI] Day started - hiding warnings and updating status");
        }
        
        private void OnPlayerSlept()
        {
            ShowStatusMessage(GetMessage("slept_correctly", "You slept well and feel refreshed!"));
            
            if (showDebugLogs)
                Debug.Log("[DayNightUI] Player slept correctly");
        }
        
        private void OnPlayerFainted()
        {
            ShowStatusMessage(GetMessage("fainted", "You fainted from exhaustion!"));
            
            if (showDebugLogs)
                Debug.Log("[DayNightUI] Player fainted");
        }
        
        private void OnPlayerEnteredSafeArea()
        {
            ShowProximityIndicator(true);
            ShowStatusMessage(GetMessage("safe_at_spawn", "You are safe at the spawn point"));
            
            if (showDebugLogs)
                Debug.Log("[DayNightUI] Player entered safe area");
        }
        
        private void OnPlayerLeftSafeArea()
        {
            ShowProximityIndicator(false);
            
            if (showDebugLogs)
                Debug.Log("[DayNightUI] Player left safe area");
        }
        
        private void UpdateTimeDisplay(float timeNormalized)
        {
            if (timeDisplay == null) return;
            
            // Calculate remaining time
            float totalSeconds = dayNightManager.Config.DayDurationSeconds;
            float elapsedSeconds = timeNormalized * totalSeconds;
            float remainingSeconds = totalSeconds - elapsedSeconds;
            
            // Format time as MM:SS
            int minutes = Mathf.FloorToInt(remainingSeconds / 60f);
            int seconds = Mathf.FloorToInt(remainingSeconds % 60f);
            
            string timeText = string.Format("{0:00}:{1:00}", minutes, seconds);
            
            // Get message from FeedbackMessagesSO or use default
            string message = GetMessage("time_remaining", $"Time Remaining: {timeText}");
            timeDisplay.text = message;
        }
        
        private void UpdateStatusDisplay()
        {
            if (statusDisplay == null) return;
            
            string statusMessage = "";
            
            if (dayNightManager != null)
            {
                switch (dayNightManager.CurrentState)
                {
                    case DayNightState.Day:
                        statusMessage = GetMessage("status_day", "Day - Explore and investigate");
                        break;
                    case DayNightState.Night:
                        statusMessage = GetMessage("status_night", "Night - Return to spawn to rest");
                        break;
                    case DayNightState.Exhausted:
                        statusMessage = GetMessage("status_exhausted", "Exhausted - Return to spawn quickly!");
                        break;
                    case DayNightState.Sleeping:
                        statusMessage = GetMessage("status_sleeping", "Sleeping...");
                        break;
                    case DayNightState.Fainted:
                        statusMessage = GetMessage("status_fainted", "Fainted - You will be penalized");
                        break;
                }
            }
            
            if (playerPenalty != null && playerPenalty.CurrentPenaltyType != PenaltyType.None)
            {
                statusMessage += $" - {playerPenalty.GetPenaltyDescription()}";
            }
            
            statusDisplay.text = statusMessage;
        }
        
        private void ShowExhaustionWarning(float secondsRemaining)
        {
            if (warningIcon == null) return;
            
            isWarningActive = true;
            warningIcon.gameObject.SetActive(true);
            
            // Start blinking effect
            if (warningBlinkCoroutine == null)
            {
                warningBlinkCoroutine = StartCoroutine(WarningBlinkCoroutine());
            }
            
            // Show warning message
            string warningMessage = secondsRemaining > 0f 
                ? GetMessage("exhaustion_warning", $"Warning: Exhaustion in {secondsRemaining:F0}s!")
                : GetMessage("exhaustion_active", "You are exhausted! Return to spawn!");
            
            ShowStatusMessage(warningMessage);
            
            if (showDebugLogs)
                Debug.Log($"[DayNightUI] Showing exhaustion warning - {secondsRemaining:F1}s remaining");
        }
        
        private void HideExhaustionWarning()
        {
            if (warningIcon == null) return;
            
            isWarningActive = false;
            warningIcon.gameObject.SetActive(false);
            StopWarningBlink();
            
            if (showDebugLogs)
                Debug.Log("[DayNightUI] Hiding exhaustion warning");
        }
        
        private void ShowProximityIndicator(bool show)
        {
            if (spawnProximityIcon == null || !showSpawnProximity) return;
            
            isProximityActive = show;
            spawnProximityIcon.gameObject.SetActive(show);
            
            if (show && proximityUpdateCoroutine == null)
            {
                proximityUpdateCoroutine = StartCoroutine(ProximityUpdateCoroutine());
            }
            else if (!show)
            {
                StopProximityUpdate();
            }
        }
        
        private void HideProximityIndicator()
        {
            ShowProximityIndicator(false);
        }
        
        private void ShowStatusMessage(string message)
        {
            if (statusDisplay != null)
            {
                statusDisplay.text = message;
                
                if (showDebugLogs)
                    Debug.Log($"[DayNightUI] Status message: {message}");
            }
        }
        
        private System.Collections.IEnumerator WarningBlinkCoroutine()
        {
            while (isWarningActive)
            {
                warningIcon.enabled = true;
                yield return new WaitForSeconds(warningBlinkRate);
                
                warningIcon.enabled = false;
                yield return new WaitForSeconds(warningBlinkRate);
            }
        }
        
        private System.Collections.IEnumerator ProximityUpdateCoroutine()
        {
            while (isProximityActive)
            {
                if (SpawnPoint.Current != null)
                {
                    float distance = SpawnPoint.GetDistanceToCurrentSpawn();
                    bool inSafeArea = SpawnPoint.IsPlayerAtSpawn();
                    
                    // Update proximity icon color based on distance
                    if (spawnProximityIcon != null)
                    {
                        Color iconColor = inSafeArea ? Color.green : 
                                        distance < 5f ? Color.yellow : Color.red;
                        spawnProximityIcon.color = iconColor;
                    }
                }
                
                yield return new WaitForSeconds(proximityUpdateRate);
            }
        }
        
        private void StopWarningBlink()
        {
            if (warningBlinkCoroutine != null)
            {
                StopCoroutine(warningBlinkCoroutine);
                warningBlinkCoroutine = null;
            }
            
            if (warningIcon != null)
            {
                warningIcon.enabled = true;
            }
        }
        
        private void StopProximityUpdate()
        {
            if (proximityUpdateCoroutine != null)
            {
                StopCoroutine(proximityUpdateCoroutine);
                proximityUpdateCoroutine = null;
            }
        }
        
        private string GetMessage(string key, string defaultValue = "")
        {
            if (feedbackMessages != null)
            {
                return feedbackMessages.GetMessage(key);
            }
            
            return defaultValue;
        }
        
        // Public method to force update time display
        public void ForceUpdateTimeDisplay()
        {
            if (dayNightManager != null)
            {
                UpdateTimeDisplay(dayNightManager.CurrentTimeNormalized);
            }
        }
        
        // Public method to show custom warning
        public void ShowCustomWarning(string message)
        {
            if (warningIcon != null)
            {
                warningIcon.gameObject.SetActive(true);
                ShowStatusMessage(message);
                Debug.Log($"[DayNightUI] Custom warning: {message}");
            }
        }
        
        // Public method to show proximity indicator
        public void ShowProximityIndicator()
        {
            ShowProximityIndicator(true);
        }
    }
}

// ScriptRole: Handles UI display for day/night cycle with proximity indicators and contextual messages
// RelatedScripts: DayNightManager, PlayerPenalty, SpawnPoint, FeedbackMessagesSO
// UsesSO: FeedbackMessagesSO
// ReceivesFrom: DayNightManager events, SpawnPoint events
// SendsTo: TextMeshProUGUI, Image (UI elements)
