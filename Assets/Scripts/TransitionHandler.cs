using UnityEngine;
using System.Collections;

namespace DayNightSystem
{
    public enum TransitionType
    {
        DayToNight,
        NightToDay,
        Sleep,
        Faint
    }
    
    public class TransitionHandler : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CanvasGroup fadeCanvas;
        
        [Header("Audio")]
        [SerializeField] private AudioManager audioManager;
        
        [Header("Transition Settings")]
        [SerializeField] private float fadeDuration = 1f;
        [SerializeField] private float sleepTransitionDuration = 2f;
        [SerializeField] private float faintTransitionDuration = 0.5f;
        [SerializeField] private float timeoutDuration = 10f;
        
        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;
        
        // Private fields
        private DayNightManager dayNightManager;
        private GameStateManager gameStateManager;
        private Coroutine currentTransition;
        private TransitionType currentTransitionType;
        
        private void Awake()
        {
            ValidateReferences();
            FindDayNightManager();
            FindGameStateManager();
        }
        
        private void OnEnable()
        {
            if (dayNightManager != null)
            {
                dayNightManager.OnDayStart += OnDayStart;
                dayNightManager.OnNightStart += OnNightStart;
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
                dayNightManager.OnPlayerSlept -= OnPlayerSlept;
                dayNightManager.OnPlayerFainted -= OnPlayerFainted;
            }
        }
        
        private void ValidateReferences()
        {
            if (fadeCanvas == null)
            {
                Debug.LogError("[TransitionHandler] Fade Canvas reference is missing!");
            }
            
            if (audioManager == null)
            {
                Debug.LogError("[TransitionHandler] AudioManager reference is missing!");
            }
        }
        
        private void FindDayNightManager()
        {
            dayNightManager = FindObjectOfType<DayNightManager>();
            if (dayNightManager == null)
            {
                Debug.LogError("[TransitionHandler] No DayNightManager found in scene!");
            }
        }
        
        private void FindGameStateManager()
        {
            gameStateManager = FindObjectOfType<GameStateManager>();
            if (gameStateManager == null)
            {
                Debug.LogWarning("[TransitionHandler] No GameStateManager found in scene!");
            }
        }
        
        private void OnDayStart()
        {
            if (showDebugLogs)
                Debug.Log("[TransitionHandler] Day started - triggering fade transition");
            
            FadeFromBlack(TransitionType.NightToDay, () => {
                if (dayNightManager != null)
                    dayNightManager.SetTransitioning(false);
                
                if (gameStateManager != null)
                    gameStateManager.EndTransition();
            });
        }
        
        private void OnNightStart()
        {
            if (showDebugLogs)
                Debug.Log("[TransitionHandler] Night started - triggering fade transition");
            
            FadeToBlack(TransitionType.DayToNight, () => {
                if (dayNightManager != null)
                    dayNightManager.SetTransitioning(false);
                
                if (gameStateManager != null)
                    gameStateManager.EndTransition();
            });
        }
        
        private void OnPlayerSlept()
        {
            if (showDebugLogs)
                Debug.Log("[TransitionHandler] Player slept - triggering sleep transition");
            
            SleepTransition(() => {
                if (dayNightManager != null)
                    dayNightManager.SetTransitioning(false);
                
                if (gameStateManager != null)
                    gameStateManager.EndTransition();
            });
        }
        
        private void OnPlayerFainted()
        {
            if (showDebugLogs)
                Debug.Log("[TransitionHandler] Player fainted - triggering faint transition");
            
            FaintTransition(() => {
                if (dayNightManager != null)
                    dayNightManager.SetTransitioning(false);
                
                if (gameStateManager != null)
                    gameStateManager.EndTransition();
            });
        }
        
