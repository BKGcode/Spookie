using UnityEngine;
using System.Collections.Generic; // For IReadOnlyList

public class TaskUIManager : MonoBehaviour
{
    [Header("System References")]
    [SerializeField] private TaskSystem taskSystem;
    [SerializeField] private TaskTimerManager taskTimerManager;

    [Header("UI View References")]
    [SerializeField] private TaskListView taskListView;
    [SerializeField] private ActiveTaskView activeTaskView;
    // AddTaskHandler doesn't need a direct reference usually, as it talks directly to TaskSystem

    [Header("Configuration")]
    [SerializeField] private bool autoSelectFirstTask = true;

    private int _currentlySelectedTaskIndex = -1;
    private bool _isInitialized = false;

    void Start()
    {
        if (!ValidateReferences())
        {
            Debug.LogError($"[{nameof(TaskUIManager)}] Critical references missing! Disabling.", this);
            enabled = false;
            return;
        }

        SubscribeToSystemEvents();
        SubscribeToUIEvents();

        // Initial population
        HandleTaskListChanged(); // Populate the list view initially

        activeTaskView.HideView(); // Ensure active view starts hidden

        _isInitialized = true;
        Debug.Log($"[{nameof(TaskUIManager)}] Initialized successfully.");

        if (autoSelectFirstTask && taskSystem.Tasks.Count > 0)
        {
             SelectTask(0); // Attempt to auto-select
        }
    }

    void OnDestroy()
    {
        UnsubscribeFromSystemEvents();
        UnsubscribeFromUIEvents();
         Debug.Log($"[{nameof(TaskUIManager)}] Destroyed. Events unsubscribed.");
    }

    private bool ValidateReferences()
    {
        bool valid = true;
        if (taskSystem == null) { Debug.LogError($"[{nameof(TaskUIManager)}] Task System reference missing!", this); valid = false; }
        if (taskTimerManager == null) { Debug.LogError($"[{nameof(TaskUIManager)}] Task Timer Manager reference missing!", this); valid = false; }
        if (taskListView == null) { Debug.LogError($"[{nameof(TaskUIManager)}] Task List View reference missing!", this); valid = false; }
        if (activeTaskView == null) { Debug.LogError($"[{nameof(TaskUIManager)}] Active Task View reference missing!", this); valid = false; }
        return valid;
    }

    private void SubscribeToSystemEvents()
    {
        if (taskSystem != null)
        {
            taskSystem.OnTaskListChanged -= HandleTaskListChanged; // Prevent double sub
            taskSystem.OnTaskListChanged += HandleTaskListChanged;
        }
        if (taskTimerManager != null)
        {
             taskTimerManager.OnTaskTimerTick -= HandleTaskTimerTick;
             taskTimerManager.OnTaskTimerTick += HandleTaskTimerTick;
        }
         Debug.Log($"[{nameof(TaskUIManager)}] Subscribed to system events (TaskSystem, TaskTimerManager).");
    }

    private void UnsubscribeFromSystemEvents()
    {
        if (taskSystem != null)
        {
            taskSystem.OnTaskListChanged -= HandleTaskListChanged;
        }
         if (taskTimerManager != null)
        {
             taskTimerManager.OnTaskTimerTick -= HandleTaskTimerTick;
        }
    }

