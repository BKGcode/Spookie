using UnityEngine;
using TransitionSystem.Visual;
using TransitionSystem.Audio;
using TransitionSystem.Specific;
using TransitionSystem.Events;
using DayNightSystem.Core;
using DayNightSystem;

namespace TransitionSystem.Core
{
    public class TransitionManager : MonoBehaviour
    {
        [Header("Component References")]
        [SerializeField] private VisualTransitionController visualController;
        [SerializeField] private AudioTransitionController audioController;
        [SerializeField] private SleepTransitionController sleepController;
        [SerializeField] private FaintTransitionController faintController;
        [SerializeField] private TransitionEventController eventController;
        
        [Header("System References")]
        [SerializeField] private CanvasGroup fadeCanvas;
        [SerializeField] private AudioManager audioManager;
        
        [Header("Settings")]
        [SerializeField] private bool autoSetupComponents = true;
        [SerializeField] private bool enableDebugLogs = true;
        
        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;
        
        // Events
        public System.Action<Events.TransitionEventController.TransitionType> OnTransitionStarted;
        public System.Action<Events.TransitionEventController.TransitionType> OnTransitionCompleted;
        
        private void Awake()
        {
            if (autoSetupComponents)
            {
                SetupComponents();
            }
            
            ValidateReferences();
            SetupEventListeners();
            
            if (showDebugLogs)
                Debug.Log("[TransitionManager] Initialized");
        }
        
        private void SetupComponents()
        {
            // Add missing components
            if (visualController == null)
                visualController = gameObject.AddComponent<VisualTransitionController>();
            
            if (audioController == null)
                audioController = gameObject.AddComponent<AudioTransitionController>();
            
            if (sleepController == null)
                sleepController = gameObject.AddComponent<SleepTransitionController>();
            
            if (faintController == null)
                faintController = gameObject.AddComponent<FaintTransitionController>();
            
            if (eventController == null)
                eventController = gameObject.AddComponent<TransitionEventController>();
            
            // Setup component references
            if (fadeCanvas != null)
                visualController.SetFadeCanvas(fadeCanvas);
            
            if (audioManager != null)
                audioController.SetAudioManager(audioManager);
            
            // Setup cross-component references
            sleepController.SetVisualController(visualController);
            sleepController.SetAudioController(audioController);
            
            faintController.SetVisualController(visualController);
            faintController.SetAudioController(audioController);
            
            eventController.SetVisualController(visualController);
            eventController.SetAudioController(audioController);
            eventController.SetSleepController(sleepController);
            eventController.SetFaintController(faintController);
        }
        
        private void ValidateReferences()
        {
            if (visualController == null)
            {
                Debug.LogError("[TransitionManager] VisualTransitionController reference is missing!");
            }
            
            if (audioController == null)
            {
                Debug.LogWarning("[TransitionManager] AudioTransitionController reference is missing!");
            }
            
            if (sleepController == null)
            {
                Debug.LogWarning("[TransitionManager] SleepTransitionController reference is missing!");
            }
            
            if (faintController == null)
            {
                Debug.LogWarning("[TransitionManager] FaintTransitionController reference is missing!");
            }
            
            if (eventController == null)
            {
                Debug.LogWarning("[TransitionManager] TransitionEventController reference is missing!");
            }
            
            if (fadeCanvas == null)
            {
                Debug.LogWarning("[TransitionManager] FadeCanvas reference is missing!");
            }
            
            if (audioManager == null)
            {
                Debug.LogWarning("[TransitionManager] AudioManager reference is missing!");
            }
        }
        
        private void SetupEventListeners()
        {
            if (eventController != null)
            {
                eventController.OnTransitionStarted += HandleTransitionStarted;
                eventController.OnTransitionCompleted += HandleTransitionCompleted;
            }
        }
        
        public void FadeToBlack(float duration = -1f, System.Action onComplete = null)
        {
            if (visualController != null)
                visualController.FadeToBlack(duration, onComplete);
        }
        
        public void FadeFromBlack(float duration = -1f, System.Action onComplete = null)
        {
            if (visualController != null)
                visualController.FadeFromBlack(duration, onComplete);
        }
        
        public void FadeToAlpha(float targetAlpha, float duration = -1f, System.Action onComplete = null)
        {
            if (visualController != null)
                visualController.FadeToAlpha(targetAlpha, duration, onComplete);
        }
        
        public void StartSleepTransition(System.Action onComplete = null)
        {
            if (sleepController != null)
                sleepController.StartSleepTransition(onComplete);
        }
        
        public void StartWakeUpTransition(System.Action onComplete = null)
        {
            if (sleepController != null)
                sleepController.StartWakeUpTransition(onComplete);
        }
        
        public void StartFaintTransition(System.Action onComplete = null)
        {
            if (faintController != null)
                faintController.StartFaintTransition(onComplete);
        }
        
        public void PlaySleepSound()
        {
            if (audioController != null)
                audioController.PlaySleepSound();
        }
        
        public void PlayWakeUpSound()
        {
            if (audioController != null)
                audioController.PlayWakeUpSound();
        }
        
        public void PlayFaintSound()
        {
            if (audioController != null)
                audioController.PlayFaintSound();
        }
        
