using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using Random = UnityEngine.Random;

public class TaskListUI : MonoBehaviour
{
    [Header("System References")]
    [SerializeField] private TaskSystem taskSystem;
    [SerializeField] private TaskTimerManager taskTimerManager;
    [SerializeField] private TaskIconSO taskIconSO;

    [Header("UI Prefabs & Containers")]
    [SerializeField] private GameObject taskItemMinimalPrefab;
    [SerializeField] private GameObject taskItemActivePrefab;
    [SerializeField] private Transform taskListContent;
    [SerializeField] private Transform activeTaskListContent;

    [Header("Add Task UI")]
    [SerializeField] private TMP_InputField addTaskInputField;
    [SerializeField] private Button addTaskButton;

    private TaskItemActive _currentActiveTaskUI = null;
    private int _currentActiveTaskIndex = -1;

    private List<GameObject> _minimalTaskItemObjects = new List<GameObject>();
    private List<TaskItemMinimal> _minimalTaskControllers = new List<TaskItemMinimal>();

    void Start()
    {
        if (!ValidateReferences())
        {
            Debug.LogError("[TaskListUI] Missing one or more essential references in the Inspector! Disabling script.", this);
            enabled = false;
            return;
        }

        if (taskSystem != null) taskSystem.OnTaskListChanged += RefreshUI;
        if (taskTimerManager != null) taskTimerManager.OnTaskTimerTick += HandleTaskTimerTick;
        if (addTaskButton != null) addTaskButton.onClick.AddListener(HandleAddTask);
        RefreshUI();
        Debug.Log("[TaskListUI] Started and Initialized successfully.");
    }

    void OnDestroy()
    {
        if (taskSystem != null) taskSystem.OnTaskListChanged -= RefreshUI;
        if (taskTimerManager != null) taskTimerManager.OnTaskTimerTick -= HandleTaskTimerTick;
        if (addTaskButton != null) addTaskButton.onClick.RemoveListener(HandleAddTask);
        UnsubscribeFromActiveTaskUI();
        UnsubscribeFromMinimalTaskUIs();
        Debug.Log("[TaskListUI] Destroyed, unsubscribed from events.");
    }

    private bool ValidateReferences()
    {
        bool valid = true;
        if (taskSystem == null) { Debug.LogError("[TaskListUI] TaskSystem reference missing!", this); valid = false; }
        if (taskTimerManager == null) { Debug.LogError("[TaskListUI] TaskTimerManager reference missing!", this); valid = false; }
        if (taskIconSO == null) { Debug.LogError("[TaskListUI] TaskIconSO reference missing!", this); valid = false; }
        if (taskItemMinimalPrefab == null) { Debug.LogError("[TaskListUI] TaskItemMinimalPrefab reference missing!", this); valid = false; }
        if (taskItemActivePrefab == null) { Debug.LogError("[TaskListUI] TaskItemActivePrefab reference missing!", this); valid = false; }
        if (taskListContent == null) { Debug.LogError("[TaskListUI] TaskListContent reference missing!", this); valid = false; }
        if (activeTaskListContent == null) { Debug.LogError("[TaskListUI] ActiveTaskListContent reference missing!", this); valid = false; }
        if (addTaskInputField == null) { Debug.LogError("[TaskListUI] AddTaskInputField reference missing!", this); valid = false; }
        if (addTaskButton == null) { Debug.LogError("[TaskListUI] AddTaskButton reference missing!", this); valid = false; }
        return valid;
    }

