using UnityEngine;
using System;

[RequireComponent(typeof(TaskItemUI))]
public class TaskItemActive : MonoBehaviour
{
    [Header("Component References")]
    [SerializeField] private TaskItemUI taskItemUI;

    [Header("Data References")]
    [SerializeField] private TaskIconSO taskIconSet;

    public event Action<int> StartRequested;
    public event Action<int> PauseRequested;
    public event Action<int, float> BreakRequested;
    public event Action<int> ResetRequested;
    public event Action<int> CloseRequested;
    public event Action<int> StopBreakAndResumeRequested;

    private int _taskIndex;
    private TaskData _currentTaskData;
    private TaskState _currentState = TaskState.Stopped;
    private bool _isInitialized = false;

    void Awake()
    {
        if (taskItemUI == null) taskItemUI = GetComponent<TaskItemUI>();

        if (taskItemUI == null)
        {
            Debug.LogError($"[TaskItemActive] TaskItemUI component not found on {gameObject.name}. Disabling.", this);
            enabled = false;
            return;
        }
        SubscribeToUIEvents();
    }

    public void SetupActive(int index, TaskData data)
    {
        if (data == null || taskItemUI == null || taskIconSet == null)
        {
            Debug.LogError($"[TaskItemActive {index}] Setup failed due to null data, UI, or IconSet on {gameObject.name}. Disabling.", this);
            gameObject.SetActive(false);
            _isInitialized = false;
            return;
        }

        _taskIndex = index;
        _currentTaskData = data;
        _currentState = data.state;

        taskItemUI.SetTitle(_currentTaskData.title);
        taskItemUI.SetTaskIcon(taskIconSet.GetIconByIndex(_currentTaskData.iconIndex));
        taskItemUI.SetBreakDurationInput(_currentTaskData.breakDuration);

        float displayTimeValue = CalculateDisplayTime(_currentState, _currentTaskData.elapsedTime, _currentTaskData.breakDuration, _currentTaskData.breakElapsedTime);
        TimeSpan displayTimeSpan = TimeSpan.FromSeconds(displayTimeValue);

        taskItemUI.UpdateVisualState(_currentState, _currentTaskData.elapsedTime);
        taskItemUI.SetTimeDisplay(displayTimeSpan, _currentState == TaskState.OnBreak);

        _isInitialized = true;
        gameObject.SetActive(true);
        Debug.Log($"[TaskItemActive {_taskIndex}] Initialized task '{_currentTaskData.title}'. State: {_currentState}");
    }

    public void UpdateTimerAndState(TaskState newState, float newElapsedTime, float newBreakElapsedTime)
    {
        if (!_isInitialized || taskItemUI == null) return;

        _currentState = newState;

        float displayTimeValue = CalculateDisplayTime(newState, newElapsedTime, _currentTaskData.breakDuration, newBreakElapsedTime);
        TimeSpan displayTimeSpan = TimeSpan.FromSeconds(displayTimeValue);

        taskItemUI.UpdateVisualState(newState, newElapsedTime);
        taskItemUI.SetTimeDisplay(displayTimeSpan, newState == TaskState.OnBreak);
    }

    private float CalculateDisplayTime(TaskState state, float elapsed, float breakDuration, float breakElapsed)
    {
        if (state == TaskState.OnBreak)
        {
            return Mathf.Max(0f, breakDuration - breakElapsed);
        }
        else
        {
            return elapsed;
        }
    }

    public void UpdateDataDisplay(TaskData newData)
    {
        if (!_isInitialized || taskItemUI == null || newData == null) return;

        Debug.Log($"[TaskItemActive {_taskIndex}] Updating data display for '{newData.title}'.");
        _currentTaskData = newData;

        taskItemUI.SetTitle(_currentTaskData.title);
        taskItemUI.SetTaskIcon(taskIconSet.GetIconByIndex(_currentTaskData.iconIndex));
        taskItemUI.SetBreakDurationInput(_currentTaskData.breakDuration);
    }