    private void SubscribeToUIEvents()
    {
        if (taskListView != null)
        {
            taskListView.OnTaskSelected.RemoveListener(HandleSelectRequestFromList);
            taskListView.OnTaskSelected.AddListener(HandleSelectRequestFromList);

            taskListView.OnTaskDeleteRequested.RemoveListener(HandleDeleteRequestFromList);
            taskListView.OnTaskDeleteRequested.AddListener(HandleDeleteRequestFromList);

            taskListView.OnTaskToggleCompleteRequested.RemoveListener(HandleToggleCompleteRequestFromList);
            taskListView.OnTaskToggleCompleteRequested.AddListener(HandleToggleCompleteRequestFromList);
        }
        if (activeTaskView != null)
        {
            activeTaskView.OnStartRequested.RemoveListener(HandleStartRequestFromActive);
            activeTaskView.OnStartRequested.AddListener(HandleStartRequestFromActive);

            activeTaskView.OnPauseRequested.RemoveListener(HandlePauseRequestFromActive);
            activeTaskView.OnPauseRequested.AddListener(HandlePauseRequestFromActive);

            activeTaskView.OnBreakRequested5m.RemoveListener(HandleBreak5mRequestFromActive);
            activeTaskView.OnBreakRequested5m.AddListener(HandleBreak5mRequestFromActive);

            activeTaskView.OnBreakRequested15m.RemoveListener(HandleBreak15mRequestFromActive);
            activeTaskView.OnBreakRequested15m.AddListener(HandleBreak15mRequestFromActive);

            activeTaskView.OnStopBreakRequested.RemoveListener(HandleStopBreakRequestFromActive);
            activeTaskView.OnStopBreakRequested.AddListener(HandleStopBreakRequestFromActive);

            activeTaskView.OnCompleteRequested.RemoveListener(HandleCompleteRequestFromActive);
            activeTaskView.OnCompleteRequested.AddListener(HandleCompleteRequestFromActive);

            activeTaskView.OnResetRequested.RemoveListener(HandleResetRequestFromActive);
            activeTaskView.OnResetRequested.AddListener(HandleResetRequestFromActive);

            activeTaskView.OnCloseRequested.RemoveListener(HandleCloseRequestFromActive);
            activeTaskView.OnCloseRequested.AddListener(HandleCloseRequestFromActive);
        }
         Debug.Log($"[{nameof(TaskUIManager)}] Subscribed to UI events (TaskListView, ActiveTaskView).");
    }

    private void UnsubscribeFromUIEvents()
    {
         if (taskListView != null)
        {
            taskListView.OnTaskSelected.RemoveListener(HandleSelectRequestFromList);
            taskListView.OnTaskDeleteRequested.RemoveListener(HandleDeleteRequestFromList);
            taskListView.OnTaskToggleCompleteRequested.RemoveListener(HandleToggleCompleteRequestFromList);
        }
         if (activeTaskView != null)
        {
            activeTaskView.OnStartRequested.RemoveListener(HandleStartRequestFromActive);
            activeTaskView.OnPauseRequested.RemoveListener(HandlePauseRequestFromActive);
            activeTaskView.OnBreakRequested5m.RemoveListener(HandleBreak5mRequestFromActive);
            activeTaskView.OnBreakRequested15m.RemoveListener(HandleBreak15mRequestFromActive);
            activeTaskView.OnStopBreakRequested.RemoveListener(HandleStopBreakRequestFromActive);
            activeTaskView.OnCompleteRequested.RemoveListener(HandleCompleteRequestFromActive);
            activeTaskView.OnResetRequested.RemoveListener(HandleResetRequestFromActive);
            activeTaskView.OnCloseRequested.RemoveListener(HandleCloseRequestFromActive);
        }
    }

    // --- System Event Handlers ---

    private void HandleTaskListChanged()
    {
        if (!_isInitialized || taskListView == null || taskSystem == null) return;

        Debug.Log($"[{nameof(TaskUIManager)}] TaskListChanged event received. Repopulating TaskListView.");
        IReadOnlyList<TaskData> tasks = taskSystem.Tasks;
        taskListView.PopulateTaskList(tasks);

        // Validate current selection after list changes
        ValidateSelection(tasks);
    }

     private void HandleTaskTimerTick(int index, TaskState state, float elapsedTime, float breakElapsedTime)
    {
        if (!_isInitialized || activeTaskView == null) return;

        // Forward the tick event ONLY if it's for the currently selected task and the view is active
        if (index == _currentlySelectedTaskIndex && activeTaskView.gameObject.activeSelf)
        {
             //Debug.Log($"[{nameof(TaskUIManager)}] Forwarding timer tick for index {index} to ActiveTaskView."); // Can be noisy
             activeTaskView.UpdateTimerDisplay(state, elapsedTime, breakElapsedTime);
        }
    }

    // --- UI Event Handlers ---

    private void HandleSelectRequestFromList(int index)
    {
        SelectTask(index);
    }

     private void HandleDeleteRequestFromList(int index)
    {
        if (taskSystem != null)
        {
             Debug.Log($"[{nameof(TaskUIManager)}] Delete request from list for index {index}. Forwarding to TaskSystem.");
             taskSystem.RemoveTask(index);
             // HandleTaskListChanged will be called automatically by TaskSystem event to update UI
        }
         else { Debug.LogError($"[{nameof(TaskUIManager)}] Cannot handle Delete Request - TaskSystem is null!"); }
    }

