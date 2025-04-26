using UnityEngine;
using UnityEngine.Events;
using System;

public class ActiveTaskView : MonoBehaviour
{
    [Header("System References")]
    [SerializeField] private TaskTimerManager taskTimerManager; // Needed to subscribe to timer ticks

    [Header("UI References")]
    [SerializeField] private GameObject activeTaskPanel; // The root panel to show/hide
    [SerializeField] private TaskItemActive taskItemActiveController; // The script managing the active item's UI elements

    [Header("Events (Outgoing Requests)")]
    // Events triggered by buttons within the TaskItemActive UI
    public UnityEvent<int> OnStartRequested;
    public UnityEvent<int> OnPauseRequested;
    public UnityEvent<int> OnBreakRequested5m;
    public UnityEvent<int> OnBreakRequested15m;
    public UnityEvent<int> OnStopBreakRequested;
    public UnityEvent<int> OnCompleteRequested; // Note: Consider if TaskSystem should handle this via Toggle
    public UnityEvent<int> OnResetRequested;
    public UnityEvent<int> OnCloseRequested;

    private int _currentlyDisplayedTaskIndex = -1;
    private bool _isActive = false;

    void Start()
    {
        if (!ValidateReferences())
        {
            Debug.LogError($"[{nameof(ActiveTaskView)}] Critical references missing. Disabling component.", this);
            enabled = false;
            return;
        }
        ShowView(false); // Start hidden
        Debug.Log($"[{nameof(ActiveTaskView)}] Initialized.");
    }

    void OnDestroy()
    {
        UnsubscribeFromTimer();
        UnsubscribeFromActiveTaskController();
    }

    private bool ValidateReferences()
    {
        bool isValid = true;
        if (taskTimerManager == null) { Debug.LogError($"[{nameof(ActiveTaskView)}] Task Timer Manager reference is missing!", this); isValid = false; }
        if (activeTaskPanel == null) { Debug.LogError($"[{nameof(ActiveTaskView)}] Active Task Panel reference is missing!", this); isValid = false; }
        if (taskItemActiveController == null) { Debug.LogError($"[{nameof(ActiveTaskView)}] Task Item Active Controller reference is missing!", this); isValid = false; }
        // Optional: Check if taskItemActiveController has necessary sub-components if applicable
        return isValid;
    }

    // Called by TaskUIManager to display a specific task
    public void ShowTask(int index, TaskData taskData)
    {
        if (taskItemActiveController == null)
        {
            Debug.LogError($"[{nameof(ActiveTaskView)}] Cannot show task - TaskItemActive Controller is null!", this);
            return;
        }

        Debug.Log($"[{nameof(ActiveTaskView)}] Showing task view for index {index}: '{taskData.title}'.", this);
        _currentlyDisplayedTaskIndex = index;
        taskItemActiveController.SetupActive(index, taskData); // Configure the detailed view
        ShowView(true);
    }

    // Called by TaskUIManager to hide the view
    public void HideView()
    {
        Debug.Log($"[{nameof(ActiveTaskView)}] Hiding task view.", this);
         _currentlyDisplayedTaskIndex = -1;
        ShowView(false);
    }

    // Called by TaskUIManager or internally when the displayed task's timer ticks
    // Note: TaskUIManager now filters ticks, so we don't need to check index here anymore.
    public void UpdateTimerDisplay(TaskState state, float elapsedTime, float breakElapsedTime)
     {
         if (_isActive && taskItemActiveController != null)
         {
             // Pass the update directly to the specific UI controller
             taskItemActiveController.UpdateTimerAndState(_currentlyDisplayedTaskIndex, state, elapsedTime, breakElapsedTime);
         }
     }

    private void ShowView(bool show)
    {
        if (activeTaskPanel == null) return;

        if (show == _isActive) return; // No change needed

        activeTaskPanel.SetActive(show);
        _isActive = show;

        if (_isActive)
        {
            SubscribeToTimer();
            SubscribeToActiveTaskController();
             Debug.Log($"[{nameof(ActiveTaskView)}] View activated. Subscribed to Timer and Controller events.", this);
        }
        else
        {
            UnsubscribeFromTimer();
            UnsubscribeFromActiveTaskController();
            Debug.Log($"[{nameof(ActiveTaskView)}] View deactivated. Unsubscribed from events.", this);
        }
    }

    private void SubscribeToTimer()
    {
        // Note: TaskUIManager will filter the event, so this view doesn't need to check the index itself.
        // The UIManager will call UpdateTimerDisplay only when appropriate.
        // However, if direct subscription is preferred (less indirection):
        /*
        if (taskTimerManager != null)
        {
            taskTimerManager.OnTaskTimerTick -= HandleTaskTimerTick; // Prevent double sub
            taskTimerManager.OnTaskTimerTick += HandleTaskTimerTick;
        }
        */
    }

     private void UnsubscribeFromTimer()
     {
         // If directly subscribed in SubscribeToTimer:
         /*
         if (taskTimerManager != null)
         {
             taskTimerManager.OnTaskTimerTick -= HandleTaskTimerTick;
         }
         */
     }

    // Only needed if directly subscribing to TaskTimerManager
    /*
    private void HandleTaskTimerTick(int index, TaskState state, float elapsedTime, float breakElapsedTime)
    {
        if (_isActive && index == _currentlyDisplayedTaskIndex)
        {
            UpdateTimerDisplay(state, elapsedTime, breakElapsedTime);
        }
    }
    */

    private void SubscribeToActiveTaskController()
    {
        if (taskItemActiveController == null) return;
        // Clear existing listeners first to prevent duplicates if re-enabled
        UnsubscribeFromActiveTaskController();

        // Forward internal UI events to public UnityEvents
        taskItemActiveController.StartRequested += (idx) => OnStartRequested?.Invoke(idx);
        taskItemActiveController.PauseRequested += (idx) => OnPauseRequested?.Invoke(idx);
        taskItemActiveController.BreakRequested5m += (idx) => OnBreakRequested5m?.Invoke(idx);
        taskItemActiveController.BreakRequested15m += (idx) => OnBreakRequested15m?.Invoke(idx);
        taskItemActiveController.StopBreakRequested += (idx) => OnStopBreakRequested?.Invoke(idx);
        taskItemActiveController.CompleteRequested += (idx) => OnCompleteRequested?.Invoke(idx);
        taskItemActiveController.ResetRequested += (idx) => OnResetRequested?.Invoke(idx);
        taskItemActiveController.CloseRequested += (idx) => OnCloseRequested?.Invoke(idx);
    }

    private void UnsubscribeFromActiveTaskController()
    {
        if (taskItemActiveController == null) return;
    }


    // --- Summary Block ---
    // ScriptRole: Manages the display and interaction for the single, currently active task detail view.
    // RelatedScripts: TaskUIManager, TaskTimerManager, TaskItemActive, TaskData, TaskState
    // UsesSO: None
    // ReceivesFrom: TaskUIManager (ShowTask, HideView, UpdateTimerDisplay), TaskItemActive (internal UI interaction events like button clicks)
    // SendsTo: TaskUIManager (via On...Requested UnityEvents)
}


