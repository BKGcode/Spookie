using UnityEngine;
using TMPro;
using System.Collections;
using DayNightSystem.Core;

namespace DayNightSystem
{
    public class MessageSystem : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI messageText;
        [SerializeField] private CanvasGroup messageCanvas;
        
        [Header("Audio")]
        [SerializeField] private AudioManager audioManager;
        
        [Header("Settings")]
        [SerializeField] private float defaultDuration = 3f;
        [SerializeField] private float fadeInDuration = 0.5f;
        [SerializeField] private float fadeOutDuration = 0.5f;
        
        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;
        
        // Private fields
        private Coroutine currentMessageCoroutine;
        private bool isMessageActive = false;
        
        // Public properties
        public bool IsMessageActive => isMessageActive;
        
        // Events
        public System.Action OnMessageStarted;
        public System.Action OnMessageCompleted;
        
        private void Awake()
        {
            ValidateReferences();
            InitializeMessageSystem();
        }
        
        private void ValidateReferences()
        {
            if (messageText == null)
            {
                Debug.LogError("[MessageSystem] MessageText reference is missing!");
            }
            
            if (messageCanvas == null)
            {
                Debug.LogError("[MessageSystem] MessageCanvas reference is missing!");
            }
            
            if (audioManager == null)
            {
                Debug.LogWarning("[MessageSystem] AudioManager reference is missing - no notification sounds");
            }
        }
        
        private void InitializeMessageSystem()
        {
            if (messageCanvas != null)
            {
                messageCanvas.alpha = 0f;
                messageCanvas.gameObject.SetActive(false);
            }
            
            if (showDebugLogs)
                Debug.Log("[MessageSystem] Initialized message system");
        }
        
        public void ShowMessage(string message, float customDuration = 0f)
        {
            if (string.IsNullOrEmpty(message))
            {
                Debug.LogWarning("[MessageSystem] Cannot show empty message");
                return;
            }
            
            // Stop any current message
            if (currentMessageCoroutine != null)
            {
                StopCoroutine(currentMessageCoroutine);
            }
            
            // Play notification sound
            if (audioManager != null)
            {
                audioManager.PlayMessageNotification();
            }
            
            // Start new message
            currentMessageCoroutine = StartCoroutine(ShowMessageCoroutine(message, customDuration));
            
            if (showDebugLogs)
                Debug.Log($"[MessageSystem] Showing message: {message}");
        }
        
        public void ShowNightMessage()
        {
            // Fetch from FeedbackMessagesSO if available in scene
            var feedback = FindObjectOfType<DayNightSystem.FeedbackMessagesSO>();
            string msg = feedback != null ? feedback.GetMessage("night_message") : string.Empty;
            if (!string.IsNullOrEmpty(msg))
            {
                ShowMessage(msg, 3f);
            }
            
            if (showDebugLogs)
                Debug.Log("[MessageSystem] Showing night message");
        }
        
        public void ShowNightMessage(string customMessage = null, float duration = 3f)
        {
            string nightMessage = customMessage;
            if (string.IsNullOrEmpty(nightMessage))
            {
                var feedback = FindObjectOfType<DayNightSystem.FeedbackMessagesSO>();
                nightMessage = feedback != null ? feedback.GetMessage("night_message") : string.Empty;
            }
            if (!string.IsNullOrEmpty(nightMessage))
            {
                ShowMessage(nightMessage, duration);
            }
            
            if (showDebugLogs)
                Debug.Log($"[MessageSystem] Showing night message: {nightMessage}");
        }
        
        private IEnumerator ShowMessageCoroutine(string message, float customDuration)
        {
            isMessageActive = true;
            OnMessageStarted?.Invoke();
            
            // Set message text
            if (messageText != null)
            {
                messageText.text = message;
            }
            
            // Show canvas
            if (messageCanvas != null)
            {
                messageCanvas.gameObject.SetActive(true);
            }
            
            // Fade in
            yield return StartCoroutine(FadeCanvasGroup(0f, 1f, fadeInDuration));
            
            // Display duration
            float displayTime = customDuration > 0f ? customDuration : defaultDuration;
            yield return new WaitForSeconds(displayTime);
            
            // Fade out
            yield return StartCoroutine(FadeCanvasGroup(1f, 0f, fadeOutDuration));
            
            // Hide canvas
            if (messageCanvas != null)
            {
                messageCanvas.gameObject.SetActive(false);
            }
            
            isMessageActive = false;
            currentMessageCoroutine = null;
            OnMessageCompleted?.Invoke();
            
            if (showDebugLogs)
                Debug.Log("[MessageSystem] Message completed");
        }
        
