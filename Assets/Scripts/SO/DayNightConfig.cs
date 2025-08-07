using UnityEngine;

namespace DayNightSystem
{
    [CreateAssetMenu(fileName = "DayNightConfig", menuName = "Spookie/Day Night Config")]
    public class DayNightConfig : ScriptableObject
    {
        [Header("Time Settings")]
        [Range(1f, 60f)]
        [SerializeField] private float dayDurationMinutes = 15f;
        [Range(10f, 120f)]
        [SerializeField] private float exhaustionTimeSeconds = 30f;
        
        [Header("Visual Settings")]
        [SerializeField] private AnimationCurve lightIntensityCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        [SerializeField] private Gradient skyboxTint = new Gradient();
        
        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;
        
        // Public properties with validation
        public float DayDurationSeconds => dayDurationMinutes * 60f;
        public float ExhaustionTimeSeconds => exhaustionTimeSeconds;
        public AnimationCurve LightIntensityCurve => lightIntensityCurve;
        public Gradient SkyboxTint => skyboxTint;
        public bool ShowDebugLogs => showDebugLogs;
        
        private void OnValidate()
        {
            // Ensure values are within valid ranges
            dayDurationMinutes = Mathf.Clamp(dayDurationMinutes, 1f, 60f);
            exhaustionTimeSeconds = Mathf.Clamp(exhaustionTimeSeconds, 10f, 120f);
            
            if (showDebugLogs)
                Debug.Log($"[DayNightConfig] Validated - Day Duration: {dayDurationMinutes}min, Exhaustion: {exhaustionTimeSeconds}s");
        }
    }
}
