using UnityEngine;
using UnityEngine.UI;
using DayNightSystem.Core;
using DayNightSystem;

namespace DayNightSystem.UI.WarningSystem
{
    public class WarningSystemController : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Image warningIcon;
        [SerializeField] private FeedbackMessagesSO feedbackMessages;
        
        [Header("Audio")]
        [SerializeField] private AudioManager audioManager;
        
        [Header("Settings")]
        [SerializeField] private float warningBlinkRate = 0.5f;
        [SerializeField] private bool showWarningIcon = true;
        [SerializeField] private bool playWarningSounds = true;
        
        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;
        
        // References
        private DayNightStateController stateController;
        private Coroutine warningBlinkCoroutine;
        
        // State
        private bool isWarningActive = false;
        private bool isIconVisible = false;
        
        // Events
        public System.Action<bool> OnWarningStateChanged;
        public System.Action OnWarningStarted;
        public System.Action OnWarningStopped;
        
        private void Awake()
        {
            FindReferences();
            ValidateReferences();
            
            if (showDebugLogs)
                Debug.Log("[WarningSystemController] Initialized");
        }
        
        private void OnEnable()
        {
            if (stateController != null)
            {
                stateController.OnExhaustionWarning += OnExhaustionWarning;
                stateController.OnExhaustionStarted += OnExhaustionStarted;
                stateController.OnPlayerFainted += OnPlayerFainted;
                stateController.OnNightBlocked += OnNightBlocked;
            }
        }
        
        private void OnDisable()
        {
            if (stateController != null)
            {
                stateController.OnExhaustionWarning -= OnExhaustionWarning;
                stateController.OnExhaustionStarted -= OnExhaustionStarted;
                stateController.OnPlayerFainted -= OnPlayerFainted;
                stateController.OnNightBlocked -= OnNightBlocked;
            }
            
            StopWarningBlink();
        }
        
        private void OnExhaustionWarning(float secondsRemaining)
        {
            ShowExhaustionWarning(secondsRemaining);
        }
        
        private void OnExhaustionStarted()
        {
            ShowWarning(GetMessage("warning_exhaustion_started"));
        }
        
        private void OnPlayerFainted()
        {
            ShowWarning(GetMessage("warning_fainted"));
        }
        
        private void OnNightBlocked()
        {
            ShowWarning(GetMessage("warning_night_blocked"));
        }
        
        private void ShowExhaustionWarning(float secondsRemaining)
        {
            string message = GetMessage("warning_exhaustion", $"Exhaustion in {secondsRemaining:F0}s");
            ShowWarning(message);
            
            if (showDebugLogs)
                Debug.Log($"[WarningSystemController] Exhaustion warning: {secondsRemaining:F1}s remaining");
        }
        
        private void ShowWarning(string message)
        {
            if (!isWarningActive)
            {
                StartWarning();
            }
            
            ShowWarningMessage(message);
            
            if (showDebugLogs)
                Debug.Log($"[WarningSystemController] Warning shown: {message}");
        }
        
        private void StartWarning()
        {
            isWarningActive = true;
            OnWarningStarted?.Invoke();
            OnWarningStateChanged?.Invoke(true);
            
            if (showWarningIcon && warningIcon != null)
            {
                StartWarningBlink();
            }
            
            if (playWarningSounds && audioManager != null)
            {
                audioManager.PlayWarningSound();
            }
            
            if (showDebugLogs)
                Debug.Log("[WarningSystemController] Warning started");
        }
        
        private void StopWarning()
        {
            if (!isWarningActive) return;
            
            isWarningActive = false;
            OnWarningStopped?.Invoke();
            OnWarningStateChanged?.Invoke(false);
            
            StopWarningBlink();
            HideWarningIcon();
            
            if (showDebugLogs)
                Debug.Log("[WarningSystemController] Warning stopped");
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
        
        private System.Collections.IEnumerator WarningBlinkCoroutine()
        {
            while (isWarningActive)
            {
                isIconVisible = !isIconVisible;
                SetWarningIconVisibility(isIconVisible);
                yield return new WaitForSeconds(warningBlinkRate);
            }
        }
        
        private void SetWarningIconVisibility(bool visible)
        {
            if (warningIcon != null)
            {
                warningIcon.gameObject.SetActive(visible);
            }
        }
        
        private void ShowWarningIcon()
        {
            SetWarningIconVisibility(true);
            isIconVisible = true;
        }
        
        private void HideWarningIcon()
        {
            SetWarningIconVisibility(false);
            isIconVisible = false;
        }
        
        private void ShowWarningMessage(string message)
        {
            // This would typically show a message in the UI
            // For now, we'll just log it
            if (showDebugLogs)
                Debug.Log($"[WarningSystemController] Warning Message: {message}");
        }
        
        private string GetMessage(string key, string defaultValue = "")
        {
            if (feedbackMessages != null)
            {
                return feedbackMessages.GetMessage(key);
            }
            
            return defaultValue;
        }
        
        public void ForceWarning(string message)
        {
            ShowWarning(message);
        }
        
        public void StopCurrentWarning()
        {
            StopWarning();
        }
        
        public void SetWarningBlinkRate(float rate)
        {
            warningBlinkRate = Mathf.Max(0.1f, rate);
        }
        
        public void SetShowWarningIcon(bool show)
        {
            showWarningIcon = show;
            if (!show)
            {
                HideWarningIcon();
            }
        }
        
        public void SetPlayWarningSounds(bool play)
        {
            playWarningSounds = play;
        }
        
        public bool IsWarningActive()
        {
            return isWarningActive;
        }
        
        public bool IsIconVisible()
        {
            return isIconVisible;
        }
        
        private void FindReferences()
        {
            stateController = GetComponent<DayNightStateController>();
            if (stateController == null)
            {
                stateController = FindObjectOfType<DayNightStateController>();
            }
            
            audioManager = FindObjectOfType<AudioManager>();
        }
        
        private void ValidateReferences()
        {
            if (warningIcon == null)
            {
                Debug.LogWarning("[WarningSystemController] WarningIcon reference is missing!");
            }
            
            if (feedbackMessages == null)
            {
                Debug.LogWarning("[WarningSystemController] FeedbackMessagesSO reference is missing!");
            }
            
            if (stateController == null)
            {
                Debug.LogError("[WarningSystemController] DayNightStateController reference is missing!");
            }
        }
        
        [ContextMenu("Test Warning")]
        public void TestWarning()
        {
            ForceWarning("Test warning message");
        }
        
        [ContextMenu("Stop Warning")]
        public void StopWarningContext()
        {
            StopCurrentWarning();
        }
        
        [ContextMenu("Toggle Warning Icon")]
        public void ToggleWarningIcon()
        {
            SetShowWarningIcon(!showWarningIcon);
        }
        
        [ContextMenu("Show Warning Status")]
        public void ShowWarningStatus()
        {
            Debug.Log($"[WarningSystemController] Warning Status - Active: {IsWarningActive()}, Icon Visible: {IsIconVisible()}, Show Icon: {showWarningIcon}");
        }
    }
}
