using UnityEngine;
using System.Collections.Generic;
using System.Linq; // Needed for Where() filtering

public class ActiveTaskPanel : MonoBehaviour
{
    [Header("System References")]
    [SerializeField] private TaskManager taskManager;

    [Header("UI Configuration")]
    [SerializeField] private GameObject taskItemPrefab; // Assign your TaskItemUI prefab here
    [SerializeField] private Transform taskItemContainer; // Assign the parent Transform for instantiated items

    // Keep track of spawned items mapped by Task ID for quick lookup
    private Dictionary<string, TaskItemUI> spawnedItemsMap = new Dictionary<string, TaskItemUI>();

    void Start()
    {
        if (!ValidateReferences())
        {
            enabled = false; // Disable component if references are missing
            return;
        }

        // Subscribe to TaskManager events
        taskManager.OnTaskListUpdated.AddListener(RefreshTaskList);
        taskManager.OnTaskFeedbackRequested.AddListener(HandleTaskFeedback); // Subscribe to feedback

        // Initial population of the list
        RefreshTaskList();
    }

    void OnDestroy()
    {
        // Unsubscribe to prevent errors
        if (taskManager != null)
        {
            taskManager.OnTaskListUpdated.RemoveListener(RefreshTaskList);
            taskManager.OnTaskFeedbackRequested.RemoveListener(HandleTaskFeedback); // Unsubscribe from feedback
        }
    }

    private bool ValidateReferences()
    {
        if (taskManager == null)
        {
            Debug.LogError($"[{gameObject.name}] Task Manager reference not set!", this);
            return false;
        }
        if (taskItemPrefab == null)
        {
            Debug.LogError($"[{gameObject.name}] Task Item Prefab not assigned!", this);
            return false;
        }
        if (taskItemContainer == null)
        {
            Debug.LogError($"[{gameObject.name}] Task Item Container not assigned!", this);
            return false;
        }
         if (taskItemPrefab.GetComponent<TaskItemUI>() == null)
        {
             Debug.LogError($"[{gameObject.name}] Task Item Prefab does not have a TaskItemUI component!", this);
             return false;
        }
        return true;
    }

    private void RefreshTaskList()
    {
        if (!enabled) return;

        // 1. Clear existing items
        foreach (TaskItemUI item in spawnedItemsMap.Values)
        {
            if (item != null)
            {
                Destroy(item.gameObject);
            }
        }
        spawnedItemsMap.Clear();

        // 2. Get relevant tasks
        IReadOnlyList<TaskData> allTasks = taskManager.GetAllTasks();
        var tasksForThisPanel = allTasks.Where(task =>
            task.state == TaskState.Active ||
            task.state == TaskState.Paused ||
            task.state == TaskState.Break
        ).ToList();

        // 3. Instantiate new items
        foreach (TaskData task in tasksForThisPanel)
        {
            GameObject newItemGO = Instantiate(taskItemPrefab, taskItemContainer);
            TaskItemUI newItemUI = newItemGO.GetComponent<TaskItemUI>();

            if (newItemUI != null)
            {
                newItemUI.Initialize(task, taskManager);
                spawnedItemsMap[task.id] = newItemUI; // Add to dictionary using Task ID as key
            }
            else
            {
                Debug.LogError($"[{gameObject.name}] Instantiated prefab is missing TaskItemUI component!", newItemGO);
                Destroy(newItemGO);
            }
        }
         Debug.Log($"[{gameObject.name}] Refreshed. Displaying {spawnedItemsMap.Count} active/paused/break tasks.");
    }

    // --- Feedback Handling ---
    private void HandleTaskFeedback(string taskId, string messageKey)
    {
        // Find the specific TaskItemUI using the taskId from the dictionary
        if (spawnedItemsMap.TryGetValue(taskId, out TaskItemUI targetItemUI))
        {
             if (targetItemUI != null) // Check if the UI object still exists
            {
                targetItemUI.ShowFeedback(messageKey);
            }
            else
            {
                 // Item might have been destroyed, remove dangling key
                 spawnedItemsMap.Remove(taskId);
            }
        }
         // If the taskId is not in the map, the feedback is not for an item in this panel
    }
}

// --- Summary Block ---
// ScriptRole: Manages the UI list displaying tasks in 'Active', 'Paused', or 'Break' state. Instantiates TaskItemUI prefabs and handles feedback events.
// RelatedScripts: TaskManager (provides data and events), TaskItemUI (is instantiated and managed)
// UsesSO: None directly
// ReceivesFrom: TaskManager (OnTaskListUpdated, OnTaskFeedbackRequested events)
// SendsTo: TaskItemUI (calls Initialize/Refresh, ShowFeedback) 