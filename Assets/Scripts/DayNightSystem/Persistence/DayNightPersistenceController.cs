using UnityEngine;
using DayNightSystem.Core;
using DayNightSystem.Validation;

namespace DayNightSystem.Persistence
{
    [System.Serializable]
    public class DayNightSaveData
    {
        public float currentTime;
        public DayNightSystem.Core.DayNightState currentState;
        public bool isInExhaustion;
        public float exhaustionTimer;
        public bool playerSleptCorrectly;
        public bool hasStartedFaintingTimer;
        public float faintingTimer;
        public bool hasCheckedSpawnAtNightfall;
    }
    
    public class DayNightPersistenceController : MonoBehaviour
    {
        [Header("Save Settings")]
        [SerializeField] private string saveKey = "DayNightData";
        [SerializeField] private bool autoSave = true;
        [SerializeField] private float autoSaveInterval = 30f;
        
        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;
        
        // References
        private DayNightTimeController timeController;
        private DayNightStateController stateController;
        private DayNightValidationController validationController;
        
        // Private fields
        private float lastAutoSaveTime;
        private DayNightSaveData currentSaveData;
        
        // Events
        public System.Action OnDataSaved;
        public System.Action OnDataLoaded;
        
        private void Awake()
        {
            FindReferences();
            ValidateReferences();
            InitializeSaveData();
            
            if (showDebugLogs)
                Debug.Log("[DayNightPersistenceController] Initialized");
        }
        
        private void OnEnable()
        {
            if (timeController != null)
            {
                timeController.OnTimeChanged += OnTimeChanged;
            }
            
            if (stateController != null)
            {
                stateController.OnStateChanged += OnStateChanged;
            }
        }
        
        private void OnDisable()
        {
            if (timeController != null)
            {
                timeController.OnTimeChanged -= OnTimeChanged;
            }
            
            if (stateController != null)
            {
                stateController.OnStateChanged -= OnStateChanged;
            }
        }
        
        private void Update()
        {
            if (autoSave)
            {
                CheckAutoSave();
            }
        }
        
        private void CheckAutoSave()
        {
            if (Time.time - lastAutoSaveTime >= autoSaveInterval)
            {
                SaveState();
                lastAutoSaveTime = Time.time;
            }
        }
        
        private void OnTimeChanged(float time)
        {
            if (currentSaveData != null)
            {
                currentSaveData.currentTime = time;
            }
        }
        
        private void OnStateChanged(DayNightSystem.Core.DayNightState newState)
        {
            if (currentSaveData != null)
            {
                currentSaveData.currentState = newState;
            }
        }
        
        public void SaveState()
        {
            if (timeController == null || stateController == null) return;
            
            currentSaveData = new DayNightSaveData
            {
                currentTime = timeController.CurrentTime,
                currentState = stateController.CurrentState,
                isInExhaustion = stateController.IsInExhaustion,
                exhaustionTimer = stateController.GetFaintingTimer(), // Using fainting timer as exhaustion timer
                playerSleptCorrectly = stateController.DidPlayerSleepCorrectly(),
                hasStartedFaintingTimer = stateController.HasStartedFaintingTimer(),
                faintingTimer = stateController.GetFaintingTimer(),
                hasCheckedSpawnAtNightfall = validationController != null ? validationController.HasCheckedSpawnAtNightfall() : false
            };
            
            string jsonData = JsonUtility.ToJson(currentSaveData, true);
            PlayerPrefs.SetString(saveKey, jsonData);
            PlayerPrefs.Save();
            
            OnDataSaved?.Invoke();
            
            if (showDebugLogs)
                Debug.Log($"[DayNightPersistenceController] Data saved - Time: {currentSaveData.currentTime:F1}s, State: {currentSaveData.currentState}");
        }
        
        public void LoadState()
        {
            if (!PlayerPrefs.HasKey(saveKey))
            {
                if (showDebugLogs)
                    Debug.Log("[DayNightPersistenceController] No save data found, using defaults");
                return;
            }
            
            try
            {
                string jsonData = PlayerPrefs.GetString(saveKey);
                currentSaveData = JsonUtility.FromJson<DayNightSaveData>(jsonData);
                
                ApplyLoadedData();
                
                OnDataLoaded?.Invoke();
                
                if (showDebugLogs)
                    Debug.Log($"[DayNightPersistenceController] Data loaded - Time: {currentSaveData.currentTime:F1}s, State: {currentSaveData.currentState}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[DayNightPersistenceController] Error loading data: {e.Message}");
                currentSaveData = null;
            }
        }
        
