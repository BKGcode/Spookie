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
                Debug.Log($"[TaskSystem] Pausing task '{task.title}' due to application quit.");
            }
        }
        if (needsSave)
        {
             SaveTasks();
        }
    }

    public void AddTask(string title, int iconIndex = 0)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            Debug.LogWarning("[TaskSystem] AddTask called with empty or whitespace title. Aborting.");
            return;
        }

        if (taskIconSO == null || taskIconSO.icons == null || iconIndex < 0 || iconIndex >= taskIconSO.icons.Length)
        {
             Debug.LogWarning($"[TaskSystem] Invalid icon index ({iconIndex}) or TaskIconSO missing/empty. Defaulting icon index.");
             iconIndex = (taskIconSO != null && taskIconSO.icons != null && taskIconSO.icons.Length > 0) ? 0 : -1;
        }

        TaskData newTask = new TaskData(title, iconIndex);
        _tasks.Add(newTask);
        Debug.Log($"[TaskSystem] Added task '{title}' (Index: {_tasks.Count - 1}) with icon index {iconIndex}.");
        OnTaskListChanged?.Invoke();
        SaveTasks();
    }

    public void RemoveTask(int index)
    {
        if (index >= 0 && index < _tasks.Count)
        {
            Debug.Log($"[TaskSystem] Removing task at index {index}: '{_tasks[index].title}'.");
            _tasks.RemoveAt(index);
            OnTaskListChanged?.Invoke();
            SaveTasks();
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
            Debug.Log($"[TaskSystem] Toggled completed status for task '{_tasks[index].title}' (Index: {index}) to {_tasks[index].isCompleted}.");

            if (_tasks[index].isCompleted && _tasks[index].state != TaskState.Stopped)
            {
            }

            OnTaskListChanged?.Invoke();
            SaveTasks();
        }
        else
        {
            Debug.LogWarning($"[TaskSystem] Attempted to toggle complete at invalid index: {index}");
        }
    }

    public void SetTaskCompleted(int index, bool completed)
    {
        if (index >= 0 && index < _tasks.Count)
        {
            if (_tasks[index].isCompleted == completed) return;

            _tasks[index].isCompleted = completed;
            Debug.Log($"[TaskSystem] Set completed status for task '{_tasks[index].title}' (Index: {index}) to {completed}.");

            if (_tasks[index].isCompleted && _tasks[index].state != TaskState.Stopped)
            {
            }

            OnTaskListChanged?.Invoke();
            SaveTasks();
        }
        else
        {
            Debug.LogWarning($"[TaskSystem] Attempted to set complete status at invalid index: {index}");
        }
    }

    public void UpdateTaskStateFromManager(int index, TaskState newState, float elapsed, float breakElapsed, float breakDuration)
    {
         if (index >= 0 && index < _tasks.Count)
         {
             TaskData task = _tasks[index];
             bool stateChanged = task.state != newState;
             bool timeChanged = !Mathf.Approximately(task.elapsedTime, elapsed) ||
                                !Mathf.Approximately(task.breakElapsedTime, breakElapsed) ||
                                !Mathf.Approximately(task.breakDuration, breakDuration);

             task.state = newState;
             task.elapsedTime = elapsed;
             task.breakElapsedTime = breakElapsed;
             task.breakDuration = breakDuration;

             if(stateChanged || timeChanged)
             {
             }
         }
         else
         {
              Debug.LogWarning($"[TaskSystem] Attempted UpdateTaskStateFromManager at invalid index: {index}");
         }
    }

    public void ResetTaskTimerState(int index, bool triggerSave = true)
    {
        if (index >= 0 && index < _tasks.Count)
        {
             TaskData task = _tasks[index];
             bool changed = task.state != TaskState.Stopped || task.elapsedTime > 0f || task.breakElapsedTime > 0f || task.breakDuration > 0f;

             task.state = TaskState.Stopped;
             task.elapsedTime = 0f;
             task.breakElapsedTime = 0f;
             task.breakDuration = 0f;

             if (changed)
             {
                 Debug.Log($"[TaskSystem] Reset timer state for task '{task.title}' (Index: {index}).");
                 OnTaskListChanged?.Invoke();
                 if (triggerSave) SaveTasks();
             }
        }
         else
        {
             Debug.LogWarning($"[TaskSystem] Attempted ResetTaskTimerState at invalid index: {index}");
        }
    }


    public void SetTaskSelected(int index, bool selected)
    {
        if (index >= 0 && index < _tasks.Count)
        {
             if(_tasks[index].isSelected != selected)
             {
                 _tasks[index].isSelected = selected;
                 Debug.Log($"[TaskSystem] Set selected status for task '{_tasks[index].title}' (Index: {index}) to {selected}.");
                 OnTaskListChanged?.Invoke();
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

                    bool listModified = false;
                    foreach (var task in _tasks)
                    {
                        task.isSelected = false;

                        if (task.state == TaskState.Running || task.state == TaskState.OnBreak)
                        {
                            task.state = TaskState.Paused;
                            listModified = true;
                            Debug.Log($"[TaskSystem] Task '{task.title}' was active on last save, setting state to Paused.");
                        }
                    }
                    if(listModified) SaveTasks();
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
        OnTaskListChanged?.Invoke();
    }
}

[Serializable]
public class TaskSaveData
{
    public List<TaskData> tasks;
}


// --- Summary Block ---
// ScriptRole: Manages the list of TaskData, handles persistence (saving/loading to JSON), and provides methods to add, remove, and modify task properties like completion status.
// RelatedScripts: TaskData, TaskListUI, TaskTimerManager, TaskItemMinimal, TaskState
// UsesSO: TaskIconSO (for validation during AddTask)
// SendsTo: TaskListUI, TaskTimerManager (via OnTaskListChanged event)
// ReceivesFrom: TaskListUI (requests to Add, Remove, ToggleComplete, SetSelected), TaskTimerManager (requests to SetComplete, updates via UpdateTaskStateFromManager)

