// FILE: TaskListUI.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System; // Needed for Action<int>

/// <summary>
/// Manages UI for both task lists (management and active). Handles interactions.
/// </summary>
public class TaskListUI : MonoBehaviour
{
    [Header("System References")]
    [SerializeField] TaskSystem taskSystem;
    [SerializeField] TaskTimerManager taskTimerManager;
    // [SerializeField] TaskIconSO taskIconSO; // Reference kept if needed for icon selection UI later

    [Header("Left List (Management)")]
    [SerializeField] Transform leftListContentParent;
    [SerializeField] GameObject taskItemMinimalPrefab; // Requires TaskItemMinimal script
    [SerializeField] TMP_InputField taskInputField;
    [SerializeField] Button addTaskButton;

    [Header("Right List (Active Timers)")]
    [SerializeField] Transform rightListContentParent;
    [SerializeField] GameObject taskItemActivePrefab; // Requires TaskItemActive script (prev. TaskItemUI)

    void Start()
    {
        // Basic validation
        if (!taskSystem || !taskTimerManager || !leftListContentParent || !rightListContentParent || !taskItemMinimalPrefab || !taskItemActivePrefab || !taskInputField || !addTaskButton)
        {
            Debug.LogError("[TaskListUI] Missing references in Inspector!");
            enabled = false;
            return;
        }

        taskSystem.OnTaskListChanged += RefreshUI;
        addTaskButton.onClick.AddListener(HandleAddTask);
        RefreshUI(); // Initial population
    }

    void OnDestroy()
    {
        if (taskSystem != null) taskSystem.OnTaskListChanged -= RefreshUI;
    }

    private void RefreshUI()
    {
        // Clear lists
        foreach (Transform child in leftListContentParent) Destroy(child.gameObject);
        foreach (Transform child in rightListContentParent) Destroy(child.gameObject);

        IReadOnlyList<TaskData> tasks = taskSystem.Tasks;

        for (int i = 0; i < tasks.Count; i++)
        {
            TaskData taskData = tasks[i];
            int currentIndex = i; // Capture for lambdas/delegates

            // --- Left List Item ---
            GameObject minGO = Instantiate(taskItemMinimalPrefab, leftListContentParent);
            var minUI = minGO.GetComponent<TaskItemMinimal>(); // Script must exist
            if (minUI != null)
            {
                 minUI.SetupMinimal(currentIndex, taskData, HandleDeleteTask, HandleToggleSelectTask);
                 minUI.SetSelectedVisual(taskData.isSelected); // Update visual based on selection
            }
             else Debug.LogError($"[TaskListUI] Prefab '{taskItemMinimalPrefab.name}' missing TaskItemMinimal script!");


            // --- Right List Item (if selected) ---
            if (taskData.isSelected)
            {
                GameObject actGO = Instantiate(taskItemActivePrefab, rightListContentParent);
                var actUI = actGO.GetComponent<TaskItemActive>(); // Script must exist (maybe rename TaskItemUI)
                if (actUI != null)
                {
                    actUI.SetupActive(currentIndex, taskData, HandleToggleTimer, HandleResetTimer, HandleCloseActiveTask);
                }
                else Debug.LogError($"[TaskListUI] Prefab '{taskItemActivePrefab.name}' missing TaskItemActive script!");
            }
        }
         Debug.Log($"[TaskListUI] UI Refreshed.");
    }

    private void HandleAddTask()
    {
        string title = taskInputField.text;
        if (!string.IsNullOrWhiteSpace(title))
        {
            taskSystem.AddTask(title); // Using default icon index 0
            taskInputField.text = "";
        }
        // RefreshUI called by event
    }

    // --- Callbacks Passed to Task Items ---

    private void HandleDeleteTask(int index) => taskSystem.RemoveTask(index);
    private void HandleToggleSelectTask(int index) => taskSystem.SetTaskSelected(index, !taskSystem.Tasks[index].isSelected);
    private void HandleCloseActiveTask(int index) => taskSystem.SetTaskSelected(index, false); // Deselect
    private void HandleToggleTimer(int index) => taskTimerManager.ToggleTaskTimer(index);
    private void HandleResetTimer(int index) => taskTimerManager.ResetTaskTimer(index);
}

// --- REQUIRED HELPER SCRIPTS (ensure these exist) ---

// Needs: TaskItemMinimal.cs (We will create this next)
// Needs: TaskItemActive.cs (We will modify/rename TaskItemUI for this later)


