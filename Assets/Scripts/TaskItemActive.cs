using UnityEngine;
using System;

public class TaskItemActive : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TaskItemUI taskItemUI;
    [SerializeField] private TaskIconSO taskIconSet;

    public event Action<int> CompleteRequested;
    public event Action<int> ResetRequested;
    public event Action<int> StartRequested;
    public event Action<int> PauseRequested;
    public event Action<int> BreakRequested5m;
    public event Action<int> BreakRequested15m;
    public event Action<int> StopBreakRequested;
    public event Action<int> CloseRequested;

    private TaskData _taskData;
    private int _taskIndex = -1;
    private bool _isInitialized = false;
    private TaskState _lastKnownState = TaskState.Stopped;
    private bool _wasBreakNotified = false;
    private bool _wasNearlyFinishedNotified = false;

    void Awake()
    {
        if (taskItemUI == null)
        {
            Debug.LogError("[TaskItemActive] TaskItemUI reference not set! Disabling.", this);
            enabled = false;
            return;
        }
        SubscribeToUIEvents();
    }

    public void SetupActive(int index, TaskData data)
    {
        if (data == null)
        {
            Debug.LogError("[TaskItemActive] SetupActive called with null TaskData! Disabling.", this);
            gameObject.SetActive(false);
            return;
        }
        _taskIndex = index;
        _taskData = data; // Keep a reference

        if (taskItemUI == null || taskIconSet == null)
        {
             Debug.LogError($"[TaskItemActive:{_taskIndex}] Missing TaskItemUI or TaskIconSet reference! Disabling.", this);
             _isInitialized = false;
             gameObject.SetActive(false);
             return;
        }

        taskItemUI.SetTitle(_taskData.title);
        taskItemUI.SetTaskIcon(taskIconSet.GetIconByIndex(_taskData.iconIndex));

        // Reset visual state based on initial TaskData
        _lastKnownState = _taskData.state;
        _wasBreakNotified = false;
        _wasNearlyFinishedNotified = false;
        UpdateTimerAndState(_taskIndex, _taskData.state, _taskData.elapsedTime, _taskData.breakElapsedTime); // Initial UI update

        _isInitialized = true;
        Debug.Log($"[TaskItemActive:{_taskIndex}] Initialized for task '{_taskData.title}'. Initial state: {_taskData.state}");
    }


    private void SubscribeToUIEvents()
    {
        if (taskItemUI == null) return;
        taskItemUI.OnMainActionButtonClicked -= HandleMainActionButton; taskItemUI.OnMainActionButtonClicked += HandleMainActionButton;
        taskItemUI.OnBreak5mClicked -= HandleBreak5m; taskItemUI.OnBreak5mClicked += HandleBreak5m;
        taskItemUI.OnBreak15mClicked -= HandleBreak15m; taskItemUI.OnBreak15mClicked += HandleBreak15m;
        taskItemUI.OnCompleteClicked -= HandleComplete; taskItemUI.OnCompleteClicked += HandleComplete;
        taskItemUI.OnResetClicked -= HandleReset; taskItemUI.OnResetClicked += HandleReset;
        taskItemUI.OnCloseClicked -= HandleClose; taskItemUI.OnCloseClicked += HandleClose;
    }

    private void UnsubscribeFromUIEvents()
    {
        if (taskItemUI == null) return;
        taskItemUI.OnMainActionButtonClicked -= HandleMainActionButton;
        taskItemUI.OnBreak5mClicked -= HandleBreak5m;
        taskItemUI.OnBreak15mClicked -= HandleBreak15m;
        taskItemUI.OnCompleteClicked -= HandleComplete;
        taskItemUI.OnResetClicked -= HandleReset;
        taskItemUI.OnCloseClicked -= HandleClose;
    }

    private void HandleMainActionButton()
    {
        if (!_isInitialized) return;
        Debug.Log($"[TaskItemActive:{_taskIndex}] Main action button clicked. Current state: {_lastKnownState}");
        switch (_lastKnownState)
        {
            case TaskState.Stopped:
            case TaskState.Paused:
                StartRequested?.Invoke(_taskIndex);
                break;
            case TaskState.Running:
                PauseRequested?.Invoke(_taskIndex);
                break;
            case TaskState.OnBreak:
                StopBreakRequested?.Invoke(_taskIndex);
                break;
            case TaskState.Completed: // Button should be disabled, but handle defensively
                 Debug.LogWarning($"[TaskItemActive:{_taskIndex}] Main action button clicked while Completed. Ignoring.");
                break;
        }
    }

    private void HandleBreak5m() { if (!_isInitialized) return; Debug.Log($"[TaskItemActive:{_taskIndex}] Break 5m requested."); BreakRequested5m?.Invoke(_taskIndex); }
    private void HandleBreak15m() { if (!_isInitialized) return; Debug.Log($"[TaskItemActive:{_taskIndex}] Break 15m requested."); BreakRequested15m?.Invoke(_taskIndex); }
    private void HandleComplete() { if (!_isInitialized) return; Debug.Log($"[TaskItemActive:{_taskIndex}] Complete requested via button."); CompleteRequested?.Invoke(_taskIndex); }
    private void HandleReset() { if (!_isInitialized) return; Debug.Log($"[TaskItemActive:{_taskIndex}] Reset requested."); ResetRequested?.Invoke(_taskIndex); }
    private void HandleClose() { if (!_isInitialized) return; Debug.Log($"[TaskItemActive:{_taskIndex}] Close requested."); CloseRequested?.Invoke(_taskIndex); }


    // Called by TaskListUI when TaskTimerManager ticks or state changes
    public void UpdateTimerAndState(int updatedIndex, TaskState newState, float newElapsedTime, float newBreakElapsedTime)
    {
        if (!_isInitialized || updatedIndex != _taskIndex) return; // Ignore if not initialized or for a different task

        bool stateChanged = _lastKnownState != newState;

        // Update internal data representation *first*
        // We assume _taskData reference is the same one TaskTimerManager is updating
         _taskData.state = newState;
         _taskData.elapsedTime = newElapsedTime;
         _taskData.breakElapsedTime = newBreakElapsedTime;
         // _taskData.breakDuration is presumably set by TaskTimerManager when break starts

        // Format time based on current state
        TimeSpan displayTime;
        if (newState == TaskState.OnBreak)
        {
             float remainingBreak = Mathf.Max(0, _taskData.breakDuration - newBreakElapsedTime);
             displayTime = TimeSpan.FromSeconds(remainingBreak);
        }
        else
        {
             displayTime = TimeSpan.FromSeconds(newElapsedTime);
        }

        // Update UI elements
        taskItemUI.SetTimeDisplay(displayTime, newState);


        // Handle state transitions and visual updates
        if (stateChanged)
        {
            Debug.Log($"[TaskItemActive:{_taskIndex}] State changed from {_lastKnownState} to {newState}. Updating UI visuals.");
             _lastKnownState = newState; // Update last known state *after* checking change

            if (newState == TaskState.Completed)
            {
                 taskItemUI.ShowCompletedState();
            }
            else
            {
                // If transitioning *out* of completed, reset visuals
                 if (_lastKnownState == TaskState.Completed && newState != TaskState.Completed)
                 {
                     taskItemUI.ResetCompletedStylesIfNeeded();
                 }
                 taskItemUI.UpdateVisualState(newState, newElapsedTime); // Update buttons, colors etc.
            }

            // Reset notification flags when state changes away from relevant state
            if (newState != TaskState.OnBreak) _wasBreakNotified = false;
            if (newState != TaskState.Running) _wasNearlyFinishedNotified = false;

            // Check for break finished *on transition*
            if (_lastKnownState == TaskState.OnBreak && newState != TaskState.OnBreak && !_wasBreakNotified)
            {
                // Break just finished
                taskItemUI.NotifyBreakFinished();
                _wasBreakNotified = true; // Prevent repeated notification
                Debug.Log($"[TaskItemActive:{_taskIndex}] Break finished notification triggered.");
            }
        }
        else if (newState == TaskState.Running || newState == TaskState.OnBreak)
        {
            // If state didn't change but timer is running, update time display only (already done above)
            // Check for nearly finished condition only when running
            // bool isNearlyFinished = (_taskData.targetDuration > 0 && newState == TaskState.Running && newElapsedTime >= _taskData.targetDuration * 0.9f);
            // if (isNearlyFinished != _wasNearlyFinishedNotified)
            // {
            //     taskItemUI.NotifyNearlyFinished(isNearlyFinished);
            //     _wasNearlyFinishedNotified = isNearlyFinished;
            //     if(isNearlyFinished) Debug.Log($"[TaskItemActive:{_taskIndex}] Nearly finished notification triggered.");
            // }

             // Still need to update button interactability etc. if time affects it (e.g., reset button)
             if(newState != TaskState.Completed) // Don't update if completed
             {
                taskItemUI.UpdateVisualState(newState, newElapsedTime /*, isNearlyFinished */);
             }
        }
        else if(newState == TaskState.Completed && !stateChanged)
        {
             // Ensure completed state visuals persist if we receive another tick while already completed
             taskItemUI.ShowCompletedState();
        }
    }


    void OnDestroy()
    {
        UnsubscribeFromUIEvents();
        CompleteRequested = null;
        ResetRequested = null;
        StartRequested = null;
        PauseRequested = null;
        BreakRequested5m = null;
        BreakRequested15m = null;
        StopBreakRequested = null;
        CloseRequested = null;
        Debug.Log($"[TaskItemActive:{_taskIndex}] Destroyed.");
    }
}


// --- Summary Block ---
// ScriptRole: Acts as a controller for the *active* task view. It holds the TaskData, listens to UI events from TaskItemUI, raises corresponding request events (e.g., StartRequested), and updates TaskItemUI based on data received (primarily timer ticks).
// RelatedScripts: TaskListUI, TaskItemUI, TaskData, TaskTimerManager, TaskIconSO, TaskState
// UsesSO: TaskIconSO
// SendsTo: TaskListUI (via C# events like StartRequested, CompleteRequested, CloseRequested etc.)
// ReceivesFrom: TaskListUI (via SetupActive, UpdateTimerAndState), TaskItemUI (via C# events like OnMainActionButtonClicked)

