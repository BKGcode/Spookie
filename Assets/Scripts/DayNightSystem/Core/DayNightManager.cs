using UnityEngine;
using DayNightSystem.Validation;
using DayNightSystem.Persistence;

namespace DayNightSystem.Core
{
    public class DayNightManager : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private DayNightConfig config;
        
        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;
        
        // Core components
        private DayNightTimeController timeController;
        private DayNightStateController stateController;
        private DayNightEventController eventController;
        private DayNightValidationController validationController;
        private DayNightPersistenceController persistenceController;
        
        // Public properties (delegated to components)
        public float CurrentTimeNormalized => timeController != null ? timeController.CurrentTimeNormalized : 0f;
        public bool IsDay => stateController != null ? stateController.IsDay : true;
        public bool IsTransitioning => stateController != null ? stateController.IsTransitioning : false;
        public bool IsPaused => timeController != null ? timeController.IsPaused : false;
        public DayNightState CurrentState => stateController != null ? stateController.CurrentState : DayNightState.Day;
        public bool PlayerSleptCorrectly => stateController != null ? stateController.DidPlayerSleepCorrectly() : false;
        public DayNightConfig Config => config;
        
        // Events (delegated from event controller)
        public System.Action OnDayStart;
        public System.Action OnNightStart;
        public System.Action<float> OnExhaustionWarning;
        public System.Action<float> OnTimeChanged;
        public System.Action OnPlayerSlept;
        public System.Action OnPlayerFainted;
        public System.Action OnExhaustionStarted;
        public System.Action<DayNightState> OnStateChanged;
        public System.Action OnNightBlocked;
        
        private void Awake()
        {
            InitializeComponents();
            ValidateReferences();
            SetupEventDelegation();
            
            if (showDebugLogs)
                Debug.Log("[DayNightManager] Initialized with modular components");
        }
        
        private void Start()
        {
            // Load saved state if available
            if (persistenceController != null)
            {
                persistenceController.LoadState();
            }
        }
        
        private void InitializeComponents()
        {
            // Get or add required components
            timeController = GetComponent<DayNightTimeController>();
            if (timeController == null)
            {
                timeController = gameObject.AddComponent<DayNightTimeController>();
            }
            
            stateController = GetComponent<DayNightStateController>();
            if (stateController == null)
            {
                stateController = gameObject.AddComponent<DayNightStateController>();
            }
            
            eventController = GetComponent<DayNightEventController>();
            if (eventController == null)
            {
                eventController = gameObject.AddComponent<DayNightEventController>();
            }
            
            validationController = GetComponent<DayNightValidationController>();
            if (validationController == null)
            {
                validationController = gameObject.AddComponent<DayNightValidationController>();
            }
            
            persistenceController = GetComponent<DayNightPersistenceController>();
            if (persistenceController == null)
            {
                persistenceController = gameObject.AddComponent<DayNightPersistenceController>();
            }
        }
        
        private void SetupEventDelegation()
        {
            if (eventController != null)
            {
                OnDayStart = eventController.OnDayStart;
                OnNightStart = eventController.OnNightStart;
                OnExhaustionWarning = eventController.OnExhaustionWarning;
                OnTimeChanged = eventController.OnTimeChanged;
                OnPlayerSlept = eventController.OnPlayerSlept;
                OnPlayerFainted = eventController.OnPlayerFainted;
                OnExhaustionStarted = eventController.OnExhaustionStarted;
                OnStateChanged = eventController.OnStateChanged;
                OnNightBlocked = eventController.OnNightBlocked;
            }
        }
        
        private void ValidateReferences()
        {
            if (config == null)
            {
                Debug.LogError("[DayNightManager] DayNightConfig reference is missing!");
            }
            
            if (timeController == null)
            {
                Debug.LogError("[DayNightManager] DayNightTimeController component is missing!");
            }
            
            if (stateController == null)
            {
                Debug.LogError("[DayNightManager] DayNightStateController component is missing!");
            }
            
            if (eventController == null)
            {
                Debug.LogError("[DayNightManager] DayNightEventController component is missing!");
            }
            
            if (validationController == null)
            {
                Debug.LogError("[DayNightManager] DayNightValidationController component is missing!");
            }
            
            if (persistenceController == null)
            {
                Debug.LogError("[DayNightManager] DayNightPersistenceController component is missing!");
            }
        }
        
