using UnityEngine;
using System.Collections;
using DayNightSystem.Core;
using DayNightSystem;

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
        private DayNightSystem.Core.DayNightManager dayNightManager;
        private GameStateManager gameStateManager;
        private Coroutine currentTransition;
        private TransitionType currentTransitionType;
        [SerializeField] private TransitionSystem.Visual.VisualTransitionController visual; // Delegation (optional)
        
        private void Awake()
        {
            // If modular TransitionSystem exists, prefer it and disable this handler to avoid duplicate transitions
            var transitionManager = FindObjectOfType<TransitionSystem.Core.TransitionManager>();
            if (transitionManager != null)
            {
                if (showDebugLogs) Debug.LogWarning("[TransitionHandler] TransitionSystem.Core.TransitionManager detected. Disabling legacy TransitionHandler to prevent duplicate transitions.");
                enabled = false;
                return;
            }
            ValidateReferences();
            FindDayNightManager();
            FindGameStateManager();
            if (visual == null) { visual = GetComponent<TransitionSystem.Visual.VisualTransitionController>(); }
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
            dayNightManager = FindObjectOfType<DayNightSystem.Core.DayNightManager>();
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
                Debug.Log("[TransitionHandler] Day started");
            
            if (gameStateManager != null)
            {
                gameStateManager.EndTransition();
            }
            
            if (dayNightManager != null)
            {
                dayNightManager.SetTransitioning(false);
            }
        }
        
        private void OnNightStart()
        {
            if (showDebugLogs)
                Debug.Log("[TransitionHandler] Night started");
            
            if (gameStateManager != null)
            {
                gameStateManager.EndTransition();
            }
            
            if (dayNightManager != null)
            {
                dayNightManager.SetTransitioning(false);
            }
        }
        
        private void OnPlayerSlept()
        {
            if (showDebugLogs)
                Debug.Log("[TransitionHandler] Player slept");
            
            if (gameStateManager != null)
            {
                gameStateManager.EndTransition();
            }
            
            if (dayNightManager != null)
            {
                dayNightManager.SetTransitioning(false);
            }
        }
        
        private void OnPlayerFainted()
        {
            if (showDebugLogs)
                Debug.Log("[TransitionHandler] Player fainted");
            
            if (gameStateManager != null)
            {
                gameStateManager.EndTransition();
            }
            
            if (dayNightManager != null)
            {
                dayNightManager.SetTransitioning(false);
            }
        }
        
        public void FadeToBlack(TransitionType type = TransitionType.DayToNight, System.Action onComplete = null)
        {
            if (IsTransitioning())
            {
                Debug.LogWarning("[TransitionHandler] Transition already in progress!");
                return;
            }
            
            currentTransitionType = type;
            if (visual != null) { visual.FadeToBlack(fadeDuration, onComplete); return; }
            currentTransition = StartCoroutine(FadeCoroutine(0f, 1f, fadeDuration, onComplete));
            
            if (showDebugLogs)
                Debug.Log($"[TransitionHandler] Started fade to black - Type: {type}");
        }
        
        public void FadeFromBlack(TransitionType type = TransitionType.NightToDay, System.Action onComplete = null)
        {
            if (IsTransitioning())
            {
                Debug.LogWarning("[TransitionHandler] Transition already in progress!");
                return;
            }
            
            currentTransitionType = type;
            if (visual != null) { visual.FadeFromBlack(fadeDuration, onComplete); return; }
            currentTransition = StartCoroutine(FadeCoroutine(1f, 0f, fadeDuration, onComplete));
            
            if (showDebugLogs)
                Debug.Log($"[TransitionHandler] Started fade from black - Type: {type}");
        }
        
        public void SleepTransition(System.Action onComplete = null)
        {
            if (IsTransitioning())
            {
                Debug.LogWarning("[TransitionHandler] Transition already in progress!");
                return;
            }
            
            currentTransitionType = TransitionType.Sleep;
            currentTransition = StartCoroutine(SleepTransitionCoroutine(onComplete));
            
            if (showDebugLogs)
                Debug.Log("[TransitionHandler] Started sleep transition");
        }
        
        public void FaintTransition(System.Action onComplete = null)
        {
            if (IsTransitioning())
            {
                Debug.LogWarning("[TransitionHandler] Transition already in progress!");
                return;
            }
            
            currentTransitionType = TransitionType.Faint;
            currentTransition = StartCoroutine(FaintTransitionCoroutine(onComplete));
            
            if (showDebugLogs)
                Debug.Log("[TransitionHandler] Started faint transition");
        }
        
        private IEnumerator FadeCoroutine(float startAlpha, float targetAlpha, float duration, System.Action onComplete)
        {
            if (fadeCanvas == null)
            {
                Debug.LogError("[TransitionHandler] Cannot fade - no CanvasGroup assigned!");
                onComplete?.Invoke();
                yield break;
            }
            
            float elapsed = 0f;
            fadeCanvas.alpha = startAlpha;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / duration;
                fadeCanvas.alpha = Mathf.Lerp(startAlpha, targetAlpha, progress);
                yield return null;
            }
            
            fadeCanvas.alpha = targetAlpha;
            currentTransition = null;
            onComplete?.Invoke();
            
            if (showDebugLogs)
                Debug.Log("[TransitionHandler] Fade transition completed");
        }
        
        private IEnumerator SleepTransitionCoroutine(System.Action onComplete)
        {
            if (fadeCanvas == null)
            {
                Debug.LogError("[TransitionHandler] Cannot perform sleep transition - no CanvasGroup assigned!");
                onComplete?.Invoke();
                yield break;
            }
            
            // Phase 1: Fade to black
            yield return StartCoroutine(FadeCoroutine(0f, 1f, sleepTransitionDuration * 0.3f, null));
            
            // Phase 2: Hold black briefly
            yield return new WaitForSeconds(sleepTransitionDuration * 0.4f);
            
            // Phase 3: Play sleep sound
            if (audioManager != null)
            {
                audioManager.PlaySleepSound();
            }
            
            // Phase 4: Hold black for sleep duration
            yield return new WaitForSeconds(sleepTransitionDuration * 0.3f);
            
            currentTransition = null;
            onComplete?.Invoke();
            
            if (showDebugLogs)
                Debug.Log("[TransitionHandler] Sleep transition completed");
        }
        
        private IEnumerator WakeUpTransitionCoroutine(System.Action onComplete)
        {
            if (fadeCanvas == null)
            {
                Debug.LogError("[TransitionHandler] Cannot perform wake up transition - no CanvasGroup assigned!");
                onComplete?.Invoke();
                yield break;
            }
            
            // Phase 1: Play wake up sound
            if (audioManager != null)
            {
                audioManager.PlayWakeUpSound();
            }
            
            // Phase 2: Fade from black
            yield return StartCoroutine(FadeCoroutine(1f, 0f, sleepTransitionDuration * 0.7f, null));
            
            currentTransition = null;
            onComplete?.Invoke();
            
            if (showDebugLogs)
                Debug.Log("[TransitionHandler] Wake up transition completed");
        }
        
        private IEnumerator FaintTransitionCoroutine(System.Action onComplete)
        {
            if (fadeCanvas == null)
            {
                Debug.LogError("[TransitionHandler] Cannot perform faint transition - no CanvasGroup assigned!");
                onComplete?.Invoke();
                yield break;
            }
            
            // Phase 1: Quick fade to black (simulating loss of consciousness)
            yield return StartCoroutine(FadeCoroutine(0f, 1f, faintTransitionDuration * 0.3f, null));
            
            // Phase 2: Hold black briefly (unconscious state)
            yield return new WaitForSeconds(faintTransitionDuration * 0.4f);
            
            // Phase 3: Play faint sound
            if (audioManager != null)
            {
                audioManager.PlayFaintSound();
            }
            
            // Phase 4: Hold black for unconscious duration
            yield return new WaitForSeconds(faintTransitionDuration * 0.3f);
            
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
                
                if (fadeCanvas != null)
                {
                    fadeCanvas.alpha = 1f; // Force to black
                }
                
                if (showDebugLogs)
                    Debug.Log("[TransitionHandler] Transition force completed");
            }
        }
        
        public bool IsTransitioning()
        {
            return currentTransition != null;
        }
        
        public TransitionType GetCurrentTransitionType()
        {
            return currentTransitionType;
        }
        
        public void StopTransition()
        {
            if (currentTransition != null)
            {
                StopCoroutine(currentTransition);
                currentTransition = null;
                
                if (showDebugLogs)
                    Debug.Log("[TransitionHandler] Transition stopped");
            }
        }
        
        public void SetFadeAlpha(float alpha)
        {
            if (visual != null) { visual.SetFadeAlpha(alpha); return; }
            if (fadeCanvas != null)
            {
                fadeCanvas.alpha = Mathf.Clamp01(alpha);
                if (showDebugLogs) Debug.Log($"[TransitionHandler] Fade alpha set to: {alpha}");
            }
        }
        
        public float GetFadeAlpha()
        {
            if (visual != null) { return visual.GetFadeAlpha(); }
            return fadeCanvas != null ? fadeCanvas.alpha : 0f;
        }
        
        
        // Deprecated methods - kept for backward compatibility
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
        
        [ContextMenu("Test Faint Transition")]
        public void TestFaintTransition()
        {
            if (IsTransitioning())
            {
                Debug.LogWarning("[TransitionHandler] Cannot test - transition already in progress!");
                return;
            }
            
            FaintTransition(() => {
                if (showDebugLogs)
                    Debug.Log("[TransitionHandler] Test faint transition completed");
            });
        }
        
        [ContextMenu("Test Sleep Transition")]
        public void TestSleepTransition()
        {
            if (IsTransitioning())
            {
                Debug.LogWarning("[TransitionHandler] Cannot test - transition already in progress!");
                return;
            }
            
            SleepTransition(() => {
                if (showDebugLogs)
                    Debug.Log("[TransitionHandler] Test sleep transition completed");
            });
        }
        
        public void StartWakeUpTransition(System.Action onComplete = null)
        {
            if (IsTransitioning())
            {
                Debug.LogWarning("[TransitionHandler] Transition already in progress!");
                return;
            }
            
            currentTransitionType = TransitionType.Sleep; // Reuse sleep type for wake up
            currentTransition = StartCoroutine(WakeUpTransitionCoroutine(onComplete));
            
            if (showDebugLogs)
                Debug.Log("[TransitionHandler] Started wake up transition");
        }
        
        [ContextMenu("Test Wake Up Transition")]
        public void TestWakeUpTransition()
        {
            if (IsTransitioning())
            {
                Debug.LogWarning("[TransitionHandler] Cannot test - transition already in progress!");
                return;
            }
            
            StartWakeUpTransition(() => {
                if (showDebugLogs)
                    Debug.Log("[TransitionHandler] Test wake up transition completed");
            });
        }
        
        [ContextMenu("Force Complete Transition")]
        public void ForceCompleteTransitionContext()
        {
            ForceCompleteTransition();
        }
        
        [ContextMenu("Stop Current Transition")]
        public void StopCurrentTransition()
        {
            StopTransition();
        }
        
        [ContextMenu("Show Transition Status")]
        public void ShowTransitionStatus()
        {
            Debug.Log($"[TransitionHandler] Status:\n" +
                     $"Is Transitioning: {IsTransitioning()}\n" +
                     $"Current Type: {GetCurrentTransitionType()}\n" +
                     $"Fade Alpha: {GetFadeAlpha():F2}\n" +
                     $"Fade Canvas: {(fadeCanvas != null ? "Found" : "Missing")}\n" +
                     $"Audio Manager: {(audioManager != null ? "Found" : "Missing")}\n" +
                     $"DayNight Manager: {(dayNightManager != null ? "Found" : "Missing")}\n" +
                     $"GameState Manager: {(gameStateManager != null ? "Found" : "Missing")}");
        }
    }
}

// ScriptRole: Handles visual transitions between day/night states and player actions
// RelatedScripts: DayNightManager, GameStateManager
// UsesSO: None
// ReceivesFrom: DayNightManager events
// SendsTo: CanvasGroup (fade effects), AudioManager (sound effects)
