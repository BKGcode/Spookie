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
    [SerializeField] private Button startResumeButton;
    [SerializeField] private Image buttonBackground;
    [SerializeField] private Image buttonIcon;
    [SerializeField] private Button pauseButton;
    [SerializeField] private Button resetButton;
    [SerializeField] private Button breakButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private TMP_InputField breakDurationInputField;

    [Header("Button Icons & Colors")]
    [SerializeField] private Sprite playIcon;
    [SerializeField] private Sprite pauseIcon;
    [SerializeField] private Sprite breakIcon;
    [SerializeField] private Color playStateColor = Color.green;
    [SerializeField] private Color pauseStateColor = Color.yellow;
    [SerializeField] private Color breakStateColor = Color.cyan;
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
            _isInitialized = false;
            gameObject.SetActive(false);
            return;
        }

        titleText.text = data.title ?? "Untitled Task";
        SetupInputField(data);
        SetupButtonListeners();

        _isInitialized = true;

        float initialTimeValue = 0f;
        if (data.state == TaskState.OnBreak)
        {
             initialTimeValue = Mathf.Max(0f, data.breakDuration - data.breakElapsedTime);
        }
        else
        {
            initialTimeValue = data.elapsedTime;
        }
        UpdateState(data.state, data.elapsedTime, initialTimeValue);
    }

    private bool ValidateReferences()
    {
        bool valid = true;
        if (titleText == null) { valid = false; }
        if (timeText == null) { valid = false; }
        if (startResumeButton == null) { valid = false; }
        if (buttonBackground == null) { valid = false; }
        if (buttonIcon == null) { valid = false; }
        if (pauseButton == null) { valid = false; }
        if (resetButton == null) { valid = false; }
        if (breakButton == null) { valid = false; }
        if (closeButton == null) { valid = false; }
        if (breakDurationInputField == null) { valid = false; }
        if (playIcon == null) { valid = false; }
        if (pauseIcon == null) { valid = false; }
        if (breakIcon == null) { valid = false; }
        if (!valid) { Debug.LogError($"[TaskItemActive {_taskIndex}] Validation failed on {gameObject.name}. Check Inspector assignments.", this); }
        return valid;
    }

    private void SetupInputField(TaskData data)
    {
        float initialMinutes = (data != null && data.breakDuration > 0)
                                ? data.breakDuration / 60f
                                : DEFAULT_BREAK_MINUTES;

        if (breakDurationInputField != null)
        {
             breakDurationInputField.text = initialMinutes.ToString("F0", CultureInfo.InvariantCulture);
             breakDurationInputField.contentType = TMP_InputField.ContentType.DecimalNumber;
        }
    }

    private void SetupButtonListeners()
    {
        startResumeButton.onClick.RemoveAllListeners();
        pauseButton.onClick.RemoveAllListeners();
        resetButton.onClick.RemoveAllListeners();
        breakButton.onClick.RemoveAllListeners();
        closeButton.onClick.RemoveAllListeners();

        startResumeButton.onClick.AddListener(HandleStartResumeClick);
        pauseButton.onClick.AddListener(HandlePauseClick);
        resetButton.onClick.AddListener(HandleResetClick);
        breakButton.onClick.AddListener(HandleBreakClick);
        closeButton.onClick.AddListener(HandleCloseClick);
    }

    public void UpdateState(TaskState newState, float elapsed, float breakTimeValue)
    {
        if (!_isInitialized) return;

        _currentState = newState;
        UpdateTimeDisplay(newState, elapsed, breakTimeValue);
        UpdateVisuals(newState, elapsed);
    }

    private void UpdateTimeDisplay(TaskState state, float elapsed, float breakTimeValue)
    {
        if (timeText == null) return;

        TimeSpan timeSpan;
        string prefix = "";

        if (state == TaskState.OnBreak)
        {
            timeSpan = TimeSpan.FromSeconds(breakTimeValue >= 0 ? breakTimeValue : 0);
            timeText.color = breakStateColor;
        }
        else
        {
            timeSpan = TimeSpan.FromSeconds(elapsed);
            timeText.color = normalTimeColor;
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

        bool canStartResume = state == TaskState.Stopped || state == TaskState.Paused || state == TaskState.OnBreak;
        startResumeButton.interactable = canStartResume;

        if (buttonBackground != null && buttonIcon != null)
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
                    buttonBackground.color = normalTimeColor;
                    buttonIcon.sprite = playIcon;
                    break;
            }
            buttonIcon.color = Color.white;
            buttonIcon.enabled = buttonIcon.sprite != null;
        }

        bool canPause = state == TaskState.Running;
        pauseButton.interactable = canPause;

        resetButton.interactable = state != TaskState.Stopped || elapsed > 0.01f;
        breakButton.interactable = state == TaskState.Running || state == TaskState.Paused;
        closeButton.interactable = true;
        breakDurationInputField.interactable = state != TaskState.OnBreak;
    }


    private void HandleStartResumeClick()
    {
        if (!_isInitialized) return;

        if (_currentState == TaskState.Stopped || _currentState == TaskState.Paused)
        {
            _onStartCallback?.Invoke(_taskIndex);
        }
        else if (_currentState == TaskState.OnBreak)
        {
             _onStopBreakAndResumeCallback?.Invoke(_taskIndex);
        }
    }

    private void HandlePauseClick()
    {
        if (!_isInitialized) return;

        if (_currentState == TaskState.Running)
        {
            _onPauseCallback?.Invoke(_taskIndex);
        }
    }

    private void HandleResetClick()
    {
        if (!_isInitialized) return;
        _onResetCallback?.Invoke(_taskIndex);
    }

    private void HandleBreakClick()
    {
        if (!_isInitialized) return;

        if (!(_currentState == TaskState.Running || _currentState == TaskState.Paused))
        {
           return;
        }

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
                breakDurationInputField.text = DEFAULT_BREAK_MINUTES.ToString("F0", CultureInfo.InvariantCulture);
                breakSeconds = DEFAULT_BREAK_MINUTES * 60f;
            }
        }
        else
        {
            breakSeconds = DEFAULT_BREAK_MINUTES * 60f;
            if(breakDurationInputField != null) {
                breakDurationInputField.text = DEFAULT_BREAK_MINUTES.ToString("F0", CultureInfo.InvariantCulture);
            }
        }
        _onBreakCallback?.Invoke(_taskIndex, breakSeconds);
    }

    private void HandleCloseClick()
    {
        if (!_isInitialized) return;
        _onCloseCallback?.Invoke(_taskIndex);
    }

    private void OnDestroy()
    {
        if (startResumeButton != null) startResumeButton.onClick.RemoveAllListeners();
        if (pauseButton != null) pauseButton.onClick.RemoveAllListeners();
        if (resetButton != null) resetButton.onClick.RemoveAllListeners();
        if (breakButton != null) breakButton.onClick.RemoveAllListeners();
        if (closeButton != null) closeButton.onClick.RemoveAllListeners();
    }
}
