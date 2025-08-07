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
        
        [Header("References")]
        [SerializeField] private FeedbackMessagesSO feedbackMessages;
        
        [Header("Audio")]
        [SerializeField] private AudioManager audioManager;
        
        [Header("Settings")]
        [SerializeField] private float warningBlinkRate = 0.5f;
        [SerializeField] private bool showWarningIcon = true;
        [SerializeField] private bool showSpawnProximity = true;
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
                dayNightManager.OnNightBlocked += OnNightBlocked;
            }
            
            if (SpawnPoint.Current != null)
            {
                SpawnPoint.Current.OnPlayerEnteredSafeArea += OnPlayerEnteredSafeArea;
                SpawnPoint.Current.OnPlayerLeftSafeArea += OnPlayerLeftSafeArea;
                SpawnPoint.Current.OnPlayerEnteredWarningZone += OnPlayerEnteredWarningZone;
                SpawnPoint.Current.OnPlayerLeftWarningZone += OnPlayerLeftWarningZone;
                SpawnPoint.Current.OnDistanceChanged += OnDistanceChanged;
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
                dayNightManager.OnNightBlocked -= OnNightBlocked;
            }
            
            if (SpawnPoint.Current != null)
            {
                SpawnPoint.Current.OnPlayerEnteredSafeArea -= OnPlayerEnteredSafeArea;
                SpawnPoint.Current.OnPlayerLeftSafeArea -= OnPlayerLeftSafeArea;
                SpawnPoint.Current.OnPlayerEnteredWarningZone -= OnPlayerEnteredWarningZone;
                SpawnPoint.Current.OnPlayerLeftWarningZone -= OnPlayerLeftWarningZone;
                SpawnPoint.Current.OnDistanceChanged -= OnDistanceChanged;
            }
            
            StopWarningBlink();
            StopProximityUpdate();
        }
        
        private void ValidateReferences()
        {
            if (timeDisplay == null)
            {
                Debug.LogError("[DayNightUI] TimeDisplay reference is missing!");
            }
            
            if (statusDisplay == null)
            {
                Debug.LogError("[DayNightUI] StatusDisplay reference is missing!");
            }
            
            if (warningIcon == null)
            {
                Debug.LogWarning("[DayNightUI] WarningIcon reference is missing!");
            }
            
            if (spawnProximityIcon == null)
            {
                Debug.LogWarning("[DayNightUI] SpawnProximityIcon reference is missing!");
            }
            
            if (feedbackMessages == null)
            {
                Debug.LogWarning("[DayNightUI] FeedbackMessagesSO reference is missing!");
            }
            
            if (audioManager == null)
            {
                Debug.LogWarning("[DayNightUI] AudioManager reference is missing - no warning sounds");
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
            if (statusDisplay != null)
            {
                string warningMessage = GetMessage("exhaustion_warning", $"Warning: You will faint in {secondsRemaining:F1}s!");
                ShowStatusMessage(warningMessage);
            }
            
            // Play warning sound
            if (audioManager != null)
            {
                audioManager.PlayWarningSound();
            }
            
            // Show fainting timer if active
            if (dayNightManager != null && dayNightManager.IsFaintingTimerActive())
            {
                ShowFaintingTimer(secondsRemaining);
            }
            
            if (showDebugLogs)
                Debug.Log($"[DayNightUI] Exhaustion warning - {secondsRemaining:F1}s remaining");
        }
        
        private void ShowFaintingTimer(float secondsRemaining)
        {
            if (statusDisplay != null)
            {
                string faintingMessage = GetMessage("fainting_timer", $"You will faint in {secondsRemaining:F1}s! Return to spawn!");
                ShowStatusMessage(faintingMessage);
            }
            
            if (showDebugLogs)
                Debug.Log($"[DayNightUI] Showing fainting timer - {secondsRemaining:F1}s remaining");
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
            HidePenaltyIndicator();
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
            if (statusDisplay != null)
            {
                string faintedMessage = GetMessage("fainting_occurred", "You fainted from exhaustion!");
                ShowStatusMessage(faintedMessage);
            }
            
            // Play penalty sound
            if (audioManager != null)
            {
                audioManager.PlayPenaltySound();
            }
            
            // Show penalty indicator
            ShowPenaltyIndicator();
            
            if (showDebugLogs)
                Debug.Log("[DayNightUI] Player fainted - showing penalty message and indicator");
        }
        
        private void ShowPenaltyIndicator()
        {
            if (warningIcon != null)
            {
                warningIcon.gameObject.SetActive(true);
                warningIcon.color = Color.red; // Red for fainted penalty
                
                // Start blinking effect
                if (warningBlinkCoroutine == null)
                {
                    warningBlinkCoroutine = StartCoroutine(WarningBlinkCoroutine());
                }
                
                if (showDebugLogs)
                    Debug.Log("[DayNightUI] Showing penalty indicator");
            }
        }
        
        private void OnNightBlocked()
        {
            ShowStatusMessage(GetMessage("night_blocked", "Night has fallen - you cannot move until dawn"));
            
            if (showDebugLogs)
                Debug.Log("[DayNightUI] Night blocked - showing restriction message");
        }
        
        private void OnPlayerEnteredSafeArea()
        {
            ShowProximityIndicator(true);
            
            // Check if player returned during fainting timer
            if (dayNightManager != null && dayNightManager.HasStartedFaintingTimer())
            {
                ShowStatusMessage(GetMessage("fainting_prevented", "You returned to spawn just in time!"));
                
                if (showDebugLogs)
                    Debug.Log("[DayNightUI] Player returned to spawn just in time!");
            }
            else
            {
                ShowStatusMessage(GetMessage("safe_at_spawn", "You are safe at the spawn point"));
                
                if (showDebugLogs)
                    Debug.Log("[DayNightUI] Player entered safe area");
            }
        }
        
        private void OnPlayerLeftSafeArea()
        {
            if (showDebugLogs)
                Debug.Log("[DayNightUI] Player left safe area");
            
            HideProximityIndicator();
            UpdateStatusDisplay();
        }
        
        private void OnPlayerEnteredWarningZone()
        {
            if (showDebugLogs)
                Debug.Log("[DayNightUI] Player entered warning zone");
            
            ShowWarningMessage(GetMessage("warning_zone_entered", "Warning: Getting far from spawn"));
            UpdateStatusDisplay();
        }
        
        private void OnPlayerLeftWarningZone()
        {
            if (showDebugLogs)
                Debug.Log("[DayNightUI] Player left warning zone");
            
            UpdateStatusDisplay();
        }
        
        private void OnDistanceChanged(float distance)
        {
            if (showDebugLogs)
                Debug.Log($"[DayNightUI] Distance changed: {distance:F2}m");
            
            UpdateStatusDisplay();
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
            
            // Add penalty information if active
            if (playerPenalty != null && playerPenalty.CurrentPenaltyType != PenaltyType.None)
            {
                string penaltyInfo = GetPenaltyDescription(playerPenalty.CurrentPenaltyType);
                statusMessage += $" - {penaltyInfo}";
            }
            
            // Add distance information if available
            if (SpawnPoint.Current != null)
            {
                float distance = SpawnPoint.GetDistanceToCurrentSpawn();
                string distanceKey = SpawnPoint.Current.GetDistanceDescriptionWithMessages();
                string distanceInfo = GetMessage(distanceKey, SpawnPoint.Current.GetDistanceDescription());
                
                if (distance < float.MaxValue)
                {
                    statusMessage += $" - {distanceInfo} ({distance:F1}m)";
                }
            }
            
            statusDisplay.text = statusMessage;
        }
        
        private string GetPenaltyDescription(PenaltyType penaltyType)
        {
            switch (penaltyType)
            {
                case PenaltyType.Exhaustion:
                    return GetMessage("exhaustion_penalty", "Exhausted - Speed reduced");
                case PenaltyType.Fainted:
                    return GetMessage("fainted_penalty", "Fainted - Severe speed penalty");
                default:
                    return "";
            }
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
        
        private void HidePenaltyIndicator()
        {
            if (warningIcon != null)
            {
                warningIcon.gameObject.SetActive(false);
                StopWarningBlink();
                
                if (showDebugLogs)
                    Debug.Log("[DayNightUI] Hiding penalty indicator");
            }
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
        
        private void ShowWarningMessage(string message)
        {
            if (statusDisplay != null)
            {
                statusDisplay.text = message;
                
                if (showDebugLogs)
                    Debug.Log($"[DayNightUI] Warning message: {message}");
            }
            
            // Play warning sound
            if (audioManager != null)
            {
                audioManager.PlayWarningSound();
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
// SendsTo: TextMeshProUGUI, Image (UI elements), AudioManager (warning sounds)