        private IEnumerator FadeCanvasGroup(float startAlpha, float targetAlpha, float duration)
        {
            if (messageCanvas == null) yield break;
            
            float elapsedTime = 0f;
            messageCanvas.alpha = startAlpha;
            
            while (elapsedTime < duration)
            {
                // Clamp delta time to prevent huge jumps
                float deltaTime = Mathf.Clamp(Time.deltaTime, 0f, 0.1f);
                elapsedTime += deltaTime;
                float progress = elapsedTime / duration;
                messageCanvas.alpha = Mathf.Lerp(startAlpha, targetAlpha, progress);
                yield return null;
            }
            
            messageCanvas.alpha = targetAlpha;
        }
        
        public void OnDayNightTransitionRequested()
        {
            if (isMessageActive)
            {
                if (showDebugLogs)
                    Debug.Log("[MessageSystem] Day/Night transition requested - force closing current message");
                
                // Force close current message after animation completes
                if (currentMessageCoroutine != null)
                {
                    StopCoroutine(currentMessageCoroutine);
                    StartCoroutine(ForceCloseMessage());
                }
            }
        }
        
        private IEnumerator ForceCloseMessage()
        {
            // Wait for current animation to complete
            yield return new WaitForSeconds(0.1f);
            
            // Fade out quickly
            if (messageCanvas != null)
            {
                yield return StartCoroutine(FadeCanvasGroup(messageCanvas.alpha, 0f, 0.2f));
                messageCanvas.gameObject.SetActive(false);
            }
            
            isMessageActive = false;
            currentMessageCoroutine = null;
            OnMessageCompleted?.Invoke();
            
            if (showDebugLogs)
                Debug.Log("[MessageSystem] Message force closed for day/night transition");
        }
        
        public void StopCurrentMessage()
        {
            if (currentMessageCoroutine != null)
            {
                StopCoroutine(currentMessageCoroutine);
                currentMessageCoroutine = null;
            }
            
            if (messageCanvas != null)
            {
                messageCanvas.alpha = 0f;
                messageCanvas.gameObject.SetActive(false);
            }
            
            isMessageActive = false;
            
            if (showDebugLogs)
                Debug.Log("[MessageSystem] Current message stopped");
        }
        
        // Public method to check if message is currently displaying
        public bool IsDisplayingMessage()
        {
            return isMessageActive;
        }
        
        // Public method to get current message text
        public string GetCurrentMessage()
        {
            if (messageText != null)
            {
                return messageText.text;
            }
            return "";
        }
        
        // Debug methods
        [ContextMenu("Show Test Message")]
        public void ShowTestMessage()
        {
            ShowMessage("This is a test message from MessageSystem!");
        }
        
        [ContextMenu("Force Close Message")]
        public void ForceCloseCurrentMessage()
        {
            OnDayNightTransitionRequested();
        }
        
        [ContextMenu("Show Message Status")]
        public void ShowMessageStatus()
        {
            string status = $"Message System Status:\n" +
                          $"Is Active: {isMessageActive}\n" +
                          $"Is Displaying: {IsDisplayingMessage()}\n" +
                          $"Current Message: {GetCurrentMessage()}\n" +
                          $"Canvas Alpha: {(messageCanvas != null ? messageCanvas.alpha.ToString("F2") : "N/A")}";
            
            Debug.Log($"[MessageSystem] {status}");
        }
    }
}

// ScriptRole: Manages UI messages and integrates with day/night transitions
// RelatedScripts: DayNightManager, GameStateManager, AudioManager
// UsesSO: None
// ReceivesFrom: DayNightManager (transition requests)
// SendsTo: TextMeshProUGUI, CanvasGroup, AudioManager (notification sounds)