    private void SubscribeToUIEvents()
    {
        if (taskItemUI == null) return;
        taskItemUI.OnStartResumeClicked += HandleStartResumeClick;
        taskItemUI.OnPauseClicked += HandlePauseClick;
        taskItemUI.OnBreakClicked += HandleBreakClick;
        taskItemUI.OnResetClicked += HandleResetClick;
        taskItemUI.OnCloseClicked += HandleCloseClick;
    }

    private void UnsubscribeFromUIEvents()
    {
        if (taskItemUI == null) return;
        taskItemUI.OnStartResumeClicked -= HandleStartResumeClick;
        taskItemUI.OnPauseClicked -= HandlePauseClick;
        taskItemUI.OnBreakClicked -= HandleBreakClick;
        taskItemUI.OnResetClicked -= HandleResetClick;
        taskItemUI.OnCloseClicked -= HandleCloseClick;
    }

    private void HandleStartResumeClick()
    {
        if (!_isInitialized) return;
        if (_currentState == TaskState.Stopped || _currentState == TaskState.Paused)
        {
            Debug.Log($"[TaskItemActive {_taskIndex}] Raising StartRequested event.");
            StartRequested?.Invoke(_taskIndex);
        }
        else if (_currentState == TaskState.OnBreak)
        {
            Debug.Log($"[TaskItemActive {_taskIndex}] Raising StopBreakAndResumeRequested event.");
            StopBreakAndResumeRequested?.Invoke(_taskIndex);
        }
    }

    private void HandlePauseClick()
    {
        if (!_isInitialized || _currentState != TaskState.Running) return;
        Debug.Log($"[TaskItemActive {_taskIndex}] Raising PauseRequested event.");
        PauseRequested?.Invoke(_taskIndex);
    }

    private void HandleBreakClick(float breakDurationSeconds)
    {
        if (!_isInitialized || !(_currentState == TaskState.Running || _currentState == TaskState.Paused)) return;
        Debug.Log($"[TaskItemActive {_taskIndex}] Raising BreakRequested event for {breakDurationSeconds}s.");
        BreakRequested?.Invoke(_taskIndex, breakDurationSeconds);
    }

    private void HandleResetClick()
    {
        if (!_isInitialized) return;
        if (_currentState != TaskState.Stopped || (_currentTaskData != null && _currentTaskData.elapsedTime > 0.01f) )
        {
            Debug.Log($"[TaskItemActive {_taskIndex}] Raising ResetRequested event.");
            ResetRequested?.Invoke(_taskIndex);
        } else {
            Debug.Log($"[TaskItemActive {_taskIndex}] Reset requested but state is Stopped with no significant time elapsed. Ignoring.");
        }
    }

    private void HandleCloseClick()
    {
        if (!_isInitialized) return;
        Debug.Log($"[TaskItemActive {_taskIndex}] Raising CloseRequested event.");
        CloseRequested?.Invoke(_taskIndex);
    }

    void OnDestroy()
    {
        UnsubscribeFromUIEvents();
        Debug.Log($"[TaskItemActive {_taskIndex}] Destroyed.");
    }

// --- Summary Block ---
// ScriptRole: Controller for a task item. Manages state, data, reacts to UI events, communicates with TaskManager via C# events.
// RelatedScripts: TaskItemUI (View), TaskManager (Subscriber), TaskData, TaskState, TaskIconSO
// UsesSO: TaskIconSO
// ReceivesFrom: TaskManager (SetupActive, UpdateTimerAndState, UpdateDataDisplay), TaskItemUI (via C# events)
// SendsTo: TaskManager (via C# events like StartRequested), TaskItemUI (via public methods like UpdateVisualState, SetTimeDisplay)
}

// Reminder: Ensure TaskState enum, TaskData class, TaskIconSO script are defined elsewhere.
// Reminder: Ensure the managing script (e.g., TaskManager) subscribes to the public events (StartRequested, etc.)
//           during SetupActive and unsubscribes when the TaskItemActive is destroyed.