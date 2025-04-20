using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class TaskSystem : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] string saveFileName = "tasks.json";
    [SerializeField] TaskIconSO taskIconSO;

    private List<TaskData> _tasks = new List<TaskData>();
    public IReadOnlyList<TaskData> Tasks => _tasks.AsReadOnly();

    public event Action OnTaskListChanged;

    private string _savePath;

    void Awake()
    {
        _savePath = Path.Combine(Application.persistentDataPath, saveFileName);
        LoadTasks();
    }

    void OnApplicationQuit()
    {
        bool needsSave = false;
        foreach (var task in _tasks)
        {
            if (task.state == TaskState.Running || task.state == TaskState.OnBreak)
            {
                task.state = TaskState.Paused;
                needsSave = true;
            }
        }
        if (needsSave)
        {
             // Save implicitly if tasks were paused on quit
             SaveTasks();
        }
        // Alternatively, always save on quit if desired, uncomment below
        // SaveTasks();
    }

    public void AddTask(string title, int iconIndex = 0)
    {
        if (string.IsNullOrWhiteSpace(title)) return;

        if (taskIconSO == null || taskIconSO.icons == null || iconIndex < 0 || iconIndex >= taskIconSO.icons.Length)
        {
             Debug.LogWarning($"[TaskSystem] Invalid icon index ({iconIndex}) or TaskIconSO missing/empty. Defaulting to index 0.");
             iconIndex = (taskIconSO != null && taskIconSO.icons != null && taskIconSO.icons.Length > 0) ? 0 : -1; // Use -1 if no icons exist at all
        }

        TaskData newTask = new TaskData(title, iconIndex);
        _tasks.Add(newTask);
        Debug.Log($"[TaskSystem] Added task '{title}' with icon index {iconIndex}.");
        OnTaskListChanged?.Invoke();
        SaveTasks(); // Save after adding
    }

    public void RemoveTask(int index)
    {
        if (index >= 0 && index < _tasks.Count)
        {
            Debug.Log($"[TaskSystem] Removing task at index {index}: '{_tasks[index].title}'.");
            _tasks.RemoveAt(index);
            OnTaskListChanged?.Invoke();
            SaveTasks(); // Save after removing
        }
         else
        {
            Debug.LogWarning($"[TaskSystem] Attempted to remove task at invalid index: {index}");
        }
    }

    public void ToggleTaskCompleted(int index)
    {
        if (index >= 0 && index < _tasks.Count)
        {
            _tasks[index].isCompleted = !_tasks[index].isCompleted;
            Debug.Log($"[TaskSystem] Toggled completed status for task {index} to {_tasks[index].isCompleted}.");

            if (_tasks[index].isCompleted && _tasks[index].state != TaskState.Stopped)
            {
                 // Option: Reset timer when task is marked complete?
                 // _tasks[index].state = TaskState.Stopped;
                 // _tasks[index].elapsedTime = 0f;
                 // _tasks[index].breakElapsedTime = 0f;
                 // _tasks[index].breakDuration = 0f;
                 // Debug.Log($"[TaskSystem] Also stopping timer for completed task {index}.");
            }
            OnTaskListChanged?.Invoke();
            SaveTasks(); // Save after toggling completion
        }
        else
        {
            Debug.LogWarning($"[TaskSystem] Attempted to toggle complete at invalid index: {index}");
        }
    }

    // Note: Start/Pause/Break/Reset methods are now primarily handled by TaskTimerManager requesting changes.
    // TaskSystem now focuses on managing the list and persistence.
    // The timer-related methods below might be redundant if TaskTimerManager directly modifies TaskData state.
    // However, keeping them allows TaskSystem to potentially enforce rules or trigger saves.

    // Consider if these methods are still needed or if TaskTimerManager should own state changes entirely.
    // If TaskTimerManager modifies the TaskData directly, these might become unnecessary.
    // For now, they are kept but might need adjustment based on final architecture choice.

    public void UpdateTaskStateFromManager(int index, TaskState newState, float elapsed, float breakElapsed)
    {
         if (index >= 0 && index < _tasks.Count)
         {
             TaskData task = _tasks[index];
             bool changed = task.state != newState || !Mathf.Approximately(task.elapsedTime, elapsed) || !Mathf.Approximately(task.breakElapsedTime, breakElapsed);

             task.state = newState;
             task.elapsedTime = elapsed;
             task.breakElapsedTime = breakElapsed; // Ensure break time is also updated if necessary

             if(changed)
             {
                // We don't invoke OnTaskListChanged here typically, as the TimerManager tick likely triggered the UI update.
                // However, we might want to save periodically or based on state changes.
                // SaveTasks(); // Consider saving on significant state changes? Might be too frequent.
             }
         }
    }


    public void SetTaskSelected(int index, bool selected)
    {
        if (index >= 0 && index < _tasks.Count)
        {
             if(_tasks[index].isSelected != selected)
             {
                _tasks[index].isSelected = selected;
                Debug.Log($"[TaskSystem] Set selected status for task {index} to {selected}.");
                OnTaskListChanged?.Invoke();
                // No save needed for selection state as it's transient UI state
             }
        }
         else
        {
            Debug.LogWarning($"[TaskSystem] Attempted to set selected at invalid index: {index}");
        }
    }

    public void SaveTasks()
    {
        TaskSaveData dataToSave = new TaskSaveData { tasks = _tasks };
        string json = JsonUtility.ToJson(dataToSave, true);
        try
        {
            File.WriteAllText(_savePath, json);
            Debug.Log($"[TaskSystem] Tasks saved successfully to '{_savePath}'.");
        }
        catch (Exception e)
        {
            Debug.LogError($"[TaskSystem] Save failed to path '{_savePath}': {e.Message}", this);
        }
    }

    public void LoadTasks()
    {
        if (File.Exists(_savePath))
        {
            try
            {
                string json = File.ReadAllText(_savePath);
                TaskSaveData loadedData = JsonUtility.FromJson<TaskSaveData>(json);

                if (loadedData != null && loadedData.tasks != null)
                {
                     _tasks = loadedData.tasks;
                     Debug.Log($"[TaskSystem] Tasks loaded successfully from '{_savePath}'. {_tasks.Count} tasks found.");

                     // Reset transient states on load
                     foreach (var task in _tasks) {
                         task.isSelected = false; // Selection is not saved/loaded
                         // Ensure tasks that were running or on break are paused on load
                         if (task.state == TaskState.Running || task.state == TaskState.OnBreak)
                         {
                             task.state = TaskState.Paused;
                             Debug.Log($"[TaskSystem] Task '{task.title}' was active on last save, setting state to Paused.");
                         }
                     }
                }
                else
                {
                    Debug.LogWarning($"[TaskSystem] Loaded file '{_savePath}' but data was null or empty. Initializing empty list.");
                    _tasks = new List<TaskData>();
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[TaskSystem] Load failed from path '{_savePath}': {e.Message}. Initializing empty list.", this);
                _tasks = new List<TaskData>();
            }
        }
        else
        {
             Debug.Log($"[TaskSystem] Save file not found at '{_savePath}'. Initializing empty task list.");
            _tasks = new List<TaskData>();
        }
        // Invoke regardless of load success to update UI with loaded or empty list
        OnTaskListChanged?.Invoke();
    }
}

// Helper class for JSON serialization
[Serializable]
public class TaskSaveData
{
    public List<TaskData> tasks;
}

