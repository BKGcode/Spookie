using UnityEngine;
using TMPro;
using DayNightSystem.Core;


namespace DayNightSystem.UI.StatusDisplay
{
    public class StatusDisplayController : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI statusDisplay;
        [SerializeField] private FeedbackMessagesSO feedbackMessages;
        
        [Header("Settings")]
        [SerializeField] private bool showDetailedStatus = true;
        [SerializeField] private bool showPenaltyInfo = true;
        [SerializeField] private float statusUpdateRate = 0.5f;
        
        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;
        
        // References
        private DayNightStateController stateController;
        private PlayerPenalty playerPenalty;
        private Coroutine statusUpdateCoroutine;
        
        // Events
        public System.Action<string> OnStatusUpdated;
        
        private void Awake()
        {
            FindReferences();
            ValidateReferences();
            
            if (showDebugLogs)
                Debug.Log("[StatusDisplayController] Initialized");
        }
        
        private void OnEnable()
        {
            if (stateController != null)
            {
                stateController.OnStateChanged += OnStateChanged;
                stateController.OnDayStart += OnDayStart;
                stateController.OnExhaustionStarted += OnExhaustionStarted;
                stateController.OnPlayerSlept += OnPlayerSlept;
                stateController.OnPlayerFainted += OnPlayerFainted;
            }
            
            StartStatusUpdate();
        }
        
        private void OnDisable()
        {
            if (stateController != null)
            {
                stateController.OnStateChanged -= OnStateChanged;
                stateController.OnDayStart -= OnDayStart;
                stateController.OnExhaustionStarted -= OnExhaustionStarted;
                stateController.OnPlayerSlept -= OnPlayerSlept;
                stateController.OnPlayerFainted -= OnPlayerFainted;
            }
            
            StopStatusUpdate();
        }
        
        private void OnStateChanged(DayNightSystem.Core.DayNightState newState)
        {
            UpdateStatusDisplay();
        }
        
        private void OnDayStart()
        {
            UpdateStatusDisplay();
        }
        
        private void OnExhaustionStarted()
        {
            UpdateStatusDisplay();
        }
        
        private void OnPlayerSlept()
        {
            UpdateStatusDisplay();
        }
        
        private void OnPlayerFainted()
        {
            UpdateStatusDisplay();
        }
        
        private void StartStatusUpdate()
        {
            if (statusUpdateCoroutine != null)
            {
                StopCoroutine(statusUpdateCoroutine);
            }
            
            statusUpdateCoroutine = StartCoroutine(StatusUpdateCoroutine());
        }
        
        private void StopStatusUpdate()
        {
            if (statusUpdateCoroutine != null)
            {
                StopCoroutine(statusUpdateCoroutine);
                statusUpdateCoroutine = null;
            }
        }
        
        private System.Collections.IEnumerator StatusUpdateCoroutine()
        {
            while (true)
            {
                UpdateStatusDisplay();
                yield return new WaitForSeconds(statusUpdateRate);
            }
        }
        
        private void UpdateStatusDisplay()
        {
            if (statusDisplay == null) return;
            
            string status = GetCurrentStatus();
            statusDisplay.text = status;
            
            OnStatusUpdated?.Invoke(status);
            
            if (showDebugLogs)
                Debug.Log($"[StatusDisplayController] Status updated: {status}");
        }
        
        private string GetCurrentStatus()
        {
            if (stateController == null) return GetMessage("status_unknown");
            
            string baseStatus = GetStateStatus(stateController.CurrentState);
            string penaltyStatus = GetPenaltyStatus();
            
            if (showDetailedStatus && !string.IsNullOrEmpty(penaltyStatus))
            {
                return $"{baseStatus} - {penaltyStatus}";
            }
            
            return baseStatus;
        }
        
        private string GetStateStatus(DayNightSystem.Core.DayNightState state)
        {
            switch (state)
            {
                case DayNightSystem.Core.DayNightState.Day:
                    return GetMessage("status_day");
                case DayNightSystem.Core.DayNightState.Night:
                    return GetMessage("status_night");
                case DayNightSystem.Core.DayNightState.Transitioning:
                    return GetMessage("status_transitioning");
                case DayNightSystem.Core.DayNightState.Exhausted:
                    return GetMessage("status_exhausted");
                case DayNightSystem.Core.DayNightState.Sleeping:
                    return GetMessage("status_sleeping");
                case DayNightSystem.Core.DayNightState.Fainted:
                    return GetMessage("status_fainted");
                default:
                    return GetMessage("status_unknown");
            }
        }
        
        private string GetPenaltyStatus()
        {
            if (!showPenaltyInfo || playerPenalty == null) return "";
            
            if (playerPenalty.CurrentPenaltyType == PenaltyType.None) return "";
            
            return GetPenaltyDescription(playerPenalty.CurrentPenaltyType);
        }
        
        private string GetPenaltyDescription(PenaltyType penaltyType)
        {
            switch (penaltyType)
            {
                case PenaltyType.Exhaustion:
                    return GetMessage("penalty_exhaustion");
                case PenaltyType.Fainted:
                    return GetMessage("penalty_fainted");
                default:
                    return GetMessage("penalty_unknown");
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
        
        public void ForceUpdate()
        {
            UpdateStatusDisplay();
        }
        
        public void SetShowDetailedStatus(bool show)
        {
            showDetailedStatus = show;
            ForceUpdate();
        }
        
        public void SetShowPenaltyInfo(bool show)
        {
            showPenaltyInfo = show;
            ForceUpdate();
        }
        
        public void SetStatusUpdateRate(float rate)
        {
            statusUpdateRate = Mathf.Max(0.1f, rate);
            StartStatusUpdate();
        }
        
        public string GetCurrentStatusText()
        {
            return GetCurrentStatus();
        }
        
        public DayNightSystem.Core.DayNightState GetCurrentState()
        {
            return stateController != null ? stateController.CurrentState : DayNightSystem.Core.DayNightState.Day;
        }
        
        public bool HasActivePenalty()
        {
            return playerPenalty != null && playerPenalty.CurrentPenaltyType != PenaltyType.None;
        }
        
        private void FindReferences()
        {
            stateController = GetComponent<DayNightStateController>();
            if (stateController == null)
            {
                stateController = FindObjectOfType<DayNightStateController>();
            }
            
            playerPenalty = FindObjectOfType<PlayerPenalty>();
        }
        
        private void ValidateReferences()
        {
            if (statusDisplay == null)
            {
                Debug.LogError("[StatusDisplayController] StatusDisplay reference is missing!");
            }
            
            if (feedbackMessages == null)
            {
                Debug.LogWarning("[StatusDisplayController] FeedbackMessagesSO reference is missing!");
            }
            
            if (stateController == null)
            {
                Debug.LogError("[StatusDisplayController] DayNightStateController reference is missing!");
            }
        }
        
        [ContextMenu("Force Update Status")]
        public void ForceUpdateContext()
        {
            ForceUpdate();
        }
        
        [ContextMenu("Show Current Status")]
        public void ShowCurrentStatus()
        {
            Debug.Log($"[StatusDisplayController] Current Status: {GetCurrentStatusText()}, State: {GetCurrentState()}, Has Penalty: {HasActivePenalty()}");
        }
        
        [ContextMenu("Toggle Detailed Status")]
        public void ToggleDetailedStatus()
        {
            SetShowDetailedStatus(!showDetailedStatus);
        }
        
        [ContextMenu("Toggle Penalty Info")]
        public void TogglePenaltyInfo()
        {
            SetShowPenaltyInfo(!showPenaltyInfo);
        }
    }
}
