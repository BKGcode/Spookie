using UnityEngine;

namespace DayNightSystem.Core
{
    public enum DayNightState
    {
        Day,
        Night,
        Transitioning,
        Exhausted,
        Sleeping,
        Fainted
    }
    
    public class DayNightStateController : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private DayNightConfig config;
        
        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;
        
        // Private fields
        private DayNightState currentState = DayNightState.Day;
        private bool isTransitioning = false;
        private bool isInExhaustion = false;
        private bool hasCheckedSpawnAtNightfall = false;
        private bool hasStartedFaintingTimer = false;
        private float exhaustionTimer;
        private float faintingTimer;

        [Header("Exhaustion & Faint Settings")]
        [SerializeField] private float faintCountdownSeconds = 30f;
        
        // Public properties
        public DayNightState CurrentState => currentState;
        public bool IsDay => currentState == DayNightState.Day;
        public bool IsTransitioning => isTransitioning;
        public bool IsInExhaustion => isInExhaustion;
        public bool PlayerSleptCorrectly { get; private set; }
        
        // Events
        public System.Action<DayNightState> OnStateChanged;
        public System.Action OnDayStart;
        public System.Action OnNightStart;
        public System.Action<float> OnExhaustionWarning;
        public System.Action OnPlayerSlept;
        public System.Action OnPlayerFainted;
        public System.Action OnExhaustionStarted;
        public System.Action OnNightBlocked;
        
        // References
        private DayNightTimeController timeController;
        private MessageSystem messageSystem;
        [SerializeField] private DayNightSystem.Validation.DayNightValidationController validationController;
        
        private void Awake()
        {
            FindReferences();
            ValidateReferences();
            
            if (showDebugLogs)
                Debug.Log($"[DayNightStateController] Initialized - Current State: {currentState}");
        }
        
        private void OnEnable()
        {
            if (timeController != null)
            {
                timeController.OnDayTimeReached += OnDayTimeReached;
                timeController.OnNightTimeReached += OnNightTimeReached;
            }
            if (validationController == null)
            {
                validationController = FindObjectOfType<DayNightSystem.Validation.DayNightValidationController>();
            }
            if (validationController != null)
            {
                validationController.OnPlayerAtSpawnChanged += OnPlayerAtSpawnChanged;
            }
        }
        
        private void OnDisable()
        {
            if (timeController != null)
            {
                timeController.OnDayTimeReached -= OnDayTimeReached;
                timeController.OnNightTimeReached -= OnNightTimeReached;
            }
            if (validationController != null)
            {
                validationController.OnPlayerAtSpawnChanged -= OnPlayerAtSpawnChanged;
            }
        }
        
        private void Update()
        {
            if (isTransitioning || currentState == DayNightState.Transitioning) return;
            
            // Check if message system is active before updating timers
            if (messageSystem != null && messageSystem.IsMessageActive)
            {
                return; // Don't update timers while message is active
            }
            
            // Check for exhaustion
            CheckExhaustion();
            
            // Check for fainting when not at spawn
            CheckFainting();
        }
        
        private void CheckExhaustion()
        {
            if (config == null) return;
            
            if (currentState == DayNightState.Night && !isInExhaustion)
            {
                exhaustionTimer += Time.deltaTime;
                
                // Warn when approaching exhaustion
                float warningTime = config.ExhaustionTimeSeconds * 0.8f;
                if (exhaustionTimer >= warningTime && exhaustionTimer < warningTime + Time.deltaTime)
                {
                    OnExhaustionWarning?.Invoke(exhaustionTimer);
                    
                    if (showDebugLogs)
                        Debug.Log($"[DayNightStateController] Exhaustion warning at {exhaustionTimer:F1}s");
                }
                
                // Start exhaustion
                if (exhaustionTimer >= config.ExhaustionTimeSeconds)
                {
                    StartExhaustion();
                }
            }
        }
        
        private void CheckFainting()
        {
            if (config == null) return;
            
            if (currentState == DayNightState.Exhausted && !hasStartedFaintingTimer)
            {
                hasStartedFaintingTimer = true;
                faintingTimer = 0f;
                
                if (showDebugLogs)
                    Debug.Log("[DayNightStateController] Started fainting timer");
            }
            
            if (hasStartedFaintingTimer && currentState == DayNightState.Exhausted)
            {
                faintingTimer += Time.deltaTime;
                float total = Mathf.Max(1f, faintCountdownSeconds);
                float remaining = Mathf.Max(0f, total - faintingTimer);
                OnExhaustionWarning?.Invoke(remaining);
                if (validationController != null && validationController.IsPlayerAtSpawn())
                {
                    StartSleeping();
                    return;
                }
                if (faintingTimer >= total)
                {
                    StartFainting();
                }
            }
        }

        private void OnPlayerAtSpawnChanged(bool isAtSpawn)
        {
            if (isAtSpawn && currentState == DayNightState.Exhausted)
            {
                StartSleeping();
            }
        }
        
        private void OnDayTimeReached()
        {
            if (currentState != DayNightState.Day)
            {
                SetState(DayNightState.Day);
                OnDayStart?.Invoke();
                
                if (showDebugLogs)
                    Debug.Log("[DayNightStateController] Day started");
            }
        }
        
