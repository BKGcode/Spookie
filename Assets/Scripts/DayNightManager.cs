using System;
using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using Game.Save; // Save integration
using Game.Core; // PauseManager, PauseReason
using Game.Core; // GameConfigProvider (same namespace, but explicit here for clarity)

namespace Game.DayNight
{
    public enum DayState { Dawn, Day, DuskWarning, Exhaustion, Sleep, Faint, Night }

    [AddComponentMenu("Spookie/Day Night Manager")]
    public class DayNightManager : MonoBehaviour
    {
    [Header("Config")]
        [Tooltip("Configuración del sistema de día. Duraciones en minutos/segundos reales y etiquetas de estado.")]
        [SerializeField] private DayNightConfigSO config;
    [Tooltip("Punto de spawn para evaluar si el jugador está 'en casa' al finalizar el día.")]
    [SerializeField] private Transform spawnPoint;
    [Tooltip("Transform del jugador (para medir distancia al spawn).")]
    [SerializeField] private Transform playerTransform;

        [Header("UI (temporal)")]
        [Tooltip("TMP Text para mostrar el tiempo restante del día (mm:ss).")]
        [SerializeField] private TMP_Text dayTimerText;
    [Tooltip("TMP Text para mostrar el estado (Day / Time to sleep / Sleep...).")]
    [SerializeField] private TMP_Text dayStateText;
    [Tooltip("CanvasGroup opcional para realizar fade a negro durante la noche.")]
    [SerializeField] private CanvasGroup nightFadeCanvasGroup;

        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;

    [Header("Integration (optional)")]
    [Tooltip("If assigned, will gate gameplay as a Transition during night fades (prevents banners/inputs).")]
    [SerializeField] private PauseManager pauseManager;

        public DayState State { get; private set; } = DayState.Dawn;
        public float DayTimeRemainingSeconds { get; private set; }
        public float DayProgress01 => Mathf.Clamp01(1f - (DayTimeRemainingSeconds / Mathf.Max(1f, totalDaySeconds)));

        public event Action<DayState> OnStateChanged;
        public event Action<int, int, float> OnDayTimeTick; // minutes, seconds, remaining

    private float totalDaySeconds;
        private int lastDisplayedWholeSeconds = -1;
    private float exhaustionRemaining;
    private bool exhaustionSprintWasDisabled;
    private int currentDay = 1;

    // Persistence keys
    private const string PP_DayKey = "DN_CurrentDay";
    private const string PP_PenaltyKey = "DN_NextDayPenalty"; // 1 if next day should start with penalty
    private const string PP_PenaltyUntilKey = "DN_NextDayPenalty_UntilSeconds"; // epoch-like seconds until which penalty is active (game-time relative)
    private float penaltyExpiresAtGameTime = -1f; // DayTimeRemainingSeconds-based timeline reference

        private void Awake()
        {
            if (config == null)
            {
                Debug.LogError("[DayNightManager] Missing DayNightConfigSO. Please assign in Inspector.");
            }
        }

        private void Start()
        {
            // Optional: auto-wire config from GameConfigProvider if left unassigned
            if (config == null)
            {
                var provider = FindObjectOfType<GameConfigProvider>();
                var cfg = provider != null ? provider.Config : null;
                if (cfg != null && cfg.DayNightConfig != null) config = cfg.DayNightConfig;
            }
            InitializeDay();

            // SAVE: Restaurar estado si existe partida cargada para esta escena (después de InitializeDay para sobrescribir)
            if (SaveGameManager.Instance != null && SaveGameManager.Instance.HasLoaded)
            {
                var data = SaveGameManager.Instance.CurrentData;
                if (data != null && !data.isGameCompleted && data.sceneName == SceneManager.GetActiveScene().name)
                {
                    // Día
                    currentDay = Mathf.Max(1, data.absoluteDay);
                    // Duración del día (compatibilidad si cambió config)
                    if (data.dayLengthSeconds > 10f) totalDaySeconds = data.dayLengthSeconds; // salvaguarda
                    // Tiempo transcurrido -> Remaining
                    float elapsed = Mathf.Clamp(data.timeOfDaySeconds, 0f, totalDaySeconds);
                    DayTimeRemainingSeconds = Mathf.Max(0f, totalDaySeconds - elapsed);
                    // Estado visual directo a Day (o mapear si en futuro reanudamos Exhaustion)
                    SetState(DayState.Day);
                    // Penalización faint
                    if (data.penaltyActive && data.penaltyRemainingSeconds > 0f)
                    {
                        float elapsedNow = totalDaySeconds - DayTimeRemainingSeconds;
                        penaltyExpiresAtGameTime = elapsedNow + data.penaltyRemainingSeconds; // timeline absoluto
                        ApplyNextDayPenalty(true);
                        // Persistir en PlayerPrefs para compat intermedia (se limpiará al expirar)
                        PlayerPrefs.SetInt(PP_PenaltyKey, 1);
                        PlayerPrefs.SetInt(PP_PenaltyUntilKey, Mathf.RoundToInt(penaltyExpiresAtGameTime));
                        PlayerPrefs.Save();
                    }
                    UpdateUI(force: true);
                    if (showDebugLogs) Debug.Log($"[DayNightManager] Save restored Day={currentDay} Elapsed={elapsed:F1}s PenaltyActive={data.penaltyActive}");
                }
            }
        }

