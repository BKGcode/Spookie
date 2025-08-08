using UnityEngine;

namespace GameStateController
{
    public class GameStateMessages : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private DayNightSystem.FeedbackMessagesSO feedbackMessages;
        [SerializeField] private DayNightSystem.MessageSystem messageSystem;

        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;

        private void Awake()
        {
            if (messageSystem == null) { messageSystem = FindObjectOfType<DayNightSystem.MessageSystem>(); }
            if (feedbackMessages == null) { feedbackMessages = FindObjectOfType<DayNightSystem.FeedbackMessagesSO>(); }

            if (messageSystem == null) { Debug.LogWarning("[GameStateMessages] MessageSystem not found"); }
            if (feedbackMessages == null) { Debug.LogWarning("[GameStateMessages] FeedbackMessagesSO not found"); }

            if (showDebugLogs)
            {
                Debug.Log("[GameStateMessages] Initialized");
            }
        }

        public void ShowNightMessage(float delay = 0.5f)
        {
            if (messageSystem == null) { return; }
            StartCoroutine(ShowDelayed("night_falling", string.Empty, 3f, delay));
        }

        public void ShowDayStartMessage(float delay = 0.5f)
        {
            if (messageSystem == null) { return; }
            StartCoroutine(ShowDelayed("new_day_started", string.Empty, 2f, delay));
        }

        private System.Collections.IEnumerator ShowDelayed(string key, string fallback, float duration, float delay)
        {
            yield return new WaitForSeconds(delay);
            string msg = feedbackMessages != null ? feedbackMessages.GetMessage(key) : string.Empty;
            if (!string.IsNullOrEmpty(msg))
            {
                messageSystem.ShowMessage(msg, duration);
            }

            if (showDebugLogs)
            {
                Debug.Log($"[GameStateMessages] Shown message: {key}");
            }
        }
    }
}

// ScriptRole: Centralizes game state related messages using FeedbackMessagesSO and MessageSystem
// RelatedScripts: GameStateManager, MessageSystem, FeedbackMessagesSO
// UsesSO: FeedbackMessagesSO
// ReceivesFrom: GameStateManager
// SendsTo: MessageSystem (UI)


