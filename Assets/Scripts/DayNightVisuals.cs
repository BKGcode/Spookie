using UnityEngine;
using UnityEngine.Rendering;
using DayNightSystem.Core;

namespace DayNightSystem
{
    public class DayNightVisuals : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Light directionalLight;
        [SerializeField] private Material skyboxMaterial;
        [SerializeField] private Volume postProcessVolume;
        
        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;
        
        // Private fields
        private DayNightSystem.Core.DayNightManager dayNightManager;
        private UnityEngine.Rendering.Universal.Vignette vignette;
        private float originalLightIntensity;
        private Color originalSkyboxTint;
        
        private void Awake()
        {
            ValidateReferences();
            FindDayNightManager();
            SetupPostProcessing();
        }
        
        private void OnEnable()
        {
            if (dayNightManager != null)
            {
                dayNightManager.OnDayStart += OnDayStart;
                dayNightManager.OnNightStart += OnNightStart;
                dayNightManager.OnExhaustionWarning += OnExhaustionWarning;
                dayNightManager.OnTimeChanged += OnTimeChanged;
                var ev = dayNightManager.GetEventController();
                if (ev != null)
                {
                    ev.OnSunsetWarning += OnSunsetWarning;
                }
            }
        }
        
        private void OnDisable()
        {
            if (dayNightManager != null)
            {
                dayNightManager.OnDayStart -= OnDayStart;
                dayNightManager.OnNightStart -= OnNightStart;
                dayNightManager.OnExhaustionWarning -= OnExhaustionWarning;
                dayNightManager.OnTimeChanged -= OnTimeChanged;
                var ev = dayNightManager.GetEventController();
                if (ev != null)
                {
                    ev.OnSunsetWarning -= OnSunsetWarning;
                }
            }
        }
        
        private void ValidateReferences()
        {
            if (directionalLight == null)
            {
                directionalLight = FindObjectOfType<Light>();
                if (directionalLight == null)
                {
                    Debug.LogError("[DayNightVisuals] No directional light found in scene!");
                }
            }
            
            if (skyboxMaterial == null)
            {
                skyboxMaterial = RenderSettings.skybox;
                if (skyboxMaterial == null)
                {
                    Debug.LogWarning("[DayNightVisuals] No skybox material found - skybox tinting disabled");
                }
            }
            
            if (postProcessVolume == null)
            {
                postProcessVolume = FindObjectOfType<Volume>();
                if (postProcessVolume == null)
                {
                    Debug.LogWarning("[DayNightVisuals] No post-process volume found - exhaustion effects disabled");
                }
            }
        }
        
        private void FindDayNightManager()
        {
            dayNightManager = FindObjectOfType<DayNightSystem.Core.DayNightManager>();
            if (dayNightManager == null)
            {
                Debug.LogError("[DayNightVisuals] No DayNightManager found in scene!");
            }
        }
        
        private void SetupPostProcessing()
        {
            if (postProcessVolume != null && postProcessVolume.profile != null)
            {
                postProcessVolume.profile.TryGet(out vignette);
                if (vignette == null)
                {
                    Debug.LogWarning("[DayNightVisuals] No Vignette effect found in post-process profile");
                }
            }
            
            // Store original values
            if (directionalLight != null)
            {
                originalLightIntensity = directionalLight.intensity;
            }
            
            if (skyboxMaterial != null)
            {
                originalSkyboxTint = skyboxMaterial.GetColor("_SkyTint");
            }
        }
        
        private void OnDayStart()
        {
            UpdateVisuals(0f); // Start of day
            
            if (showDebugLogs)
                Debug.Log("[DayNightVisuals] Day started - updated visuals");
        }
        
        private void OnNightStart()
        {
            UpdateVisuals(1f); // End of day/start of night
            
            if (showDebugLogs)
                Debug.Log("[DayNightVisuals] Night started - updated visuals");
        }
        
        private void OnExhaustionWarning(float secondsRemaining)
        {
            // Apply exhaustion effects based on remaining time
            float intensity = Mathf.Clamp01(1f - (secondsRemaining / 30f)); // 30s warning period
            ApplyExhaustionEffects(intensity);
        }
        
        private void OnTimeChanged(float timeNormalized)
        {
            // Convert seconds to normalized [0..1] of the current phase (day segment)
            float normalized = 0f;
            if (dayNightManager != null && dayNightManager.Config != null)
            {
                float dayDuration = dayNightManager.Config.DayDurationSeconds;
                float cycle = dayDuration * 2f;
                float cycleTime = cycle > 0f ? (timeNormalized % cycle) : 0f;
                normalized = Mathf.Clamp01((cycleTime % dayDuration) / dayDuration);
            }
            UpdateVisuals(normalized);
        }

        private void OnSunsetWarning(float secondsToNight)
        {
            // Subtle warm shift to suggest sunset
            if (directionalLight != null)
            {
                directionalLight.color = Color.Lerp(directionalLight.color, new Color(1f, 0.78f, 0.65f), 0.35f);
            }
        }
        
        private void UpdateVisuals(float timeNormalized)
        {
            // Update directional light
            if (directionalLight != null)
            {
                // Simulate sun movement
                float sunAngle = Mathf.Lerp(0f, 180f, timeNormalized);
                directionalLight.transform.rotation = Quaternion.Euler(sunAngle, 0f, 0f);
                
                // Adjust light intensity based on time
                float intensity = Mathf.Lerp(0.1f, originalLightIntensity, Mathf.Sin(timeNormalized * Mathf.PI));
                directionalLight.intensity = intensity;
                
                // Adjust light color based on time
                Color lightColor = Color.Lerp(Color.blue, Color.white, Mathf.Sin(timeNormalized * Mathf.PI));
                directionalLight.color = lightColor;
            }
            
            // Update skybox tint
            if (skyboxMaterial != null)
            {
                Color skyTint = Color.Lerp(Color.blue, originalSkyboxTint, Mathf.Sin(timeNormalized * Mathf.PI));
                skyboxMaterial.SetColor("_SkyTint", skyTint);
            }
        }
        
        public void ApplyExhaustionEffects(float intensity)
        {
            if (vignette != null)
            {
                vignette.intensity.value = intensity;
                vignette.color.value = Color.red;
            }
            
            if (showDebugLogs)
                Debug.Log($"[DayNightVisuals] Applied exhaustion effects - intensity: {intensity:F2}");
        }
        
        public void RemoveExhaustionEffects()
        {
            if (vignette != null)
            {
                vignette.intensity.value = 0f;
                vignette.color.value = Color.black;
            }
            
            if (showDebugLogs)
                Debug.Log("[DayNightVisuals] Removed exhaustion effects");
        }
        
        public void ForceUpdateVisuals()
        {
            if (dayNightManager != null)
            {
                UpdateVisuals(dayNightManager.CurrentTimeNormalized);
                
                if (showDebugLogs)
                    Debug.Log("[DayNightVisuals] Force updated visuals");
            }
        }
    }
}

// ScriptRole: Manages visual effects for day/night cycle including lighting and post-processing
// RelatedScripts: DayNightManager
// UsesSO: None
// ReceivesFrom: DayNightManager events
// SendsTo: Light, Material, Volume (visual effects)