        private void InitializeDay()
        {
            // Load persisted day/penalty once per initialization
            if (PlayerPrefs.HasKey(PP_DayKey))
            {
                currentDay = Mathf.Max(1, PlayerPrefs.GetInt(PP_DayKey, 1));
            }
            else
            {
                currentDay = 1;
                PlayerPrefs.SetInt(PP_DayKey, currentDay);
            }

            totalDaySeconds = config != null ? config.DayDurationSeconds : 300f;
            DayTimeRemainingSeconds = totalDaySeconds;
            SetState(DayState.Dawn);

            // Garantizar despertar en spawn: teletransportar durante Dawn, con CC seguro
            TeleportPlayerToSpawn();

            // Aplicar penalización si el día anterior fue desmayo (persistida) y aún no expiró
            bool persistedPenalty = PlayerPrefs.GetInt(PP_PenaltyKey, 0) == 1;
            int until = PlayerPrefs.GetInt(PP_PenaltyUntilKey, 0);
            penaltyExpiresAtGameTime = until > 0 ? until : -1f;
            if (persistedPenalty)
            {
                // Aplica penalización entrante una sola vez al amanecer; si tiene expiración > 0, Update la limpiará
                ApplyNextDayPenalty(true);
            }

            UpdateUI(force:true);
            // Entramos inmediatamente a Day (amanecer es instantáneo en Fase 1)
            SetState(DayState.Day);
        }

        private void TeleportPlayerToSpawn()
        {
            if (playerTransform == null || spawnPoint == null) return;
            var cc = playerTransform.GetComponent<CharacterController>();
            bool hadCC = cc != null && cc.enabled;
            if (cc != null) cc.enabled = false;
            playerTransform.SetPositionAndRotation(spawnPoint.position, spawnPoint.rotation);
            if (cc != null) cc.enabled = hadCC;
        }

        private void Update()
        {
            if (State == DayState.Day || State == DayState.DuskWarning)
            {
                float dt = Mathf.Clamp(Time.deltaTime, 0f, 0.25f);
                DayTimeRemainingSeconds = Mathf.Max(0f, DayTimeRemainingSeconds - dt);

                // Si hay una penalización con expiración, verificar si debe limpiarse
                if (penaltyExpiresAtGameTime > 0f && (totalDaySeconds - DayTimeRemainingSeconds) >= penaltyExpiresAtGameTime)
                {
                    // Expiró durante el día
                    ApplyNextDayPenalty(false);
                    PlayerPrefs.SetInt(PP_PenaltyKey, 0);
                    PlayerPrefs.DeleteKey(PP_PenaltyUntilKey);
                    PlayerPrefs.Save();
                    penaltyExpiresAtGameTime = -1f;
                }

                // Cambiar a DuskWarning si estamos dentro del umbral
                if (State == DayState.Day && config != null && DayTimeRemainingSeconds <= config.DuskWarningThresholdSeconds)
                {
                    SetState(DayState.DuskWarning);
                }

                // Actualizar UI por segundo
                UpdateUI();

                // Fin del día alcanzado: decidir Sleep o Exhaustion según si está en spawn
                if (DayTimeRemainingSeconds <= 0.0001f)
                {
                    bool atSpawn = IsPlayerAtSpawn();
                    if (atSpawn)
                    {
                        SetState(DayState.Sleep);
                        UpdateUI(force:true);
                        BeginNightSequence(fainted:false);
                    }
                    else
                    {
                        // Entrar en agotamiento (Fase 3)
                        exhaustionRemaining = config != null ? config.ExhaustionDurationSeconds : 30f;
                        lastDisplayedWholeSeconds = -1; // forzar actualización de UI
                        ApplyExhaustionEnter();
                        SetState(DayState.Exhaustion);
                    }
                }
            }
            else if (State == DayState.Exhaustion)
            {
                float dt = Mathf.Clamp(Time.deltaTime, 0f, 0.25f);
                exhaustionRemaining = Mathf.Max(0f, exhaustionRemaining - dt);

                // Reducir velocidad progresivamente y opcionalmente desactivar sprint
                ApplyExhaustionEffects();

                // ¿Ha llegado al spawn durante el agotamiento?
                if (IsPlayerAtSpawn())
                {
                    CleanupExhaustion();
                    SetState(DayState.Sleep);
                    UpdateUI(force:true);
                    BeginNightSequence(fainted:false);
                    return;
                }

                // ¿Se acabó el agotamiento?
                if (exhaustionRemaining <= 0.0001f)
                {
                    CleanupExhaustion();
                    SetState(DayState.Faint);
                    UpdateUI(force:true);
                    BeginNightSequence(fainted:true);
                    return;
                }

                UpdateUI();
            }
        }

