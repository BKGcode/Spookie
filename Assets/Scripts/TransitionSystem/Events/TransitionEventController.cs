using UnityEngine;
using DayNightSystem.Core;
using DayNightSystem;

namespace TransitionSystem.Events
{
    public class TransitionEventController : MonoBehaviour
    {
        [Header("System References")]
        [SerializeField] private DayNightSystem.Core.DayNightManager dayNightManager;
        [SerializeField] private GameStateManager gameStateManager;
        
        [Header("Transition References")]
        [SerializeField] private Specific.SleepTransitionController sleepController;
        [SerializeField] private Specific.FaintTransitionController faintController;
        [SerializeField] private Visual.VisualTransitionController visualController;
        [SerializeField] private Audio.AudioTransitionController audioController;
        
        [Header("Event Settings")]
        [SerializeField] private bool enableAutomaticTransitions = true;
        [SerializeField] private bool enableStateManagement = true;
        
        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;
        
        // Events
        public System.Action<TransitionType> OnTransitionStarted;
        public System.Action<TransitionType> OnTransitionCompleted;
        public System.Action OnDayNightTransitionStarted;
        public System.Action OnDayNightTransitionCompleted;
        
        public enum TransitionType
        {
            DayToNight,
            NightToDay,
            Sleep,
            Faint,
            WakeUp
        }
        
        private void Awake()
        {
            FindReferences();
            ValidateReferences();
            
            if (showDebugLogs)
                Debug.Log("[TransitionEventController] Initialized");
        }
        
        private void OnEnable()
        {
            SubscribeToDayNightEvents();
            SubscribeToTransitionEvents();
        }
        
        private void OnDisable()
        {
            UnsubscribeFromDayNightEvents();
            UnsubscribeFromTransitionEvents();
        }
        
        private void FindReferences()
        {
            if (dayNightManager == null)
                dayNightManager = FindObjectOfType<DayNightSystem.Core.DayNightManager>();
            
            if (gameStateManager == null)
                gameStateManager = FindObjectOfType<GameStateManager>();
            
            if (sleepController == null)
                sleepController = GetComponent<Specific.SleepTransitionController>();
            
            if (faintController == null)
                faintController = GetComponent<Specific.FaintTransitionController>();
            
            if (visualController == null)
                visualController = GetComponent<Visual.VisualTransitionController>();
            
            if (audioController == null)
                audioController = GetComponent<Audio.AudioTransitionController>();
        }
        
        private void ValidateReferences()
        {
            if (dayNightManager == null)
            {
                Debug.LogWarning("[TransitionEventController] DayNightManager reference is missing!");
            }
            
            if (gameStateManager == null)
            {
                Debug.LogWarning("[TransitionEventController] GameStateManager reference is missing!");
            }
            
            if (sleepController == null)
            {
                Debug.LogWarning("[TransitionEventController] SleepTransitionController reference is missing!");
            }
            
            if (faintController == null)
            {
                Debug.LogWarning("[TransitionEventController] FaintTransitionController reference is missing!");
            }
            
            if (visualController == null)
            {
                Debug.LogWarning("[TransitionEventController] VisualTransitionController reference is missing!");
            }
            
            if (audioController == null)
            {
                Debug.LogWarning("[TransitionEventController] AudioTransitionController reference is missing!");
            }
        }
        
        private void SubscribeToDayNightEvents()
        {
            if (dayNightManager != null)
            {
                dayNightManager.OnDayStart += OnDayStart;
                dayNightManager.OnNightStart += OnNightStart;
                dayNightManager.OnPlayerSlept += OnPlayerSlept;
                dayNightManager.OnPlayerFainted += OnPlayerFainted;
            }
        }
        
        private void UnsubscribeFromDayNightEvents()
        {
            if (dayNightManager != null)
            {
                dayNightManager.OnDayStart -= OnDayStart;
                dayNightManager.OnNightStart -= OnNightStart;
                dayNightManager.OnPlayerSlept -= OnPlayerSlept;
                dayNightManager.OnPlayerFainted -= OnPlayerFainted;
            }
        }
        
