using UnityEngine;
using System.Collections;
using TransitionSystem.Visual;
using TransitionSystem.Audio;

namespace TransitionSystem.Specific
{
    public class SleepTransitionController : MonoBehaviour
    {
        [Header("Component References")]
        [SerializeField] private VisualTransitionController visualController;
        [SerializeField] private AudioTransitionController audioController;
        
        [Header("Sleep Settings")]
        [SerializeField] private float sleepTransitionDuration = 2f;
        [SerializeField] private float wakeUpTransitionDuration = 1.5f;
        
        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;
        
        // Private fields
        private Coroutine currentSleepTransition;
        private System.Action onSleepComplete;
        private System.Action onWakeUpComplete;
        
        // Events
        public System.Action OnSleepTransitionStarted;
        public System.Action OnSleepTransitionCompleted;
        public System.Action OnWakeUpTransitionStarted;
        public System.Action OnWakeUpTransitionCompleted;
        
        private void Awake()
        {
            FindReferences();
            ValidateReferences();
            
            if (showDebugLogs)
                Debug.Log("[SleepTransitionController] Initialized");
        }
        
        private void FindReferences()
        {
            if (visualController == null)
                visualController = GetComponent<VisualTransitionController>();
            
            if (audioController == null)
                audioController = GetComponent<AudioTransitionController>();
        }
        
        private void ValidateReferences()
        {
            if (visualController == null)
            {
                Debug.LogError("[SleepTransitionController] VisualTransitionController reference is missing!");
            }
            
            if (audioController == null)
            {
                Debug.LogWarning("[SleepTransitionController] AudioTransitionController reference is missing!");
            }
        }
        
        public void StartSleepTransition(System.Action onComplete = null)
        {
            if (visualController == null)
            {
                Debug.LogError("[SleepTransitionController] Cannot start sleep transition - no VisualTransitionController!");
                onComplete?.Invoke();
                return;
            }
            
            if (IsTransitioning())
            {
                Debug.LogWarning("[SleepTransitionController] Sleep transition already in progress!");
                return;
            }
            
            onSleepComplete = onComplete;
            currentSleepTransition = StartCoroutine(SleepTransitionCoroutine());
            
            OnSleepTransitionStarted?.Invoke();
            
            if (showDebugLogs)
                Debug.Log("[SleepTransitionController] Sleep transition started");
        }
        
        public void StartWakeUpTransition(System.Action onComplete = null)
        {
            if (visualController == null)
            {
                Debug.LogError("[SleepTransitionController] Cannot start wake up transition - no VisualTransitionController!");
                onComplete?.Invoke();
                return;
            }
            
            if (IsTransitioning())
            {
                Debug.LogWarning("[SleepTransitionController] Wake up transition already in progress!");
                return;
            }
            
            onWakeUpComplete = onComplete;
            currentSleepTransition = StartCoroutine(WakeUpTransitionCoroutine());
            
            OnWakeUpTransitionStarted?.Invoke();
            
            if (showDebugLogs)
                Debug.Log("[SleepTransitionController] Wake up transition started");
        }
        
        private IEnumerator SleepTransitionCoroutine()
        {
            if (showDebugLogs)
                Debug.Log("[SleepTransitionController] Starting sleep transition");

            // Play sleep sound effect
            audioController?.PlaySleepSound();

            // Phase 1: Fade to black (falling asleep)
            yield return StartCoroutine(WaitForFade(visualController.GetFadeAlpha(), 1f, sleepTransitionDuration * 0.4f));

            // Phase 2: Hold black screen (sleeping)
            yield return new WaitForSeconds(sleepTransitionDuration * 0.2f);

            // Phase 3: Fade from black to normal (waking up)
            yield return StartCoroutine(WaitForFade(1f, 0f, sleepTransitionDuration * 0.4f));

            if (showDebugLogs)
                Debug.Log("[SleepTransitionController] Sleep transition completed");

            currentSleepTransition = null;
            onSleepComplete?.Invoke();
            OnSleepTransitionCompleted?.Invoke();
        }
        
