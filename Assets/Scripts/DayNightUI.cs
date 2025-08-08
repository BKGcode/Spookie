using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DayNightSystem.Core;

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
        private DayNightSystem.Core.DayNightManager dayNightManager;
        private PlayerPenalty playerPenalty;
        private Coroutine warningBlinkCoroutine;
        private Coroutine proximityUpdateCoroutine;
        private bool isWarningActive = false;
        private bool isProximityActive = false;
        [SerializeField] private DayNightSystem.UI.DayNight.DayNightTimeAndStatusHUD timeAndStatusHUD; // Delegation (optional)
        [SerializeField] private DayNightSystem.UI.DayNight.DayNightWarningAndProximityUI warningAndProximityUI; // Delegation (optional)
        
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
            dayNightManager = FindObjectOfType<DayNightSystem.Core.DayNightManager>();
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
            if (timeAndStatusHUD != null) { timeAndStatusHUD.UpdateTime(timeNormalized); }
            else { UpdateTimeDisplay(timeNormalized); }
        }
        
        private void OnExhaustionWarning(float secondsRemaining)
        {
            if (secondsRemaining <= 0f)
            {
                ShowFaintingTimer(secondsRemaining);
            }
            else
            {
                ShowExhaustionWarning(secondsRemaining);
            }
        }
        
        private void ShowFaintingTimer(float secondsRemaining)
        {
            string template = GetMessage("fainting_timer");
            string message = string.IsNullOrEmpty(template) ? string.Empty : string.Format(template, Mathf.Abs(secondsRemaining).ToString("F0"));
            if (!string.IsNullOrEmpty(message)) { ShowWarningMessage(message); }
            
            if (audioManager != null)
            {
                audioManager.PlayWarningSound();
            }
            
            if (showDebugLogs)
                Debug.Log($"[DayNightUI] Fainting timer: {secondsRemaining:F1}s");
        }
        
        private void OnExhaustionStarted()
        {
            ShowPenaltyIndicator();
        }
        
        private void OnDayStart()
        {
            HideExhaustionWarning();
            HidePenaltyIndicator();
            
            string message = GetMessage("new_day_started");
            if (timeAndStatusHUD != null) { timeAndStatusHUD.ShowStatus(message); }
            else { ShowStatusMessage(message); }
            
            if (showDebugLogs)
                Debug.Log("[DayNightUI] Day started - cleared warnings");
        }
        
        private void OnPlayerSlept()
        {
            HideExhaustionWarning();
            HidePenaltyIndicator();
            
            if (showDebugLogs)
                Debug.Log("[DayNightUI] Player slept - cleared warnings");
        }
        
        private void OnPlayerFainted()
        {
            ShowPenaltyIndicator();
            
            if (audioManager != null)
            {
                audioManager.PlayPenaltySound();
            }
            
            if (showDebugLogs)
                Debug.Log("[DayNightUI] Player fainted - showing penalty indicator");
        }
        
        private void ShowPenaltyIndicator()
        {
            if (playerPenalty != null)
            {
                string penaltyText = playerPenalty.GetPenaltyDescription();
                ShowWarningMessage(penaltyText);
                
                if (warningAndProximityUI != null) { warningAndProximityUI.ShowPenaltyIndicator(); }
                else if (warningIcon != null && showWarningIcon) { warningIcon.gameObject.SetActive(true); StartWarningBlink(); }
            }
        }
        
        private void OnNightBlocked()
        {
            string message = GetMessage("night_blocked");
            ShowWarningMessage(message);
            
            if (warningAndProximityUI != null) { warningAndProximityUI.ShowWarning(message); }
            else if (audioManager != null) { audioManager.PlayWarningSound(); }
            
            if (showDebugLogs)
                Debug.Log("[DayNightUI] Night blocked - showing warning");
        }
        
        private void OnPlayerEnteredSafeArea()
        {
            isProximityActive = false;
            HideProximityIndicator();
            
            string message = GetMessage("status_safe_area");
            if (timeAndStatusHUD != null) { timeAndStatusHUD.ShowStatus(message); }
            else { ShowStatusMessage(message); }
            
            if (showDebugLogs)
                Debug.Log("[DayNightUI] Player entered safe area");
        }
        
        private void OnPlayerLeftSafeArea()
        {
            if (showSpawnProximity)
            {
                isProximityActive = true;
                ShowProximityIndicator(true);
                StartProximityUpdate();
            }
            
            if (showDebugLogs)
                Debug.Log("[DayNightUI] Player left safe area");
        }
        
        private void OnPlayerEnteredWarningZone()
        {
            string message = GetMessage("status_warning_zone");
            ShowWarningMessage(message);
            
            if (warningAndProximityUI != null) { warningAndProximityUI.ShowWarning(message); }
            else if (audioManager != null) { audioManager.PlayWarningSound(); }
            
            if (showDebugLogs)
                Debug.Log("[DayNightUI] Player entered warning zone");
        }
        
        private void OnPlayerLeftWarningZone()
        {
            if (showDebugLogs)
                Debug.Log("[DayNightUI] Player left warning zone");
        }
        
        private void OnDistanceChanged(float distance)
        {
            if (warningAndProximityUI != null) { warningAndProximityUI.UpdateDistance(distance); return; }
            if (isProximityActive && spawnProximityIcon != null)
            {
                float normalizedDistance = Mathf.Clamp01(distance / 10f);
                spawnProximityIcon.color = Color.Lerp(Color.green, Color.red, normalizedDistance);
            }
        }
        
        private void UpdateTimeDisplay(float timeNormalized)
        {
            if (timeDisplay == null) return;
            
            // Convert normalized time (0-1) to minutes and seconds
            float totalSeconds = timeNormalized * 1440f; // 24 hours in seconds
            int minutes = Mathf.FloorToInt(totalSeconds / 60f);
            int seconds = Mathf.FloorToInt(totalSeconds % 60f);
            
            string timeString = string.Format("{0:D2}:{1:D2}", minutes, seconds);
            timeDisplay.text = timeString;
            
            if (showDebugLogs && Mathf.FloorToInt(totalSeconds) % 60 == 0)
            {
                Debug.Log($"[DayNightUI] Time updated: {timeString}");
            }
        }
        
        private void UpdateStatusDisplay()
        {
            if (timeAndStatusHUD != null) { timeAndStatusHUD.UpdateStatus(dayNightManager, playerPenalty); return; }
            if (statusDisplay == null) return;
            
            string statusText = "";
            
            if (dayNightManager != null)
            {
                if (dayNightManager.IsDay)
                {
                    statusText = GetMessage("status_day", "Day - Explore and investigate");
                }
                else
                {
                    statusText = GetMessage("status_night", "Night - Return to spawn to rest");
                }
            }
            
            if (playerPenalty != null && playerPenalty.CurrentPenaltyType != PenaltyType.None)
            {
                statusText += " | " + playerPenalty.GetPenaltyDescription();
            }
            
            statusDisplay.text = statusText;
        }
        
        private string GetPenaltyDescription(PenaltyType penaltyType)
        {
            switch (penaltyType)
            {
                case PenaltyType.None:
                    return "";
                case PenaltyType.Exhaustion:
                    return GetMessage("status_exhausted", "Exhausted - Return to spawn quickly!");
                case PenaltyType.Fainted:
                    return GetMessage("status_fainted", "Fainted - You will be penalized");
                default:
                    return "";
            }
        }
        
        private void ShowExhaustionWarning(float secondsRemaining)
        {
            if (isWarningActive) return;
            
            isWarningActive = true;
            
            string message = GetMessage("exhaustion_warning", $"Exhaustion in {secondsRemaining:F0}s");
            ShowWarningMessage(message);
            
            if (warningAndProximityUI != null) { warningAndProximityUI.ShowWarning(message); }
            else if (warningIcon != null && showWarningIcon) { warningIcon.gameObject.SetActive(true); StartWarningBlink(); }
            
            if (audioManager != null)
            {
                audioManager.PlayWarningSound();
            }
            
            if (showDebugLogs)
                Debug.Log($"[DayNightUI] Exhaustion warning: {secondsRemaining:F1}s");
        }
        
        private void HideExhaustionWarning()
        {
            isWarningActive = false;
            if (warningAndProximityUI != null) { warningAndProximityUI.HideWarning(); }
            else
            {
                StopWarningBlink();
                if (warningIcon != null) { warningIcon.gameObject.SetActive(false); }
            }
            
            if (showDebugLogs)
                Debug.Log("[DayNightUI] Exhaustion warning hidden");
        }
        
        private void ShowProximityIndicator(bool show)
        {
            if (warningAndProximityUI != null) { if (show) warningAndProximityUI.ShowProximityIndicator(); else warningAndProximityUI.HideProximityIndicator(); return; }
            if (spawnProximityIcon != null && showSpawnProximity)
            {
                spawnProximityIcon.gameObject.SetActive(show);
                if (showDebugLogs) Debug.Log($"[DayNightUI] Proximity indicator {(show ? "shown" : "hidden")}");
            }
        }
        
        private void HideProximityIndicator()
        {
            ShowProximityIndicator(false);
        }
        
        private void HidePenaltyIndicator()
        {
            if (warningAndProximityUI != null) { warningAndProximityUI.HidePenaltyIndicator(); }
            else
            {
                if (warningIcon != null) { warningIcon.gameObject.SetActive(false); }
                StopWarningBlink();
            }
            
            if (showDebugLogs)
                Debug.Log("[DayNightUI] Penalty indicator hidden");
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
        }
        
        private System.Collections.IEnumerator WarningBlinkCoroutine()
        {
            if (warningIcon == null) yield break;
            
            while (isWarningActive)
            {
                warningIcon.enabled = !warningIcon.enabled;
                yield return new WaitForSeconds(warningBlinkRate);
            }
            
            warningIcon.enabled = true;
        }
        
        private System.Collections.IEnumerator ProximityUpdateCoroutine()
        {
            while (isProximityActive)
            {
                if (SpawnPoint.Current != null)
                {
                    float distance = SpawnPoint.Current.GetDistanceToPlayer();
                    OnDistanceChanged(distance);
                }
                
                yield return new WaitForSeconds(proximityUpdateRate);
            }
        }
        
        private void StartWarningBlink()
        {
            if (warningBlinkCoroutine != null)
            {
                StopCoroutine(warningBlinkCoroutine);
            }
            
            warningBlinkCoroutine = StartCoroutine(WarningBlinkCoroutine());
        }
        
        private void StopWarningBlink()
        {
            if (warningBlinkCoroutine != null)
            {
                StopCoroutine(warningBlinkCoroutine);
                warningBlinkCoroutine = null;
            }
        }
        
        private void StartProximityUpdate()
        {
            if (warningAndProximityUI != null) { /* handled by module */ return; }
            if (proximityUpdateCoroutine != null) { StopCoroutine(proximityUpdateCoroutine); }
            proximityUpdateCoroutine = StartCoroutine(ProximityUpdateCoroutine());
        }
        
        private void StopProximityUpdate()
        {
            if (warningAndProximityUI != null) { /* handled by module */ return; }
            if (proximityUpdateCoroutine != null) { StopCoroutine(proximityUpdateCoroutine); proximityUpdateCoroutine = null; }
        }
        
        private string GetMessage(string key, string defaultValue = "")
        {
            if (feedbackMessages != null)
            {
                return feedbackMessages.GetMessage(key);
            }
            
            return defaultValue;
        }
        
        public void ForceUpdateTimeDisplay()
        {
            if (dayNightManager != null)
            {
                UpdateTimeDisplay(dayNightManager.CurrentTimeNormalized);
            }
        }
        
        public void ShowCustomWarning(string message)
        {
            ShowWarningMessage(message);
            
            if (audioManager != null)
            {
                audioManager.PlayWarningSound();
            }
        }
        
        public void ShowProximityIndicator()
        {
            ShowProximityIndicator(true);
            StartProximityUpdate();
        }
    }
}

// ScriptRole: Manages UI display for day/night system including time, status, warnings, and proximity indicators
// RelatedScripts: DayNightManager, PlayerPenalty, SpawnPoint, AudioManager
// UsesSO: FeedbackMessagesSO
// ReceivesFrom: DayNightManager events, PlayerPenalty events, SpawnPoint events
// SendsTo: TextMeshProUGUI, Image (UI elements), AudioManager (warning sounds)