        private void SubscribeToTransitionEvents()
        {
            if (sleepController != null)
            {
                sleepController.OnSleepTransitionStarted += OnSleepTransitionStarted;
                sleepController.OnSleepTransitionCompleted += OnSleepTransitionCompleted;
                sleepController.OnWakeUpTransitionStarted += OnWakeUpTransitionStarted;
                sleepController.OnWakeUpTransitionCompleted += OnWakeUpTransitionCompleted;
            }
            
            if (faintController != null)
            {
                faintController.OnFaintTransitionStarted += OnFaintTransitionStarted;
                faintController.OnFaintTransitionCompleted += OnFaintTransitionCompleted;
            }
        }
        
        private void UnsubscribeFromTransitionEvents()
        {
            if (sleepController != null)
            {
                sleepController.OnSleepTransitionStarted -= OnSleepTransitionStarted;
                sleepController.OnSleepTransitionCompleted -= OnSleepTransitionCompleted;
                sleepController.OnWakeUpTransitionStarted -= OnWakeUpTransitionStarted;
                sleepController.OnWakeUpTransitionCompleted -= OnWakeUpTransitionCompleted;
            }
            
            if (faintController != null)
            {
                faintController.OnFaintTransitionStarted -= OnFaintTransitionStarted;
                faintController.OnFaintTransitionCompleted -= OnFaintTransitionCompleted;
            }
        }
        
        private void OnDayStart()
        {
            if (!enableAutomaticTransitions) return;
            
            if (showDebugLogs)
                Debug.Log("[TransitionEventController] Day started - triggering transition");
            
            OnDayNightTransitionStarted?.Invoke();
            
            // Trigger day transition
            if (visualController != null)
            {
                visualController.FadeFromBlack(1.5f, () => {
                    OnDayNightTransitionCompleted?.Invoke();
                    if (showDebugLogs)
                        Debug.Log("[TransitionEventController] Day transition completed");
                });
            }
        }
        
        private void OnNightStart()
        {
            if (!enableAutomaticTransitions) return;
            
            if (showDebugLogs)
                Debug.Log("[TransitionEventController] Night started - triggering transition");
            
            OnDayNightTransitionStarted?.Invoke();
            
            // Trigger night transition
            if (visualController != null)
            {
                visualController.FadeToBlack(1.5f, () => {
                    OnDayNightTransitionCompleted?.Invoke();
                    if (showDebugLogs)
                        Debug.Log("[TransitionEventController] Night transition completed");
                });
            }
        }
        
        private void OnPlayerSlept()
        {
            if (!enableAutomaticTransitions) return;
            
            if (showDebugLogs)
                Debug.Log("[TransitionEventController] Player slept - triggering sleep transition");
            
            if (sleepController != null)
            {
                sleepController.StartSleepTransition();
            }
        }
        
        private void OnPlayerFainted()
        {
            if (!enableAutomaticTransitions) return;
            
            if (showDebugLogs)
                Debug.Log("[TransitionEventController] Player fainted - triggering faint transition");
            
            if (faintController != null)
            {
                faintController.StartFaintTransition();
            }
        }
        
        private void OnSleepTransitionStarted()
        {
            OnTransitionStarted?.Invoke(TransitionType.Sleep);
            
            if (enableStateManagement && gameStateManager != null)
            {
                gameStateManager.SetTransitioningState();
            }
        }
        
        private void OnSleepTransitionCompleted()
        {
            OnTransitionCompleted?.Invoke(TransitionType.Sleep);
            
            if (enableStateManagement && gameStateManager != null)
            {
                gameStateManager.EndTransition();
            }
        }
        
        private void OnWakeUpTransitionStarted()
        {
            OnTransitionStarted?.Invoke(TransitionType.WakeUp);
            
            if (enableStateManagement && gameStateManager != null)
            {
                gameStateManager.SetTransitioningState();
            }
        }
        
        private void OnWakeUpTransitionCompleted()
        {
            OnTransitionCompleted?.Invoke(TransitionType.WakeUp);
            
            if (enableStateManagement && gameStateManager != null)
            {
                gameStateManager.EndTransition();
            }
        }
        
        private void OnFaintTransitionStarted()
        {
            OnTransitionStarted?.Invoke(TransitionType.Faint);
            
            if (enableStateManagement && gameStateManager != null)
            {
                gameStateManager.SetTransitioningState();
            }
        }
        
