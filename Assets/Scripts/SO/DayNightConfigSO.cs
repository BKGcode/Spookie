using UnityEngine;

namespace Game.DayNight
{
    [CreateAssetMenu(fileName = "DayNightConfig", menuName = "Spookie/DayNight Config")]
    public class DayNightConfigSO : ScriptableObject
    {
        [Header("Day Timing (real minutes/seconds)")]
        [Tooltip("Duración total del día en minutos reales (1..60)." )]
        [Range(1, 60)]
        [SerializeField] private int dayDurationMinutes = 5;

        [Tooltip("Umbral de aviso en segundos reales (0..60) antes de que termine el día.")]
        [Range(0, 60)]
        [SerializeField] private int duskWarningThresholdSeconds = 60;

        [Header("UI (temporal para pruebas)")]
        [Tooltip("Mostrar siempre el temporizador desde el inicio de la escena.")]
        [SerializeField] private bool showDayTimerAlways = true;

        [Tooltip("Formato del tiempo mostrado (por ejemplo mm:ss). No se aplica en runtime si contiene formato inválido.")]
        [SerializeField] private string uiTimeFormat = "mm:ss";

        [Tooltip("Etiqueta para el estado 'Día'.")]
        [SerializeField] private string labelDay = "Day";
        [Tooltip("Etiqueta para el estado 'Aviso Atardecer'.")]
        [SerializeField] private string labelDuskWarning = "Time to sleep";
        [Tooltip("Etiqueta para el estado 'Dormir' (fin del día).")]
        [SerializeField] private string labelSleep = "Sleep";
        [Tooltip("Etiqueta para el estado 'Desmayo' (no usado en Fase 1).")]
        [SerializeField] private string labelFaint = "Faint";
        [Tooltip("Etiqueta para el estado 'Noche' (no usado en Fase 1).")]
        [SerializeField] private string labelNight = "Night";

    [Header("Spawn Detection")]
    [Tooltip("Radio en metros para considerar que el jugador está en el spawn al final del día.")]
    [Min(0f)]
    [SerializeField] private float spawnRadiusToCountAsAtSpawn = 2f;
    [Tooltip("Etiqueta para el estado 'Exhaustion' (agotamiento) cuando no se llega al spawn a tiempo.")]
    [SerializeField] private string labelExhaustion = "Exhaustion";

    [Header("Exhaustion (Fase 3)")]
    [Tooltip("Duración del agotamiento en segundos reales (1..60). Durante este tiempo el jugador puede volver al spawn." )]
    [Range(1, 60)]
    [SerializeField] private int exhaustionDurationSeconds = 30;
    [Tooltip("Multiplicador mínimo de velocidad al final del agotamiento (0..1). 0.4 reduce al 40%.")]
    [Range(0f, 1f)]
    [SerializeField] private float exhaustionMinSpeedMultiplier = 0.4f;
    [Tooltip("Desactivar sprint durante el agotamiento.")]
    [SerializeField] private bool exhaustionDisableSprint = true;

    [Header("Night Transition (Fase 4)")]
    [Tooltip("Fade in hacia negro (segundos). 0 = sin fade.")]
    [Range(0f, 5f)]
    [SerializeField] private float nightFadeInSeconds = 0.6f;
    [Tooltip("Tiempo de espera en negro (segundos). 0 = pasar inmediatamente al amanecer.")]
    [Range(0f, 5f)]
    [SerializeField] private float nightHoldSeconds = 1.0f;
    [Tooltip("Fade out desde negro al amanecer (segundos). 0 = sin fade.")]
    [Range(0f, 5f)]
    [SerializeField] private float nightFadeOutSeconds = 0.6f;
    [Header("Penalty Next Day (por desmayo)")]
    [Tooltip("Multiplicador de velocidad para el día siguiente si hubo desmayo (0..1). 0.7 = 70% de la velocidad.")]
    [Range(0f, 1f)]
    [SerializeField] private float faintNextDaySpeedMultiplier = 0.7f;
    [Tooltip("Desactivar sprint durante el día siguiente si hubo desmayo.")]
    [SerializeField] private bool faintNextDayDisableSprint = true;
    [Tooltip("Duración de la penalización del día siguiente en minutos. 0 = toda la jornada del siguiente día.")]
    [Range(0, 60)]
    [SerializeField] private int faintNextDayPenaltyDurationMinutes = 0;

