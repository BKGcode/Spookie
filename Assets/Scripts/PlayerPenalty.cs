using UnityEngine;

namespace DayNightSystem
{
    public enum PenaltyType
    {
        None,
        Exhaustion,
        Fainted
    }
    
    public class PlayerPenalty : MonoBehaviour
    {
        [Header("Penalty Settings")]
        [SerializeField] private float exhaustionSpeedMultiplier = 0.7f;
        [SerializeField] private float faintedSpeedMultiplier = 0.5f;
        [SerializeField] private bool disableSprintOnExhaustion = true;
        [SerializeField] private bool disableSprintOnFainted = true;
        
        [Header("Progressive Penalty")]
        [SerializeField] private bool useProgressivePenalty = true;
        [SerializeField] private float progressivePenaltyDuration = 10f;
        [SerializeField] private AnimationCurve progressiveSpeedCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0.5f);
        
        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;
        
        // Private fields
        private PlayerController.PlayerMovement playerMovement;
        private DayNightManager dayNightManager;
        private float originalWalkSpeed;
        private float originalSprintSpeed;
        private bool originalCanSprint = true;
        private float progressivePenaltyTimer = 0f;
        private bool isProgressivePenaltyActive = false;
        
        // Public properties
        public bool HasExhaustionPenalty { get; private set; }
        public bool HasFaintedPenalty { get; private set; }
        public PenaltyType CurrentPenaltyType { get; private set; }
        public float CurrentSpeedMultiplier { get; private set; } = 1f;
        
        private void Awake()
        {
            ValidateReferences();
            FindDayNightManager();
            StoreOriginalValues();
        }
        
        private void OnEnable()
        {
            if (dayNightManager != null)
            {
                dayNightManager.OnDayStart += OnDayStart;
                dayNightManager.OnNightStart += OnNightStart;
                dayNightManager.OnExhaustionWarning += OnExhaustionWarning;
                dayNightManager.OnExhaustionStarted += OnExhaustionStarted;
                dayNightManager.OnPlayerSlept += OnPlayerSlept;
                dayNightManager.OnPlayerFainted += OnPlayerFainted;
            }
        }
        
        private void OnDisable()
        {
            if (dayNightManager != null)
            {
                dayNightManager.OnDayStart -= OnDayStart;
                dayNightManager.OnNightStart -= OnNightStart;
                dayNightManager.OnExhaustionWarning -= OnExhaustionWarning;
                dayNightManager.OnExhaustionStarted -= OnExhaustionStarted;
                dayNightManager.OnPlayerSlept -= OnPlayerSlept;
                dayNightManager.OnPlayerFainted -= OnPlayerFainted;
            }
        }
        
        private void Update()
        {
            UpdateProgressivePenalty();
        }
        
        private void Start()
        {
            LoadPenaltyState();
        }
        
        private void ValidateReferences()
        {
            playerMovement = GetComponent<PlayerController.PlayerMovement>();
            if (playerMovement == null)
            {
                Debug.LogError("[PlayerPenalty] PlayerMovement component not found on this GameObject!");
            }
        }
        
        private void FindDayNightManager()
        {
            dayNightManager = FindObjectOfType<DayNightManager>();
            if (dayNightManager == null)
            {
                Debug.LogError("[PlayerPenalty] No DayNightManager found in scene!");
            }
        }
        
        private void StoreOriginalValues()
        {
            if (playerMovement != null)
            {
                // Store original values for restoration
                originalWalkSpeed = playerMovement.GetCurrentSpeedValue();
                originalSprintSpeed = originalWalkSpeed * 1.6f; // Estimate sprint speed
                originalCanSprint = true;
                
                if (showDebugLogs)
                    Debug.Log($"[PlayerPenalty] Stored original values - Walk: {originalWalkSpeed}, Sprint: {originalSprintSpeed}");
            }
        }
        
        private void UpdateProgressivePenalty()
        {
            if (!useProgressivePenalty || !isProgressivePenaltyActive) return;
            
            progressivePenaltyTimer += Time.deltaTime;
            float normalizedTime = Mathf.Clamp01(progressivePenaltyTimer / progressivePenaltyDuration);
            
            // Apply progressive penalty based on curve
            float progressiveMultiplier = progressiveSpeedCurve.Evaluate(normalizedTime);
            float finalMultiplier = Mathf.Min(CurrentSpeedMultiplier, progressiveMultiplier);
            
            if (playerMovement != null)
            {
                playerMovement.SetSpeedMultiplier(finalMultiplier);
            }
            
            if (normalizedTime >= 1f)
            {
                isProgressivePenaltyActive = false;
                progressivePenaltyTimer = 0f;
                
                if (showDebugLogs)
                    Debug.Log("[PlayerPenalty] Progressive penalty completed");
            }
        }
        
        private void OnDayStart()
        {
            // Check if player slept correctly
            if (dayNightManager != null && dayNightManager.DidPlayerSleepCorrectly())
            {
                RemoveAllPenalties();
                
                if (showDebugLogs)
                    Debug.Log("[PlayerPenalty] Day started - player slept correctly, removed all penalties");
            }
            else
            {
                // Player didn't sleep correctly - apply fainted penalty
                ApplyFaintedPenalty();
                
                if (showDebugLogs)
                    Debug.Log("[PlayerPenalty] Day started - player didn't sleep correctly, applying fainted penalty");
            }
        }
        
        private void OnNightStart()
        {
            if (showDebugLogs)
                Debug.Log("[PlayerPenalty] Night started - monitoring for exhaustion");
        }
        
