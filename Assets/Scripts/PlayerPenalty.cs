using UnityEngine;
using UnityEngine.UI;
using DayNightSystem.Core;

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
        [SerializeField] private bool useProgressiveExhaustion = true;
        [Tooltip("Should match DayNightStateController.faintCountdownSeconds for consistency")]
        [SerializeField] private float exhaustionCountdownSeconds = 30f;
        
        [Header("Morning After Faint")] 
        [SerializeField] private bool enableMorningPenalty = true;
        [Tooltip("How long the player stays in the heavy morning penalty after fainting the previous night")]
        [SerializeField] private float morningPenaltyDurationSeconds = 30f; // Y
        [Tooltip("Initial speed multiplier right after waking up from a faint (0..1)")]
        [SerializeField] private float morningStartSpeedMultiplier = 0.5f;
        [Tooltip("Recovered speed multiplier kept for the rest of the day (0..1)")]
        [SerializeField] private float morningRecoveredSpeedMultiplier = 0.8f; // Z
        [SerializeField] private bool disableSprintDuringMorningPenalty = true;
        [SerializeField] private bool disableSprintRestOfDayAfterRecovery = true;

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
        private DayNightSystem.Core.DayNightManager dayNightManager;
        private Coroutine morningPenaltyCoroutine;
        private bool morningPenaltyActive;
        private bool morningPenaltyRecovered;
        
        // Public properties
        public PenaltyType CurrentPenaltyType { get; private set; }
        public float CurrentSpeedMultiplier { get; private set; } = 1f;
        
        private void Awake()
        {
            if (playerMovement == null)
            {
                TryGetComponent(out playerMovement);
                if (playerMovement == null)
                {
                    playerMovement = FindObjectOfType<PlayerController.PlayerMovement>();
                }
            }
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
            // If we enter a new scene already in Day after a faint, start the morning penalty
            TryStartMorningPenaltyIfNeeded();
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
            dayNightManager = FindObjectOfType<DayNightSystem.Core.DayNightManager>();
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
                    Debug.Log("[PlayerPenalty] Day started - player slept correctly, penalties removed");
            }
            else
            {
                TryStartMorningPenaltyIfNeeded();
                if (showDebugLogs)
                    Debug.Log("[PlayerPenalty] Day started - player did not sleep correctly, morning penalty evaluated");
            }
        }
        
        private void OnNightStart()
        {
            // Clear any morning penalty state when night begins
            if (morningPenaltyCoroutine != null)
            {
                StopCoroutine(morningPenaltyCoroutine);
                morningPenaltyCoroutine = null;
            }
            morningPenaltyActive = false;
            morningPenaltyRecovered = false;
            if (showDebugLogs)
                Debug.Log("[PlayerPenalty] Night started - morning penalty state cleared");
        }
        
        private void OnExhaustionWarning(float secondsRemaining)
        {
            if (useProgressiveExhaustion && CurrentPenaltyType == PenaltyType.Exhaustion)
            {
                float total = Mathf.Max(0.01f, exhaustionCountdownSeconds);
                float t = 1f - Mathf.Clamp01(secondsRemaining / total);
                float targetMultiplier = Mathf.Lerp(1f, exhaustionSpeedMultiplier, t);
                if (Mathf.Abs(targetMultiplier - CurrentSpeedMultiplier) > 0.01f)
                {
                    CurrentSpeedMultiplier = targetMultiplier;
                    if (playerMovement != null)
                    {
                        playerMovement.SetSpeedMultiplier(CurrentSpeedMultiplier);
                        bool canSprintNow = !disableSprintWhenExhausted && t < 0.5f;
                        playerMovement.SetCanSprint(canSprintNow);
                    }
                }
                if (showDebugLogs)
                    Debug.Log($"[PlayerPenalty] Progressive exhaustion: t={t:F2}, speed x{CurrentSpeedMultiplier:F2}");
            }
            else
            {
                if (showDebugLogs)
                    Debug.Log($"[PlayerPenalty] Exhaustion warning: {secondsRemaining:F1}s remaining");
            }
        }
        
        private void OnExhaustionStarted()
        {
            if (useProgressiveExhaustion)
            {
                CurrentPenaltyType = PenaltyType.Exhaustion;
                CurrentSpeedMultiplier = 1f;
                if (playerMovement != null)
                {
                    playerMovement.SetSpeedMultiplier(CurrentSpeedMultiplier);
                    playerMovement.SetCanSprint(!disableSprintWhenExhausted);
                }
                ShowPenaltyIndicator(exhaustionColor);
                if (audioManager != null) { audioManager.PlayPenaltySound(); }
                SavePenaltyState();
            }
            else
            {
                ApplyExhaustionPenalty();
            }
            
            if (showDebugLogs)
                Debug.Log("[PlayerPenalty] Exhaustion started - penalty applied");
        }
        
        private void OnPlayerSlept()
        {
            // Player slept - penalties will be removed when day starts if slept correctly
            if (showDebugLogs)
                Debug.Log("[PlayerPenalty] Player slept");
        }
        
        private void OnPlayerFainted()
        {
            ApplyFaintedPenalty();
            
            if (showDebugLogs)
                Debug.Log("[PlayerPenalty] Player fainted - severe penalty applied");
        }

        private void TryStartMorningPenaltyIfNeeded()
        {
            if (!enableMorningPenalty || dayNightManager == null) return;
            if (dayNightManager.CurrentState != DayNightSystem.Core.DayNightState.Day) return;
            if (dayNightManager.DidPlayerSleepCorrectly()) return;
            if (morningPenaltyActive || morningPenaltyRecovered) return;

            // Start morning after-faint penalty
            if (morningPenaltyCoroutine != null) StopCoroutine(morningPenaltyCoroutine);
            morningPenaltyCoroutine = StartCoroutine(MorningPenaltyRoutine());
            morningPenaltyActive = true;
            if (showDebugLogs)
                Debug.Log("[PlayerPenalty] Morning penalty started after faint");
        }

        private System.Collections.IEnumerator MorningPenaltyRoutine()
        {
            float duration = Mathf.Max(0.1f, morningPenaltyDurationSeconds);
            float elapsed = 0f;
            CurrentPenaltyType = PenaltyType.Exhaustion;
            if (playerMovement != null)
            {
                playerMovement.SetCanSprint(!disableSprintDuringMorningPenalty);
            }
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float speed = Mathf.Lerp(morningStartSpeedMultiplier, morningRecoveredSpeedMultiplier, t);
                CurrentSpeedMultiplier = speed;
                if (playerMovement != null)
                {
                    playerMovement.SetSpeedMultiplier(CurrentSpeedMultiplier);
                }
                yield return null;
            }
            // Hold recovered state for the rest of the day
            morningPenaltyActive = false;
            morningPenaltyRecovered = true;
            CurrentSpeedMultiplier = morningRecoveredSpeedMultiplier;
            if (playerMovement != null)
            {
                playerMovement.SetSpeedMultiplier(CurrentSpeedMultiplier);
                playerMovement.SetCanSprint(!disableSprintRestOfDayAfterRecovery);
            }
            SavePenaltyState();
            if (showDebugLogs)
                Debug.Log($"[PlayerPenalty] Morning penalty finished - recovered to x{CurrentSpeedMultiplier:F2} for rest of day");
        }
        
        public void ApplyExhaustionPenalty()
        {
            if (CurrentPenaltyType == PenaltyType.Fainted)
            {
                // Don't downgrade from fainted to exhausted
                return;
            }
            
            CurrentPenaltyType = PenaltyType.Exhaustion;
            CurrentSpeedMultiplier = exhaustionSpeedMultiplier;
            
            // Apply penalty to player movement
            if (playerMovement != null)
            {
                playerMovement.SetSpeedMultiplier(CurrentSpeedMultiplier);
                playerMovement.SetCanSprint(!disableSprintWhenExhausted);
            }
            
            // Show visual feedback
            ShowPenaltyIndicator(exhaustionColor);
            
            // Play penalty sound
            if (audioManager != null)
            {
                audioManager.PlayPenaltySound();
            }
            
            // Save penalty state
            SavePenaltyState();
            
            if (showDebugLogs)
                Debug.Log($"[PlayerPenalty] Exhaustion penalty applied - Speed multiplier: {CurrentSpeedMultiplier}");
        }
        
        public void ApplyFaintedPenalty()
        {
            CurrentPenaltyType = PenaltyType.Fainted;
            CurrentSpeedMultiplier = faintedSpeedMultiplier;
            
            // Apply penalty to player movement
            if (playerMovement != null)
            {
                playerMovement.SetSpeedMultiplier(CurrentSpeedMultiplier);
                playerMovement.SetCanSprint(!disableSprintWhenFainted);
            }
            
            // Show visual feedback
            ShowPenaltyIndicator(faintedColor);
            
            // Play penalty sound
            if (audioManager != null)
            {
                audioManager.PlayPenaltySound();
            }
            
            // Save penalty state
            SavePenaltyState();
            
            if (showDebugLogs)
                Debug.Log($"[PlayerPenalty] Fainted penalty applied - Speed multiplier: {CurrentSpeedMultiplier}");
        }
        
        public void RemoveAllPenalties()
        {
            CurrentPenaltyType = PenaltyType.None;
            CurrentSpeedMultiplier = 1f;
            
            // Remove penalty from player movement
            if (playerMovement != null)
            {
                playerMovement.SetSpeedMultiplier(1f);
                playerMovement.SetCanSprint(true);
            }
            
            // Hide visual feedback
            HidePenaltyIndicator();
            
            // Save penalty state
            SavePenaltyState();
            
            if (showDebugLogs)
                Debug.Log("[PlayerPenalty] All penalties removed");
        }
        
        private void ShowPenaltyIndicator(Color color)
        {
            if (penaltyIndicator != null)
            {
                penaltyIndicator.color = color;
                penaltyIndicator.gameObject.SetActive(true);
                
                // Start blinking effect
                StartCoroutine(PenaltyIndicatorBlink());
            }
        }
        
        private void HidePenaltyIndicator()
        {
            if (penaltyIndicator != null)
            {
                penaltyIndicator.gameObject.SetActive(false);
            }
        }
        
        private System.Collections.IEnumerator PenaltyIndicatorBlink()
        {
            if (penaltyIndicator == null) yield break;
            
            while (CurrentPenaltyType != PenaltyType.None)
            {
                penaltyIndicator.enabled = !penaltyIndicator.enabled;
                yield return new WaitForSeconds(indicatorBlinkRate);
            }
            
            penaltyIndicator.enabled = true;
        }
        
        private void LoadPenaltyState()
        {
            // Load penalty state from PlayerPrefs
            int penaltyType = PlayerPrefs.GetInt("PlayerPenalty_Type", 0);
            float speedMultiplier = PlayerPrefs.GetFloat("PlayerPenalty_SpeedMultiplier", 1f);
            
            CurrentPenaltyType = (PenaltyType)penaltyType;
            CurrentSpeedMultiplier = speedMultiplier;
            
            // Apply loaded state to player movement
            if (playerMovement != null && CurrentPenaltyType != PenaltyType.None)
            {
                playerMovement.SetSpeedMultiplier(CurrentSpeedMultiplier);
                playerMovement.SetCanSprint(CurrentPenaltyType == PenaltyType.None);
            }
            
            if (showDebugLogs)
                Debug.Log($"[PlayerPenalty] Loaded penalty state: {CurrentPenaltyType}, Speed multiplier: {CurrentSpeedMultiplier}");
        }
        
        private void SavePenaltyState()
        {
            // Save penalty state to PlayerPrefs
            PlayerPrefs.SetInt("PlayerPenalty_Type", (int)CurrentPenaltyType);
            PlayerPrefs.SetFloat("PlayerPenalty_SpeedMultiplier", CurrentSpeedMultiplier);
            PlayerPrefs.Save();
            
            if (showDebugLogs)
                Debug.Log($"[PlayerPenalty] Saved penalty state: {CurrentPenaltyType}, Speed multiplier: {CurrentSpeedMultiplier}");
        }
        
        public bool CanPerformAction()
        {
            return CurrentPenaltyType == PenaltyType.None;
        }
        
        public float GetSpeedMultiplier()
        {
            return CurrentSpeedMultiplier;
        }
        
        public bool CanSprint()
        {
            switch (CurrentPenaltyType)
            {
                case PenaltyType.None:
                    return true;
                case PenaltyType.Exhaustion:
                    return !disableSprintWhenExhausted;
                case PenaltyType.Fainted:
                    return !disableSprintWhenFainted;
                default:
                    return true;
            }
        }
        
        public string GetPenaltyDescription()
        {
            switch (CurrentPenaltyType)
            {
                case PenaltyType.None:
                    return "No penalty";
                case PenaltyType.Exhaustion:
                    return $"Exhausted - Speed reduced to {CurrentSpeedMultiplier * 100:F0}%";
                case PenaltyType.Fainted:
                    return $"Fainted - Speed reduced to {CurrentSpeedMultiplier * 100:F0}%";
                default:
                    return "Unknown penalty";
            }
        }
        
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
        
        [ContextMenu("Test Save Penalty State")]
        public void TestSavePenaltyState()
        {
            SavePenaltyState();
        }
        
        [ContextMenu("Test Load Penalty State")]
        public void TestLoadPenaltyState()
        {
            LoadPenaltyState();
        }
        
        [ContextMenu("Show Penalty Status")]
        public void ShowPenaltyStatus()
        {
            Debug.Log($"[PlayerPenalty] Status:\n" +
                     $"Current Penalty: {CurrentPenaltyType}\n" +
                     $"Speed Multiplier: {CurrentSpeedMultiplier}\n" +
                     $"Can Sprint: {CanSprint()}\n" +
                     $"Can Perform Action: {CanPerformAction()}\n" +
                     $"Player Movement: {(playerMovement != null ? "Found" : "Missing")}\n" +
                     $"DayNight Manager: {(dayNightManager != null ? "Found" : "Missing")}\n" +
                     $"Audio Manager: {(audioManager != null ? "Found" : "Missing")}\n" +
                     $"Penalty Indicator: {(penaltyIndicator != null ? "Found" : "Missing")}");
        }
    }
}

// ScriptRole: Manages player penalties based on exhaustion and fainting states
// RelatedScripts: PlayerMovement, DayNightManager, AudioManager
// UsesSO: None
// ReceivesFrom: DayNightManager events
// SendsTo: PlayerMovement (speed/sprint modifications), AudioManager (penalty sounds), UI (visual indicators)
