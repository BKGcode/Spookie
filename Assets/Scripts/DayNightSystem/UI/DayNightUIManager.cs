using UnityEngine;
using DayNightSystem.UI.TimeDisplay;
using DayNightSystem.UI.StatusDisplay;
using DayNightSystem.UI.WarningSystem;
using DayNightSystem.UI.ProximitySystem;
using DayNightSystem;

namespace DayNightSystem.UI
{
    public class DayNightUIManager : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private FeedbackMessagesSO feedbackMessages;
        [SerializeField] private AudioManager audioManager;
        
        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;
        
        // UI Components
        private TimeDisplayController timeDisplayController;
        private StatusDisplayController statusDisplayController;
        private WarningSystemController warningSystemController;
        private ProximitySystemController proximitySystemController;
        
        // Events
        public System.Action<string> OnTimeDisplayUpdated;
        public System.Action<string> OnStatusUpdated;
        public System.Action<bool> OnWarningStateChanged;
        public System.Action<bool> OnProximityStateChanged;
        
        private void Awake()
        {
            InitializeComponents();
            ValidateReferences();
            SetupEventDelegation();
            
            if (showDebugLogs)
                Debug.Log("[DayNightUIManager] Initialized with modular UI components");
        }
        
        private void InitializeComponents()
        {
            // Get or add required components
            timeDisplayController = GetComponent<TimeDisplayController>();
            if (timeDisplayController == null)
            {
                timeDisplayController = gameObject.AddComponent<TimeDisplayController>();
            }
            
            statusDisplayController = GetComponent<StatusDisplayController>();
            if (statusDisplayController == null)
            {
                statusDisplayController = gameObject.AddComponent<StatusDisplayController>();
            }
            
            warningSystemController = GetComponent<WarningSystemController>();
            if (warningSystemController == null)
            {
                warningSystemController = gameObject.AddComponent<WarningSystemController>();
            }
            
            proximitySystemController = GetComponent<ProximitySystemController>();
            if (proximitySystemController == null)
            {
                proximitySystemController = gameObject.AddComponent<ProximitySystemController>();
            }
        }
        
        private void SetupEventDelegation()
        {
            // Delegate events from sub-components
            if (timeDisplayController != null)
            {
                OnTimeDisplayUpdated = timeDisplayController.OnTimeDisplayUpdated;
            }
            
            if (statusDisplayController != null)
            {
                OnStatusUpdated = statusDisplayController.OnStatusUpdated;
            }
            
            if (warningSystemController != null)
            {
                OnWarningStateChanged = warningSystemController.OnWarningStateChanged;
            }
            
            if (proximitySystemController != null)
            {
                OnProximityStateChanged = proximitySystemController.OnProximityStateChanged;
            }
        }
        
        private void ValidateReferences()
        {
            if (feedbackMessages == null)
            {
                Debug.LogWarning("[DayNightUIManager] FeedbackMessagesSO reference is missing!");
            }
            
            if (audioManager == null)
            {
                Debug.LogWarning("[DayNightUIManager] AudioManager reference is missing!");
            }
            
            if (timeDisplayController == null)
            {
                Debug.LogError("[DayNightUIManager] TimeDisplayController reference is missing!");
            }
            
            if (statusDisplayController == null)
            {
                Debug.LogError("[DayNightUIManager] StatusDisplayController reference is missing!");
            }
            
            if (warningSystemController == null)
            {
                Debug.LogError("[DayNightUIManager] WarningSystemController reference is missing!");
            }
            
            if (proximitySystemController == null)
            {
                Debug.LogError("[DayNightUIManager] ProximitySystemController reference is missing!");
            }
        }
        
        // Public methods (delegated to appropriate components)
        public void ForceUpdateTimeDisplay()
        {
            if (timeDisplayController != null)
            {
                timeDisplayController.ForceUpdate();
            }
        }
        
        public void ForceUpdateStatusDisplay()
        {
            if (statusDisplayController != null)
            {
                statusDisplayController.ForceUpdate();
            }
        }
        