        private void OnExhaustionWarning(float secondsRemaining)
        {
            if (secondsRemaining <= 0f && !HasExhaustionPenalty && !HasFaintedPenalty)
            {
                ApplyExhaustionPenalty();
            }
        }
        
        private void OnExhaustionStarted()
        {
            if (!HasExhaustionPenalty && !HasFaintedPenalty)
            {
                ApplyExhaustionPenalty();
            }
        }
        
        private void OnPlayerSlept()
        {
            // Player slept correctly - remove penalties
            RemoveAllPenalties();
            
            if (showDebugLogs)
                Debug.Log("[PlayerPenalty] Player slept correctly - removed all penalties");
        }
        
        private void OnPlayerFainted()
        {
            // Player fainted - apply fainted penalty
            ApplyFaintedPenalty();
            
            if (showDebugLogs)
                Debug.Log("[PlayerPenalty] Player fainted - applied fainted penalty");
        }
        
        public void ApplyExhaustionPenalty()
        {
            if (HasExhaustionPenalty) return;
            
            HasExhaustionPenalty = true;
            CurrentPenaltyType = PenaltyType.Exhaustion;
            CurrentSpeedMultiplier = exhaustionSpeedMultiplier;
            
            if (playerMovement != null)
            {
                // Apply speed penalty
                playerMovement.SetSpeedMultiplier(exhaustionSpeedMultiplier);
                playerMovement.SetCanSprint(!disableSprintOnExhaustion);
                
                if (showDebugLogs)
                    Debug.Log($"[PlayerPenalty] Applied exhaustion penalty - Speed: {exhaustionSpeedMultiplier}, Sprint: {!disableSprintOnExhaustion}");
            }
            
            // Start progressive penalty if enabled
            if (useProgressivePenalty)
            {
                StartProgressivePenalty();
            }
            
            // Save penalty state
            SaveUtility.SavePlayerPenaltyState(true);
        }
        
        public void ApplyFaintedPenalty()
        {
            if (HasFaintedPenalty) return;
            
            HasFaintedPenalty = true;
            CurrentPenaltyType = PenaltyType.Fainted;
            CurrentSpeedMultiplier = faintedSpeedMultiplier;
            
            if (playerMovement != null)
            {
                // Apply more severe speed penalty
                playerMovement.SetSpeedMultiplier(faintedSpeedMultiplier);
                playerMovement.SetCanSprint(!disableSprintOnFainted);
                
                if (showDebugLogs)
                    Debug.Log($"[PlayerPenalty] Applied fainted penalty - Speed: {faintedSpeedMultiplier}, Sprint: {!disableSprintOnFainted}");
            }
            
            // Start progressive penalty if enabled
            if (useProgressivePenalty)
            {
                StartProgressivePenalty();
            }
            
            // Save penalty state
            SaveUtility.SavePlayerPenaltyState(true);
        }
        
        private void StartProgressivePenalty()
        {
            isProgressivePenaltyActive = true;
            progressivePenaltyTimer = 0f;
            
            if (showDebugLogs)
                Debug.Log("[PlayerPenalty] Started progressive penalty");
        }
        
        public void RemoveAllPenalties()
        {
            HasExhaustionPenalty = false;
            HasFaintedPenalty = false;
            CurrentPenaltyType = PenaltyType.None;
            CurrentSpeedMultiplier = 1f;
            isProgressivePenaltyActive = false;
            progressivePenaltyTimer = 0f;
            
            if (playerMovement != null)
            {
                // Restore original values
                playerMovement.SetSpeedMultiplier(1f);
                playerMovement.SetCanSprint(true);
                
                if (showDebugLogs)
                    Debug.Log("[PlayerPenalty] Removed all penalties - restored normal speed and sprint");
            }
            
            // Save penalty state
            SaveUtility.SavePlayerPenaltyState(false);
        }
        
        private void LoadPenaltyState()
        {
            bool savedPenalty = SaveUtility.LoadPlayerPenaltyState();
            
            if (savedPenalty && !HasExhaustionPenalty && !HasFaintedPenalty)
            {
                // Apply fainted penalty by default when loading saved penalty
                ApplyFaintedPenalty();
                
                if (showDebugLogs)
                    Debug.Log("[PlayerPenalty] Loaded saved penalty state");
            }
        }
        
        // Public method to check if player can perform actions
        public bool CanPerformAction()
        {
            return CurrentPenaltyType == PenaltyType.None;
        }
        
        // Public method to get current speed multiplier
        public float GetSpeedMultiplier()
        {
            return CurrentSpeedMultiplier;
        }
        
        // Public method to check if sprint is allowed
        public bool CanSprint()
        {
            switch (CurrentPenaltyType)
            {
                case PenaltyType.Exhaustion:
                    return !disableSprintOnExhaustion;
                case PenaltyType.Fainted:
                    return !disableSprintOnFainted;
                default:
                    return true;
            }
        }
        
        // Public method to get penalty description
        public string GetPenaltyDescription()
        {
            switch (CurrentPenaltyType)
            {
                case PenaltyType.Exhaustion:
                    return "Exhausted - Speed reduced";
                case PenaltyType.Fainted:
                    return "Fainted - Severe speed penalty";
                default:
                    return "No penalty";
            }
        }
        
        // Public method to check if progressive penalty is active
        public bool IsProgressivePenaltyActive()
        {
            return isProgressivePenaltyActive;
        }
    }
}

// ScriptRole: Handles player penalties with differentiated types and progressive effects
// RelatedScripts: PlayerMovement, DayNightManager
// UsesSO: None (direct component interaction)
// ReceivesFrom: DayNightManager events
// SendsTo: PlayerMovement (speed modifications)
