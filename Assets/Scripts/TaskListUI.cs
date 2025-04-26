using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;

public class TaskListUI : MonoBehaviour
{
    [Header("System References")]
    [SerializeField] private TaskSystem taskSystem;
    [SerializeField] private TaskTimerManager taskTimerManager;

    [Header("UI References")]
    [SerializeField] private GameObject taskListPanel;
    [SerializeField] private Transform taskListContent;
    [SerializeField] private GameObject taskItemMinimalPrefab;
    [SerializeField] private TMP_InputField newTaskInputField;
    [SerializeField] private Button addTaskButton;
    [SerializeField] private GameObject activeTaskPanel;
    [SerializeField] private TaskItemActive currentActiveTaskController;

    [Header("Configuration")]
    [SerializeField] private bool autoSelectFirstTask = true;

    private List<GameObject> _minimalTaskItemObjects = new List<GameObject>();
    private List<TaskItemMinimal> _minimalTaskControllers = new List<TaskItemMinimal>();
    private int _currentlySelectedTaskIndex = -1;
    private bool _isInitialized = false;

    void Start()
    {
        if (!ValidateCoreReferences())
        {
            Debug.LogError("[TaskListUI] Core references missing! Disabling UI.", this);
            enabled = false;
            return;
        }

        SubscribeToSystemEvents();
        SetupInputListeners();
        PopulateTaskList();

        ShowActiveTaskView(false);

        _isInitialized = true;
        Debug.Log("[TaskListUI] Initialized successfully.");

        if (autoSelectFirstTask && taskSystem.Tasks.Count > 0)
        {
            Debug.Log("[TaskListUI] Auto-selecting first task.");
            HandleSelectRequest(0);
        }
    }

    void OnDestroy()
    {
        UnsubscribeFromSystemEvents();
        UnsubscribeFromAllMinimalTasks();
        UnsubscribeFromActiveTask();
        ClearTaskListVisuals();
        Debug.Log("[TaskListUI] Destroyed. Events unsubscribed, UI cleared.");
    }

    private bool ValidateCoreReferences()
    {
        bool valid = true;
        if (taskSystem == null) { Debug.LogError("[TaskListUI] 'Task System' reference is missing!", this); valid = false; }
        if (taskTimerManager == null) { Debug.LogError("[TaskListUI] 'Task Timer Manager' reference is missing!", this); valid = false; }
        if (taskListPanel == null) { Debug.LogError("[TaskListUI] 'Task List Panel' reference is missing!", this); valid = false; }
        if (taskListContent == null) { Debug.LogError("[TaskListUI] 'Task List Content' reference is missing!", this); valid = false; }
        if (taskItemMinimalPrefab == null) { Debug.LogError("[TaskListUI] 'Task Item Minimal Prefab' reference is missing!", this); valid = false; }
        if (newTaskInputField == null) { Debug.LogError("[TaskListUI] 'New Task Input Field' reference is missing!", this); valid = false; }
        if (addTaskButton == null) { Debug.LogError("[TaskListUI] 'Add Task Button' reference is missing!", this); valid = false; }
        if (activeTaskPanel == null) { Debug.LogError("[TaskListUI] 'Active Task Panel' reference is missing!", this); valid = false; }
        if (currentActiveTaskController == null) { Debug.LogError("[TaskListUI] 'Current Active Task Controller' reference is missing!", this); valid = false; }

        if (taskItemMinimalPrefab != null && taskItemMinimalPrefab.GetComponent<TaskItemMinimal>() == null)
        {
             Debug.LogError($"[TaskListUI] Minimal Task Prefab '{taskItemMinimalPrefab.name}' is MISSING the required TaskItemMinimal script!", taskItemMinimalPrefab);
             valid = false;
        }
        if (currentActiveTaskController != null && currentActiveTaskController.GetComponentInChildren<TaskItemUI>(true) == null)
        {
             Debug.LogError($"[TaskListUI] Active Task Controller GameObject '{currentActiveTaskController.gameObject.name}' or its children are MISSING the required TaskItemUI script!", currentActiveTaskController.gameObject);
             valid = false;
        }
        return valid;
    }