        private bool IsPlayerAtSpawn()
        {
            if (playerTransform == null || spawnPoint == null) return false;
            float radius = config != null ? config.SpawnRadiusToCountAsAtSpawn : 2f;
            return Vector3.SqrMagnitude(playerTransform.position - spawnPoint.position) <= (radius * radius);
        }

        private void UpdateUI(bool force = false)
        {
            // Timer siempre visible según requisito; si falta reference, solo logueamos
            if (dayTimerText != null)
            {
                int whole;
                if (State == DayState.Exhaustion)
                {
                    whole = Mathf.Max(0, Mathf.FloorToInt(exhaustionRemaining));
                }
                else
                {
                    whole = Mathf.Max(0, Mathf.FloorToInt(DayTimeRemainingSeconds));
                }
                if (force || whole != lastDisplayedWholeSeconds)
                {
                    lastDisplayedWholeSeconds = whole;
                    int minutes = whole / 60;
                    int seconds = whole % 60;
                    dayTimerText.text = $"{minutes:00}:{seconds:00}";
                    float remaining = (State == DayState.Exhaustion) ? exhaustionRemaining : DayTimeRemainingSeconds;
                    OnDayTimeTick?.Invoke(minutes, seconds, remaining);
                }
            }
            else if (showDebugLogs)
            {
                Debug.LogWarning("[DayNightManager] dayTimerText not assigned.");
            }

            if (dayStateText != null)
            {
                switch (State)
                {
                    case DayState.Dawn:
                        dayStateText.text = config != null ? config.LabelDay : "Day"; // amanecer muestra "Day"
                        break;
                    case DayState.Day:
                        dayStateText.text = config != null ? config.LabelDay : "Day";
                        break;
                    case DayState.DuskWarning:
                        dayStateText.text = config != null ? config.LabelDuskWarning : "Time to sleep";
                        break;
                    case DayState.Exhaustion:
                        dayStateText.text = config != null ? config.LabelExhaustion : "Exhaustion";
                        break;
                    case DayState.Sleep:
                        dayStateText.text = config != null ? config.LabelSleep : "Sleep";
                        break;
                    case DayState.Faint:
                        dayStateText.text = config != null ? config.LabelFaint : "Faint";
                        break;
                    case DayState.Night:
                        dayStateText.text = config != null ? config.LabelNight : "Night";
                        break;
                }
            }
            else if (showDebugLogs)
            {
                Debug.LogWarning("[DayNightManager] dayStateText not assigned.");
            }
        }

        private void SetState(DayState newState)
        {
            if (State == newState) return;
            State = newState;
            if (showDebugLogs)
            {
                Debug.Log($"[DayNightManager] State -> {State}");
            }
            OnStateChanged?.Invoke(State);
        }

        // Night transition and next-day penalties
        private bool nextDayPenalty;

        [SerializeField, Tooltip("Referencia opcional a MouseLook para bloquear input durante la noche.")]
        private PlayerController.MouseLook mouseLook;
        [SerializeField, Tooltip("Referencia opcional a PlayerInteraction para bloquear input durante la noche.")]
        private PlayerController.PlayerInteraction playerInteraction;

    private void BeginNightSequence(bool fainted)
        {
            nextDayPenalty = fainted;
            SetState(DayState.Night);
            StopAllCoroutines();
            StartCoroutine(NightRoutine());
        }