    private void RefreshUI()
    {
        if (taskSystem == null)
        {
            Debug.LogError("[TaskListUI] Cannot refresh UI, TaskSystem is null.", this);
            return;
        }
        Debug.Log("[TaskListUI] Refreshing UI...");
        ClearMinimalTasksUI();
        ClearActiveTaskUI();
        IReadOnlyList<TaskData> tasks = taskSystem.Tasks;
        Debug.Log($"[TaskListUI] Populating UI with {tasks.Count} tasks.");
        for (int i = 0; i < tasks.Count; i++)
        {
            TaskData task = tasks[i];
            int currentIndex = i;
            if (task.isSelected)
            {
                if (taskItemActivePrefab != null)
                {
                    GameObject activeItemGO = Instantiate(taskItemActivePrefab, activeTaskListContent);
                    TaskItemActive activeUI = activeItemGO.GetComponent<TaskItemActive>();
                    if (activeUI != null)
                    {
                        _currentActiveTaskUI = activeUI;
                        _currentActiveTaskIndex = currentIndex;
                        activeUI.SetupActive(currentIndex, task);
                        SubscribeToActiveTaskUI(activeUI);
                        Debug.Log($"[TaskListUI] Instantiated and Subscribed to Active Task Item for index {currentIndex}: '{task.title}'");
                    }
                    else
                    {
                        Debug.LogError($"[TaskListUI] Prefab '{taskItemActivePrefab.name}' is missing the TaskItemActive script!", activeItemGO);
                        Destroy(activeItemGO);
                    }
                }
                else { Debug.LogError("[TaskListUI] Active Task Prefab is null!", this); }
            }
            else
            {
                if (taskItemMinimalPrefab != null)
                {
                    GameObject itemGO = Instantiate(taskItemMinimalPrefab, taskListContent);
                    _minimalTaskItemObjects.Add(itemGO);
                    TaskItemMinimal itemUI = itemGO.GetComponent<TaskItemMinimal>();
                    if (itemUI != null)
                    {
                        itemUI.SetupMinimal(currentIndex, task);
                        SubscribeToMinimalTaskUI(itemUI);
                        _minimalTaskControllers.Add(itemUI);
                        Debug.Log($"[TaskListUI] Instantiated and Subscribed to Minimal Task Item for index {currentIndex}: '{task.title}'");
                    }
                    else
                    {
                        Debug.LogError($"[TaskListUI] Prefab '{taskItemMinimalPrefab.name}' is missing the TaskItemMinimal script!", itemGO);
                        Destroy(itemGO);
                        _minimalTaskItemObjects.Remove(itemGO);
                    }
                }
                else { Debug.LogError("[TaskListUI] Minimal Task Prefab is null!", this); }
            }
        }
        Debug.Log("[TaskListUI] UI Refresh complete.");
    }

    private void ClearMinimalTasksUI()
    {
        UnsubscribeFromMinimalTaskUIs();
        foreach (GameObject itemGO in _minimalTaskItemObjects)
        {
            if (itemGO != null)
            {
                Destroy(itemGO);
            }
        }
        _minimalTaskItemObjects.Clear();
    }

    private void ClearActiveTaskUI()
    {
        UnsubscribeFromActiveTaskUI();
        GameObject activeGO = activeTaskListContent.childCount > 0 ? activeTaskListContent.GetChild(0).gameObject : null;
        if(activeGO != null)
        {
            Destroy(activeGO);
        }
        _currentActiveTaskUI = null;
        _currentActiveTaskIndex = -1;
    }

    private void SubscribeToActiveTaskUI(TaskItemActive activeUI)
    {
        if (activeUI == null) return;
        activeUI.StartRequested += HandleStartTaskRequest;
        activeUI.PauseRequested += HandlePauseTaskRequest;
        activeUI.BreakRequested += HandleBreakTaskRequest;
        activeUI.ResetRequested += HandleResetTaskRequest;
        activeUI.CloseRequested += HandleCloseActiveTaskRequest;
        activeUI.StopBreakAndResumeRequested += HandleStopBreakAndResumeRequest;
    }

    private void UnsubscribeFromActiveTaskUI()
    {
        if (_currentActiveTaskUI == null) return;
        _currentActiveTaskUI.StartRequested -= HandleStartTaskRequest;
        _currentActiveTaskUI.PauseRequested -= HandlePauseTaskRequest;
        _currentActiveTaskUI.BreakRequested -= HandleBreakTaskRequest;
        _currentActiveTaskUI.ResetRequested -= HandleResetTaskRequest;
        _currentActiveTaskUI.CloseRequested -= HandleCloseActiveTaskRequest;
        _currentActiveTaskUI.StopBreakAndResumeRequested -= HandleStopBreakAndResumeRequest;
        _currentActiveTaskUI = null;
    }

    private void SubscribeToMinimalTaskUI(TaskItemMinimal minimalUI)
    {
        if (minimalUI == null) return;
        minimalUI.DeleteRequested += HandleDeleteTaskRequest;
        minimalUI.SelectRequested += HandleSelectTaskRequest;
        minimalUI.ToggleCompleteRequested += HandleToggleCompleteRequest;
    }

    private void UnsubscribeFromMinimalTaskUIs()
    {
        foreach (var controller in _minimalTaskControllers)
        {
            if (controller != null)
            {
                controller.DeleteRequested -= HandleDeleteTaskRequest;
                controller.SelectRequested -= HandleSelectTaskRequest;
                controller.ToggleCompleteRequested -= HandleToggleCompleteRequest;
            }
        }
        _minimalTaskControllers.Clear();
        Debug.Log("[TaskListUI] Unsubscribed from all minimal task UI events.");
    }

