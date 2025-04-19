// FILE: TaskSystem.cs
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// Manages tasks: data, saving/loading, state changes (completion, timer, selection).
/// </summary>
public class TaskSystem : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] string saveFileName = "tasks.json";
    [SerializeField] TaskIconSO taskIconSO; // Assign the created TaskIconSO asset

    private List<TaskData> _tasks = new List<TaskData>();
    public IReadOnlyList<TaskData> Tasks => _tasks.AsReadOnly();
    public event Action OnTaskListChanged;
    private string _savePath;

    void Awake()
    {
        _savePath = Path.Combine(Application.persistentDataPath, saveFileName);
        LoadTasks();
    }

    public void AddTask(string title, int iconIndex = 0)
    {
        if (string.IsNullOrWhiteSpace(title)) return;
        if (taskIconSO == null || iconIndex < 0 || iconIndex >= taskIconSO.icons.Length)
        {
             Debug.LogWarning($"[TaskSystem] Invalid icon index {iconIndex}. Defaulting to 0.");
            iconIndex = 0;
        }

        TaskData newTask = new TaskData(title, iconIndex);
        _tasks.Add(newTask);
         Debug.Log($"[TaskSystem] Task added: '{title}'");
        SaveTasks();
        OnTaskListChanged?.Invoke();
    }

    public void RemoveTask(int index)
    {
        if (index >= 0 && index < _tasks.Count)
        {
             Debug.Log($"[TaskSystem] Task removed: '{_tasks[index].title}'");
            _tasks.RemoveAt(index);
            SaveTasks();
            OnTaskListChanged?.Invoke();
        }
    }

    public void ToggleTaskCompleted(int index)
    {
        if (index >= 0 && index < _tasks.Count)
        {
            _tasks[index].isCompleted = !_tasks[index].isCompleted;
            SaveTasks();
            OnTaskListChanged?.Invoke();
        }
    }

    public void SetTaskTimerRunning(int index, bool isRunning)
    {
        if (index >= 0 && index < _tasks.Count && _tasks[index].isTimerRunning != isRunning)
        {
            _tasks[index].isTimerRunning = isRunning;
            SaveTasks();
            OnTaskListChanged?.Invoke();
        }
    }

    public void ResetTaskElapsedTime(int index)
    {
        if (index >= 0 && index < _tasks.Count && _tasks[index].elapsedTime != 0f)
        {
            _tasks[index].elapsedTime = 0f;
            SaveTasks();
            OnTaskListChanged?.Invoke();
        }
    }

    /// <summary>Sets task selection state (for active list).</summary>
    public void SetTaskSelected(int index, bool selected)
    {
        if (index >= 0 && index < _tasks.Count && _tasks[index].isSelected != selected)
        {
            _tasks[index].isSelected = selected;
             Debug.Log($"[TaskSystem] Task '{_tasks[index].title}' selection set to: {selected}");

            // Pause timer automatically when deselecting
            if (!selected && _tasks[index].isTimerRunning)
            {
                 SetTaskTimerRunning(index, false);
            }

            SaveTasks();
            OnTaskListChanged?.Invoke();
        }
    }

    public void SaveTasks()
    {
        TaskSaveData dataToSave = new TaskSaveData { tasks = _tasks };
        string json = JsonUtility.ToJson(dataToSave, true);
        try
        {
            File.WriteAllText(_savePath, json);
             Debug.Log($"[TaskSystem] Tasks saved to {_savePath}");
        }
        catch (Exception e) { Debug.LogError($"[TaskSystem] Save failed: {e.Message}"); }
    }

    public void LoadTasks()
    {
        if (File.Exists(_savePath))
        {
            try
            {
                string json = File.ReadAllText(_savePath);
                TaskSaveData loadedData = JsonUtility.FromJson<TaskSaveData>(json);
                _tasks = (loadedData != null && loadedData.tasks != null) ? loadedData.tasks : new List<TaskData>();
                 Debug.Log($"[TaskSystem] Tasks loaded from {_savePath}. Count: {_tasks.Count}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[TaskSystem] Load failed: {e.Message}");
                _tasks = new List<TaskData>();
            }
        }
        else { _tasks = new List<TaskData>(); Debug.Log($"[TaskSystem] No save file found."); }

        OnTaskListChanged?.Invoke(); // Notify UI after load
    }
}