        public void PlayMorningAmbient()
        {
            if (audioController != null)
                audioController.PlayMorningAmbient();
        }
        
        public bool IsTransitioning()
        {
            bool isTransitioning = false;
            
            if (visualController != null)
                isTransitioning |= visualController.IsTransitioning();
            
            if (sleepController != null)
                isTransitioning |= sleepController.IsTransitioning();
            
            if (faintController != null)
                isTransitioning |= faintController.IsTransitioning();
            
            return isTransitioning;
        }
        
        public void StopAllTransitions()
        {
            if (visualController != null)
                visualController.StopTransition();
            
            if (sleepController != null)
                sleepController.StopTransition();
            
            if (faintController != null)
                faintController.StopTransition();
            
            if (showDebugLogs)
                Debug.Log("[TransitionManager] All transitions stopped");
        }
        
        public void SetFadeCanvas(CanvasGroup canvas)
        {
            fadeCanvas = canvas;
            if (visualController != null)
                visualController.SetFadeCanvas(canvas);
        }
        
        public void SetAudioManager(AudioManager manager)
        {
            audioManager = manager;
            if (audioController != null)
                audioController.SetAudioManager(manager);
        }
        
        public void SetDefaultFadeDuration(float duration)
        {
            if (visualController != null)
                visualController.SetDefaultFadeDuration(duration);
        }
        
        public void SetSleepTransitionDuration(float duration)
        {
            if (sleepController != null)
                sleepController.SetTransitionDuration(duration);
        }
        
        public void SetFaintTransitionDuration(float duration)
        {
            if (faintController != null)
                faintController.SetTransitionDuration(duration);
        }
        
        public void SetEnableAutomaticTransitions(bool enable)
        {
            if (eventController != null)
                eventController.SetEnableAutomaticTransitions(enable);
        }
        
        public void SetEnableStateManagement(bool enable)
        {
            if (eventController != null)
                eventController.SetEnableStateManagement(enable);
        }
        
        public VisualTransitionController GetVisualController()
        {
            return visualController;
        }
        
        public AudioTransitionController GetAudioController()
        {
            return audioController;
        }
        
        public SleepTransitionController GetSleepController()
        {
            return sleepController;
        }
        
        public FaintTransitionController GetFaintController()
        {
            return faintController;
        }
        
        public TransitionEventController GetEventController()
        {
            return eventController;
        }
        
        [ContextMenu("Test Fade To Black")]
        public void TestFadeToBlack()
        {
            FadeToBlack(1f, () => {
                if (showDebugLogs)
                    Debug.Log("[TransitionManager] Test fade to black completed");
            });
        }
        
        [ContextMenu("Test Fade From Black")]
        public void TestFadeFromBlack()
        {
            FadeFromBlack(1f, () => {
                if (showDebugLogs)
                    Debug.Log("[TransitionManager] Test fade from black completed");
            });
        }
        
        [ContextMenu("Test Sleep Transition")]
        public void TestSleepTransition()
        {
            StartSleepTransition(() => {
                if (showDebugLogs)
                    Debug.Log("[TransitionManager] Test sleep transition completed");
            });
        }
        
        [ContextMenu("Test Wake Up Transition")]
        public void TestWakeUpTransition()
        {
            StartWakeUpTransition(() => {
                if (showDebugLogs)
                    Debug.Log("[TransitionManager] Test wake up transition completed");
            });
        }
        
        [ContextMenu("Test Faint Transition")]
        public void TestFaintTransition()
        {
            StartFaintTransition(() => {
                if (showDebugLogs)
                    Debug.Log("[TransitionManager] Test faint transition completed");
            });
        }
        
        [ContextMenu("Stop All Transitions")]
        public void StopAllTransitionsContext()
        {
            StopAllTransitions();
        }
        
        [ContextMenu("Show Transition Status")]
        public void ShowTransitionStatus()
        {
            Debug.Log($"[TransitionManager] Transition Status:");
            Debug.Log($"- Visual Controller: {(visualController != null ? "Found" : "Missing")}");
            Debug.Log($"- Audio Controller: {(audioController != null ? "Found" : "Missing")}");
            Debug.Log($"- Sleep Controller: {(sleepController != null ? "Found" : "Missing")}");
            Debug.Log($"- Faint Controller: {(faintController != null ? "Found" : "Missing")}");
            Debug.Log($"- Event Controller: {(eventController != null ? "Found" : "Missing")}");
            Debug.Log($"- Is Transitioning: {IsTransitioning()}");
        }
        
        private void HandleTransitionStarted(Events.TransitionEventController.TransitionType type)
        {
            OnTransitionStarted?.Invoke(type);
        }
        
        private void HandleTransitionCompleted(Events.TransitionEventController.TransitionType type)
        {
            OnTransitionCompleted?.Invoke(type);
        }
        
        // ScriptRole: Central manager for all transition systems (visual, audio, sleep, faint)
        // RelatedScripts: VisualTransitionController, AudioTransitionController, SleepTransitionController, FaintTransitionController, TransitionEventController
        // UsesSO: None
        // ReceivesFrom: TransitionEventController, DayNightManager, GameStateManager
        // SendsTo: All transition controllers, AudioManager
    }
}
