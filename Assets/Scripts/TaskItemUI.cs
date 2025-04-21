using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Globalization;

[RequireComponent(typeof(CanvasGroup))]
public class TaskItemUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private Image taskIconImage;
    [SerializeField] private Image buttonBackground;
    [SerializeField] private Image buttonIcon;
    [SerializeField] private TMP_InputField breakDurationInputField;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Buttons")]
    [SerializeField] private Button startResumeButton;
    [SerializeField] private Button pauseButton;
    [SerializeField] private Button resetButton;
    [SerializeField] private Button breakButton;
    [SerializeField] private Button closeButton;

    [Header("Visual Assets")]
    [SerializeField] private Sprite playIcon;
    [SerializeField] private Sprite pauseIcon;
    [SerializeField] private Sprite breakIcon;
    [SerializeField] private Color playStateColor = Color.green;
    [SerializeField] private Color pauseStateColor = Color.yellow;
    [SerializeField] private Color breakStateColor = Color.cyan;
    [SerializeField] private Color normalTimeColor = Color.white;
    [SerializeField] private Color stoppedStateColor = Color.grey;

    public event Action OnStartResumeClicked;
    public event Action OnPauseClicked;
    public event Action<float> OnBreakClicked;
    public event Action OnResetClicked;
    public event Action OnCloseClicked;

    private const float DEFAULT_BREAK_MINUTES = 5f;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (!ValidateReferences())
        {
            Debug.LogError($"[TaskItemUI] Validation failed on {gameObject.name}. Disabling.", this);
            gameObject.SetActive(false);
            return;
        }
        SetupButtonListeners();
        SetBreakDurationInput(DEFAULT_BREAK_MINUTES * 60f);
    }

    private bool ValidateReferences()
    {
        bool valid = titleText && timeText && taskIconImage && buttonBackground && buttonIcon &&
                     breakDurationInputField && canvasGroup && startResumeButton && pauseButton &&
                     resetButton && breakButton && closeButton && playIcon && pauseIcon && breakIcon;
        if (!valid) Debug.LogError("[TaskItemUI] One or more essential UI references or assets are missing!", this);
        return valid;
    }

    private void SetupButtonListeners()
    {
        startResumeButton.onClick.RemoveAllListeners();
        pauseButton.onClick.RemoveAllListeners();
        resetButton.onClick.RemoveAllListeners();
        breakButton.onClick.RemoveAllListeners();
        closeButton.onClick.RemoveAllListeners();

        startResumeButton.onClick.AddListener(() => OnStartResumeClicked?.Invoke());
        pauseButton.onClick.AddListener(() => OnPauseClicked?.Invoke());
        resetButton.onClick.AddListener(() => OnResetClicked?.Invoke());
        breakButton.onClick.AddListener(HandleBreakClickInternal);
        closeButton.onClick.AddListener(() => OnCloseClicked?.Invoke());
    }

    private void HandleBreakClickInternal()
    {
        float breakMinutes = DEFAULT_BREAK_MINUTES;
        float breakSeconds = breakMinutes * 60f;

        if (!string.IsNullOrWhiteSpace(breakDurationInputField.text))
        {
            if (float.TryParse(breakDurationInputField.text, NumberStyles.Float, CultureInfo.InvariantCulture, out float parsedMinutes) && parsedMinutes > 0)
            {
                breakSeconds = parsedMinutes * 60f;
            }
            else
            {
                breakDurationInputField.text = DEFAULT_BREAK_MINUTES.ToString("F0", CultureInfo.InvariantCulture);
                Debug.LogWarning($"[TaskItemUI] Invalid break duration input. Using default {DEFAULT_BREAK_MINUTES} minutes.", this);
            }
        }
        else
             breakDurationInputField.text = DEFAULT_BREAK_MINUTES.ToString("F0", CultureInfo.InvariantCulture);

        OnBreakClicked?.Invoke(breakSeconds);
    }

    public void SetTitle(string title)
    {
        titleText.text = title ?? "Untitled Task";
    }

    public void SetTaskIcon(Sprite iconSprite)
    {
        taskIconImage.sprite = iconSprite;
        taskIconImage.enabled = (iconSprite != null);
    }

    public void SetTimeDisplay(TimeSpan timeSpan, bool isBreakActive)
    {
        string formattedTime;
        if (timeSpan.TotalHours >= 1)
            formattedTime = $"{(int)timeSpan.TotalHours:D2}:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
        else
            formattedTime = $"{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";

        timeText.text = formattedTime;
    }

     public void SetBreakDurationInput(float durationSeconds)
    {
        float minutes = durationSeconds > 0 ? durationSeconds / 60f : DEFAULT_BREAK_MINUTES;
        breakDurationInputField.text = minutes.ToString("F0", CultureInfo.InvariantCulture);
    }

    public void UpdateVisualState(TaskState state, float currentElapsedTime)
    {
        switch (state)
        {
            case TaskState.Running:
                buttonBackground.color = playStateColor;
                buttonIcon.sprite = playIcon;
                break;
            case TaskState.Paused:
                buttonBackground.color = pauseStateColor;
                buttonIcon.sprite = pauseIcon;
                break;
            case TaskState.OnBreak:
                buttonBackground.color = breakStateColor;
                buttonIcon.sprite = breakIcon;
                break;
            default:
                buttonBackground.color = stoppedStateColor;
                buttonIcon.sprite = playIcon;
                break;
        }
        buttonIcon.enabled = buttonIcon.sprite != null;

        timeText.color = (state == TaskState.OnBreak) ? breakStateColor : normalTimeColor;

        startResumeButton.interactable = (state == TaskState.Stopped || state == TaskState.Paused || state == TaskState.OnBreak);
        pauseButton.interactable = (state == TaskState.Running);
        resetButton.interactable = (state != TaskState.Stopped || currentElapsedTime > 0.01f);
        breakButton.interactable = (state == TaskState.Running || state == TaskState.Paused);
        closeButton.interactable = true;

        breakDurationInputField.interactable = (state != TaskState.OnBreak);
    }

    void OnDestroy()
    {
        if (startResumeButton != null) startResumeButton.onClick.RemoveAllListeners();
        if (pauseButton != null) pauseButton.onClick.RemoveAllListeners();
        if (resetButton != null) resetButton.onClick.RemoveAllListeners();
        if (breakButton != null) breakButton.onClick.RemoveAllListeners();
        if (closeButton != null) closeButton.onClick.RemoveAllListeners();

        OnStartResumeClicked = null;
        OnPauseClicked = null;
        OnBreakClicked = null;
        OnResetClicked = null;
        OnCloseClicked = null;
    }

// --- Summary Block ---
// ScriptRole: Manages UI elements, updates appearance based on state, forwards button clicks via C# events.
// RelatedScripts: TaskItemActive (Controller)
// UsesSO: None directly (Receives Sprites/Colors from Inspector)
// ReceivesFrom: TaskItemActive (calls SetTitle, SetTaskIcon, SetTimeDisplay, UpdateVisualState)
// SendsTo: TaskItemActive (via C# events like OnStartResumeClicked)

}
