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
        public bool IsTransitioning => currentState == DayNightState.Transitioning;
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
        
        // Private fields
        private float currentTime;
        private float exhaustionTimer;
        private DayNightState currentState = DayNightState.Day;
        private bool isPaused = false;
        private bool isInExhaustion = false;
        private bool hasCheckedSpawnAtNightfall = false;
        private MessageSystem messageSystem;
        
        private void Awake()
        {
            ValidateReferences();
            ValidateSpawnPoint();
            FindMessageSystem();
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
            
            if (currentTime >= config.DayDurationSeconds)
            {
                if (showDebugLogs)
                    Debug.Log($"[DayNightManager] Time threshold reached! Current: {currentTime:F1}s, Threshold: {config.DayDurationSeconds:F1}s");
                
                if (currentState == DayNightState.Day)
                {
                    // Transition to night
                    StartNightTransition();
                }
                else
                {
                    // Transition to day
                    StartDayTransition();
                }
                
                currentTime = 0f;
            }
        }
        
        private void CheckExhaustion()
        {
            if (currentState == DayNightState.Night && !isInExhaustion && !hasCheckedSpawnAtNightfall)
            {
                // Check if player is at spawn when night falls
                CheckPlayerAtSpawnWhenNightFalls();
            }
            
            if (currentState == DayNightState.Night && !isInExhaustion && !PlayerSleptCorrectly)
            {
                // Clamp delta time to prevent huge jumps
                float deltaTime = Mathf.Clamp(Time.deltaTime, 0f, 0.1f);
                exhaustionTimer += deltaTime;
                
                if (exhaustionTimer >= config.ExhaustionTimeSeconds)
                {
                    StartExhaustion();
                }
                else
                {
                    // Warning when approaching exhaustion
                    float remainingTime = config.ExhaustionTimeSeconds - exhaustionTimer;
                    if (remainingTime <= 10f && Mathf.FloorToInt(remainingTime) != Mathf.FloorToInt(remainingTime + deltaTime))
                    {
                        OnExhaustionWarning?.Invoke(remainingTime);
                    }
                }
            }
        }
        
        private void CheckPlayerAtSpawnWhenNightFalls()
        {
            hasCheckedSpawnAtNightfall = true;
            
            if (SpawnPoint.IsPlayerAtSpawn())
            {
                // Player is at spawn - they can sleep correctly
                PlayerSleptCorrectly = true;
                StartSleeping();
                
                if (showDebugLogs)
                    Debug.Log("[DayNightManager] Player is at spawn - can sleep correctly");
            }
            else
            {
                // Player is not at spawn - they will need to return or faint
                PlayerSleptCorrectly = false;
                
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
            OnPlayerSlept?.Invoke();
            
            if (showDebugLogs)
                Debug.Log("[DayNightManager] Player started sleeping");
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
            currentState = DayNightState.Transitioning;
            
            // Reset state for new day
            isInExhaustion = false;
            exhaustionTimer = 0f;
            hasCheckedSpawnAtNightfall = false;
            PlayerSleptCorrectly = false;
            
            OnDayStart?.Invoke();
            
            if (showDebugLogs)
                Debug.Log("[DayNightManager] Day transition started");
        }
        
        private void StartNightTransition()
        {
            currentState = DayNightState.Transitioning;
            exhaustionTimer = 0f;
            
            OnNightStart?.Invoke();
            
            if (showDebugLogs)
                Debug.Log("[DayNightManager] Night transition started");
        }
        
        // Public methods
        public void ForceDay()
        {
            currentState = DayNightState.Day;
            isInExhaustion = false;
            exhaustionTimer = 0f;
            currentTime = 0f;
            hasCheckedSpawnAtNightfall = false;
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
                // Determine appropriate state based on time
                currentState = currentTime < config.DayDurationSeconds * 0.5f ? DayNightState.Day : DayNightState.Night;
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
        
        private void LoadState()
        {
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
        
        private void OnDestroy()
        {
            SaveState();
        }
    }
}

// ScriptRole: Controls the day/night cycle with differentiated sleep/faint logic
// RelatedScripts: DayNightVisuals, PlayerPenalty, TransitionHandler, SpawnPoint
// UsesSO: DayNightConfig
// ReceivesFrom: SpawnPoint (proximity events)
// SendsTo: All day/night system components via events