        private IEnumerator NightRoutine()
        {
            bool transitionHeld = false;
            // Request Transition pause (optional) so other systems gate during fades
            if (pauseManager != null)
            {
                try { pauseManager.RequestPause(PauseReason.Transition); transitionHeld = true; } catch { }
            }

            // Bloquear jugabilidad durante la transición
            BlockGameplay(true);

            float fadeIn = config != null ? config.NightFadeInSeconds : 0.6f;
            float hold = config != null ? config.NightHoldSeconds : 1.0f;
            float fadeOut = config != null ? config.NightFadeOutSeconds : 0.6f;

            // Fade in
            if (nightFadeCanvasGroup != null)
            {
                nightFadeCanvasGroup.blocksRaycasts = true;
                nightFadeCanvasGroup.interactable = false;
                yield return FadeCanvasGroup(nightFadeCanvasGroup, nightFadeCanvasGroup.alpha, 1f, fadeIn);
            }
            else if (fadeIn > 0f)
            {
                yield return new WaitForSeconds(fadeIn);
            }

            if (hold > 0f)
            {
                yield return new WaitForSeconds(hold);
            }

            // Paso de día: integrar SaveGameManager + posible cambio de escena
            bool sceneChanged = false;
            string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            string nextScene = currentScene;

            // Incrementar día lógico
            int newDay = Mathf.Max(1, currentDay + 1);

            // Determinar escena destino usando SaveGameManager si disponible
            var sgm = SaveGameManager.Instance;
            if (sgm != null && sgm.HasLoaded)
            {
                nextScene = sgm.GetSceneForDay(newDay) ?? currentScene;
            }

            // Persistencia legacy (se eliminará tras migración completa)
            currentDay = newDay;
            PlayerPrefs.SetInt(PP_DayKey, currentDay);
            PlayerPrefs.SetInt(PP_PenaltyKey, nextDayPenalty ? 1 : 0);
            if (nextDayPenalty && config != null)
            {
                int dur = config.FaintNextDayPenaltyDurationSeconds;
                if (dur > 0) PlayerPrefs.SetInt(PP_PenaltyUntilKey, dur); else PlayerPrefs.DeleteKey(PP_PenaltyUntilKey);
            }
            else
            {
                PlayerPrefs.DeleteKey(PP_PenaltyUntilKey);
            }
            PlayerPrefs.Save();

            // Guardado moderno
            if (sgm != null && sgm.HasLoaded)
            {
                var data = sgm.CurrentData;
                data.absoluteDay = currentDay;
                data.sceneName = nextScene;
                data.timeOfDaySeconds = 0f; // nuevo amanecer
                data.dayState = DayState.Day.ToString();
                // Penalización próxima jornada si faint
                if (nextDayPenalty && config != null)
                {
                    data.penaltyActive = true;
                    int dur = config.FaintNextDayPenaltyDurationSeconds;
                    data.penaltyRemainingSeconds = dur > 0 ? dur : data.dayLengthSeconds; // si 0 => todo el día
                }
                else
                {
                    data.penaltyActive = false;
                    data.penaltyRemainingSeconds = 0f;
                }
                // Asegurar spawn: sobreescribir posición guardada con spawnPoint si existe
                if (spawnPoint != null)
                {
                    data.playerPosition = spawnPoint.position;
                    data.playerYaw = spawnPoint.eulerAngles.y;
                }
                sgm.SaveManual(); // etiquetado Manual (podríamos usar tipo especial si se desea)
            }

            // Cargar nueva escena si cambia
            if (nextScene != currentScene)
            {
                sceneChanged = Game.Scenes.SceneFlowService.Instance != null && Game.Scenes.SceneFlowService.Instance.LoadSceneIfNeeded(nextScene);
            }

            if (!sceneChanged)
            {
                // Continuar flujo original en misma escena
                InitializeDay();

                // Fade out
                if (nightFadeCanvasGroup != null)
                {
                    yield return FadeCanvasGroup(nightFadeCanvasGroup, 1f, 0f, fadeOut);
                    nightFadeCanvasGroup.blocksRaycasts = false;
                }
                else if (fadeOut > 0f)
                {
                    yield return new WaitForSeconds(fadeOut);
                }

                // Desbloquear jugabilidad
                BlockGameplay(false);
            }
            else
            {
                // Escena nueva: no intentamos fade out local (el objeto será destruido)
            }

            // Release Transition pause if held
            if (transitionHeld && pauseManager != null)
            {
                try { pauseManager.ReleasePause(PauseReason.Transition); } catch { }
            }
        }

        private IEnumerator FadeCanvasGroup(CanvasGroup cg, float from, float to, float duration)
        {
            if (cg == null || duration <= 0f)
            {
                if (cg != null) cg.alpha = to;
                yield break;
            }
            cg.alpha = from;
            float t = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(t / duration);
                cg.alpha = Mathf.Lerp(from, to, k);
                yield return null;
            }
            cg.alpha = to;
        }

