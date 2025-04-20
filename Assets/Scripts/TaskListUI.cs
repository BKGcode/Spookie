using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class TaskListUI : MonoBehaviour
{
    [Header("System References")]
    [SerializeField] private TaskSystem taskSystem;
    [SerializeField] private TaskTimerManager taskTimerManager;
    [SerializeField] private TaskIconSO taskIconSO;

    [Header("UI References")]
    [SerializeField] private GameObject taskItemMinimalPrefab;
    [SerializeField] private GameObject taskItemActivePrefab;
    [SerializeField] private Transform taskListContent;
    [SerializeField] private Transform activeTaskListContent;
    [SerializeField] private TMP_InputField addTaskInputField;
    [SerializeField] private Button addTaskButton;

    private List<GameObject> _taskItemObjects = new List<GameObject>();
    private List<GameObject> _activeTaskItemObjects = new List<GameObject>();
    private Dictionary<int, TaskItemActive> _activeTaskUIMap = new Dictionary<int, TaskItemActive>();

    void Start()
    {
        bool referencesValid = taskSystem != null && taskTimerManager != null && taskIconSO != null &&
                               taskItemMinimalPrefab != null && taskItemActivePrefab != null && taskListContent != null &&
                               activeTaskListContent != null && addTaskInputField != null && addTaskButton != null;

        if (!referencesValid)
        {
            Debug.LogError("[TaskListUI] Missing one or more references in the Inspector! Disabling script.", this);
            enabled = false;
            return;
        }

        taskSystem.OnTaskListChanged += RefreshUI;
        taskTimerManager.OnTaskTimerTick += HandleTaskTimerTick;
        addTaskButton.onClick.AddListener(HandleAddTask);

        RefreshUI();
        Debug.Log("[TaskListUI] Started and Initialized.");
    }

    void OnDestroy()
    {
        if (taskSystem != null)
        {
            taskSystem.OnTaskListChanged -= RefreshUI;
        }
        if (taskTimerManager != null)
        {
            taskTimerManager.OnTaskTimerTick -= HandleTaskTimerTick;
        }
        if (addTaskButton != null)
        {
            addTaskButton.onClick.RemoveListener(HandleAddTask);
        }
        Debug.Log("[TaskListUI] Destroyed, unsubscribed from events.");
    }

    private void RefreshUI()
    {
        if (taskSystem == null) return;
        Debug.Log("[TaskListUI] Refreshing UI...");

        foreach (GameObject item in _taskItemObjects) Destroy(item);
        _taskItemObjects.Clear();
        foreach (GameObject item in _activeTaskItemObjects) Destroy(item);
        _activeTaskItemObjects.Clear();
        _activeTaskUIMap.Clear();

        IReadOnlyList<TaskData> tasks = taskSystem.Tasks;
        Debug.Log($"[TaskListUI] Populating UI with {tasks.Count} tasks.");

        for (int i = 0; i < tasks.Count; i++)
        {
            TaskData task = tasks[i];
            int currentIndex = i;

            if (task.isSelected)
            {
                GameObject activeItemGO = Instantiate(taskItemActivePrefab, activeTaskListContent);
                TaskItemActive activeUI = activeItemGO.GetComponent<TaskItemActive>();
                if (activeUI != null)
                {
                    activeUI.SetupActive(
                        currentIndex,
                        task,
                        HandleStartTaskRequest,
                        HandlePauseTaskRequest,
                        HandleBreakTaskRequest,
                        HandleResetTaskRequest,
                        HandleCloseActiveTaskRequest,
                        HandleStopBreakAndResumeRequest
                    );
                    _activeTaskItemObjects.Add(activeItemGO);
                    _activeTaskUIMap[currentIndex] = activeUI;
                }
                else
                {
                    Debug.LogError($"[TaskListUI] Prefab '{taskItemActivePrefab.name}' is missing TaskItemActive script!", activeItemGO);
                    Destroy(activeItemGO);
                }
            }
            else
            {
                GameObject itemGO = Instantiate(taskItemMinimalPrefab, taskListContent);
                TaskItemMinimal itemUI = itemGO.GetComponent<TaskItemMinimal>();
                if (itemUI != null)
                {
                    itemUI.SetupMinimal(
                        currentIndex,
                        task,
                        HandleDeleteTaskRequest,
                        HandleSelectTaskRequest,
                        HandleToggleCompleteRequest
                    );
                    _taskItemObjects.Add(itemGO);
                }
                else
                {
                     Debug.LogError($"[TaskListUI] Prefab '{taskItemMinimalPrefab.name}' is missing TaskItemMinimal script!", itemGO);
                     Destroy(itemGO);
                }
            }
        }
        Debug.Log("[TaskListUI] UI Refresh complete.");
    }


    private void HandleAddTask()
    {
        if (taskSystem != null && addTaskInputField != null && !string.IsNullOrWhiteSpace(addTaskInputField.text))
        {
            int randomIconIndex = -1;

            if (taskIconSO != null && taskIconSO.icons != null && taskIconSO.icons.Length > 0)
            {
                randomIconIndex = Random.Range(0, taskIconSO.icons.Length);
                Debug.Log($"[TaskListUI] Assigning random icon index: {randomIconIndex} for new task '{addTaskInputField.text}'.");
            }
            else
            {
                Debug.LogWarning("[TaskListUI] TaskIconSO not assigned or has no icons. Using default index.");
                randomIconIndex = (taskIconSO != null && taskIconSO.icons != null && taskIconSO.icons.Length > 0) ? 0 : -1;
            }

            taskSystem.AddTask(addTaskInputField.text, randomIconIndex);

            addTaskInputField.text = "";
        }
        else
        {
            if(taskSystem == null) Debug.LogError("[TaskListUI] TaskSystem reference is missing!");
            if(addTaskInputField == null) Debug.LogError("[TaskListUI] AddTaskInputField reference is missing!");
            if(addTaskInputField != null && string.IsNullOrWhiteSpace(addTaskInputField.text)) Debug.LogWarning("[TaskListUI] Task input field is empty.");
        }
    }

    private void HandleDeleteTaskRequest(int index)
    {
        Debug.Log($"[TaskListUI] Handling delete request for index: {index}");
        taskSystem?.RemoveTask(index);
    }

    private void HandleSelectTaskRequest(int index)
    {
        Debug.Log($"[TaskListUI] Handling select request for index: {index}");
        taskSystem?.SetTaskSelected(index, true);
    }

    private void HandleToggleCompleteRequest(int index)
    {
        Debug.Log($"[TaskListUI] Handling toggle complete request for index: {index}");
        taskSystem?.ToggleTaskCompleted(index);
    }

    private void HandleStartTaskRequest(int index)
    {
        Debug.Log($"[TaskListUI] Handling start timer request for index: {index}");
        taskTimerManager?.RequestStartTimer(index);
    }

    private void HandlePauseTaskRequest(int index)
    {
        Debug.Log($"[TaskListUI] Handling pause timer request for index: {index}");
        taskTimerManager?.RequestPauseTimer(index);
    }

    private void HandleBreakTaskRequest(int index, float durationInSeconds)
    {
        Debug.Log($"[TaskListUI] Handling break request for index: {index} (Duration: {durationInSeconds}s)");
        taskTimerManager?.RequestStartBreak(index, durationInSeconds);
    }

    private void HandleResetTaskRequest(int index)
    {
        Debug.Log($"[TaskListUI] Handling reset timer request for index: {index}");
        taskTimerManager?.RequestResetTimer(index);
    }

    private void HandleCloseActiveTaskRequest(int index)
    {
        Debug.Log($"[TaskListUI] Handling close active task request for index: {index}");
        taskSystem?.SetTaskSelected(index, false);
    }

    private void HandleStopBreakAndResumeRequest(int index)
    {
        Debug.Log($"[TaskListUI] Handling stop break and resume request for index: {index}");
        taskTimerManager?.RequestStopBreakAndResume(index);
    }

    private void HandleTaskTimerTick(int taskIndex, TaskState state, float elapsedTime, float breakElapsedTime)
    {
        if (_activeTaskUIMap.TryGetValue(taskIndex, out TaskItemActive activeUI))
        {
            if (activeUI != null)
            {
                activeUI.UpdateState(state, elapsedTime, breakElapsedTime);
            }
            else
            {
                 _activeTaskUIMap.Remove(taskIndex);
                 Debug.LogWarning($"[TaskListUI] Found destroyed TaskItemActive in map for index {taskIndex}. Removing entry.");
            }
        }
    }
}