        // Public methods (delegated to appropriate components)
        public void ForceDay()
        {
            if (stateController != null)
            {
                stateController.ForceDay();
            }
        }
        
        public void ForceNight()
        {
            if (stateController != null)
            {
                stateController.ForceNight();
            }
        }
        
        public void PauseTime(bool pause)
        {
            if (timeController != null)
            {
                timeController.PauseTime(pause);
            }
        }
        
        public void SetTransitioning(bool transitioning)
        {
            if (stateController != null)
            {
                stateController.SetTransitioning(transitioning);
            }
        }
        
        public void AddTime(float minutes)
        {
            if (timeController != null)
            {
                timeController.AddTime(minutes);
            }
        }
        
        public void ForceFaint()
        {
            if (stateController != null)
            {
                stateController.ForceFaint();
            }
        }
        
        public void ForceSleep()
        {
            if (stateController != null)
            {
                stateController.ForceSleep();
            }
        }
        
        public bool IsPlayerExhausted()
        {
            return stateController != null ? stateController.IsPlayerExhausted() : false;
        }
        
        public bool DidPlayerSleepCorrectly()
        {
            return stateController != null ? stateController.DidPlayerSleepCorrectly() : false;
        }
        
        public float GetFaintingTimer()
        {
            return stateController != null ? stateController.GetFaintingTimer() : 0f;
        }
        
        public bool IsFaintingTimerActive()
        {
            return stateController != null ? stateController.IsFaintingTimerActive() : false;
        }
        
        public bool HasStartedFaintingTimer()
        {
            return stateController != null ? stateController.HasStartedFaintingTimer() : false;
        }
        
        public bool IsPlayerAtSpawn()
        {
            return validationController != null ? validationController.IsPlayerAtSpawn() : false;
        }
        
        public float GetDistanceToSpawn()
        {
            return validationController != null ? validationController.GetDistanceToSpawn() : float.MaxValue;
        }
        
        public void SaveState()
        {
            if (persistenceController != null)
            {
                persistenceController.SaveState();
            }
        }
        
        public void LoadState()
        {
            if (persistenceController != null)
            {
                persistenceController.LoadState();
            }
        }
        
        public void ClearSaveData()
        {
            if (persistenceController != null)
            {
                persistenceController.ClearSaveData();
            }
        }
        
        public bool HasSaveData()
        {
            return persistenceController != null ? persistenceController.HasSaveData() : false;
        }
        
        // Component access methods
        public DayNightTimeController GetTimeController()
        {
            return timeController;
        }
        
        public DayNightStateController GetStateController()
        {
            return stateController;
        }
        
        public DayNightEventController GetEventController()
        {
            return eventController;
        }
        
        public DayNightValidationController GetValidationController()
        {
            return validationController;
        }
        
        public DayNightPersistenceController GetPersistenceController()
        {
            return persistenceController;
        }
        
        [ContextMenu("Test Save State")]
        public void TestSaveState()
        {
            SaveState();
        }
        
        [ContextMenu("Test Load State")]
        public void TestLoadState()
        {
            LoadState();
        }
        
        [ContextMenu("Show Save Data Summary")]
        public void ShowSaveDataSummary()
        {
            if (persistenceController != null)
            {
                persistenceController.ShowSaveDataSummary();
            }
        }
        
        [ContextMenu("Validate Save Data")]
        public void ValidateSaveData()
        {
            if (persistenceController != null)
            {
                persistenceController.ValidateSaveData();
            }
        }
        
        [ContextMenu("Show Component Status")]
        public void ShowComponentStatus()
        {
            Debug.Log($"[DayNightManager] Component Status:\n" +
                     $"TimeController: {timeController != null}\n" +
                     $"StateController: {stateController != null}\n" +
                     $"EventController: {eventController != null}\n" +
                     $"ValidationController: {validationController != null}\n" +
                     $"PersistenceController: {persistenceController != null}");
        }
    }
}