     private void HandleToggleCompleteRequestFromList(int index)
    {
        if (taskSystem != null)
        {
            Debug.Log($"[{nameof(TaskUIManager)}] Toggle Complete request from list for index {index}. Forwarding to TaskSystem.");
            taskSystem.ToggleTaskCompleted(index);
            // HandleTaskListChanged will update the list view.
            // We also need to update the active view if this task was selected.
            if (index == _currentlySelectedTaskIndex && activeTaskView != null && taskSystem.Tasks.Count > index)
            {
                 Debug.Log($"[{nameof(TaskUIManager)}] Toggled task is active. Refreshing active view for index {index}.");
                 activeTaskView.ShowTask(index, taskSystem.Tasks[index]);
            }
        }
         else { Debug.LogError($"[{nameof(TaskUIManager)}] Cannot handle ToggleComplete Request - TaskSystem is null!", this); }
    }

    // --- Active Task View Event Handlers ---

    private void HandleStartRequestFromActive(int index) { ForwardRequestToTimer(taskTimerManager.RequestStartTimer, index, "Start"); }
    private void HandlePauseRequestFromActive(int index) { ForwardRequestToTimer(taskTimerManager.RequestPauseTimer, index, "Pause"); }
    private void HandleBreak5mRequestFromActive(int index) { ForwardRequestToTimer(taskTimerManager.RequestStartBreak, index, "Break 5m", 300f); }
    private void HandleBreak15mRequestFromActive(int index) { ForwardRequestToTimer(taskTimerManager.RequestStartBreak, index, "Break 15m", 900f); }
    private void HandleStopBreakRequestFromActive(int index) { ForwardRequestToTimer(taskTimerManager.RequestStopBreakAndResume, index, "Stop Break"); }
    private void HandleCompleteRequestFromActive(int index) { ForwardRequestToTimer(taskTimerManager.RequestCompleteTask, index, "Complete Task"); } // Assuming Timer handles completion state change
    private void HandleResetRequestFromActive(int index) { ForwardRequestToTimer(taskTimerManager.RequestResetTimer, index, "Reset"); }

    private void HandleCloseRequestFromActive(int index)
    {
        if (index != _currentlySelectedTaskIndex)
        {
             Debug.LogWarning($"[{nameof(TaskUIManager)}] Close requested from active view for index {index}, but current index is {_currentlySelectedTaskIndex}. Ignoring.", this);
            return;
        }
        Debug.Log($"[{nameof(TaskUIManager)}] Close requested for index {index}. Hiding active view and deselecting.", this);
        DeselectTask();
    }

    // --- Helper Methods ---

    private void SelectTask(int index)
    {
        if (taskSystem == null || taskListView == null || activeTaskView == null) return;

        IReadOnlyList<TaskData> tasks = taskSystem.Tasks;
        if (index < 0 || index >= tasks.Count)
        {
            Debug.LogWarning($"[{nameof(TaskUIManager)}] Select requested for invalid index: {index}. Task count: {tasks.Count}. Deselecting if necessary.", this);
            DeselectTask(); // Ensure nothing is selected if index is bad
            return;
        }

        if (_currentlySelectedTaskIndex == index)
        {
            Debug.Log($"[{nameof(TaskUIManager)}] Task at index {index} is already selected. No change.", this);
            return; // Already selected
        }

        Debug.Log($"[{nameof(TaskUIManager)}] Selecting task at index {index}. Showing Active View.", this);
        _currentlySelectedTaskIndex = index;

        // Update the list view's highlight
        taskListView.SetSelectedTaskIndex(_currentlySelectedTaskIndex, tasks);

        // Show the active view with the selected task's data
        activeTaskView.ShowTask(_currentlySelectedTaskIndex, tasks[_currentlySelectedTaskIndex]);
    }

    private void DeselectTask()
    {
        if (_currentlySelectedTaskIndex == -1) return; // Nothing to deselect

         Debug.Log($"[{nameof(TaskUIManager)}] Deselecting task index {_currentlySelectedTaskIndex}. Hiding Active View.", this);
        _currentlySelectedTaskIndex = -1;

        if(taskListView != null && taskSystem != null)
        {
             taskListView.SetSelectedTaskIndex(_currentlySelectedTaskIndex, taskSystem.Tasks); // Update list highlight (remove)
        }
        if(activeTaskView != null)
        {
             activeTaskView.HideView(); // Hide the active panel
        }
    }

