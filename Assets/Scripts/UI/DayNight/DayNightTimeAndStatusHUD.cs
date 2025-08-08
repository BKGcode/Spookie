using UnityEngine;
using TMPro;

namespace DayNightSystem.UI.DayNight
{
    public class DayNightTimeAndStatusHUD : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI timeDisplay;
        [SerializeField] private TextMeshProUGUI statusDisplay;

        [Header("References")]
        [SerializeField] private DayNightSystem.FeedbackMessagesSO feedbackMessages;

        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;

        public void UpdateTime(float timeNormalized)
        {
            if (timeDisplay == null) { return; }

            float totalSeconds = Mathf.Clamp01(timeNormalized) * 1440f;
            int minutes = Mathf.FloorToInt(totalSeconds / 60f);
            int seconds = Mathf.FloorToInt(totalSeconds % 60f);
            string timeString = string.Format("{0:D2}:{1:D2}", minutes, seconds);
            timeDisplay.text = timeString;

            if (showDebugLogs && Mathf.FloorToInt(totalSeconds) % 60 == 0)
            {
                Debug.Log($"[DayNightTimeAndStatusHUD] Time updated: {timeString}");
            }
        }

        public void UpdateStatus(DayNightSystem.Core.DayNightManager dayNightManager, DayNightSystem.PlayerPenalty playerPenalty)
        {
            if (statusDisplay == null) { return; }

            string statusText = string.Empty;
            if (dayNightManager != null)
            {
                if (dayNightManager.IsDay)
                {
                    statusText = GetMessage("status_day", "Day - Explore and investigate");
                }
                else
                {
                    statusText = GetMessage("status_night", "Night - Return to spawn to rest");
                }
            }

            if (playerPenalty != null && playerPenalty.CurrentPenaltyType != DayNightSystem.PenaltyType.None)
            {
                statusText += " | " + playerPenalty.GetPenaltyDescription();
            }

            statusDisplay.text = statusText;
        }

        public void ShowStatus(string message)
        {
            if (statusDisplay == null) { return; }
            statusDisplay.text = message;

            if (showDebugLogs)
            {
                Debug.Log($"[DayNightTimeAndStatusHUD] Status message: {message}");
            }
        }

        private string GetMessage(string key, string fallback)
        {
            if (feedbackMessages != null)
            {
                return feedbackMessages.GetMessage(key);
            }
            return fallback;
        }
    }
}

// ScriptRole: Updates time and status texts for the day/night HUD
// RelatedScripts: DayNightUI, DayNightManager, PlayerPenalty
// UsesSO: FeedbackMessagesSO
// ReceivesFrom: DayNightUI
// SendsTo: TextMeshProUGUI