        private IEnumerator WakeUpTransitionCoroutine()
        {
            if (showDebugLogs)
                Debug.Log("[SleepTransitionController] Starting wake up transition");

            // Play wake up sound effect
            audioController?.PlayWakeUpSound();

            // Phase 1: Quick flash of light (eyes opening)
            yield return StartCoroutine(WaitForFade(0f, 0.3f, 0.2f));
            yield return StartCoroutine(WaitForFade(0.3f, 0f, 0.2f));

            // Phase 2: Gradual fade to normal (adjusting to light)
            yield return StartCoroutine(WaitForFade(0f, 0.1f, 0.5f));
            yield return StartCoroutine(WaitForFade(0.1f, 0f, 0.8f));

            // Play morning ambient sound
            audioController?.PlayMorningAmbient();

            if (showDebugLogs)
                Debug.Log("[SleepTransitionController] Wake up transition completed");

            currentSleepTransition = null;
            onWakeUpComplete?.Invoke();
            OnWakeUpTransitionCompleted?.Invoke();
        }
        
        private IEnumerator WaitForFade(float startAlpha, float targetAlpha, float duration)
        {
            bool fadeCompleted = false;
            
            visualController.FadeToAlpha(targetAlpha, duration, () => {
                fadeCompleted = true;
            });
            
            while (!fadeCompleted && visualController.IsFading())
            {
                yield return null;
            }
        }
        
        public bool IsTransitioning()
        {
            return currentSleepTransition != null || (visualController != null && visualController.IsFading());
        }
        
        public void StopTransition()
        {
            if (currentSleepTransition != null)
            {
                StopCoroutine(currentSleepTransition);
                currentSleepTransition = null;
                
                if (showDebugLogs)
                    Debug.Log("[SleepTransitionController] Sleep transition stopped manually");
            }
            
            visualController?.StopFade();
        }
        
        public void SetSleepTransitionDuration(float duration)
        {
            sleepTransitionDuration = Mathf.Max(0.5f, duration);
        }
        
        public void SetWakeUpTransitionDuration(float duration)
        {
            wakeUpTransitionDuration = Mathf.Max(0.5f, duration);
        }
        
        // Compatibility method for TransitionManager
        public void SetTransitionDuration(float duration)
        {
            SetSleepTransitionDuration(duration);
        }
        
        public void SetVisualController(VisualTransitionController controller)
        {
            visualController = controller;
            ValidateReferences();
        }
        
        public void SetAudioController(AudioTransitionController controller)
        {
            audioController = controller;
            ValidateReferences();
        }
        
        [ContextMenu("Test Sleep Transition")]
        public void TestSleepTransition()
        {
            if (!IsTransitioning())
            {
                StartSleepTransition(() => {
                    if (showDebugLogs)
                        Debug.Log("[SleepTransitionController] Test sleep transition completed");
                });
            }
            else
            {
                Debug.LogWarning("[SleepTransitionController] Cannot test sleep transition - another transition is active");
            }
        }
        
        [ContextMenu("Test Wake Up Transition")]
        public void TestWakeUpTransition()
        {
            if (!IsTransitioning())
            {
                StartWakeUpTransition(() => {
                    if (showDebugLogs)
                        Debug.Log("[SleepTransitionController] Test wake up transition completed");
                });
            }
            else
            {
                Debug.LogWarning("[SleepTransitionController] Cannot test wake up transition - another transition is active");
            }
        }
        
        [ContextMenu("Stop Current Transition")]
        public void StopCurrentTransition()
        {
            StopTransition();
        }
    }
}

// ScriptRole: Handles specific sleep and wake up transitions with complex multi-phase sequences
// RelatedScripts: VisualTransitionController, AudioTransitionController, TransitionManager
// UsesSO: None
// ReceivesFrom: TransitionManager, DayNightManager events
// SendsTo: VisualTransitionController (fade effects), AudioTransitionController (sound effects), OnSleepTransitionStarted, OnSleepTransitionCompleted, OnWakeUpTransitionStarted, OnWakeUpTransitionCompleted events
