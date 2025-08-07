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
            // Fade to black quickly
            yield return StartCoroutine(FadeCoroutine(fadeCanvas.alpha, 1f, 0.5f, null));
            
            // Hold black for sleep duration
            yield return new WaitForSeconds(sleepTransitionDuration);
            
            // Fade from black
            yield return StartCoroutine(FadeCoroutine(1f, 0f, 0.5f, null));
            
            currentTransition = null;
            onComplete?.Invoke();
            
            if (showDebugLogs)
                Debug.Log("[TransitionHandler] Sleep transition completed");
        }
        
        private IEnumerator FaintTransitionCoroutine(System.Action onComplete)
        {
            // Quick fade to black
            yield return StartCoroutine(FadeCoroutine(fadeCanvas.alpha, 1f, faintTransitionDuration, null));
            
            // Hold black briefly
            yield return new WaitForSeconds(0.2f);
            
            // Quick fade from black
            yield return StartCoroutine(FadeCoroutine(1f, 0f, faintTransitionDuration, null));
            
            currentTransition = null;
            onComplete?.Invoke();
            
            if (showDebugLogs)
                Debug.Log("[TransitionHandler] Faint transition completed");
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
    }
}

// ScriptRole: Handles visual transitions with specific types for sleep/faint scenarios
// RelatedScripts: DayNightManager, GameStateManager
// UsesSO: None
// ReceivesFrom: DayNightManager events
// SendsTo: CanvasGroup (fade effects)
