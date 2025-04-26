using UnityEngine;
using System.Collections.Generic;
using System.Linq; // Needed for LINQ queries

public class ActiveTasksUI : MonoBehaviour
{
    [Header("System References")]
    [SerializeField] private TaskManager taskManager;
    [SerializeField] private IconSetSO iconSet;
    [SerializeField] private FeedbackMessagesSO feedbackMessages;

    [Header("UI References")]
    [SerializeField] private GameObject activeTaskItemPrefab; // Prefab for ActiveTaskItemUI
    [SerializeField] private Transform listContainer; // Parent transform for instantiated items

    // Dictionary to keep track of instantiated items (Task ID -> UI Component)
    private Dictionary<string, ActiveTaskItemUI> instantiatedItems = new Dictionary<string, ActiveTaskItemUI>();

    // Changed from Start to OnEnable for robust event subscription
    void OnEnable()
    {
        if (!ValidateReferences())
        {
            enabled = false;
            return;
        }

        // Subscribe to TaskManager events
        taskManager.OnTaskListUpdated.AddListener(RefreshActiveTaskList);
        taskManager.OnTaskFeedbackRequested.AddListener(HandleTaskFeedback); // Listen for feedback

        // Initial population
        RefreshActiveTaskList();
    }

    // Changed from OnDestroy to OnDisable
    void OnDisable()
    {
        // Unsubscribe
        if (taskManager != null)
        {
            taskManager.OnTaskListUpdated.RemoveListener(RefreshActiveTaskList);
            taskManager.OnTaskFeedbackRequested.RemoveListener(HandleTaskFeedback);
        }
    }

    private bool ValidateReferences()
    {
        if (taskManager == null) { Debug.LogError($"[{gameObject.name}] Task Manager reference not set!", this); return false; }
        if (iconSet == null) { Debug.LogError($"[{gameObject.name}] Icon Set SO reference not set!", this); return false; }
        if (feedbackMessages == null) { Debug.LogError($"[{gameObject.name}] Feedback Messages SO reference not set!", this); return false; }
        if (activeTaskItemPrefab == null) { Debug.LogError($"[{gameObject.name}] Active Task Item Prefab not assigned!", this); return false; }
        if (listContainer == null) { Debug.LogError($"[{gameObject.name}] List Container transform not assigned!", this); return false; }
        return true;
    }

    private void RefreshActiveTaskList()
    {
        if (taskManager == null) return;
        var allTasks = taskManager.GetAllTasks();

        // 1. Identify tasks that should be visible in the active list
        var activeTaskIds = allTasks
            .Where(task => task.state == TaskState.Active || task.state == TaskState.Paused || task.state == TaskState.Break)
            .Select(task => task.id)
            .ToHashSet(); // Use HashSet for efficient lookup

        // 2. Remove UI items for tasks that are no longer active
        List<string> itemsToRemove = new List<string>();
        foreach (var kvp in instantiatedItems)
        {
            if (!activeTaskIds.Contains(kvp.Key))
            {
                // This task is no longer active, remove its UI element
                Destroy(kvp.Value.gameObject);
                itemsToRemove.Add(kvp.Key);
            }
        }
        foreach (string taskId in itemsToRemove)
        {
            instantiatedItems.Remove(taskId);
        }

        // 3. Add/Update UI items for currently active tasks
        foreach (TaskData taskData in allTasks)
        {
            // Check if this task should be displayed
            if (activeTaskIds.Contains(taskData.id))
            {
                if (instantiatedItems.TryGetValue(taskData.id, out ActiveTaskItemUI existingItemUI))
                {
                    // Task already has a UI item, just update its data
                    existingItemUI.UpdateTaskData(taskData);
                }
                else
                {
                    // New active task, instantiate its UI item
                    GameObject newItemGO = Instantiate(activeTaskItemPrefab, listContainer);
                    ActiveTaskItemUI newItemUI = newItemGO.GetComponent<ActiveTaskItemUI>();

                    if (newItemUI != null)
                    {
                        // Setup the new item with references and initial data
                        newItemUI.Setup(taskData, taskManager, iconSet, feedbackMessages);
                        instantiatedItems.Add(taskData.id, newItemUI); // Add to our tracking dictionary
                    }
                    else
                    {
                         Debug.LogError($"Prefab '{activeTaskItemPrefab.name}' is missing the ActiveTaskItemUI script!", activeTaskItemPrefab);
                         Destroy(newItemGO);
                    }
                }
            }
        }
    }

    // Handles feedback messages targeted at specific tasks
    private void HandleTaskFeedback(string taskId, string messageKey)
    {
        if (instantiatedItems.TryGetValue(taskId, out ActiveTaskItemUI itemUI))
        {
            itemUI.ShowFeedback(messageKey);
        }
        else
        {
             // Feedback for a task not currently displayed in the active list (e.g., max tasks reached on activate attempt)
             // We might need a general feedback area or handle this differently.
             Debug.Log($"Feedback requested for inactive task {taskId}: {messageKey}");
             // Optionally, show this feedback in a general UI area if available
        }
    }
}

// --- Summary Block ---
// ScriptRole: Manages the display of active tasks (right panel). Listens for updates and feedback from TaskManager, instantiates/updates/destroys ActiveTaskItemUI prefabs.
// RelatedScripts: TaskManager (gets data/events), ActiveTaskItemUI (instantiates, controls), IconSetSO, FeedbackMessagesSO
// UsesSO: IconSetSO, FeedbackMessagesSO
// ReceivesFrom: TaskManager (OnTaskListUpdated, OnTaskFeedbackRequested events)
// SendsTo: ActiveTaskItemUI (Setup, UpdateTaskData, ShowFeedback calls) 