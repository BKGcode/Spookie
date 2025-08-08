using UnityEngine;
using UnityEngine.UI;

namespace DayNightSystem.UI.DayNight
{
    public class DayNightWarningAndProximityUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Image warningIcon;
        [SerializeField] private Image spawnProximityIcon;

        [Header("Settings")]
        [SerializeField] private float warningBlinkRate = 0.5f;
        [SerializeField] private bool showWarningIcon = true;
        [SerializeField] private bool showSpawnProximity = true;
        [SerializeField] private float proximityUpdateRate = 0.5f;

        [Header("Audio")]
        [SerializeField] private DayNightSystem.AudioManager audioManager;

        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;

        private bool isWarningActive;
        private bool isProximityActive;
        private Coroutine warningBlinkCoroutine;
        private Coroutine proximityUpdateCoroutine;

        public void ShowWarning(string message)
        {
            if (audioManager != null)
            {
                audioManager.PlayWarningSound();
            }

            if (warningIcon != null && showWarningIcon)
            {
                warningIcon.gameObject.SetActive(true);
                StartWarningBlink();
            }

            if (showDebugLogs)
            {
                Debug.Log($"[DayNightWarningAndProximityUI] Warning: {message}");
            }
        }

        public void HideWarning()
        {
            isWarningActive = false;
            StopWarningBlink();
            if (warningIcon != null) { warningIcon.gameObject.SetActive(false); }
            if (showDebugLogs) { Debug.Log("[DayNightWarningAndProximityUI] Warning hidden"); }
        }

        public void ShowPenaltyIndicator()
        {
            if (warningIcon != null && showWarningIcon)
            {
                warningIcon.gameObject.SetActive(true);
                StartWarningBlink();
            }
        }

        public void HidePenaltyIndicator()
        {
            if (warningIcon != null) { warningIcon.gameObject.SetActive(false); }
            StopWarningBlink();
        }

        public void ShowProximityIndicator()
        {
            if (spawnProximityIcon == null || !showSpawnProximity) { return; }
            spawnProximityIcon.gameObject.SetActive(true);
            isProximityActive = true;
            StartProximityUpdate();
        }

        public void HideProximityIndicator()
        {
            isProximityActive = false;
            if (spawnProximityIcon != null) { spawnProximityIcon.gameObject.SetActive(false); }
            StopProximityUpdate();
        }

        public void UpdateDistance(float distance)
        {
            if (!isProximityActive || spawnProximityIcon == null) { return; }
            float normalizedDistance = Mathf.Clamp01(distance / 10f);
            spawnProximityIcon.color = Color.Lerp(Color.green, Color.red, normalizedDistance);
        }

        private void StartWarningBlink()
        {
            isWarningActive = true;
            if (warningBlinkCoroutine != null) { StopCoroutine(warningBlinkCoroutine); }
            warningBlinkCoroutine = StartCoroutine(WarningBlinkCoroutine());
        }

        private void StopWarningBlink()
        {
            if (warningBlinkCoroutine != null)
            {
                StopCoroutine(warningBlinkCoroutine);
                warningBlinkCoroutine = null;
            }
        }

        private System.Collections.IEnumerator WarningBlinkCoroutine()
        {
            if (warningIcon == null) { yield break; }
            while (isWarningActive)
            {
                warningIcon.enabled = !warningIcon.enabled;
                yield return new WaitForSeconds(warningBlinkRate);
            }
            warningIcon.enabled = true;
        }

        private void StartProximityUpdate()
        {
            if (proximityUpdateCoroutine != null) { StopCoroutine(proximityUpdateCoroutine); }
            proximityUpdateCoroutine = StartCoroutine(ProximityUpdateCoroutine());
        }

        private void StopProximityUpdate()
        {
            if (proximityUpdateCoroutine != null)
            {
                StopCoroutine(proximityUpdateCoroutine);
                proximityUpdateCoroutine = null;
            }
        }

        private System.Collections.IEnumerator ProximityUpdateCoroutine()
        {
            while (isProximityActive)
            {
                if (DayNightSystem.SpawnPoint.Current != null)
                {
                    float distance = DayNightSystem.SpawnPoint.Current.GetDistanceToPlayer();
                    UpdateDistance(distance);
                }
                yield return new WaitForSeconds(proximityUpdateRate);
            }
        }
    }
}

// ScriptRole: Manages warning and proximity UI with audio cues and coroutines
// RelatedScripts: DayNightUI, AudioManager, SpawnPoint
// UsesSO: None
// ReceivesFrom: DayNightUI
// SendsTo: Image (UI), AudioManager (warning sound)


