using System.Collections.Generic;
using UnityEngine;
using Game.Core; // GameConfigProvider

namespace Game.DayNight
{
    /// <summary>
    /// Minimal ambience controller that drives ambient/fog colors and the main directional light
    /// (and optional lamp lights) using a LightPreset and the DayNightManager's progress/state.
    /// KISS: no UI, no volumes/skybox for now. Inspector-first wiring.
    /// </summary>
    [AddComponentMenu("Spookie/Day Night Ambience Controller")]
    public class DayNightAmbienceController : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Day/Night manager providing State and DayProgress01. If null, will try FindObjectOfType at Start.")]
        [SerializeField] private DayNightManager manager;
        [Tooltip("Scene's main directional light (the sun)")]
        [SerializeField] private Light directionalLight;
        [Tooltip("Preset with gradients for Ambient/Directional/Fog over the day.")]
        [SerializeField] private LightPreset dayNightPreset;
        [Tooltip("Optional preset to tint non-directional lights (lamps) over the day.")]
        [SerializeField] private LightPreset lampPreset;

        [Header("Behavior")]
        [Tooltip("If true, reads time from DayNightManager. If false, use Manual Time slider.")]
        [SerializeField] private bool driveFromManager = true;
        [Tooltip("Yaw angle for the sun's path (rotation around Y).")]
        [SerializeField] private float sunDirection = 170f;
        [Tooltip("Factor de suavizado para la rotación del sol. Un valor más bajo es más suave.")]
        [Range(0.01f, 1f)]
        [SerializeField] private float smoothFactor = 0.1f;
        [Tooltip("Collect non-directional lights automatically at Start to act as lamps.")]
        [SerializeField] private bool controlLights = true;
        [Tooltip("Additional lamps to drive (merged with auto-collected ones if enabled).")]
        [SerializeField] private List<Light> extraLamps = new List<Light>();

        [Header("Manual (when driveFromManager = false)")]
        [Range(0f, 1f)]
        [SerializeField] private float manualTimePercent = 0f;

        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = false;

        // Internal
        private readonly List<Light> _lamps = new List<Light>();
        private DayState _lastState;
        private float _lastAppliedPercent = -1f;

        [Tooltip("Curva para remapear el tiempo del día. Permite acelerar la noche y ralentizar el día.")]
        [SerializeField] private AnimationCurve timeCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        private void Start()
        {
            // Lazy find
            if (manager == null)
            {
                manager = FindObjectOfType<DayNightManager>();
                if (showDebugLogs && manager == null)
                {
                    Debug.LogWarning("[DayNightAmbience] No DayNightManager found in scene.");
                }
            }

            // Optional: auto-wire LightPresets from GameConfigProvider if left unassigned
            if (dayNightPreset == null || lampPreset == null)
            {
                var provider = FindObjectOfType<GameConfigProvider>();
                var cfg = provider != null ? provider.Config : null;
                if (cfg != null)
                {
                    if (dayNightPreset == null) dayNightPreset = cfg.SunLightPreset;
                    if (lampPreset == null) lampPreset = cfg.LampLightPreset;
                }
            }

            // Collect lamps if requested
            _lamps.Clear();
            if (controlLights)
            {
                var lights = FindObjectsOfType<Light>();
                foreach (var li in lights)
                {
                    if (li == null) continue;
                    if (li == directionalLight) continue;
                    switch (li.type)
                    {
                        case LightType.Point:
                        case LightType.Spot:
                        case LightType.Rectangle:
                        case LightType.Disc:
                            _lamps.Add(li);
                            break;
                    }
                }
            }
            if (extraLamps != null && extraLamps.Count > 0)
            {
                foreach (var li in extraLamps)
                {
                    if (li != null && li != directionalLight && !_lamps.Contains(li))
                        _lamps.Add(li);
                }
            }
        }

        private void Update()
        {
            float percent = driveFromManager && manager != null
                ? ComputePercentFromManager(manager)
                : Mathf.Clamp01(manualTimePercent);

            ApplyLighting(percent);
        }

        private float ComputePercentFromManager(DayNightManager m)
        {
            switch (m.State)
            {
                case DayState.Dawn:
                case DayState.Day:
                case DayState.DuskWarning:
                    return Mathf.Clamp01(m.DayProgress01);
                case DayState.Exhaustion:
                case DayState.Sleep:
                case DayState.Faint:
                case DayState.Night:
                    // Lock visuals at night-end of the curve for non-playable/night/terminal day states
                    return 1f;
                default:
                    return Mathf.Clamp01(m.DayProgress01);
            }
        }

        private void ApplyLighting(float timePercent)
        {
            if (dayNightPreset == null)
            {
                if (showDebugLogs)
                    Debug.LogWarning("[DayNightAmbience] Missing dayNightPreset.");
                return;
            }

            // Early-out if nothing changed significantly
            if (Mathf.Abs(timePercent - _lastAppliedPercent) < 0.0005f && _lastState == (manager != null ? manager.State : _lastState))
                return;

            // Remapear el tiempo usando la curva para controlar la velocidad del ciclo
            float remappedTime = timeCurve.Evaluate(timePercent);

            // Ambient & Fog
            RenderSettings.ambientLight = dayNightPreset.AmbientColour.Evaluate(remappedTime);
            RenderSettings.fogColor = dayNightPreset.FogColour.Evaluate(remappedTime);

            // Directional (sun)
            if (directionalLight != null)
            {
                directionalLight.color = dayNightPreset.DirectionalColour.Evaluate(remappedTime);
                
                // Calcular la rotación objetivo
                Quaternion targetRotation = Quaternion.Euler(new Vector3((remappedTime * 360f) - 90f, sunDirection, 0f));
                
                // Interpolar suavemente hacia la rotación objetivo para evitar saltos
                directionalLight.transform.localRotation = Quaternion.Slerp(directionalLight.transform.localRotation, targetRotation, smoothFactor);
            }

            // Lamps tint (optional)
            if (lampPreset != null && _lamps.Count > 0)
            {
                var c = lampPreset.DirectionalColour.Evaluate(remappedTime);
                for (int i = 0; i < _lamps.Count; i++)
                {
                    var li = _lamps[i];
                    if (li == null) continue;
                    li.color = c;
                }
            }

            _lastAppliedPercent = timePercent;
            if (manager != null) _lastState = manager.State;
        }

        private void OnValidate()
        {
            sunDirection = Mathf.Repeat(sunDirection, 360f);
            manualTimePercent = Mathf.Clamp01(manualTimePercent);
        }
    }
}

// ScriptRole: Control ambiental mínimo sincronizado con DayNightManager (ambient/fog/luz direccional y lámparas via LightPreset)
// RelatedScripts: DayNightManager, LightManager (origen), LightPreset
// UsesSO: LightPreset
// ReceivesFrom: DayNightManager (State, DayProgress01)
// SendsTo: RenderSettings, Light (Directional + Lamps)
// Adjuntar a: GameObject en escena raíz ("Ambience" o similar). Asignar: DayNightManager, Directional Light, LightPreset(s). Sin UI.
