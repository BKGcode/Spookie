using UnityEngine;
using System.Collections;
using TransitionSystem.Visual;
using TransitionSystem.Audio;

namespace TransitionSystem.Specific
{
    public class FaintTransitionController : MonoBehaviour
    {
        [Header("Component References")]
        [SerializeField] private VisualTransitionController visualController;
        [SerializeField] private AudioTransitionController audioController;
        
        [Header("Faint Settings")]
        [SerializeField] private float faintTransitionDuration = 0.5f;
        [SerializeField] private float unconsciousHoldDuration = 0.8f;
        [SerializeField] private float consciousnessFlashDuration = 0.1f;
        
        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;
        
        // Private fields
        private Coroutine currentFaintTransition;
        private System.Action onFaintComplete;
        
        // Events
        public System.Action OnFaintTransitionStarted;
        public System.Action OnFaintTransitionCompleted;
        public System.Action OnConsciousnessLost;
        public System.Action OnConsciousnessRegained;
        
        private void Awake()
        {
            FindReferences();
            ValidateReferences();
            
            if (showDebugLogs)
                Debug.Log("[FaintTransitionController] Initialized");
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
                Debug.LogError("[FaintTransitionController] VisualTransitionController reference is missing!");
            }
            
            if (audioController == null)
            {
                Debug.LogWarning("[FaintTransitionController] AudioTransitionController reference is missing!");
            }
        }
        
        public void StartFaintTransition(System.Action onComplete = null)
        {
            if (visualController == null)
            {
                Debug.LogError("[FaintTransitionController] Cannot start faint transition - no VisualTransitionController!");
                onComplete?.Invoke();
                return;
            }
            
            if (IsTransitioning())
            {
                Debug.LogWarning("[FaintTransitionController] Faint transition already in progress!");
                return;
            }
            
            onFaintComplete = onComplete;
            currentFaintTransition = StartCoroutine(FaintTransitionCoroutine());
            
            OnFaintTransitionStarted?.Invoke();
            
            if (showDebugLogs)
                Debug.Log("[FaintTransitionController] Faint transition started");
        }
        
        private IEnumerator FaintTransitionCoroutine()
        {
            if (showDebugLogs)
                Debug.Log("[FaintTransitionController] Starting dramatic faint transition");
            
            // Play faint sound effect
            audioController?.PlayFaintSound();
            
            // Phase 1: Quick fade to black (simulating loss of consciousness)
            yield return StartCoroutine(WaitForFade(visualController.GetFadeAlpha(), 1f, faintTransitionDuration * 0.3f));
            
            OnConsciousnessLost?.Invoke();
            
            // Phase 2: Hold black briefly (unconscious state)
            yield return new WaitForSeconds(unconsciousHoldDuration);
            
            // Phase 3: Quick flash of light (brief consciousness)
            yield return StartCoroutine(WaitForFade(1f, 0.3f, consciousnessFlashDuration));
            yield return new WaitForSeconds(consciousnessFlashDuration);
            
            // Phase 4: Final fade to black (complete unconsciousness)
            yield return StartCoroutine(WaitForFade(0.3f, 1f, faintTransitionDuration * 0.4f));
            
            // Phase 5: Hold black longer (extended unconscious state)
            yield return new WaitForSeconds(unconsciousHoldDuration * 1.5f);
            
            // Phase 6: Gradual fade from black (waking up)
            yield return StartCoroutine(WaitForFade(1f, 0f, faintTransitionDuration * 0.8f));
            
            OnConsciousnessRegained?.Invoke();
            
            if (showDebugLogs)
                Debug.Log("[FaintTransitionController] Dramatic faint transition completed");
            
            currentFaintTransition = null;
            onFaintComplete?.Invoke();
            OnFaintTransitionCompleted?.Invoke();
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
            return currentFaintTransition != null || (visualController != null && visualController.IsFading());
        }
        
        public void StopTransition()
        {
            if (currentFaintTransition != null)
            {
                StopCoroutine(currentFaintTransition);
                currentFaintTransition = null;
                
                if (showDebugLogs)
                    Debug.Log("[FaintTransitionController] Faint transition stopped manually");
            }
            
            visualController?.StopFade();
        }
        
        public void SetFaintTransitionDuration(float duration)
        {
            faintTransitionDuration = Mathf.Max(0.1f, duration);
        }
        
        public void SetUnconsciousHoldDuration(float duration)
        {
            unconsciousHoldDuration = Mathf.Max(0.1f, duration);
        }
        
        public void SetConsciousnessFlashDuration(float duration)
        {
            consciousnessFlashDuration = Mathf.Max(0.05f, duration);
        }
        
        // Compatibility method for TransitionManager
        public void SetTransitionDuration(float duration)
        {
            SetFaintTransitionDuration(duration);
        }
        
        public void SetVisualController(VisualTransitionController controller)
        {
            visualController = controller;
        }
        
        public void SetAudioController(AudioTransitionController controller)
        {
            audioController = controller;
        }
        
        [ContextMenu("Test Faint Transition")]
        public void TestFaintTransition()
        {
            if (!IsTransitioning())
            {
                StartFaintTransition(() => {
                    if (showDebugLogs)
                        Debug.Log("[FaintTransitionController] Test faint transition completed");
                });
            }
            else
            {
                Debug.LogWarning("[FaintTransitionController] Cannot test faint transition - another transition is active");
            }
        }
        
        [ContextMenu("Stop Current Transition")]
        public void StopCurrentTransition()
        {
            StopTransition();
        }
        
        [ContextMenu("Simulate Consciousness Loss")]
        public void SimulateConsciousnessLoss()
        {
            OnConsciousnessLost?.Invoke();
            
            if (showDebugLogs)
                Debug.Log("[FaintTransitionController] Simulated consciousness loss");
        }
        
        [ContextMenu("Simulate Consciousness Regain")]
        public void SimulateConsciousnessRegain()
        {
            OnConsciousnessRegained?.Invoke();
            
            if (showDebugLogs)
                Debug.Log("[FaintTransitionController] Simulated consciousness regain");
        }
    }
}

// ScriptRole: Handles specific faint transitions with dramatic multi-phase sequences simulating loss of consciousness
// RelatedScripts: VisualTransitionController, AudioTransitionController, TransitionManager
// UsesSO: None
// ReceivesFrom: TransitionManager, DayNightManager events
// SendsTo: VisualTransitionController (fade effects), AudioTransitionController (sound effects), OnFaintTransitionStarted, OnFaintTransitionCompleted, OnConsciousnessLost, OnConsciousnessRegained events
