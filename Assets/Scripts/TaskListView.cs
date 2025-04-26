using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using System;

public class TaskListView : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Transform taskListContent;
    [SerializeField] private GameObject taskItemMinimalPrefab;

    [Header("Events")]
    public UnityEvent<int> OnTaskSelected;
    public UnityEvent<int> OnTaskDeleteRequested;
    public UnityEvent<int> OnTaskToggleCompleteRequested;

    private List<GameObject> _minimalTaskItemObjects = new List<GameObject>();
    private List<TaskItemMinimal> _minimalTaskControllers = new List<TaskItemMinimal>();
    private int _lastKnownSelectedTaskIndex = -1;

    void Start()
    {
        if (!ValidateReferences())
        {
             Debug.LogError($"[{nameof(TaskListView)}] Critical references missing during Start. Check Inspector assignments.", this);
        }
         else
        {
             Debug.Log($"[{nameof(TaskListView)}] Initialized successfully.", this);
        }
    }

    void OnDestroy()
    {
        UnsubscribeFromAllMinimalTasks();
        ClearTaskListVisuals();
        Debug.Log($"[{nameof(TaskListView)}] Destroyed. Listeners cleared, visuals destroyed.", this);
    }

    private bool ValidateReferences()
    {
        bool isValid = true;
        if (taskListContent == null) { Debug.LogError($"[{nameof(TaskListView)}] Task List Content reference is missing!", this); isValid = false; }
        if (taskItemMinimalPrefab == null) { Debug.LogError($"[{nameof(TaskListView)}] Task Item Minimal Prefab reference is missing!", this); isValid = false; }
        else if (taskItemMinimalPrefab.GetComponent<TaskItemMinimal>() == null)
        {
             Debug.LogError($"[{nameof(TaskListView)}] Minimal Task Prefab '{taskItemMinimalPrefab.name}' is MISSING the required TaskItemMinimal script!", taskItemMinimalPrefab);
             isValid = false;
        }
        return isValid;
    }

    public void PopulateTaskList(IReadOnlyList<TaskData> tasks)
    {
        if (!ValidateReferences())
        {
            Debug.LogError($"[{nameof(TaskListView)}] Cannot populate list due to missing references.", this);
            return;
        }

        Debug.Log($"[{nameof(TaskListView)}] Clearing and repopulating task list visuals with {tasks.Count} items.", this);

        UnsubscribeFromAllMinimalTasks();
        ClearTaskListVisuals();
        _minimalTaskControllers.Clear();

        for (int i = 0; i < tasks.Count; i++)
        {
            InstantiateMinimalTaskItem(tasks[i], i);
        }

        RefreshMinimalTaskVisuals(_lastKnownSelectedTaskIndex, tasks);
        Debug.Log($"[{nameof(TaskListView)}] Task list repopulated.", this);
    }

    public void SetSelectedTaskIndex(int newSelectedIndex, IReadOnlyList<TaskData> currentTasks)
    {
        _lastKnownSelectedTaskIndex = newSelectedIndex;
        RefreshMinimalTaskVisuals(newSelectedIndex, currentTasks);
         Debug.Log($"[{nameof(TaskListView)}] Selection index set to {newSelectedIndex}. Visuals refreshed.", this);
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
            Debug.LogError($"[{nameof(TaskListView)}] Prefab '{taskItemMinimalPrefab.name}' is MISSING the TaskItemMinimal script! Destroying instance.", itemGO);
            Destroy(itemGO);
            _minimalTaskItemObjects.Remove(itemGO);
        }
    }

    private void ClearTaskListVisuals()
    {
        Debug.Log($"[{nameof(TaskListView)}] Clearing {_minimalTaskItemObjects.Count} visual task items.", this);
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
        itemUI.DeleteRequested -= HandleDeleteRequestInternal;
        itemUI.DeleteRequested += HandleDeleteRequestInternal;

        itemUI.SelectRequested -= HandleSelectRequestInternal;
        itemUI.SelectRequested += HandleSelectRequestInternal;

        itemUI.ToggleCompleteRequested -= HandleToggleCompleteRequestInternal;
        itemUI.ToggleCompleteRequested += HandleToggleCompleteRequestInternal;
    }

    private void UnsubscribeFromMinimalTaskUI(TaskItemMinimal itemUI)
    {
        if (itemUI == null) return;
        itemUI.DeleteRequested -= HandleDeleteRequestInternal;
        itemUI.SelectRequested -= HandleSelectRequestInternal;
        itemUI.ToggleCompleteRequested -= HandleToggleCompleteRequestInternal;
    }

    private void UnsubscribeFromAllMinimalTasks()
    {
         Debug.Log($"[{nameof(TaskListView)}] Unsubscribing from events of {_minimalTaskControllers.Count} minimal task controllers.", this);
        foreach (var controller in _minimalTaskControllers)
        {
            if (controller != null)
            {
                UnsubscribeFromMinimalTaskUI(controller);
            }
        }
    }

    private void HandleDeleteRequestInternal(int index)
    {
        Debug.Log($"[{nameof(TaskListView)}] Delete request received internally for index {index}. Invoking public event.", this);
        OnTaskDeleteRequested?.Invoke(index);
    }

    private void HandleSelectRequestInternal(int index)
    {
         Debug.Log($"[{nameof(TaskListView)}] Select request received internally for index {index}. Invoking public event.", this);
         OnTaskSelected?.Invoke(index);
    }

    private void HandleToggleCompleteRequestInternal(int index)
    {
        Debug.Log($"[{nameof(TaskListView)}] Toggle Complete request received internally for index {index}. Invoking public event.", this);
        OnTaskToggleCompleteRequested?.Invoke(index);
    }

    private void RefreshMinimalTaskVisuals(int currentlySelectedIndex, IReadOnlyList<TaskData> tasks)
    {
        if (_minimalTaskControllers.Count != _minimalTaskItemObjects.Count)
        {
            Debug.LogWarning($"[{nameof(TaskListView)}] Mismatch between controller count ({_minimalTaskControllers.Count}) and GameObject count ({_minimalTaskItemObjects.Count}). Repopulation might be needed or an error occurred.", this);
        }

        Debug.Log($"[{nameof(TaskListView)}] Refreshing minimal task visuals. Item Count: {_minimalTaskControllers.Count}, Selected Index: {currentlySelectedIndex}", this);

        for (int i = 0; i < _minimalTaskControllers.Count; i++)
        {
            if (i < tasks.Count && _minimalTaskControllers[i] != null)
            {
                TaskData currentData = tasks[i];
                bool isSelected = (i == currentlySelectedIndex);

                _minimalTaskControllers[i].UpdateData(currentData);
                _minimalTaskControllers[i].SetSelectedHighlight(isSelected);
            }
            else if (_minimalTaskControllers[i] == null)
            {
                 Debug.LogWarning($"[{nameof(TaskListView)}] Found a null controller at index {i} during visual refresh. It might have been destroyed prematurely.", this);
            }
        }
    }
}

    // --- Summary Block ---
    // ScriptRole: Displays and manages the list of minimal task items. Emits events for user interactions on items.
    // RelatedScripts: TaskUIManager, TaskItemMinimal, TaskData
    // UsesSO: None
    // UsesPrefabs: TaskItemMinimal prefab.
    // ReceivesFrom: TaskUIManager (PopulateTaskList, SetSelectedTaskIndex)
    // SendsTo: TaskUIManager (via OnTaskSelected, OnTaskDeleteRequested, OnTaskToggleCompleteRequested events)