        public void FadeToBlack(TransitionType type = TransitionType.DayToNight, System.Action onComplete = null)
        {
            if (fadeCanvas == null)
            {
                Debug.LogError("[TransitionHandler] Cannot fade - no CanvasGroup assigned!");
                onComplete?.Invoke();
                return;
            }
            
            // Stop current transition if running
            if (currentTransition != null)
            {
                StopCoroutine(currentTransition);
            }
            
            currentTransitionType = type;
            currentTransition = StartCoroutine(FadeCoroutine(0f, 1f, fadeDuration, onComplete));
            
            if (showDebugLogs)
                Debug.Log($"[TransitionHandler] Started fade to black - Type: {type}");
        }
        
        public void FadeFromBlack(TransitionType type = TransitionType.NightToDay, System.Action onComplete = null)
        {
            if (fadeCanvas == null)
            {
                Debug.LogError("[TransitionHandler] Cannot fade - no CanvasGroup assigned!");
                onComplete?.Invoke();
                return;
            }
            
            // Stop current transition if running
            if (currentTransition != null)
            {
                StopCoroutine(currentTransition);
            }
            
            currentTransitionType = type;
            currentTransition = StartCoroutine(FadeCoroutine(1f, 0f, fadeDuration, onComplete));
            
            if (showDebugLogs)
                Debug.Log($"[TransitionHandler] Started fade from black - Type: {type}");
        }
        
        public void SleepTransition(System.Action onComplete = null)
        {
            if (fadeCanvas == null)
            {
                Debug.LogError("[TransitionHandler] Cannot sleep transition - no CanvasGroup assigned!");
                onComplete?.Invoke();
                return;
            }
            
            // Stop current transition if running
            if (currentTransition != null)
            {
                StopCoroutine(currentTransition);
            }
            
            currentTransitionType = TransitionType.Sleep;
            currentTransition = StartCoroutine(SleepTransitionCoroutine(onComplete));
            
            if (showDebugLogs)
                Debug.Log("[TransitionHandler] Started sleep transition");
        }
        
        public void FaintTransition(System.Action onComplete = null)
        {
            if (fadeCanvas == null)
            {
                Debug.LogError("[TransitionHandler] Cannot faint transition - no CanvasGroup assigned!");
                onComplete?.Invoke();
                return;
            }
            
            // Stop current transition if running
            if (currentTransition != null)
            {
                StopCoroutine(currentTransition);
            }
            
            currentTransitionType = TransitionType.Faint;
            currentTransition = StartCoroutine(FaintTransitionCoroutine(onComplete));
            
            if (showDebugLogs)
                Debug.Log("[TransitionHandler] Started faint transition");
        }
        
        private IEnumerator FadeCoroutine(float startAlpha, float targetAlpha, float duration, System.Action onComplete)
        {
            float elapsedTime = 0f;
            fadeCanvas.alpha = startAlpha;
            
            // Set timeout with clamped duration
            float clampedTimeout = Mathf.Clamp(timeoutDuration, 1f, 30f);
            Invoke("ForceCompleteTransition", clampedTimeout);
            
            while (elapsedTime < duration)
            {
                // Clamp delta time to prevent huge jumps
                float deltaTime = Mathf.Clamp(Time.deltaTime, 0f, 0.1f);
                elapsedTime += deltaTime;
                float progress = elapsedTime / duration;
                fadeCanvas.alpha = Mathf.Lerp(startAlpha, targetAlpha, progress);
                yield return null;
            }
            
            // Ensure final value
            fadeCanvas.alpha = targetAlpha;
            
            // Cancel timeout
            CancelInvoke("ForceCompleteTransition");
            
            currentTransition = null;
            onComplete?.Invoke();
            
            if (showDebugLogs)
                Debug.Log($"[TransitionHandler] Fade completed - Alpha: {targetAlpha:F2}, Type: {currentTransitionType}");
        }
        