        private void BlockGameplay(bool block)
        {
            // PlayerMovement
            if (playerMovement == null && playerTransform != null)
            {
                playerMovement = playerTransform.GetComponent<PlayerController.PlayerMovement>();
            }
            if (playerMovement != null)
            {
                playerMovement.enabled = !block;
            }

            // MouseLook
            if (mouseLook == null && playerTransform != null)
            {
                mouseLook = playerTransform.GetComponent<PlayerController.MouseLook>();
            }
            if (mouseLook != null)
            {
                mouseLook.enabled = !block;
            }

            // PlayerInteraction
            if (playerInteraction == null && playerTransform != null)
            {
                playerInteraction = playerTransform.GetComponent<PlayerController.PlayerInteraction>();
            }
            if (playerInteraction != null)
            {
                playerInteraction.enabled = !block;
            }
        }

        private void ApplyNextDayPenalty(bool enable)
        {
            if (playerMovement == null && playerTransform != null)
            {
                playerMovement = playerTransform.GetComponent<PlayerController.PlayerMovement>();
            }
            if (playerMovement == null) return;

            if (enable)
            {
                float mul = config != null ? config.FaintNextDaySpeedMultiplier : 0.7f;
                playerMovement.SetSpeedMultiplier(Mathf.Clamp01(mul));
                if (config != null && config.FaintNextDayDisableSprint)
                {
                    playerMovement.SetCanSprint(false);
                }
            }
            else
            {
                // Reset to normal
                playerMovement.SetSpeedMultiplier(1f);
                playerMovement.SetCanSprint(true);
            }
        }

        // Exhaustion helpers
        [SerializeField, Tooltip("Referencia opcional a PlayerMovement para aplicar penalizaciones de agotamiento.")]
        private PlayerController.PlayerMovement playerMovement;

        private void ApplyExhaustionEnter()
        {
            if (playerMovement == null && playerTransform != null)
            {
                playerMovement = playerTransform.GetComponent<PlayerController.PlayerMovement>();
            }
            // Sprint off si está configurado
            if (config != null && config.ExhaustionDisableSprint && playerMovement != null)
            {
                playerMovement.SetCanSprint(false);
                exhaustionSprintWasDisabled = true;
            }
        }

        private void ApplyExhaustionEffects()
        {
            if (playerMovement == null) return;
            float total = Mathf.Max(0.0001f, config != null ? config.ExhaustionDurationSeconds : 30f);
            float t = 1f - Mathf.Clamp01(exhaustionRemaining / total); // 0 al inicio -> 1 al final
            float minMul = config != null ? config.ExhaustionMinSpeedMultiplier : 0.4f;
            float currentMul = Mathf.Lerp(1f, Mathf.Clamp01(minMul), t);
            playerMovement.SetSpeedMultiplier(currentMul);
        }

        private void CleanupExhaustion()
        {
            if (playerMovement != null)
            {
                // Restaurar velocidad base y sprint si lo desactivamos aquí
                playerMovement.SetSpeedMultiplier(1f);
                if (exhaustionSprintWasDisabled)
                {
                    playerMovement.SetCanSprint(true);
                }
            }
            exhaustionSprintWasDisabled = false;
        }

        // --- SAVE INTEGRATION PUBLIC API ---
        public int CurrentDay => currentDay;
        public float TotalDaySeconds => totalDaySeconds;
        public float GetElapsedDaySeconds() => Mathf.Clamp(totalDaySeconds - DayTimeRemainingSeconds, 0f, totalDaySeconds);
        public void FillSaveData(SaveData data)
        {
            if (data == null) return;
            data.absoluteDay = currentDay;
            data.dayLengthSeconds = totalDaySeconds;
            data.timeOfDaySeconds = GetElapsedDaySeconds();
            data.dayState = State.ToString();
            if (penaltyExpiresAtGameTime > 0f)
            {
                float elapsed = GetElapsedDaySeconds();
                bool active = penaltyExpiresAtGameTime > elapsed;
                data.penaltyActive = active;
                data.penaltyRemainingSeconds = active ? Mathf.Max(0f, penaltyExpiresAtGameTime - elapsed) : 0f;
            }
            else
            {
                data.penaltyActive = false;
                data.penaltyRemainingSeconds = 0f;
            }
        }
    }
}

// ScriptRole: Gestor del ciclo de Día (Fase 1-3): timer de día, aviso de atardecer, agotamiento con penalización y estado en UI TMP.
// RelatedScripts: DayNightConfigSO
// UsesSO: DayNightConfigSO
// ReceivesFrom: —
// SendsTo: TMP_Text (timer y state), PlayerMovement (penalizaciones en agotamiento).