    private void ValidateSelection(IReadOnlyList<TaskData> tasks)
    {
        if (_currentlySelectedTaskIndex != -1) // If something was selected
        {
            if (_currentlySelectedTaskIndex >= tasks.Count) // And it's now out of bounds (e.g., deleted)
            {
                Debug.Log($"[{nameof(TaskUIManager)}] Previously selected index {_currentlySelectedTaskIndex} is now invalid (Task count: {tasks.Count}). Deselecting.", this);
                DeselectTask();
            }
            else
            {
                // Optional: Refresh active view data if needed, though ToggleComplete handles its case.
                // Could be useful if task data can change in other ways.
                 // activeTaskView.ShowTask(_currentlySelectedTaskIndex, tasks[_currentlySelectedTaskIndex]);
                 // Also refresh list view selection highlight
                 if (taskListView != null)
                 {
                     taskListView.SetSelectedTaskIndex(_currentlySelectedTaskIndex, tasks);
                 }
            }
        }
        else // Nothing was selected, ensure active view is hidden
        {
             if (activeTaskView != null && activeTaskView.gameObject.activeSelf)
             {
                 Debug.Log($"[{nameof(TaskUIManager)}] No task selected, ensuring Active View is hidden.", this);
                 activeTaskView.HideView();
             }
              // Ensure list view has no selection highlight
             if (taskListView != null)
             {
                 taskListView.SetSelectedTaskIndex(-1, tasks);
             }
        }
    }

    // Generic helpers to forward requests to the timer manager, checking index match
    private void ForwardRequestToTimer(System.Action<int> requestAction, int index, string actionName)
    {
        if (index != _currentlySelectedTaskIndex)
        {
             Debug.LogWarning($"[{nameof(TaskUIManager)}] {actionName} requested for index {index} but active index is {_currentlySelectedTaskIndex}. Request ignored.", this);
            return;
        }
        if (taskTimerManager != null && requestAction != null)
        {
             Debug.Log($"[{nameof(TaskUIManager)}] Forwarding '{actionName}' request for index {index} to TaskTimerManager.", this);
            requestAction.Invoke(index);
        }
        else { Debug.LogError($"[{nameof(TaskUIManager)}] Cannot forward '{actionName}' request - TaskTimerManager is null or action delegate invalid!", this); }
    }

    private void ForwardRequestToTimer(System.Action<int, float> requestAction, int index, string actionName, float param)
    {
         if (index != _currentlySelectedTaskIndex)
        {
             Debug.LogWarning($"[{nameof(TaskUIManager)}] {actionName} requested for index {index} but active index is {_currentlySelectedTaskIndex}. Request ignored.", this);
            return;
        }
        if (taskTimerManager != null && requestAction != null)
        {
             Debug.Log($"[{nameof(TaskUIManager)}] Forwarding '{actionName}' request with param {param} for index {index} to TaskTimerManager.", this);
            requestAction.Invoke(index, param);
        }
        else { Debug.LogError($"[{nameof(TaskUIManager)}] Cannot forward '{actionName}' request - TaskTimerManager is null or action delegate invalid!", this); }
    }


    // --- Summary Block ---
    // ScriptRole: Coordinates interaction between task UI views (List, Active) and backend systems (TaskSystem, TaskTimerManager). Manages overall UI state like task selection.
    // RelatedScripts: TaskSystem, TaskTimerManager, TaskListView, ActiveTaskView, AddTaskHandler (implicitly related)
    // UsesSO: None
    // ReceivesFrom: TaskSystem (OnTaskListChanged), TaskTimerManager (OnTaskTimerTick), TaskListView (OnTaskSelected, etc.), ActiveTaskView (OnStartRequested, etc.)
    // SendsTo: TaskSystem (RemoveTask, ToggleTaskCompleted), TaskTimerManager (Request... methods), TaskListView (PopulateTaskList, SetSelectedTaskIndex), ActiveTaskView (ShowTask, HideView, UpdateTimerDisplay)
}