        private IEnumerator SleepTransitionCoroutine(System.Action onComplete)
        {
            if (showDebugLogs)
                Debug.Log("[TransitionHandler] Starting sleep transition");

            // Play sleep sound effect
            if (audioManager != null)
            {
                audioManager.PlaySleepSound();
            }

            // Phase 1: Fade to black (falling asleep)
            yield return StartCoroutine(FadeCoroutine(fadeCanvas.alpha, 1f, sleepTransitionDuration * 0.4f, null));

            // Phase 2: Hold black screen (sleeping)
            yield return new WaitForSeconds(sleepTransitionDuration * 0.2f);

            // Phase 3: Fade from black to normal (waking up)
            yield return StartCoroutine(FadeCoroutine(1f, 0f, sleepTransitionDuration * 0.4f, null));

            if (showDebugLogs)
                Debug.Log("[TransitionHandler] Sleep transition completed");

            onComplete?.Invoke();
        }
        
        private IEnumerator WakeUpTransitionCoroutine(System.Action onComplete)
        {
            if (showDebugLogs)
                Debug.Log("[TransitionHandler] Starting wake up transition");

            // Play wake up sound effect
            if (audioManager != null)
            {
                audioManager.PlayWakeUpSound();
            }

            // Phase 1: Quick flash of light (eyes opening)
            yield return StartCoroutine(FadeCoroutine(0f, 0.3f, 0.2f, null));
            yield return StartCoroutine(FadeCoroutine(0.3f, 0f, 0.2f, null));

            // Phase 2: Gradual fade to normal (adjusting to light)
            yield return StartCoroutine(FadeCoroutine(0f, 0.1f, 0.5f, null));
            yield return StartCoroutine(FadeCoroutine(0.1f, 0f, 0.8f, null));

            // Play morning ambient sound
            if (audioManager != null)
            {
                audioManager.PlayMorningAmbient();
            }

            if (showDebugLogs)
                Debug.Log("[TransitionHandler] Wake up transition completed");

            onComplete?.Invoke();
        }
        
        private IEnumerator FaintTransitionCoroutine(System.Action onComplete)
        {
            if (showDebugLogs)
                Debug.Log("[TransitionHandler] Starting dramatic faint transition");
            
            // Play faint sound effect
            if (audioManager != null)
            {
                audioManager.PlayFaintSound();
            }
            
            // Phase 1: Quick fade to black (simulating loss of consciousness)
            yield return StartCoroutine(FadeCoroutine(fadeCanvas.alpha, 1f, faintTransitionDuration * 0.3f, null));
            
            // Phase 2: Hold black briefly (unconscious state)
            yield return new WaitForSeconds(0.5f);
            
            // Phase 3: Quick flash of light (brief consciousness)
            yield return StartCoroutine(FadeCoroutine(1f, 0.3f, 0.1f, null));
            yield return new WaitForSeconds(0.1f);
            
            // Phase 4: Final fade to black (complete unconsciousness)
            yield return StartCoroutine(FadeCoroutine(0.3f, 1f, faintTransitionDuration * 0.4f, null));
            
            // Phase 5: Hold black longer (extended unconscious state)
            yield return new WaitForSeconds(0.8f);
            
            // Phase 6: Gradual fade from black (waking up)
            yield return StartCoroutine(FadeCoroutine(1f, 0f, faintTransitionDuration * 0.8f, null));
            
            currentTransition = null;
            onComplete?.Invoke();
            
            if (showDebugLogs)
                Debug.Log("[TransitionHandler] Dramatic faint transition completed");
        }
        
        private void ForceCompleteTransition()
        {
            if (currentTransition != null)
            {
                StopCoroutine(currentTransition);
                currentTransition = null;
                
                if (showDebugLogs)
                    Debug.LogWarning($"[TransitionHandler] Transition timed out - forcing completion. Type: {currentTransitionType}");
                
                // Force complete the transition based on type
                if (fadeCanvas != null)
                {
                    switch (currentTransitionType)
                    {
                        case TransitionType.DayToNight:
                        case TransitionType.Sleep:
                        case TransitionType.Faint:
                            fadeCanvas.alpha = 0f; // End with clear screen
                            break;
                        case TransitionType.NightToDay:
                            fadeCanvas.alpha = 0f; // End with clear screen
                            break;
                    }
                }
            }
        }
        
