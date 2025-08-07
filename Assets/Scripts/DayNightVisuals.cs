using UnityEngine;
using UnityEngine.Rendering;

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
        private DayNightManager dayNightManager;
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
            dayNightManager = FindObjectOfType<DayNightManager>();
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
                originalLightIntensity = directionalLight.intensity;
            
            if (skyboxMaterial != null && skyboxMaterial.HasProperty("_Tint"))
                originalSkyboxTint = skyboxMaterial.GetColor("_Tint");
        }
        
        private void OnDayStart()
        {
            if (showDebugLogs)
                Debug.Log("[DayNightVisuals] Day started - applying day visuals");
        }
        
        private void OnNightStart()
        {
            if (showDebugLogs)
                Debug.Log("[DayNightVisuals] Night started - applying night visuals");
        }
        
        private void OnExhaustionWarning(float secondsRemaining)
        {
            if (showDebugLogs)
                Debug.Log($"[DayNightVisuals] Exhaustion warning - {secondsRemaining:F1}s remaining");
        }
        
        private void OnTimeChanged(float timeNormalized)
        {
            UpdateVisuals(timeNormalized);
        }
        
        private void UpdateVisuals(float timeNormalized)
        {
            if (dayNightManager == null || dayNightManager.Config == null)
            {
                Debug.LogWarning("[DayNightVisuals] Cannot update visuals - DayNightManager or Config is null");
                return;
            }
            
            // Clamp timeNormalized to prevent out of bounds
            timeNormalized = Mathf.Clamp01(timeNormalized);
            
            // Update light intensity
            if (directionalLight != null)
            {
                float intensity = dayNightManager.Config.LightIntensityCurve.Evaluate(timeNormalized);
                directionalLight.intensity = originalLightIntensity * intensity;
            }
            
            // Update skybox tint
            if (skyboxMaterial != null && skyboxMaterial.HasProperty("_Tint"))
            {
                Color tint = dayNightManager.Config.SkyboxTint.Evaluate(timeNormalized);
                skyboxMaterial.SetColor("_Tint", tint);
            }
        }
        
        public void ApplyExhaustionEffects(float intensity)
        {
            if (vignette != null)
            {
                vignette.intensity.value = Mathf.Clamp01(intensity);
                
                if (showDebugLogs)
                    Debug.Log($"[DayNightVisuals] Applied exhaustion effect with intensity: {intensity:F2}");
            }
        }
        
        public void RemoveExhaustionEffects()
        {
            if (vignette != null)
            {
                vignette.intensity.value = 0f;
                
                if (showDebugLogs)
                    Debug.Log("[DayNightVisuals] Removed exhaustion effects");
            }
        }
        
        // Public method to force update visuals
        public void ForceUpdateVisuals()
        {
            if (dayNightManager != null)
            {
                UpdateVisuals(dayNightManager.CurrentTimeNormalized);
            }
        }
    }
}

// ScriptRole: Handles visual effects for day/night cycle and exhaustion
// RelatedScripts: DayNightManager
// UsesSO: DayNightConfig (via DayNightManager)
// ReceivesFrom: DayNightManager events
// SendsTo: Light, Skybox Material, Post-Process Volume