    private void HandleAddTask()
    {
        if (taskSystem == null) { Debug.LogError("[TaskListUI] Cannot add task, TaskSystem is null.", this); return; }
        if (addTaskInputField == null) { Debug.LogError("[TaskListUI] Cannot add task, AddTaskInputField is null.", this); return; }
        string taskTitle = addTaskInputField.text;
        if (!string.IsNullOrWhiteSpace(taskTitle))
        {
            int iconIndex = -1;
            if (taskIconSO != null && taskIconSO.icons != null && taskIconSO.icons.Length > 0)
            {
                iconIndex = Random.Range(0, taskIconSO.icons.Length);
            }
            else { Debug.LogWarning("[TaskListUI] TaskIconSO not assigned or has no icons.", this); }
            taskSystem.AddTask(taskTitle, iconIndex);
            addTaskInputField.text = "";
            Debug.Log($"[TaskListUI] Add Task requested: '{taskTitle}', Icon Index: {iconIndex}");
        }
        else { Debug.LogWarning("[TaskListUI] Add Task button clicked, but input field is empty.", this); }
    }

    private void HandleDeleteTaskRequest(int index)
    {
        Debug.Log($"[TaskListUI] Handling delete request for index: {index} (from event)");
        taskSystem?.RemoveTask(index);
    }

    private void HandleSelectTaskRequest(int index)
    {
        Debug.Log($"[TaskListUI] Handling select request for index: {index} (from event)");
        taskSystem?.SetTaskSelected(index, true);
    }

    private void HandleToggleCompleteRequest(int index)
    {
        Debug.Log($"[TaskListUI] Handling toggle complete request for index: {index} (from event)");
        taskSystem?.ToggleTaskCompleted(index);
    }

    private void HandleStartTaskRequest(int index)
    {
        Debug.Log($"[TaskListUI] Handling start timer request for index: {index} (from event)");
        taskTimerManager?.RequestStartTimer(index);
    }
    private void HandlePauseTaskRequest(int index)
    {
        Debug.Log($"[TaskListUI] Handling pause timer request for index: {index} (from event)");
        taskTimerManager?.RequestPauseTimer(index);
    }
    private void HandleBreakTaskRequest(int index, float durationInSeconds)
    {
        Debug.Log($"[TaskListUI] Handling break request for index: {index} (Duration: {durationInSeconds}s) (from event)");
        taskTimerManager?.RequestStartBreak(index, durationInSeconds);
    }
    private void HandleResetTaskRequest(int index)
    {
        Debug.Log($"[TaskListUI] Handling reset timer request for index: {index} (from event)");
        taskTimerManager?.RequestResetTimer(index);
    }
    private void HandleCloseActiveTaskRequest(int index)
    {
        Debug.Log($"[TaskListUI] Handling close active task request (deselect) for index: {index} (from event)");
        if (_currentActiveTaskUI != null && index == _currentActiveTaskIndex)
        {
             taskSystem?.SetTaskSelected(index, false);
        } else {
             Debug.LogWarning($"[TaskListUI] Close requested for index {index}, but it's not the current active task ({_currentActiveTaskIndex}). TaskSystem state might be ahead of UI refresh.");
             if(taskSystem.Tasks.Count > index && taskSystem.Tasks[index].isSelected)
             {
                taskSystem?.SetTaskSelected(index, false);
             }
        }
    }
    private void HandleStopBreakAndResumeRequest(int index)
    {
        Debug.Log($"[TaskListUI] Handling stop break and resume request for index: {index} (from event)");
        taskTimerManager?.RequestStopBreakAndResume(index);
    }

    private void HandleTaskTimerTick(int taskIndex, TaskState state, float elapsedTime, float breakElapsedTime)
    {
        if (_currentActiveTaskUI != null && taskIndex == _currentActiveTaskIndex)
        {
            _currentActiveTaskUI.UpdateTimerAndState(state, elapsedTime, breakElapsedTime);
        }
    }
        // --- Summary Block ---
    // ScriptRole: Manages task list display, handles add task input. Subscribes to TaskItem UI C# events and routes interactions to TaskSystem/TaskTimerManager. Listens to system updates.
    // RelatedScripts: TaskSystem, TaskTimerManager, TaskItemMinimal, TaskItemActive, TaskData, TaskIconSO
    // UsesSO: TaskIconSO
    // ReceivesFrom: TaskSystem (OnTaskListChanged), TaskTimerManager (OnTaskTimerTick), TaskItemActive (via C# events), TaskItemMinimal (via C# events) // <-- Updated
    // SendsTo: TaskSystem (AddTask, RemoveTask, SetTaskSelected, ToggleTaskCompleted), TaskTimerManager (Request...), TaskItemMinimal/Active (Setup..., UpdateTimerAndState)

}