        // Public method to check if transition is active
        public bool IsTransitioning()
        {
            return currentTransition != null;
        }
        
        // Public method to get current transition type
        public TransitionType GetCurrentTransitionType()
        {
            return currentTransitionType;
        }
        
        // Public method to force stop transition
        public void StopTransition()
        {
            if (currentTransition != null)
            {
                StopCoroutine(currentTransition);
                currentTransition = null;
                
                if (showDebugLogs)
                    Debug.Log($"[TransitionHandler] Transition stopped manually - Type: {currentTransitionType}");
            }
        }
        
        // Public method to set fade alpha directly
        public void SetFadeAlpha(float alpha)
        {
            if (fadeCanvas != null)
            {
                fadeCanvas.alpha = Mathf.Clamp01(alpha);
            }
        }
        
        // Public method to get current fade alpha
        public float GetFadeAlpha()
        {
            return fadeCanvas != null ? fadeCanvas.alpha : 0f;
        }
        
        // Public methods for audio configuration
        public void SetFaintSound(AudioClip sound)
        {
            // This method is no longer needed as audio is managed by AudioManager
            Debug.LogWarning("[TransitionHandler] SetFaintSound is deprecated. Use AudioManager instead.");
        }
        
        public void SetSleepSound(AudioClip sound)
        {
            // This method is no longer needed as audio is managed by AudioManager
            Debug.LogWarning("[TransitionHandler] SetSleepSound is deprecated. Use AudioManager instead.");
        }
        
        public void SetAudioSource(AudioSource audioSource)
        {
            // This method is no longer needed as audio is managed by AudioManager
            Debug.LogWarning("[TransitionHandler] SetAudioSource is deprecated. Use AudioManager instead.");
        }
        
        // Public method to test faint transition
        [ContextMenu("Test Faint Transition")]
        public void TestFaintTransition()
        {
            if (!IsTransitioning())
            {
                FaintTransition(() => {
                    if (showDebugLogs)
                        Debug.Log("[TransitionHandler] Test faint transition completed");
                });
            }
            else
            {
                Debug.LogWarning("[TransitionHandler] Cannot test faint transition - another transition is active");
            }
        }
        
        // Public method to test sleep transition
        [ContextMenu("Test Sleep Transition")]
        public void TestSleepTransition()
        {
            if (!IsTransitioning())
            {
                SleepTransition(() => {
                    if (showDebugLogs)
                        Debug.Log("[TransitionHandler] Test sleep transition completed");
                });
            }
            else
            {
                Debug.LogWarning("[TransitionHandler] Cannot test sleep transition - another transition is active");
            }
        }

        public void StartWakeUpTransition(System.Action onComplete = null)
        {
            if (currentTransition != null)
            {
                if (showDebugLogs)
                    Debug.LogWarning("[TransitionHandler] Transition already in progress, ignoring wake up request");
                return;
            }

            currentTransition = StartCoroutine(WakeUpTransitionCoroutine(onComplete));
            
            if (showDebugLogs)
                Debug.Log("[TransitionHandler] Started wake up transition");
        }

        [ContextMenu("Test Wake Up Transition")]
        public void TestWakeUpTransition()
        {
            StartWakeUpTransition(() => {
                if (showDebugLogs)
                    Debug.Log("[TransitionHandler] Test wake up transition completed");
            });
        }
    }
}

// ScriptRole: Handles visual and audio transitions with specific types for sleep/faint scenarios
// RelatedScripts: DayNightManager, GameStateManager
// UsesSO: None
// ReceivesFrom: DayNightManager events
// SendsTo: CanvasGroup (fade effects), AudioManager (sound effects)