        private void OnFaintTransitionCompleted()
        {
            OnTransitionCompleted?.Invoke(TransitionType.Faint);
            
            if (enableStateManagement && gameStateManager != null)
            {
                gameStateManager.EndTransition();
            }
        }
        
        public void SetEnableAutomaticTransitions(bool enable)
        {
            enableAutomaticTransitions = enable;
            
            if (showDebugLogs)
                Debug.Log($"[TransitionEventController] Automatic transitions {(enable ? "enabled" : "disabled")}");
        }
        
        public void SetEnableStateManagement(bool enable)
        {
            enableStateManagement = enable;
            
            if (showDebugLogs)
                Debug.Log($"[TransitionEventController] State management {(enable ? "enabled" : "disabled")}");
        }
        
        public void SetDayNightManager(DayNightSystem.Core.DayNightManager manager)
        {
            dayNightManager = manager;
            
            if (showDebugLogs)
                Debug.Log("[TransitionEventController] DayNightManager reference set");
        }
        
        public void SetGameStateManager(GameStateManager manager)
        {
            gameStateManager = manager;
            
            if (showDebugLogs)
                Debug.Log("[TransitionEventController] GameStateManager reference set");
        }
        
        public void SetSleepController(Specific.SleepTransitionController controller)
        {
            sleepController = controller;
            
            if (showDebugLogs)
                Debug.Log("[TransitionEventController] SleepTransitionController reference set");
        }
        
        public void SetFaintController(Specific.FaintTransitionController controller)
        {
            faintController = controller;
            
            if (showDebugLogs)
                Debug.Log("[TransitionEventController] FaintTransitionController reference set");
        }
        
        public void SetVisualController(Visual.VisualTransitionController controller)
        {
            visualController = controller;
            
            if (showDebugLogs)
                Debug.Log("[TransitionEventController] VisualTransitionController reference set");
        }
        
        public void SetAudioController(Audio.AudioTransitionController controller)
        {
            audioController = controller;
            
            if (showDebugLogs)
                Debug.Log("[TransitionEventController] AudioTransitionController reference set");
        }
        
        [ContextMenu("Test Day Start Transition")]
        public void TestDayStartTransition()
        {
            OnDayStart();
        }
        
        [ContextMenu("Test Night Start Transition")]
        public void TestNightStartTransition()
        {
            OnNightStart();
        }
        
        [ContextMenu("Test Sleep Transition")]
        public void TestSleepTransition()
        {
            OnPlayerSlept();
        }
        
        [ContextMenu("Test Faint Transition")]
        public void TestFaintTransition()
        {
            OnPlayerFainted();
        }
        
        [ContextMenu("Toggle Automatic Transitions")]
        public void ToggleAutomaticTransitions()
        {
            SetEnableAutomaticTransitions(!enableAutomaticTransitions);
        }
        
        [ContextMenu("Toggle State Management")]
        public void ToggleStateManagement()
        {
            SetEnableStateManagement(!enableStateManagement);
        }
        
        [ContextMenu("Show Transition Status")]
        public void ShowTransitionStatus()
        {
            Debug.Log($"[TransitionEventController] Status:\n" +
                     $"Automatic Transitions: {enableAutomaticTransitions}\n" +
                     $"State Management: {enableStateManagement}\n" +
                     $"DayNightManager: {(dayNightManager != null ? "Found" : "Missing")}\n" +
                     $"GameStateManager: {(gameStateManager != null ? "Found" : "Missing")}\n" +
                     $"SleepController: {(sleepController != null ? "Found" : "Missing")}\n" +
                     $"FaintController: {(faintController != null ? "Found" : "Missing")}\n" +
                     $"VisualController: {(visualController != null ? "Found" : "Missing")}\n" +
                     $"AudioController: {(audioController != null ? "Found" : "Missing")}");
        }
    }
}

// ScriptRole: Coordinates events between DayNightManager and transition controllers
// RelatedScripts: DayNightManager, GameStateManager, SleepTransitionController, FaintTransitionController, VisualTransitionController, AudioTransitionController
// UsesSO: None
// ReceivesFrom: DayNightManager, GameStateManager, transition controllers
// SendsTo: VisualTransitionController, AudioTransitionController, GameStateManager
