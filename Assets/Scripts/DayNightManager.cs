using UnityEngine;

namespace DayNightSystem
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
    
    public class DayNightManager : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private DayNightConfig config;
        
        // Public property to access config
        public DayNightConfig Config => config;
        
        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;
        
        // Public properties
        public float CurrentTimeNormalized => config != null ? currentTime / config.DayDurationSeconds : 0f;
        public bool IsDay => currentState == DayNightState.Day;
        public bool IsTransitioning => isTransitioning;
        public bool IsPaused => isPaused;
        public DayNightState CurrentState => currentState;
        public bool PlayerSleptCorrectly { get; private set; }
        
        // Events
        public System.Action OnDayStart;
        public System.Action OnNightStart;
        public System.Action<float> OnExhaustionWarning;
        public System.Action<float> OnTimeChanged;
        public System.Action OnPlayerSlept;
        public System.Action OnPlayerFainted;
        public System.Action OnExhaustionStarted;
        public System.Action<DayNightState> OnStateChanged; // New: Notify state changes
        public System.Action OnNightBlocked; // New: Notify when night is blocked
        
        // Private fields
        private float currentTime;
        private float exhaustionTimer;
        private float faintingTimer; // New: Timer for fainting when not at spawn
        private DayNightState currentState = DayNightState.Day;
        private bool isPaused = false;
        private bool isInExhaustion = false;
        private bool hasCheckedSpawnAtNightfall = false;
        private bool hasStartedFaintingTimer = false; // New: Track if fainting timer started
        private bool isTransitioning = false; // New: Track transition state
        private MessageSystem messageSystem;
        private TransitionHandler transitionHandler; // New: Reference to transition handler
        
        private void Awake()
        {
            ValidateReferences();
            ValidateSpawnPoint();
            FindMessageSystem();
            FindTransitionHandler();
            LoadState();
            
            if (showDebugLogs)
                Debug.Log($"[DayNightManager] Initialized - Current Time: {currentTime:F1}s, State: {currentState}");
        }
        
        private void Update()
        {
            if (isPaused || currentState == DayNightState.Transitioning) return;
            
            // Check if message system is active before updating timers
            if (messageSystem != null && messageSystem.IsMessageActive)
            {
                return; // Don't update timers while message is active
            }
            
            // Validate config is available
            if (config == null)
            {
                Debug.LogError("[DayNightManager] Config is null! Cannot update time.");
                return;
            }
            
            // Clamp delta time to prevent huge jumps
            float deltaTime = Mathf.Clamp(Time.deltaTime, 0f, 0.1f);
            currentTime += deltaTime;
            
            // Debug log every 10 seconds
            if (Mathf.FloorToInt(currentTime) % 10 == 0 && Mathf.FloorToInt(currentTime) != Mathf.FloorToInt(currentTime - deltaTime))
            {
                Debug.Log($"[DayNightManager] Current Time: {currentTime:F1}s / {config.DayDurationSeconds:F1}s ({(currentTime/config.DayDurationSeconds)*100:F1}%) - State: {currentState}");
            }
            
            // Check for day/night transition
            CheckDayNightTransition();
            
            // Check for exhaustion
            CheckExhaustion();
            
            // Trigger time change event every second
            if (Mathf.FloorToInt(currentTime) != Mathf.FloorToInt(currentTime - deltaTime))
            {
                OnTimeChanged?.Invoke(CurrentTimeNormalized);
            }
        }
        
        private void CheckDayNightTransition()
        {
            if (config == null) return;
            
            // Check if time threshold reached
            if (currentTime >= config.DayDurationSeconds)
            {
                if (currentState == DayNightState.Day)
                {
                    // Transition to night
                    if (showDebugLogs)
                        Debug.Log($"[DayNightManager] Time threshold reached! Current: {currentTime:F1}s, Threshold: {config.DayDurationSeconds:F1}s");
                    
                    StartNightTransition();
                }
                else if (currentState == DayNightState.Night)
                {
                    // Transition to day
                    if (showDebugLogs)
                        Debug.Log($"[DayNightManager] Night time completed! Current: {currentTime:F1}s, Threshold: {config.DayDurationSeconds:F1}s");
                    
                    StartDayTransition();
                }
            }
        }
        
        private void CheckExhaustion()
        {
            if (currentState != DayNightState.Night) return;
            
            // Check if player is at spawn when night falls
            if (!hasCheckedSpawnAtNightfall)
            {
                CheckPlayerAtSpawnWhenNightFalls();
            }
            
            // If player is not at spawn, start fainting timer
            if (!PlayerSleptCorrectly && !hasStartedFaintingTimer)
            {
                hasStartedFaintingTimer = true;
                faintingTimer = config.ExhaustionTimeSeconds;
                
                if (showDebugLogs)
                    Debug.Log($"[DayNightManager] Player not at spawn - starting fainting timer: {faintingTimer}s");
            }
            
            // Update fainting timer if active
            if (hasStartedFaintingTimer && faintingTimer > 0f)
            {
                faintingTimer -= Time.deltaTime;
                
                // Trigger exhaustion warning
                if (faintingTimer > 0f && faintingTimer <= config.ExhaustionTimeSeconds)
                {
                    OnExhaustionWarning?.Invoke(faintingTimer);
                }
                
                // Player faints when timer reaches 0
                if (faintingTimer <= 0f)
                {
                    StartFainting();
                    return;
                }
            }
            
            // Check if player returned to spawn during fainting timer
            if (hasStartedFaintingTimer && SpawnPoint.IsPlayerAtSpawn())
            {
                // Player returned to spawn - they can sleep correctly
                PlayerSleptCorrectly = true;
                SaveSleepState(); // Save immediately
                hasStartedFaintingTimer = false;
                faintingTimer = 0f;
                StartSleeping();
                
                if (showDebugLogs)
                    Debug.Log("[DayNightManager] Player returned to spawn - can sleep correctly");
            }
        }
        
        private void CheckPlayerAtSpawnWhenNightFalls()
        {
            hasCheckedSpawnAtNightfall = true;
            
            if (SpawnPoint.IsPlayerAtSpawn())
            {
                // Player is at spawn - they can sleep correctly
                PlayerSleptCorrectly = true;
                SaveSleepState(); // Save immediately
                StartSleeping();
                
                if (showDebugLogs)
                    Debug.Log("[DayNightManager] Player is at spawn - can sleep correctly");
            }
            else
            {
                // Player is not at spawn - they will need to return or faint
                PlayerSleptCorrectly = false;
                SaveSleepState(); // Save immediately
                
                if (showDebugLogs)
                    Debug.Log("[DayNightManager] Player not at spawn - must return or will faint");
            }
        }
        
        private void StartExhaustion()
        {
            isInExhaustion = true;
            currentState = DayNightState.Exhausted;
            OnExhaustionStarted?.Invoke();
            
            if (showDebugLogs)
                Debug.Log("[DayNightManager] Player entered exhaustion state");
        }
        
        private void StartSleeping()
        {
            currentState = DayNightState.Sleeping;
            PlayerSleptCorrectly = true;
            
            // Save sleep state immediately
            SaveSleepState();
            
            if (showDebugLogs)
                Debug.Log("[DayNightManager] Player slept correctly");
            
            // Start wake up transition
            StartWakeUpTransition();
            
            OnPlayerSlept?.Invoke();
        }
        
        private void StartFainting()
        {
            currentState = DayNightState.Fainted;
            OnPlayerFainted?.Invoke();
            
            if (showDebugLogs)
                Debug.Log("[DayNightManager] Player fainted");
        }
        
        private void StartDayTransition()
        {
            if (isTransitioning) return;
            
            isTransitioning = true;
            currentState = DayNightState.Transitioning;
            
            if (showDebugLogs)
                Debug.Log("[DayNightManager] Starting day transition");
            
            // Trigger state change event
            OnStateChanged?.Invoke(currentState);
            
            // Start transition
            if (transitionHandler != null)
            {
                transitionHandler.FadeFromBlack(TransitionType.NightToDay, () => {
                    CompleteDayTransition();
                });
            }
            else
            {
                CompleteDayTransition();
            }
        }
        
        private void StartNightTransition()
        {
            if (isTransitioning) return;
            
            isTransitioning = true;
            currentState = DayNightState.Transitioning;
            
            if (showDebugLogs)
                Debug.Log("[DayNightManager] Starting night transition");
            
            // Trigger state change event
            OnStateChanged?.Invoke(currentState);
            
            // Start transition
            if (transitionHandler != null)
            {
                transitionHandler.FadeToBlack(TransitionType.DayToNight, () => {
                    CompleteNightTransition();
                });
            }
            else
            {
                CompleteNightTransition();
            }
        }
        
        private void StartWakeUpTransition()
        {
            if (isTransitioning) return;
            
            isTransitioning = true;
            
            if (showDebugLogs)
                Debug.Log("[DayNightManager] Starting wake up transition");
            
            // Start wake up transition
            if (transitionHandler != null)
            {
                transitionHandler.StartWakeUpTransition(() => {
                    CompleteWakeUpTransition();
                });
            }
            else
            {
                CompleteWakeUpTransition();
            }
        }
        
        // Public methods
        public void ForceDay()
        {
            currentState = DayNightState.Day;
            isInExhaustion = false;
            exhaustionTimer = 0f;
            faintingTimer = 0f;
            currentTime = 0f;
            hasCheckedSpawnAtNightfall = false;
            hasStartedFaintingTimer = false;
            PlayerSleptCorrectly = false;
            
            OnDayStart?.Invoke();
            
            if (showDebugLogs)
                Debug.Log("[DayNightManager] Forced to day");
        }
        
        public void ForceNight()
        {
            currentState = DayNightState.Night;
            exhaustionTimer = 0f;
            currentTime = 0f;
            hasCheckedSpawnAtNightfall = false;
            PlayerSleptCorrectly = false;
            
            OnNightStart?.Invoke();
            
            if (showDebugLogs)
                Debug.Log("[DayNightManager] Forced to night");
        }
        
        public void PauseTime(bool pause)
        {
            isPaused = pause;
            
            if (showDebugLogs)
                Debug.Log($"[DayNightManager] Time {(pause ? "paused" : "resumed")}");
        }
        
        public void SetTransitioning(bool transitioning)
        {
            if (transitioning)
            {
                currentState = DayNightState.Transitioning;
            }
            else
            {
                // Set final state based on time
                if (currentTime >= config.DayDurationSeconds)
                {
                    currentState = DayNightState.Night;
                    OnStateChanged?.Invoke(currentState);
                }
                else
                {
                    currentState = DayNightState.Day;
                    OnStateChanged?.Invoke(currentState);
                }
            }
            
            if (showDebugLogs)
                Debug.Log($"[DayNightManager] Transitioning: {transitioning}, State: {currentState}");
        }
        
        public void AddTime(float minutes)
        {
            float seconds = minutes * 60f;
            currentTime += seconds;
            
            if (showDebugLogs)
                Debug.Log($"[DayNightManager] Added {minutes} minutes ({seconds}s) to current time");
        }
        
        public void ForceFaint()
        {
            if (currentState == DayNightState.Night && !PlayerSleptCorrectly)
            {
                StartFainting();
            }
        }
        
        public void ForceSleep()
        {
            if (currentState == DayNightState.Night && SpawnPoint.IsPlayerAtSpawn())
            {
                PlayerSleptCorrectly = true;
                StartSleeping();
            }
        }
        
        public bool IsPlayerExhausted()
        {
            return isInExhaustion;
        }
        
        public bool DidPlayerSleepCorrectly()
        {
            return PlayerSleptCorrectly;
        }
        
        // New: Get fainting timer information
        public float GetFaintingTimer()
        {
            return hasStartedFaintingTimer ? faintingTimer : 0f;
        }
        
        public bool IsFaintingTimerActive()
        {
            return hasStartedFaintingTimer && faintingTimer > 0f;
        }
        
        public bool HasStartedFaintingTimer()
        {
            return hasStartedFaintingTimer;
        }
        
        private void ValidateReferences()
        {
            if (config == null)
            {
                Debug.LogError("[DayNightManager] DayNightConfig reference is missing!");
            }
        }
        
        private void ValidateSpawnPoint()
        {
            if (SpawnPoint.Current == null)
            {
                Debug.LogWarning("[DayNightManager] No SpawnPoint found in scene! Creating emergency spawn at (0,0,0)");
                
                // Create emergency spawn point
                GameObject emergencySpawn = new GameObject("EmergencySpawnPoint");
                emergencySpawn.transform.position = Vector3.zero;
                emergencySpawn.AddComponent<SpawnPoint>();
                
                if (showDebugLogs)
                    Debug.Log("[DayNightManager] Created emergency spawn point at (0,0,0)");
            }
        }
        
        private void FindMessageSystem()
        {
            messageSystem = FindObjectOfType<MessageSystem>();
            if (messageSystem == null)
            {
                Debug.LogWarning("[DayNightManager] No MessageSystem found in scene!");
            }
        }
        
        private void FindTransitionHandler()
        {
            transitionHandler = FindObjectOfType<TransitionHandler>();
            if (transitionHandler == null)
            {
                Debug.LogWarning("[DayNightManager] No TransitionHandler found in scene!");
            }
        }
        
        private void LoadState()
        {
            // Validate save data before loading
            if (!SaveUtility.ValidateSaveData())
            {
                Debug.LogWarning("[DayNightManager] Save data validation failed - using default state");
            }
            
            // Load saved state using SaveUtility
            var saveData = SaveUtility.LoadDayNightState();
            currentTime = saveData.currentTime;
            currentState = saveData.isDay ? DayNightState.Day : DayNightState.Night;
            PlayerSleptCorrectly = saveData.playerSleptCorrectly;
            
            if (showDebugLogs)
                Debug.Log($"[DayNightManager] Loaded state - Time: {currentTime:F1}s, State: {currentState}, SleptCorrectly: {PlayerSleptCorrectly}");
        }
        
        private void OnApplicationPause(bool pauseStatus)
        {
            SaveState();
        }
        
        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus)
                SaveState();
        }
        
        private void SaveState()
        {
            // Save state using SaveUtility
            SaveUtility.SaveDayNightState(currentTime, currentState == DayNightState.Day, PlayerSleptCorrectly);
            
            if (showDebugLogs)
                Debug.Log($"[DayNightManager] Saved state - Time: {currentTime:F1}s, State: {currentState}, SleptCorrectly: {PlayerSleptCorrectly}");
        }
        
        private void SaveSleepState()
        {
            // Save sleep state separately for consistency
            SaveUtility.SaveSleepState(PlayerSleptCorrectly);
            
            if (showDebugLogs)
                Debug.Log($"[DayNightManager] Saved sleep state: {PlayerSleptCorrectly}");
        }
        
        private void OnDestroy()
        {
            SaveState();
        }

        private void CompleteDayTransition()
        {
            // Reset state for new day
            isInExhaustion = false;
            exhaustionTimer = 0f;
            faintingTimer = 0f;
            hasCheckedSpawnAtNightfall = false;
            hasStartedFaintingTimer = false;
            // Don't reset PlayerSleptCorrectly here - it should persist for penalty system
            
            currentState = DayNightState.Day;
            isTransitioning = false;
            
            OnDayStart?.Invoke();
            OnStateChanged?.Invoke(currentState);
            
            if (showDebugLogs)
                Debug.Log("[DayNightManager] Day transition completed");
        }
        
        private void CompleteNightTransition()
        {
            exhaustionTimer = 0f;
            faintingTimer = 0f;
            hasStartedFaintingTimer = false;
            // Don't reset PlayerSleptCorrectly here - it should persist for penalty system
            // PlayerSleptCorrectly will be set to false only when player actually fails to sleep
            
            currentState = DayNightState.Night;
            isTransitioning = false;
            
            OnNightStart?.Invoke();
            OnStateChanged?.Invoke(currentState);
            
            if (showDebugLogs)
                Debug.Log("[DayNightManager] Night transition completed");
        }
        
        private void CompleteWakeUpTransition()
        {
            isTransitioning = false;
            
            if (showDebugLogs)
                Debug.Log("[DayNightManager] Wake up transition completed");
        }
        
        // Context menu methods for testing
        [ContextMenu("Test Save State")]
        public void TestSaveState()
        {
            SaveState();
            Debug.Log($"[DayNightManager] Test save completed - Time: {currentTime:F1}s, State: {currentState}, SleptCorrectly: {PlayerSleptCorrectly}");
        }
        
        [ContextMenu("Test Load State")]
        public void TestLoadState()
        {
            LoadState();
            Debug.Log($"[DayNightManager] Test load completed - Time: {currentTime:F1}s, State: {currentState}, SleptCorrectly: {PlayerSleptCorrectly}");
        }
        
        [ContextMenu("Show Save Data Summary")]
        public void ShowSaveDataSummary()
        {
            string summary = SaveUtility.GetSaveDataSummary();
            Debug.Log($"[DayNightManager] Save data summary: {summary}");
        }
        
        [ContextMenu("Validate Save Data")]
        public void ValidateSaveData()
        {
            bool isValid = SaveUtility.ValidateSaveData();
            Debug.Log($"[DayNightManager] Save data validation: {(isValid ? "PASSED" : "FAILED")}");
        }
    }
}

// ScriptRole: Controls the day/night cycle with differentiated sleep/faint logic and night blocking
// RelatedScripts: DayNightVisuals, PlayerPenalty, TransitionHandler, SpawnPoint, GameStateManager
// UsesSO: DayNightConfig
// ReceivesFrom: SpawnPoint (proximity events)
// SendsTo: All day/night system components via events
