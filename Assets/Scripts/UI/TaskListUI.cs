using UnityEngine;
using System.Collections.Generic;
using System.Linq; // Needed for LINQ queries (Where)

public class TaskListUI : MonoBehaviour
{
    [Header("System References")]
    [SerializeField] private TaskManager taskManager;
    [SerializeField] private IconSetSO iconSet; // Needs the icon set to pass to list items

    [Header("UI References")]
    [SerializeField] private GameObject taskListItemPrefab; // Prefab for the TaskListItemUI
    [SerializeField] private Transform listContainer; // Parent transform for instantiated items

    // Keep track of instantiated items to clear them later
    private List<GameObject> instantiatedItems = new List<GameObject>();

    void Start()
    {
        if (!ValidateReferences())
        {
            enabled = false;
            return;
        }

        // Subscribe to the TaskManager's update event
        taskManager.OnTaskListUpdated.AddListener(RefreshTaskList);

        // Initial population of the list
        RefreshTaskList();
    }

    void OnDestroy()
    {
        // Unsubscribe to prevent memory leaks
        if (taskManager != null)
        {
            taskManager.OnTaskListUpdated.RemoveListener(RefreshTaskList);
        }
    }

    private bool ValidateReferences()
    {
        if (taskManager == null) { Debug.LogError($"[{gameObject.name}] Task Manager reference not set!", this); return false; }
        if (iconSet == null) { Debug.LogError($"[{gameObject.name}] Icon Set SO reference not set!", this); return false; }
        if (taskListItemPrefab == null) { Debug.LogError($"[{gameObject.name}] Task List Item Prefab not assigned!", this); return false; }
        if (listContainer == null) { Debug.LogError($"[{gameObject.name}] List Container transform not assigned!", this); return false; }
        return true;
    }

    private void RefreshTaskList()
    {
        // 1. Clear existing items
        foreach (GameObject item in instantiatedItems)
        {
            // Use DestroyImmediate if issues occur in editor, but Destroy is usually fine
            Destroy(item);
        }
        instantiatedItems.Clear();

        // 2. Get tasks from TaskManager
        if (taskManager == null) return;
        var allTasks = taskManager.GetAllTasks();

        // 3. Filter for Pending and Stopped tasks
        var tasksToShow = allTasks.Where(task => task.state == TaskState.Pending || task.state == TaskState.Stopped)
                                   .ToList(); // Convert to list for potential sorting later if needed

        // 4. Instantiate and Setup new items
        foreach (TaskData taskData in tasksToShow)
        {
            GameObject newItemGO = Instantiate(taskListItemPrefab, listContainer);
            TaskListItemUI itemUI = newItemGO.GetComponent<TaskListItemUI>();

            if (itemUI != null)
            {
                // Pass task data, icon set, and methods to call on button clicks
                itemUI.Setup(taskData, iconSet, HandleTaskSelected, HandleTaskDeleteRequested);
                instantiatedItems.Add(newItemGO); // Track for clearing
            }
            else
            {
                Debug.LogError($"Prefab '{taskListItemPrefab.name}' is missing the TaskListItemUI script!", taskListItemPrefab);
                Destroy(newItemGO); // Clean up incorrect prefab instance
            }
        }
    }

    // Called by TaskListItemUI when its 'Select' button is clicked
    private void HandleTaskSelected(string taskId)
    {
        Debug.Log($"TaskListUI: Select requested for task {taskId}");
        taskManager.PrepareTaskForActivation(taskId);
    }

    // Called by TaskListItemUI when its 'Delete' button is clicked
    private void HandleTaskDeleteRequested(string taskId)
    {
        Debug.Log($"TaskListUI: Delete requested for task {taskId}");
        taskManager.RequestDeleteTask(taskId); // TaskManager will handle confirmation logic
    }
}

// --- Summary Block ---
// ScriptRole: Manages the display of pending and stopped tasks (left panel). Listens for updates from TaskManager and instantiates/updates TaskListItemUI prefabs.
// RelatedScripts: TaskManager (gets data, calls methods), TaskListItemUI (instantiates and configures), IconSetSO (provides icons)
// UsesSO: IconSetSO
// ReceivesFrom: TaskManager (OnTaskListUpdated event)
// SendsTo: TaskManager (PrepareTaskForActivation, RequestDeleteTask), TaskListItemUI (Setup call)