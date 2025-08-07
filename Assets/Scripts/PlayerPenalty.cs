using UnityEngine;
using UnityEngine.UI;

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
        [SerializeField] private float exhaustionSpeedMultiplier = 0.5f;
        [SerializeField] private float faintedSpeedMultiplier = 0.25f;
        [SerializeField] private bool disableSprintWhenExhausted = true;
        [SerializeField] private bool disableSprintWhenFainted = true;
        
        [Header("Visual Feedback")]
        [SerializeField] private Image penaltyIndicator;
        [SerializeField] private Color exhaustionColor = Color.yellow;
        [SerializeField] private Color faintedColor = Color.red;
        [SerializeField] private float indicatorBlinkRate = 0.3f;
        
        [Header("Audio")]
        [SerializeField] private AudioManager audioManager;
        
        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;
        
        // Private fields
        private PlayerController.PlayerMovement playerMovement;
        private DayNightManager dayNightManager;
        
        // Public properties
        public PenaltyType CurrentPenaltyType { get; private set; }
        public float CurrentSpeedMultiplier { get; private set; } = 1f;
        
        private void Awake()
        {
            ValidateReferences();
            FindDayNightManager();
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
            if (playerMovement == null)
            {
                Debug.LogError("[PlayerPenalty] PlayerMovement reference is missing!");
            }
            
            if (dayNightManager == null)
            {
                Debug.LogError("[PlayerPenalty] DayNightManager reference is missing!");
            }
            
            if (penaltyIndicator == null)
            {
                Debug.LogWarning("[PlayerPenalty] PenaltyIndicator reference is missing - no visual feedback");
            }
            
            if (audioManager == null)
            {
                Debug.LogWarning("[PlayerPenalty] AudioManager reference is missing - no penalty sounds");
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
        

        
        private void UpdateProgressivePenalty()
        {
            // Progressive penalty system removed for simplicity
            // This method is kept for compatibility but does nothing
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
                // Player didn't sleep correctly - but don't apply penalty here
                // Penalty will be applied when player actually faints during night
                if (showDebugLogs)
                    Debug.Log("[PlayerPenalty] Day started - player didn't sleep correctly, but no penalty applied yet");
            }
        }
        
        private void OnNightStart()
        {
            if (showDebugLogs)
                Debug.Log("[PlayerPenalty] Night started - monitoring for exhaustion");
        }
        
        private void OnExhaustionWarning(float secondsRemaining)
        {
            if (secondsRemaining <= 0f && CurrentPenaltyType == PenaltyType.None)
            {
                ApplyExhaustionPenalty();
            }
        }
        
        private void OnExhaustionStarted()
        {
            if (CurrentPenaltyType == PenaltyType.None)
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
        
        // Public methods for applying penalties
        public void ApplyExhaustionPenalty()
        {
            if (CurrentPenaltyType == PenaltyType.Exhaustion) return;
            
            CurrentPenaltyType = PenaltyType.Exhaustion;
            CurrentSpeedMultiplier = exhaustionSpeedMultiplier;
            
            // Apply speed penalty
            if (playerMovement != null)
            {
                playerMovement.SetSpeedMultiplier(exhaustionSpeedMultiplier);
                playerMovement.SetCanSprint(!disableSprintWhenExhausted);
            }
            
            // Show visual indicator
            ShowPenaltyIndicator(exhaustionColor);
            
            // Play penalty sound
            if (audioManager != null)
            {
                audioManager.PlayPenaltySound();
            }
            
            // Save penalty state
            SavePenaltyState();
            
            if (showDebugLogs)
                Debug.Log($"[PlayerPenalty] Applied exhaustion penalty - Speed: {exhaustionSpeedMultiplier}, Sprint: {!disableSprintWhenExhausted}");
        }
        
        public void ApplyFaintedPenalty()
        {
            if (CurrentPenaltyType == PenaltyType.Fainted) return;
            
            CurrentPenaltyType = PenaltyType.Fainted;
            CurrentSpeedMultiplier = faintedSpeedMultiplier;
            
            // Apply more severe speed penalty
            if (playerMovement != null)
            {
                playerMovement.SetSpeedMultiplier(faintedSpeedMultiplier);
                playerMovement.SetCanSprint(!disableSprintWhenFainted);
            }
            
            // Show visual indicator
            ShowPenaltyIndicator(faintedColor);
            
            // Play penalty sound
            if (audioManager != null)
            {
                audioManager.PlayPenaltySound();
            }
            
            // Save penalty state
            SavePenaltyState();
            
            if (showDebugLogs)
                Debug.Log($"[PlayerPenalty] Applied fainted penalty - Speed: {faintedSpeedMultiplier}, Sprint: {!disableSprintWhenFainted}");
        }
        

        
        public void RemoveAllPenalties()
        {
            if (CurrentPenaltyType == PenaltyType.None) return;
            
            CurrentPenaltyType = PenaltyType.None;
            CurrentSpeedMultiplier = 1f;
            
            // Restore normal speed and sprint
            if (playerMovement != null)
            {
                playerMovement.SetSpeedMultiplier(1f);
                playerMovement.SetCanSprint(true);
            }
            
            // Hide visual indicator
            HidePenaltyIndicator();
            
            // Save penalty state
            SavePenaltyState();
            
            if (showDebugLogs)
                Debug.Log("[PlayerPenalty] Removed all penalties - restored normal movement");
        }
        
        private void ShowPenaltyIndicator(Color color)
        {
            if (penaltyIndicator != null)
            {
                penaltyIndicator.gameObject.SetActive(true);
                penaltyIndicator.color = color;
                
                // Start blinking effect
                StartCoroutine(PenaltyIndicatorBlink());
                
                if (showDebugLogs)
                    Debug.Log($"[PlayerPenalty] Showing penalty indicator - Color: {color}");
            }
        }
        
        private void HidePenaltyIndicator()
        {
            if (penaltyIndicator != null)
            {
                penaltyIndicator.gameObject.SetActive(false);
                
                if (showDebugLogs)
                    Debug.Log("[PlayerPenalty] Hiding penalty indicator");
            }
        }
        
        private System.Collections.IEnumerator PenaltyIndicatorBlink()
        {
            while (CurrentPenaltyType != PenaltyType.None && penaltyIndicator != null)
            {
                penaltyIndicator.enabled = true;
                yield return new WaitForSeconds(indicatorBlinkRate);
                
                penaltyIndicator.enabled = false;
                yield return new WaitForSeconds(indicatorBlinkRate);
            }
        }
        
        private void LoadPenaltyState()
        {
            bool savedPenalty = SaveUtility.LoadPlayerPenaltyState();
            
            if (savedPenalty && CurrentPenaltyType == PenaltyType.None)
            {
                // Apply fainted penalty by default when loading saved penalty
                ApplyFaintedPenalty();
                
                if (showDebugLogs)
                    Debug.Log("[PlayerPenalty] Loaded saved penalty state");
            }
        }
        
        private void SavePenaltyState()
        {
            bool hasPenalty = CurrentPenaltyType != PenaltyType.None;
            SaveUtility.SavePlayerPenaltyState(hasPenalty);
            
            if (showDebugLogs)
                Debug.Log($"[PlayerPenalty] Saved penalty state: {hasPenalty} (Type: {CurrentPenaltyType})");
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
                    return !disableSprintWhenExhausted;
                case PenaltyType.Fainted:
                    return !disableSprintWhenFainted;
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
        

        
        // Testing methods
        [ContextMenu("Test Exhaustion Penalty")]
        public void TestExhaustionPenalty()
        {
            ApplyExhaustionPenalty();
        }
        
        [ContextMenu("Test Fainted Penalty")]
        public void TestFaintedPenalty()
        {
            ApplyFaintedPenalty();
        }
        
        [ContextMenu("Remove All Penalties")]
        public void TestRemovePenalties()
        {
            RemoveAllPenalties();
        }
        
        // Context menu methods for testing
        [ContextMenu("Test Save Penalty State")]
        public void TestSavePenaltyState()
        {
            SavePenaltyState();
            Debug.Log($"[PlayerPenalty] Test save penalty state: {CurrentPenaltyType}");
        }
        
        [ContextMenu("Test Load Penalty State")]
        public void TestLoadPenaltyState()
        {
            LoadPenaltyState();
            Debug.Log($"[PlayerPenalty] Test load penalty state: {CurrentPenaltyType}");
        }
        
        [ContextMenu("Show Penalty Status")]
        public void ShowPenaltyStatus()
        {
            string status = $"Penalty Status:\n" +
                          $"Current Type: {CurrentPenaltyType}\n" +
                          $"Current Speed Multiplier: {CurrentSpeedMultiplier:F2}\n" +
                          $"Can Perform Action: {CanPerformAction()}\n" +
                          $"Can Sprint: {CanSprint()}\n" +
                          $"Indicator Active: {(penaltyIndicator != null ? penaltyIndicator.gameObject.activeInHierarchy.ToString() : "N/A")}";
            
            Debug.Log($"[PlayerPenalty] {status}");
        }
    }
}

// ScriptRole: Handles player penalties with differentiated types and progressive effects
// RelatedScripts: PlayerMovement, DayNightManager, AudioManager
// UsesSO: None
// ReceivesFrom: DayNightManager events
// SendsTo: PlayerMovement (speed/sprint modifications), AudioManager (penalty sounds), UI (visual indicators)