        public void ForceWarning(string message)
        {
            if (warningSystemController != null)
            {
                warningSystemController.ForceWarning(message);
            }
        }
        
        public void StopCurrentWarning()
        {
            if (warningSystemController != null)
            {
                warningSystemController.StopCurrentWarning();
            }
        }
        
        public void ForceProximityCheck()
        {
            if (proximitySystemController != null)
            {
                proximitySystemController.ForceProximityCheck();
            }
        }
        
        // Component access methods
        public TimeDisplayController GetTimeDisplayController()
        {
            return timeDisplayController;
        }
        
        public StatusDisplayController GetStatusDisplayController()
        {
            return statusDisplayController;
        }
        
        public WarningSystemController GetWarningSystemController()
        {
            return warningSystemController;
        }
        
        public ProximitySystemController GetProximitySystemController()
        {
            return proximitySystemController;
        }
        
        // Status methods
        public string GetCurrentTimeString()
        {
            return timeDisplayController != null ? timeDisplayController.GetCurrentTimeString() : "00:00";
        }
        
        public string GetCurrentStatusText()
        {
            return statusDisplayController != null ? statusDisplayController.GetCurrentStatusText() : "";
        }
        
        public bool IsWarningActive()
        {
            return warningSystemController != null ? warningSystemController.IsWarningActive() : false;
        }
        
        public bool IsProximityActive()
        {
            return proximitySystemController != null ? proximitySystemController.IsProximityActive() : false;
        }
        
        // Configuration methods
        public void SetTimeFormat(string format)
        {
            if (timeDisplayController != null)
            {
                timeDisplayController.SetTimeFormat(format);
            }
        }
        
        public void SetShowSeconds(bool show)
        {
            if (timeDisplayController != null)
            {
                timeDisplayController.SetShowSeconds(show);
            }
        }
        
        public void SetShowDetailedStatus(bool show)
        {
            if (statusDisplayController != null)
            {
                statusDisplayController.SetShowDetailedStatus(show);
            }
        }
        
        public void SetShowWarningIcon(bool show)
        {
            if (warningSystemController != null)
            {
                warningSystemController.SetShowWarningIcon(show);
            }
        }
        
        public void SetShowSpawnProximity(bool show)
        {
            if (proximitySystemController != null)
            {
                proximitySystemController.SetShowSpawnProximity(show);
            }
        }
        
        [ContextMenu("Force Update All UI")]
        public void ForceUpdateAllUI()
        {
            ForceUpdateTimeDisplay();
            ForceUpdateStatusDisplay();
            ForceProximityCheck();
        }
        
        [ContextMenu("Show UI Status")]
        public void ShowUIStatus()
        {
            Debug.Log($"[DayNightUIManager] UI Status:\n" +
                     $"Time: {GetCurrentTimeString()}\n" +
                     $"Status: {GetCurrentStatusText()}\n" +
                     $"Warning Active: {IsWarningActive()}\n" +
                     $"Proximity Active: {IsProximityActive()}");
        }
        
        [ContextMenu("Test Warning")]
        public void TestWarning()
        {
            ForceWarning("Test warning from UI Manager");
        }
        
        [ContextMenu("Stop Warning")]
        public void StopWarning()
        {
            StopCurrentWarning();
        }
        
        [ContextMenu("Validate UI Components")]
        public void ValidateUIComponents()
        {
            bool allComponentsPresent = timeDisplayController != null && 
                                     statusDisplayController != null && 
                                     warningSystemController != null && 
                                     proximitySystemController != null;
            
            Debug.Log($"[DayNightUIManager] UI Components Validation:\n" +
                     $"All Components Present: {allComponentsPresent}\n" +
                     $"TimeDisplayController: {timeDisplayController != null}\n" +
                     $"StatusDisplayController: {statusDisplayController != null}\n" +
                     $"WarningSystemController: {warningSystemController != null}\n" +
                     $"ProximitySystemController: {proximitySystemController != null}");
        }
    }
}