        private void OnNightTimeReached()
        {
            if (currentState != DayNightState.Night)
            {
                bool atSpawn = validationController != null && validationController.IsPlayerAtSpawn();
                if (atSpawn)
                {
                    StartSleeping();
                }
                else
                {
                    SetState(DayNightState.Night);
                    OnNightStart?.Invoke();
                    StartExhaustion();
                    if (showDebugLogs)
                        Debug.Log("[DayNightStateController] Night started - exhaustion started");
                    return;
                }
                if (showDebugLogs)
                    Debug.Log("[DayNightStateController] Night started - sleeping at spawn");
            }
        }
        
        private void StartExhaustion()
        {
            if (isInExhaustion) return;
            
            isInExhaustion = true;
            SetState(DayNightState.Exhausted);
            OnExhaustionStarted?.Invoke();
            
            if (showDebugLogs)
                Debug.Log("[DayNightStateController] Player is now exhausted");
        }
        
        private void StartSleeping()
        {
            isInExhaustion = false;
            hasStartedFaintingTimer = false;
            faintingTimer = 0f;
            exhaustionTimer = 0f;
            SetState(DayNightState.Sleeping);
            PlayerSleptCorrectly = true;
            OnPlayerSlept?.Invoke();
            
            if (showDebugLogs)
                Debug.Log("[DayNightStateController] Player started sleeping");
        }
        
        private void StartFainting()
        {
            SetState(DayNightState.Fainted);
            PlayerSleptCorrectly = false;
            OnPlayerFainted?.Invoke();
            
            if (showDebugLogs)
                Debug.Log("[DayNightStateController] Player fainted");
        }
        
        public void SetState(DayNightState newState)
        {
            if (currentState == newState) return;
            
            DayNightState previousState = currentState;
            currentState = newState;
            
            OnStateChanged?.Invoke(newState);
            
            if (showDebugLogs)
                Debug.Log($"[DayNightStateController] State changed from {previousState} to {newState}");
        }
        
        public void SetTransitioning(bool transitioning)
        {
            isTransitioning = transitioning;
            
            if (transitioning)
            {
                SetState(DayNightState.Transitioning);
            }
            
            if (showDebugLogs)
                Debug.Log($"[DayNightStateController] Transitioning set to: {transitioning}");
        }
        
        public void ForceDay()
        {
            SetState(DayNightState.Day);
            ResetExhaustion();
            
            if (showDebugLogs)
                Debug.Log("[DayNightStateController] Forced to day state");
        }
        
        public void ForceNight()
        {
            SetState(DayNightState.Night);
            ResetExhaustion();
            
            if (showDebugLogs)
                Debug.Log("[DayNightStateController] Forced to night state");
        }
        
        public void ForceExhaustion()
        {
            StartExhaustion();
        }
        
        public void ForceSleep()
        {
            StartSleeping();
        }
        
        public void ForceFaint()
        {
            StartFainting();
        }
        
        public void ResetExhaustion()
        {
            isInExhaustion = false;
            exhaustionTimer = 0f;
            hasStartedFaintingTimer = false;
            faintingTimer = 0f;
            
            if (showDebugLogs)
                Debug.Log("[DayNightStateController] Exhaustion reset");
        }
        
        public bool IsPlayerExhausted()
        {
            return isInExhaustion;
        }
        
        public bool DidPlayerSleepCorrectly()
        {
            return PlayerSleptCorrectly;
        }
        
        public float GetFaintingTimer()
        {
            return faintingTimer;
        }
        
        public bool IsFaintingTimerActive()
        {
            return hasStartedFaintingTimer && currentState == DayNightState.Exhausted;
        }
        
        public bool HasStartedFaintingTimer()
        {
            return hasStartedFaintingTimer;
        }
        
        private void FindReferences()
        {
            timeController = GetComponent<DayNightTimeController>();
            messageSystem = FindObjectOfType<MessageSystem>();
            if (validationController == null)
            {
                validationController = FindObjectOfType<DayNightSystem.Validation.DayNightValidationController>();
            }
        }
        
        private void ValidateReferences()
        {
            if (config == null)
            {
                Debug.LogError("[DayNightStateController] DayNightConfig reference is missing!");
            }
            
            if (timeController == null)
            {
                Debug.LogError("[DayNightStateController] DayNightTimeController reference is missing!");
            }
        }
        
        [ContextMenu("Force Day State")]
        public void ForceDayContext()
        {
            ForceDay();
        }
        
        [ContextMenu("Force Night State")]
        public void ForceNightContext()
        {
            ForceNight();
        }
        
        [ContextMenu("Force Exhaustion")]
        public void ForceExhaustionContext()
        {
            ForceExhaustion();
        }
        
        [ContextMenu("Show Current State")]
        public void ShowCurrentState()
        {
            Debug.Log($"[DayNightStateController] Current State: {currentState}, Exhausted: {isInExhaustion}, Timer: {exhaustionTimer:F1}s");
        }
    }
}
