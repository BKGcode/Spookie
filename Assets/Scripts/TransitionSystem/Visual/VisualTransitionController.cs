using UnityEngine;
using System.Collections;

namespace TransitionSystem.Visual
{
    public class VisualTransitionController : MonoBehaviour
    {
        [Header("Visual References")]
        [SerializeField] private CanvasGroup fadeCanvas;
        
        [Header("Fade Settings")]
        [SerializeField] private float defaultFadeDuration = 1f;
        [SerializeField] private float timeoutDuration = 10f;
        
        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;
        
        // Private fields
        private Coroutine currentFadeCoroutine;
        private System.Action onFadeComplete;
        
        // Events
        public System.Action<float> OnFadeProgressChanged;
        public System.Action OnFadeStarted;
        public System.Action OnFadeCompleted;
        
        private void Awake()
        {
            ValidateReferences();
            
            if (showDebugLogs)
                Debug.Log("[VisualTransitionController] Initialized");
        }
        
        private void ValidateReferences()
        {
            if (fadeCanvas == null)
            {
                Debug.LogError("[VisualTransitionController] Fade Canvas reference is missing!");
            }
        }
        
        public void FadeToBlack(float duration = -1f, System.Action onComplete = null)
        {
            if (fadeCanvas == null)
            {
                Debug.LogError("[VisualTransitionController] Cannot fade - no CanvasGroup assigned!");
                onComplete?.Invoke();
                return;
            }
            
            float fadeDuration = duration > 0 ? duration : defaultFadeDuration;
            StartFade(0f, 1f, fadeDuration, onComplete);
            
            if (showDebugLogs)
                Debug.Log($"[VisualTransitionController] Started fade to black - Duration: {fadeDuration}");
        }
        
        public void FadeFromBlack(float duration = -1f, System.Action onComplete = null)
        {
            if (fadeCanvas == null)
            {
                Debug.LogError("[VisualTransitionController] Cannot fade - no CanvasGroup assigned!");
                onComplete?.Invoke();
                return;
            }
            
            float fadeDuration = duration > 0 ? duration : defaultFadeDuration;
            StartFade(1f, 0f, fadeDuration, onComplete);
            
            if (showDebugLogs)
                Debug.Log($"[VisualTransitionController] Started fade from black - Duration: {fadeDuration}");
        }
        
        public void FadeToAlpha(float targetAlpha, float duration = -1f, System.Action onComplete = null)
        {
            if (fadeCanvas == null)
            {
                Debug.LogError("[VisualTransitionController] Cannot fade - no CanvasGroup assigned!");
                onComplete?.Invoke();
                return;
            }
            
            float fadeDuration = duration > 0 ? duration : defaultFadeDuration;
            StartFade(fadeCanvas.alpha, targetAlpha, fadeDuration, onComplete);
            
            if (showDebugLogs)
                Debug.Log($"[VisualTransitionController] Started fade to alpha {targetAlpha} - Duration: {fadeDuration}");
        }
        
        private void StartFade(float startAlpha, float targetAlpha, float duration, System.Action onComplete)
        {
            // Stop current fade if running
            if (currentFadeCoroutine != null)
            {
                StopCoroutine(currentFadeCoroutine);
            }
            
            onFadeComplete = onComplete;
            currentFadeCoroutine = StartCoroutine(FadeCoroutine(startAlpha, targetAlpha, duration));
            
            OnFadeStarted?.Invoke();
        }
        
        private IEnumerator FadeCoroutine(float startAlpha, float targetAlpha, float duration)
        {
            if (fadeCanvas == null)
            {
                Debug.LogError("[VisualTransitionController] Cannot fade - no CanvasGroup assigned!");
                onFadeComplete?.Invoke();
                yield break;
            }
            
            float elapsed = 0f;
            fadeCanvas.alpha = startAlpha;
            
            // Set timeout
            Invoke("ForceCompleteFade", timeoutDuration);
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / duration;
                fadeCanvas.alpha = Mathf.Lerp(startAlpha, targetAlpha, progress);
                
                OnFadeProgressChanged?.Invoke(progress);
                yield return null;
            }
            