        public int DayDurationMinutes => dayDurationMinutes;
        public int DayDurationSeconds => Mathf.Clamp(dayDurationMinutes, 1, 60) * 60;
        public int DuskWarningThresholdSeconds => Mathf.Clamp(duskWarningThresholdSeconds, 0, 60);

        public bool ShowDayTimerAlways => showDayTimerAlways;
        public string UiTimeFormat => string.IsNullOrEmpty(uiTimeFormat) ? "mm:ss" : uiTimeFormat;

        public string LabelDay => string.IsNullOrEmpty(labelDay) ? "Day" : labelDay;
        public string LabelDuskWarning => string.IsNullOrEmpty(labelDuskWarning) ? "Time to sleep" : labelDuskWarning;
        public string LabelSleep => string.IsNullOrEmpty(labelSleep) ? "Sleep" : labelSleep;
        public string LabelFaint => string.IsNullOrEmpty(labelFaint) ? "Faint" : labelFaint;
        public string LabelNight => string.IsNullOrEmpty(labelNight) ? "Night" : labelNight;
    public string LabelExhaustion => string.IsNullOrEmpty(labelExhaustion) ? "Exhaustion" : labelExhaustion;
    public float SpawnRadiusToCountAsAtSpawn => Mathf.Max(0f, spawnRadiusToCountAsAtSpawn);
    public int ExhaustionDurationSeconds => Mathf.Clamp(exhaustionDurationSeconds, 1, 60);
    public float ExhaustionMinSpeedMultiplier => Mathf.Clamp01(exhaustionMinSpeedMultiplier);
    public bool ExhaustionDisableSprint => exhaustionDisableSprint;
    public float NightFadeInSeconds => Mathf.Max(0f, nightFadeInSeconds);
    public float NightHoldSeconds => Mathf.Max(0f, nightHoldSeconds);
    public float NightFadeOutSeconds => Mathf.Max(0f, nightFadeOutSeconds);
    public float FaintNextDaySpeedMultiplier => Mathf.Clamp01(faintNextDaySpeedMultiplier);
    public bool FaintNextDayDisableSprint => faintNextDayDisableSprint;
    public int FaintNextDayPenaltyDurationSeconds => Mathf.Clamp(faintNextDayPenaltyDurationMinutes, 0, 60) * 60;

        private void OnValidate()
        {
            // Ligeros clamps; sin trabajo pesado
            dayDurationMinutes = Mathf.Clamp(dayDurationMinutes, 1, 60);
            duskWarningThresholdSeconds = Mathf.Clamp(duskWarningThresholdSeconds, 0, 60);

            // Evitar que el umbral supere la duración total en segundos
            int totalSeconds = dayDurationMinutes * 60;
            if (duskWarningThresholdSeconds > totalSeconds)
            {
                duskWarningThresholdSeconds = Mathf.Min(totalSeconds, 60);
            }
            if (spawnRadiusToCountAsAtSpawn < 0f) spawnRadiusToCountAsAtSpawn = 0f;
            exhaustionDurationSeconds = Mathf.Clamp(exhaustionDurationSeconds, 1, 60);
            exhaustionMinSpeedMultiplier = Mathf.Clamp01(exhaustionMinSpeedMultiplier);
            nightFadeInSeconds = Mathf.Clamp(nightFadeInSeconds, 0f, 5f);
            nightHoldSeconds = Mathf.Clamp(nightHoldSeconds, 0f, 5f);
            nightFadeOutSeconds = Mathf.Clamp(nightFadeOutSeconds, 0f, 5f);
            faintNextDaySpeedMultiplier = Mathf.Clamp01(faintNextDaySpeedMultiplier);
            faintNextDayPenaltyDurationMinutes = Mathf.Clamp(faintNextDayPenaltyDurationMinutes, 0, 60);
        }
    }
}

// ScriptRole: Configuración del ciclo de Día (Fase 1) con tiempos reales y etiquetas temporales de estado
// RelatedScripts: DayNightManager
// UsesSO: —
// ReceivesFrom: —
// SendsTo: DayNightManager (lectura de valores)
