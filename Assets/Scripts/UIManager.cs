using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Handles all user interface updates for the timer and interacts with TimerSystem. No hardcoded texts.
/// </summary>
public class UIManager : MonoBehaviour
{
    [Header("References")]
    public TimerSystem timerSystem;
    public UITextSO uiTextSO;

    // Texts
    public TextMeshProUGUI timerLabelText;      // Example: "Trabajo" / "Descanso" / "Pausado"
    public TextMeshProUGUI timerValueText;      // Example: "00:24:11"
    public TextMeshProUGUI timerStateText;      // Example: "Tiempo Restante:"

    // Botones
    public Button startWorkButton;
    public Button startBreakButton;
    public Button pauseButton;
    public Button resumeButton;
    public Button stopButton;

    void Awake()
    {
        // --- Vincular botones a métodos ---
        startWorkButton.onClick.AddListener(OnStartWorkClicked);
        startBreakButton.onClick.AddListener(OnStartBreakClicked);
        pauseButton.onClick.AddListener(OnPauseClicked);
        resumeButton.onClick.AddListener(OnResumeClicked);
        stopButton.onClick.AddListener(OnStopClicked);

        // Asignar textos de botones (sin hardcode)
        startWorkButton.GetComponentInChildren<TextMeshProUGUI>().text = uiTextSO.startWorkLabel;
        startBreakButton.GetComponentInChildren<TextMeshProUGUI>().text = uiTextSO.startBreakLabel;
        pauseButton.GetComponentInChildren<TextMeshProUGUI>().text = uiTextSO.pauseLabel;
        resumeButton.GetComponentInChildren<TextMeshProUGUI>().text = uiTextSO.resumeLabel;
        stopButton.GetComponentInChildren<TextMeshProUGUI>().text = uiTextSO.stopLabel;

        // Etiqueta tiempo
        timerStateText.text = uiTextSO.timeLeftLabel;
    }

    void OnEnable()
    {
        // Suscribir a eventos del TimerSystem
        timerSystem.OnTimerStarted += HandleTimerStarted;
        timerSystem.OnTimerTick += HandleTimerTick;
        timerSystem.OnTimerCompleted += HandleTimerCompleted;
        timerSystem.OnTimerPaused += HandleTimerPaused;
        timerSystem.OnTimerResumed += HandleTimerResumed;
        timerSystem.OnTimerStopped += HandleTimerStopped;
    }

    void OnDisable()
    {
        timerSystem.OnTimerStarted -= HandleTimerStarted;
        timerSystem.OnTimerTick -= HandleTimerTick;
        timerSystem.OnTimerCompleted -= HandleTimerCompleted;
        timerSystem.OnTimerPaused -= HandleTimerPaused;
        timerSystem.OnTimerResumed -= HandleTimerResumed;
        timerSystem.OnTimerStopped -= HandleTimerStopped;
    }

    // --- EVENTOS DEL TIMER ---

    private void HandleTimerStarted(TimerType type)
    {
        UpdateTimerLabel(type);
        SetButtonStates(true);
    }

    private void HandleTimerTick(float timeLeft, TimerType type)
    {
        timerValueText.text = FormatTime(timeLeft);
    }

    private void HandleTimerCompleted(TimerType type)
    {
        timerLabelText.text = uiTextSO.stoppedLabel;
        timerValueText.text = "00:00";
        SetButtonStates(false);
    }

    private void HandleTimerPaused()
    {
        timerLabelText.text = uiTextSO.pausedLabel;
        SetPauseResumeState(true);
    }

    private void HandleTimerResumed()
    {
        UpdateTimerLabel(timerSystem.GetCurrentTimerType());
        SetPauseResumeState(false);
    }

    private void HandleTimerStopped()
    {
        timerLabelText.text = uiTextSO.stoppedLabel;
        SetButtonStates(false);
    }

    // --- BOTONES ---

    void OnStartWorkClicked() => timerSystem.StartWorkTimer();
    void OnStartBreakClicked() => timerSystem.StartBreakTimer();
    void OnPauseClicked() => timerSystem.PauseTimer();
    void OnResumeClicked() => timerSystem.ResumeTimer();
    void OnStopClicked() => timerSystem.StopTimer();

    // --- UTILIDAD UI ---

    private void UpdateTimerLabel(TimerType type)
    {
        timerLabelText.text = (type == TimerType.Work) ? uiTextSO.workLabel : uiTextSO.breakLabel;
    }

    private string FormatTime(float seconds)
    {
        int s = Mathf.Max(0, Mathf.FloorToInt(seconds));
        int min = s / 60;
        int sec = s % 60;
        return min.ToString("00") + ":" + sec.ToString("00");
    }

    private void SetButtonStates(bool timerRunning)
    {
        // Solo StartWork y StartBreak activos si timer NO corriendo
        startWorkButton.interactable = !timerRunning;
        startBreakButton.interactable = !timerRunning;
        pauseButton.interactable = timerRunning;
        resumeButton.interactable = false;
        stopButton.interactable = timerRunning;
    }

    private void SetPauseResumeState(bool paused)
    {
        pauseButton.interactable = !paused;
        resumeButton.interactable = paused;
    }
}