            // Ensure final value
            fadeCanvas.alpha = targetAlpha;
            
            // Cancel timeout
            CancelInvoke("ForceCompleteFade");
            
            currentFadeCoroutine = null;
            onFadeComplete?.Invoke();
            OnFadeCompleted?.Invoke();
            
            if (showDebugLogs)
                Debug.Log($"[VisualTransitionController] Fade completed - Alpha: {targetAlpha:F2}");
        }
        
        private void ForceCompleteFade()
        {
            if (currentFadeCoroutine != null)
            {
                StopCoroutine(currentFadeCoroutine);
                currentFadeCoroutine = null;
                
                if (fadeCanvas != null)
                {
                    fadeCanvas.alpha = 1f; // Force to black
                }
                
                onFadeComplete?.Invoke();
                OnFadeCompleted?.Invoke();
                
                if (showDebugLogs)
                    Debug.LogWarning("[VisualTransitionController] Fade timed out - forcing completion");
            }
        }
        
        public bool IsFading()
        {
            return currentFadeCoroutine != null;
        }
        
        // Compatibility method for TransitionManager
        public bool IsTransitioning()
        {
            return IsFading();
        }
        
        public void StopFade()
        {
            if (currentFadeCoroutine != null)
            {
                StopCoroutine(currentFadeCoroutine);
                currentFadeCoroutine = null;
                
                if (showDebugLogs)
                    Debug.Log("[VisualTransitionController] Fade stopped");
            }
        }
        
        // Compatibility method for TransitionManager
        public void StopTransition()
        {
            StopFade();
        }
        
        public void SetFadeAlpha(float alpha)
        {
            if (fadeCanvas != null)
            {
                fadeCanvas.alpha = Mathf.Clamp01(alpha);
            }
        }
        
        public float GetFadeAlpha()
        {
            return fadeCanvas != null ? fadeCanvas.alpha : 0f;
        }
        
        public void SetFadeCanvas(CanvasGroup canvas)
        {
            fadeCanvas = canvas;
        }
        
        public void SetDefaultFadeDuration(float duration)
        {
            defaultFadeDuration = Mathf.Max(0.1f, duration);
        }
        
        public void SetTimeoutDuration(float duration)
        {
            timeoutDuration = Mathf.Max(1f, duration);
        }
        
        [ContextMenu("Test Fade To Black")]
        public void TestFadeToBlack()
        {
            if (IsFading())
            {
                Debug.LogWarning("[VisualTransitionController] Cannot test - fade already in progress!");
                return;
            }
            
            FadeToBlack(2f, () => {
                if (showDebugLogs)
                    Debug.Log("[VisualTransitionController] Test fade to black completed");
            });
        }
        
        [ContextMenu("Test Fade From Black")]
        public void TestFadeFromBlack()
        {
            if (IsFading())
            {
                Debug.LogWarning("[VisualTransitionController] Cannot test - fade already in progress!");
                return;
            }
            
            FadeFromBlack(2f, () => {
                if (showDebugLogs)
                    Debug.Log("[VisualTransitionController] Test fade from black completed");
            });
        }
        
        [ContextMenu("Stop Current Fade")]
        public void StopCurrentFade()
        {
            StopFade();
        }
        
        [ContextMenu("Show Current Alpha")]
        public void ShowCurrentAlpha()
        {
            Debug.Log($"[VisualTransitionController] Current alpha: {GetFadeAlpha():F2}");
        }
    }
}

// ScriptRole: Manages visual fade transitions for the transition system
// RelatedScripts: TransitionManager, SleepTransitionController, FaintTransitionController
// UsesSO: None
// ReceivesFrom: TransitionManager, SleepTransitionController, FaintTransitionController
// SendsTo: CanvasGroup (fade effects)