        private void ApplyLoadedData()
        {
            if (currentSaveData == null) return;
            
            // Apply time
            if (timeController != null)
            {
                timeController.SetTime(currentSaveData.currentTime);
            }
            
            // Apply state
            if (stateController != null)
            {
                stateController.SetState(currentSaveData.currentState);
                
                // Apply exhaustion state
                if (currentSaveData.isInExhaustion)
                {
                    stateController.ForceExhaustion();
                }
            }
        }
        
        public void ClearSaveData()
        {
            PlayerPrefs.DeleteKey(saveKey);
            currentSaveData = null;
            
            if (showDebugLogs)
                Debug.Log("[DayNightPersistenceController] Save data cleared");
        }
        
        public DayNightSaveData GetCurrentSaveData()
        {
            return currentSaveData;
        }
        
        public bool HasSaveData()
        {
            return PlayerPrefs.HasKey(saveKey);
        }
        
        public void SetAutoSave(bool enabled)
        {
            autoSave = enabled;
            
            if (showDebugLogs)
                Debug.Log($"[DayNightPersistenceController] Auto save {(enabled ? "enabled" : "disabled")}");
        }
        
        public void SetAutoSaveInterval(float interval)
        {
            autoSaveInterval = Mathf.Max(1f, interval);
            
            if (showDebugLogs)
                Debug.Log($"[DayNightPersistenceController] Auto save interval set to {autoSaveInterval}s");
        }
        
        private void InitializeSaveData()
        {
            currentSaveData = new DayNightSaveData();
            lastAutoSaveTime = Time.time;
        }
        
        private void FindReferences()
        {
            timeController = GetComponent<DayNightTimeController>();
            stateController = GetComponent<DayNightStateController>();
            validationController = GetComponent<DayNightValidationController>();
        }
        
        private void ValidateReferences()
        {
            if (timeController == null)
            {
                Debug.LogError("[DayNightPersistenceController] DayNightTimeController reference is missing!");
            }
            
            if (stateController == null)
            {
                Debug.LogError("[DayNightPersistenceController] DayNightStateController reference is missing!");
            }
        }
        
        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                SaveState();
            }
        }
        
        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus)
            {
                SaveState();
            }
        }
        
        private void OnDestroy()
        {
            SaveState();
        }
        
        [ContextMenu("Save State")]
        public void SaveStateContext()
        {
            SaveState();
        }
        
        [ContextMenu("Load State")]
        public void LoadStateContext()
        {
            LoadState();
        }
        
        [ContextMenu("Clear Save Data")]
        public void ClearSaveDataContext()
        {
            ClearSaveData();
        }
        
        [ContextMenu("Show Save Data Summary")]
        public void ShowSaveDataSummary()
        {
            if (currentSaveData != null)
            {
                Debug.Log($"[DayNightPersistenceController] Save Data Summary:\n" +
                         $"Time: {currentSaveData.currentTime:F1}s\n" +
                         $"State: {currentSaveData.currentState}\n" +
                         $"Exhausted: {currentSaveData.isInExhaustion}\n" +
                         $"Slept Correctly: {currentSaveData.playerSleptCorrectly}\n" +
                         $"Has Save Data: {HasSaveData()}");
            }
            else
            {
                Debug.Log("[DayNightPersistenceController] No current save data");
            }
        }
        
        [ContextMenu("Validate Save Data")]
        public void ValidateSaveData()
        {
            if (HasSaveData())
            {
                string jsonData = PlayerPrefs.GetString(saveKey);
                try
                {
                    DayNightSaveData testData = JsonUtility.FromJson<DayNightSaveData>(jsonData);
                    Debug.Log($"[DayNightPersistenceController] Save data is valid - Time: {testData.currentTime:F1}s, State: {testData.currentState}");
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"[DayNightPersistenceController] Save data is invalid: {e.Message}");
                }
            }
            else
            {
                Debug.Log("[DayNightPersistenceController] No save data to validate");
            }
        }
    }
}
