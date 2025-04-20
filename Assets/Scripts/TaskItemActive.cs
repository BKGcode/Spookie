using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Globalization;

public class TaskItemActive : MonoBehaviour
{
    [Header("Core UI References")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private Button startResumeButton; // Renamed for clarity
    [SerializeField] private Image startResumeIcon; // Icon for Start/Resume button
    [SerializeField] private Button pauseButton;       // *** NEW: Separate Pause Button ***
    [SerializeField] private Image pauseIconImage;    // *** NEW: Optional Image for Pause Button Icon ***
    [SerializeField] private Button resetButton;
    [SerializeField] private Button breakButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private TMP_InputField breakDurationInputField;

    [Header("Button Icons & Colors")]
    [SerializeField] private Sprite playIcon;    // Used for Start/Resume
    [SerializeField] private Sprite pauseIcon;   // Used for Pause
    [SerializeField] private Color breakTimeColor = Color.cyan;
    [SerializeField] private Color normalTimeColor = Color.white;

    private Action<int> _onStartCallback;
    private Action<int> _onPauseCallback;
    private Action<int, float> _onBreakCallback;
    private Action<int> _onResetCallback;
    private Action<int> _onCloseCallback;
    private Action<int> _onStopBreakAndResumeCallback;

    private int _taskIndex;
    private bool _isInitialized = false;
    private TaskState _currentState = TaskState.Stopped;

    private const float DEFAULT_BREAK_MINUTES = 5f;

    public void SetupActive(
        int index,
        TaskData data,
        Action<int> onStart,
        Action<int> onPause,
        Action<int, float> onBreak,
        Action<int> onReset,
        Action<int> onClose,
        Action<int> onStopBreakAndResume)
    {
        _taskIndex = index;
        _onStartCallback = onStart;
        _onPauseCallback = onPause;
        _onBreakCallback = onBreak;
        _onResetCallback = onReset;
        _onCloseCallback = onClose;
        _onStopBreakAndResumeCallback = onStopBreakAndResume;

        if (!ValidateReferences())
        {
             Debug.LogError($"[TaskItemActive] Initialization failed due to missing references on {gameObject.name}.", this);
            _isInitialized = false;
            gameObject.SetActive(false);
            return;
        }

        titleText.text = data.title ?? "Untitled Task";

        // Assign icons to their respective image components initially
        if(startResumeIcon != null) startResumeIcon.sprite = playIcon;
        if(pauseIconImage != null) pauseIconImage.sprite = pauseIcon;
        // Ensure pauseIconImage component exists if pauseIcon sprite is assigned
        if (pauseIcon != null && pauseIconImage == null)
        {
            Debug.LogWarning("[TaskItemActive] pauseIcon sprite is assigned, but pauseIconImage component is missing on the Pause button.", this);
        }


        SetupInputField();
        SetupButtonListeners();

        _isInitialized = true;
        Debug.Log($"[TaskItemActive] Initialized for task index {_taskIndex} ('{data.title}').");
        UpdateState(data.state, data.elapsedTime, data.breakElapsedTime);
    }

     private bool ValidateReferences()
    {
        bool valid = true;
        if (titleText == null) { Debug.LogError("[TaskItemActive] titleText is not assigned.", this); valid = false; }
        if (timeText == null) { Debug.LogError("[TaskItemActive] timeText is not assigned.", this); valid = false; }
        if (startResumeButton == null) { Debug.LogError("[TaskItemActive] startResumeButton is not assigned.", this); valid = false; }
        // startResumeIcon is optional if the button itself has the image
        // if (startResumeIcon == null) { Debug.LogError("[TaskItemActive] startResumeIcon is not assigned.", this); valid = false; }
        if (pauseButton == null) { Debug.LogError("[TaskItemActive] pauseButton is not assigned.", this); valid = false; } // *** NEW CHECK ***
        // pauseIconImage is optional
        if (resetButton == null) { Debug.LogError("[TaskItemActive] resetButton is not assigned.", this); valid = false; }
        if (breakButton == null) { Debug.LogError("[TaskItemActive] breakButton is not assigned.", this); valid = false; }
        if (closeButton == null) { Debug.LogError("[TaskItemActive] closeButton is not assigned.", this); valid = false; }
        if (breakDurationInputField == null) { Debug.LogError("[TaskItemActive] breakDurationInputField is not assigned.", this); valid = false; }
        if (playIcon == null) { Debug.LogError("[TaskItemActive] playIcon is not assigned.", this); valid = false; }
        if (pauseIcon == null) { Debug.LogError("[TaskItemActive] pauseIcon is not assigned.", this); valid = false; }
        return valid;
    }

    private void SetupInputField()
    {
        if (string.IsNullOrWhiteSpace(breakDurationInputField.text))
        {
            breakDurationInputField.text = DEFAULT_BREAK_MINUTES.ToString("F0", CultureInfo.InvariantCulture);
        }
        breakDurationInputField.contentType = TMP_InputField.ContentType.DecimalNumber;
    }

     private void SetupButtonListeners()
    {
        startResumeButton.onClick.RemoveAllListeners();
        pauseButton.onClick.RemoveAllListeners(); // *** NEW ***
        resetButton.onClick.RemoveAllListeners();
        breakButton.onClick.RemoveAllListeners();
        closeButton.onClick.RemoveAllListeners();

        startResumeButton.onClick.AddListener(HandleStartResumeClick); // Renamed handler
        pauseButton.onClick.AddListener(HandlePauseClick);          // *** NEW ***
        resetButton.onClick.AddListener(HandleResetClick);
        breakButton.onClick.AddListener(HandleBreakClick);
        closeButton.onClick.AddListener(HandleCloseClick);
    }

    public void UpdateState(TaskState newState, float elapsed, float breakElapsed)
    {
        if (!_isInitialized) return;

        _currentState = newState;
        UpdateTimeDisplay(newState, elapsed, breakElapsed);
        UpdateVisuals(newState, elapsed); // Logic moved here
    }

    private void UpdateTimeDisplay(TaskState state, float elapsed, float breakElapsed)
    {
         if (timeText == null) return;

        TimeSpan timeSpan;
        string prefix = "";

        if (state == TaskState.OnBreak)
        {
            timeSpan = TimeSpan.FromSeconds(breakElapsed);
            timeText.color = breakTimeColor;
            prefix = "Break: ";
        }
        else
        {
            timeSpan = TimeSpan.FromSeconds(elapsed);
            timeText.color = normalTimeColor;
            prefix = "";
        }

        string formattedTime;
        if (timeSpan.TotalHours >= 1)
            formattedTime = $"{(int)timeSpan.TotalHours:D2}:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
        else
            formattedTime = $"{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";

        timeText.text = prefix + formattedTime;
    }

    private void UpdateVisuals(TaskState state, float elapsed)
    {
         if (!_isInitialized) return;

        // --- Control Start/Resume Button ---
        bool canStartResume = state == TaskState.Stopped || state == TaskState.Paused || state == TaskState.OnBreak;
        startResumeButton.interactable = canStartResume;
        // Optionally hide Start/Resume button when running if Pause is visible
        // startResumeButton.gameObject.SetActive(canStartResume);
        if (startResumeIcon != null) startResumeIcon.sprite = playIcon; // Always shows play/resume symbol

        // --- Control Pause Button ---
        bool canPause = state == TaskState.Running;
        pauseButton.interactable = canPause;
        // Optionally hide Pause button when not running
        // pauseButton.gameObject.SetActive(canPause);
         if (pauseIconImage != null) pauseIconImage.sprite = pauseIcon; // Always shows pause symbol

        // --- Control Other Buttons ---
        resetButton.interactable = state != TaskState.Stopped || elapsed > 0.01f;
        breakButton.interactable = state == TaskState.Running;
        closeButton.interactable = true;
        breakDurationInputField.interactable = state != TaskState.OnBreak;
    }

    // Renamed from HandleStartPauseClick
    private void HandleStartResumeClick()
    {
        if (!_isInitialized) return;
        Debug.Log($"[TaskItemActive {_taskIndex}] Start/Resume button clicked. Current state: {_currentState}");

        if (_currentState == TaskState.Stopped || _currentState == TaskState.Paused)
        {
            _onStartCallback?.Invoke(_taskIndex); // Re-starts or resumes from pause
        }
        else if (_currentState == TaskState.OnBreak)
        {
            _onStopBreakAndResumeCallback?.Invoke(_taskIndex); // Stops break and resumes task
        }
        // No action needed if state is Running, as Pause button handles that
    }

    // *** NEW Method for the Pause Button ***
    private void HandlePauseClick()
    {
        if (!_isInitialized) return;

        if (_currentState == TaskState.Running)
        {
             Debug.Log($"[TaskItemActive {_taskIndex}] Pause button clicked.");
            _onPauseCallback?.Invoke(_taskIndex);
        }
        else
        {
            // Should not be clickable if not running, but log just in case.
            Debug.LogWarning($"[TaskItemActive {_taskIndex}] Pause button clicked but state is {_currentState}. No action taken.");
        }
    }


    private void HandleResetClick()
    {
        if (!_isInitialized) return;
        Debug.Log($"[TaskItemActive {_taskIndex}] Reset button clicked.");
        _onResetCallback?.Invoke(_taskIndex);
    }

    private void HandleBreakClick()
    {
        if (!_isInitialized || _currentState != TaskState.Running) return;

        float breakMinutes = DEFAULT_BREAK_MINUTES;
        float breakSeconds = breakMinutes * 60f;

        if (breakDurationInputField != null && !string.IsNullOrWhiteSpace(breakDurationInputField.text))
        {
             if (float.TryParse(breakDurationInputField.text, NumberStyles.Float, CultureInfo.InvariantCulture, out float parsedMinutes) && parsedMinutes > 0)
             {
                 breakMinutes = parsedMinutes;
                 breakSeconds = breakMinutes * 60f;
             }
             else
             {
                 Debug.LogWarning($"[TaskItemActive {_taskIndex}] Invalid break duration input '{breakDurationInputField.text}'. Using default {DEFAULT_BREAK_MINUTES} min ({DEFAULT_BREAK_MINUTES * 60f} sec).");
                 breakDurationInputField.text = DEFAULT_BREAK_MINUTES.ToString("F0", CultureInfo.InvariantCulture);
             }
        }
        Debug.Log($"[TaskItemActive {_taskIndex}] Break button clicked. Requesting break for {breakMinutes} min ({breakSeconds} sec).");
        _onBreakCallback?.Invoke(_taskIndex, breakSeconds);
    }

    private void HandleCloseClick()
    {
        if (!_isInitialized) return;
        Debug.Log($"[TaskItemActive {_taskIndex}] Close button clicked.");
        _onCloseCallback?.Invoke(_taskIndex);
    }

    private void OnDestroy()
    {
        if (startResumeButton != null) startResumeButton.onClick.RemoveAllListeners();
        if (pauseButton != null) pauseButton.onClick.RemoveAllListeners(); // *** NEW ***
        if (resetButton != null) resetButton.onClick.RemoveAllListeners();
        if (breakButton != null) breakButton.onClick.RemoveAllListeners();
        if (closeButton != null) closeButton.onClick.RemoveAllListeners();
    }
}