    private void SubscribeToSystemEvents()
    {
        if (taskSystem != null)
        {
            taskSystem.OnTaskListChanged -= HandleTaskListChanged;
            taskSystem.OnTaskListChanged += HandleTaskListChanged;
        }
        if (taskTimerManager != null)
        {
            taskTimerManager.OnTaskTimerTick -= HandleTaskTimerTick;
            taskTimerManager.OnTaskTimerTick += HandleTaskTimerTick;
        }
         Debug.Log("[TaskListUI] Subscribed to TaskSystem and TaskTimerManager events.");
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

    private void SetupInputListeners()
    {
        addTaskButton.onClick.RemoveAllListeners();
        addTaskButton.onClick.AddListener(HandleAddTask);

        newTaskInputField.onSubmit.RemoveAllListeners();
        newTaskInputField.onSubmit.AddListener((text) => HandleAddTask());
    }

    private void HandleAddTask()
    {
        string taskTitle = newTaskInputField.text.Trim();
        if (!string.IsNullOrWhiteSpace(taskTitle) && taskSystem != null)
        {
            taskSystem.AddTask(taskTitle);
            newTaskInputField.text = "";
            newTaskInputField.Select();
            newTaskInputField.ActivateInputField();
            Debug.Log($"[TaskListUI] Add Task requested for title: '{taskTitle}'. Forwarded to TaskSystem.");
        }
        else if (taskSystem == null)
        {
             Debug.LogError("[TaskListUI] Cannot add task - TaskSystem reference is null!");
        }
    }

    private void PopulateTaskList()
    {
        if (taskSystem == null || taskListContent == null || taskItemMinimalPrefab == null)
        {
             Debug.LogError("[TaskListUI] Cannot populate list - TaskSystem, Content Parent, or Prefab is missing.");
             return;
        }

        Debug.Log("[TaskListUI] Clearing and repopulating task list visuals...");

        UnsubscribeFromAllMinimalTasks();
        ClearTaskListVisuals();
        _minimalTaskControllers.Clear();

        IReadOnlyList<TaskData> tasks = taskSystem.Tasks;
        for (int i = 0; i < tasks.Count; i++)
        {
            InstantiateMinimalTaskItem(tasks[i], i);
        }

        Debug.Log($"[TaskListUI] Task list repopulated with {tasks.Count} items.");
        RefreshSelectionState();
    }

    private void InstantiateMinimalTaskItem(TaskData task, int currentIndex)
    {
        if (taskItemMinimalPrefab == null || taskListContent == null) return;

        GameObject itemGO = Instantiate(taskItemMinimalPrefab, taskListContent);
        _minimalTaskItemObjects.Add(itemGO);
        TaskItemMinimal itemController = itemGO.GetComponent<TaskItemMinimal>();

        if (itemController != null)
        {
            itemController.SetupMinimal(currentIndex, task);
            SubscribeToMinimalTaskUI(itemController);
            _minimalTaskControllers.Add(itemController);
        }
        else
        {
            Debug.LogError($"[TaskListUI] Prefab '{taskItemMinimalPrefab.name}' is MISSING the TaskItemMinimal script! Destroying instance.", itemGO);
            Destroy(itemGO);
            _minimalTaskItemObjects.Remove(itemGO);
        }
    }

    private void ClearTaskListVisuals()
    {
        for (int i = _minimalTaskItemObjects.Count - 1; i >= 0; i--)
        {
            if (_minimalTaskItemObjects[i] != null)
            {
                Destroy(_minimalTaskItemObjects[i]);
            }
        }
        _minimalTaskItemObjects.Clear();
    }

    private void SubscribeToMinimalTaskUI(TaskItemMinimal itemUI)
    {
        if (itemUI == null) return;
        itemUI.DeleteRequested -= HandleDeleteRequest;
        itemUI.DeleteRequested += HandleDeleteRequest;
        itemUI.SelectRequested -= HandleSelectRequest;
        itemUI.SelectRequested += HandleSelectRequest;
        itemUI.ToggleCompleteRequested -= HandleToggleCompleteRequest;
        itemUI.ToggleCompleteRequested += HandleToggleCompleteRequest;
    }

    private void UnsubscribeFromMinimalTaskUI(TaskItemMinimal itemUI)
    {
        if (itemUI == null) return;
        itemUI.DeleteRequested -= HandleDeleteRequest;
        itemUI.SelectRequested -= HandleSelectRequest;
        itemUI.ToggleCompleteRequested -= HandleToggleCompleteRequest;
    }

    private void UnsubscribeFromAllMinimalTasks()
    {
        foreach (var controller in _minimalTaskControllers)
        {
            UnsubscribeFromMinimalTaskUI(controller);
        }
    }

    private void SubscribeToActiveTask()
    {
        if (currentActiveTaskController == null) return;
         Debug.Log("[TaskListUI] Subscribing to Active Task Controller events.");
        currentActiveTaskController.StartRequested += HandleStartRequest;
        currentActiveTaskController.PauseRequested += HandlePauseRequest;
        currentActiveTaskController.BreakRequested5m += HandleBreak5mRequest;
        currentActiveTaskController.BreakRequested15m += HandleBreak15mRequest;
        currentActiveTaskController.StopBreakRequested += HandleStopBreakRequest;
        currentActiveTaskController.CompleteRequested += HandleCompleteRequest;
        currentActiveTaskController.ResetRequested += HandleResetRequest;
        currentActiveTaskController.CloseRequested += HandleCloseRequest;
    }

    private void UnsubscribeFromActiveTask()
    {
        if (currentActiveTaskController == null) return;
        Debug.Log("[TaskListUI] Unsubscribing from Active Task Controller events.");
        currentActiveTaskController.StartRequested -= HandleStartRequest;
        currentActiveTaskController.PauseRequested -= HandlePauseRequest;
        currentActiveTaskController.BreakRequested5m -= HandleBreak5mRequest;
        currentActiveTaskController.BreakRequested15m -= HandleBreak15mRequest;
        currentActiveTaskController.StopBreakRequested -= HandleStopBreakRequest;
        currentActiveTaskController.CompleteRequested -= HandleCompleteRequest;
        currentActiveTaskController.ResetRequested -= HandleResetRequest;
        currentActiveTaskController.CloseRequested -= HandleCloseRequest;
    }

    private void HandleTaskListChanged()
    {
        if (!_isInitialized) return;
        Debug.Log("[TaskListUI] Received TaskListChanged event from TaskSystem. Repopulating UI.");
        PopulateTaskList();
    }

    private void HandleTaskTimerTick(int index, TaskState state, float elapsedTime, float breakElapsedTime)
    {
        if (!_isInitialized) return;

        if (activeTaskPanel.activeSelf && index == _currentlySelectedTaskIndex && currentActiveTaskController != null)
        {
            currentActiveTaskController.UpdateTimerAndState(index, state, elapsedTime, breakElapsedTime);
        }
    }

    private void HandleDeleteRequest(int index)
    {
        if (taskSystem != null)
        {
            Debug.Log($"[TaskListUI] Delete requested for index {index}. Forwarding to TaskSystem.");
            taskSystem.RemoveTask(index);
        }
        else { Debug.LogError("[TaskListUI] Cannot handle Delete Request - TaskSystem is null!"); }
    }

    private void HandleSelectRequest(int index)
    {
        if (taskSystem == null || currentActiveTaskController == null) {
             Debug.LogError("[TaskListUI] Cannot handle Select Request - TaskSystem or Active Controller is null!");
             return;
        }
        if (index < 0 || index >= taskSystem.Tasks.Count)
        {
             Debug.LogWarning($"[TaskListUI] Select requested for invalid index: {index}. Task count: {taskSystem.Tasks.Count}");
             if (_currentlySelectedTaskIndex != -1) HandleCloseRequest(_currentlySelectedTaskIndex);
             return;
        }

        Debug.Log($"[TaskListUI] Select requested for index {index}.");

        if (_currentlySelectedTaskIndex != index)
        {
             _currentlySelectedTaskIndex = index;
        }

        RefreshMinimalTaskVisuals();

        TaskData selectedTaskData = taskSystem.Tasks[index];
        currentActiveTaskController.SetupActive(index, selectedTaskData);
        ShowActiveTaskView(true);

        Debug.Log($"[TaskListUI] Task selected at index {index}: '{selectedTaskData.title}'. Active view shown.");
    }

    private void HandleToggleCompleteRequest(int index)
    {
        if (taskSystem != null)
        {
            Debug.Log($"[TaskListUI] Forwarding ToggleComplete request for index {index} to TaskSystem.");
            taskSystem.ToggleTaskCompleted(index);

            if (index == _currentlySelectedTaskIndex && currentActiveTaskController != null && activeTaskPanel.activeSelf)
            {
                 if (index < taskSystem.Tasks.Count)
                 {
                    Debug.Log($"[TaskListUI] Toggled task is active. Refreshing active view for index {index}.");
                    TaskData currentData = taskSystem.Tasks[index];
                    currentActiveTaskController.SetupActive(index, currentData);
                 }
                 else
                 {
                     Debug.LogWarning($"[TaskListUI] Active task index {index} became invalid after toggle, likely removed simultaneously.");
                     HandleCloseRequest(index);
                 }
            }
        }
        else
        {
            Debug.LogError("[TaskListUI] Cannot handle ToggleComplete Request - TaskSystem is null!", this);
        }
    }

    // Corrected: Pass method groups directly without ?. The null check is inside ForwardRequestToTimer.
    private void HandleStartRequest(int index) { ForwardRequestToTimer(taskTimerManager.RequestStartTimer, index, "Start"); }
    private void HandlePauseRequest(int index) { ForwardRequestToTimer(taskTimerManager.RequestPauseTimer, index, "Pause"); }
    private void HandleBreak5mRequest(int index) { ForwardRequestToTimer(taskTimerManager.RequestStartBreak, index, "Break 5m", 300f); }
    private void HandleBreak15mRequest(int index) { ForwardRequestToTimer(taskTimerManager.RequestStartBreak, index, "Break 15m", 900f); }
    private void HandleStopBreakRequest(int index) { ForwardRequestToTimer(taskTimerManager.RequestStopBreakAndResume, index, "Stop Break"); }
    private void HandleCompleteRequest(int index) { ForwardRequestToTimer(taskTimerManager.RequestCompleteTask, index, "Complete Task"); }
    private void HandleResetRequest(int index) { ForwardRequestToTimer(taskTimerManager.RequestResetTimer, index, "Reset"); }

    private void HandleCloseRequest(int index)
    {
        if (index != _currentlySelectedTaskIndex && index != -1)
        {
             Debug.LogWarning($"[TaskListUI] Close requested for index {index} but active index is {_currentlySelectedTaskIndex}. Ignoring.");
             return;
        }

        Debug.Log($"[TaskListUI] Close requested for active task index {_currentlySelectedTaskIndex}. Hiding active view.");
        ShowActiveTaskView(false);

        if (_currentlySelectedTaskIndex != -1 && taskSystem != null && _currentlySelectedTaskIndex < taskSystem.Tasks.Count)
        {
             RefreshMinimalTaskVisuals();
        }

        _currentlySelectedTaskIndex = -1;
    }

    private void ForwardRequestToTimer(Action<int> requestAction, int index, string actionName)
    {
        if (index != _currentlySelectedTaskIndex)
        {
             Debug.LogWarning($"[TaskListUI] {actionName} requested for index {index} but active index is {_currentlySelectedTaskIndex}. Request ignored.");
             return;
        }
        // Check if manager exists AND the passed action is valid (it should be if manager exists)
        if (taskTimerManager != null && requestAction != null)
        {
            Debug.Log($"[TaskListUI] Forwarding '{actionName}' request for index {index} to TaskTimerManager.");
            requestAction.Invoke(index);
        }
        else
        {
            // Log error if manager is null or somehow the action delegate is null
            Debug.LogError($"[TaskListUI] Cannot forward '{actionName}' request: TaskTimerManager is {(taskTimerManager == null ? "null" : "assigned")} and requestAction is {(requestAction == null ? "null" : "valid")}.", this);
        }
    }

    private void ForwardRequestToTimer(Action<int, float> requestAction, int index, string actionName, float param)
    {
        if (index != _currentlySelectedTaskIndex)
        {
             Debug.LogWarning($"[TaskListUI] {actionName} requested for index {index} but active index is {_currentlySelectedTaskIndex}. Request ignored.");
             return;
        }
        // Check if manager exists AND the passed action is valid
        if (taskTimerManager != null && requestAction != null)
        {
            Debug.Log($"[TaskListUI] Forwarding '{actionName}' request with param {param} for index {index} to TaskTimerManager.");
            requestAction.Invoke(index, param);
        }
        else
        {
             Debug.LogError($"[TaskListUI] Cannot forward '{actionName}' request: TaskTimerManager is {(taskTimerManager == null ? "null" : "assigned")} and requestAction is {(requestAction == null ? "null" : "valid")}.", this);
        }
    }

    private void ShowActiveTaskView(bool show)
    {
        if (activeTaskPanel != null)
        {
            if (show && _currentlySelectedTaskIndex == -1)
            {
                Debug.LogWarning("[TaskListUI] Attempted to show active task view, but no task is selected (_currentlySelectedTaskIndex = -1). Keeping hidden.");
                activeTaskPanel.SetActive(false);
                UnsubscribeFromActiveTask();
                return;
            }

            activeTaskPanel.SetActive(show);

            if (show)
            {
                SubscribeToActiveTask();
            }
            else
            {
                UnsubscribeFromActiveTask();
            }
        }
         else { Debug.LogError("[TaskListUI] Cannot show/hide Active Task Panel - reference is null!", this); }
    }

    private void RefreshSelectionState()
    {
        if (taskSystem == null) return;

        bool selectedIndexIsValid = _currentlySelectedTaskIndex != -1 && _currentlySelectedTaskIndex < taskSystem.Tasks.Count;

        if (!selectedIndexIsValid && _currentlySelectedTaskIndex != -1)
        {
             Debug.Log($"[TaskListUI] Previously selected index {_currentlySelectedTaskIndex} is now out of bounds (Task Count: {taskSystem.Tasks.Count}). Closing active view.");
             HandleCloseRequest(_currentlySelectedTaskIndex);
        }
        else if (!activeTaskPanel.activeSelf && _currentlySelectedTaskIndex != -1)
        {
             RefreshMinimalTaskVisuals();
        }
        else
        {
             RefreshMinimalTaskVisuals();
        }
    }

    private void RefreshMinimalTaskVisuals()
    {
        if (taskSystem == null) return;
        Debug.Log($"[TaskListUI] Refreshing minimal task visuals. Current selection index: {_currentlySelectedTaskIndex}");
        IReadOnlyList<TaskData> tasks = taskSystem.Tasks;
        for(int i=0; i < _minimalTaskControllers.Count; i++)
        {
             if (i < tasks.Count && _minimalTaskControllers[i] != null)
             {
                 TaskData currentData = tasks[i];
                 bool isSelected = (i == _currentlySelectedTaskIndex);
                 _minimalTaskControllers[i].UpdateData(currentData);
                 // Corrected: Commented out as SetSelectedHighlight needs implementation in TaskItemMinimal
                 // _minimalTaskControllers[i].SetSelectedHighlight(isSelected); // TODO: Uncomment and implement SetSelectedHighlight(bool selected) in TaskItemMinimal.cs
             }
        }
    }

    // --- Summary Block ---
    // ScriptRole: Manages task UI, displaying list (minimal view) & active task detail. Mediates UI interaction with backend systems.
    // RelatedScripts: TaskSystem, TaskTimerManager, TaskItemMinimal, TaskItemActive, TaskItemUI, TaskData, TaskState
    // UsesPrefabs: TaskItemMinimal prefab.
    // ReceivesFrom: TaskSystem (OnTaskListChanged), TaskTimerManager (OnTaskTimerTick), TaskItemMinimal (DeleteRequested, SelectRequested, ToggleCompleteRequested), TaskItemActive (various action Requests like StartRequested, CloseRequested)
    // SendsTo: TaskSystem (AddTask, RemoveTask, ToggleTaskCompleted), TaskTimerManager (Request... methods), TaskItemMinimal (SetupMinimal, UpdateData), TaskItemActive (SetupActive, UpdateTimerAndState)
}

