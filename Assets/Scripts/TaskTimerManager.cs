using UnityEngine;
using System;
using System.Collections.Generic;

public enum TaskState
{
    Stopped,
    Running,
    Paused,
    OnBreak
}

public class TaskTimerManager : MonoBehaviour
{
    [Header("System References")]
    [SerializeField] private TaskSystem taskSystem;

    public event Action<int, TaskState, float, float> OnTaskTimerTick;

    private bool _isInitialized = false;

    void Start()
    {
        if (taskSystem == null)
        {
            Debug.LogError("[TaskTimerManager] TaskSystem reference not set in the inspector! Disabling script.", this);
            enabled = false;
            return;
        }
        taskSystem.OnTaskListChanged += HandleTaskListChanged;
        _isInitialized = true;
        Debug.Log("[TaskTimerManager] Initialized.");
    }

    void OnDestroy()
    {
        if (taskSystem != null)
        {
            taskSystem.OnTaskListChanged -= HandleTaskListChanged;
        }
        Debug.Log("[TaskTimerManager] Destroyed.");
    }

    void Update()
    {
        if (!_isInitialized || taskSystem == null) return;

        IReadOnlyList<TaskData> tasks = taskSystem.Tasks;
        bool changed = false;

        for (int i = 0; i < tasks.Count; i++)
        {
            TaskData task = tasks[i];
            if (task == null) continue;

            float deltaTime = Time.deltaTime;
            bool taskChangedThisFrame = false;

            switch (task.state)
            {
                case TaskState.Running:
                    task.elapsedTime += deltaTime;
                    taskChangedThisFrame = true;
                    break;

                case TaskState.OnBreak:
                    task.breakElapsedTime += deltaTime;
                    if (task.breakElapsedTime >= task.breakDuration)
                    {
                        task.state = TaskState.Paused;
                        task.breakElapsedTime = 0;
                        Debug.Log($"[TaskTimerManager] Break finished for task {i}. Task paused.");
                        taskChangedThisFrame = true;
                    }
                    else
                    {
                        taskChangedThisFrame = true;
                    }
                    break;

                case TaskState.Stopped:
                case TaskState.Paused:
                    break;
            }

            if (taskChangedThisFrame)
            {
                OnTaskTimerTick?.Invoke(i, task.state, task.elapsedTime, task.breakElapsedTime);
                changed = true;
            }
        }
    }

    private void HandleTaskListChanged()
    {
        if (!_isInitialized) return;
        Debug.Log("[TaskTimerManager] Task list changed, state consistency check (optional).");
    }

    private bool IsValidIndex(int index)
    {
        if (taskSystem == null || index < 0 || index >= taskSystem.Tasks.Count)
        {
            Debug.LogError($"[TaskTimerManager] Invalid task index requested: {index}");
            return false;
        }
        return true;
    }

    public void RequestStartTimer(int index)
    {
        if (!IsValidIndex(index)) return;

        TaskData task = taskSystem.Tasks[index];
        if (task.state == TaskState.Stopped || task.state == TaskState.Paused || task.state == TaskState.OnBreak)
        {
            task.state = TaskState.Running;
            task.breakElapsedTime = 0;
            Debug.Log($"[TaskTimerManager] Starting/Resuming timer for task {index}.");
            OnTaskTimerTick?.Invoke(index, task.state, task.elapsedTime, task.breakElapsedTime);
        }
        else
        {
            Debug.LogWarning($"[TaskTimerManager] RequestStartTimer called for task {index} but it was in state {task.state}.");
        }
    }

    public void RequestPauseTimer(int index)
    {
        if (!IsValidIndex(index)) return;

        TaskData task = taskSystem.Tasks[index];
        if (task.state == TaskState.Running)
        {
            task.state = TaskState.Paused;
            Debug.Log($"[TaskTimerManager] Pausing timer for task {index}.");
            OnTaskTimerTick?.Invoke(index, task.state, task.elapsedTime, task.breakElapsedTime);
        }
        else
        {
            Debug.LogWarning($"[TaskTimerManager] RequestPauseTimer called for task {index} but it was in state {task.state}.");
        }
    }

    public void RequestStartBreak(int index, float durationSeconds)
    {
        if (!IsValidIndex(index)) return;

        TaskData task = taskSystem.Tasks[index];
        if (task.state == TaskState.Running || task.state == TaskState.Paused)
        {
            task.state = TaskState.OnBreak;
            task.breakDuration = durationSeconds;
            task.breakElapsedTime = 0f;
            Debug.Log($"[TaskTimerManager] Starting break for task {index} (Duration: {durationSeconds}s).");
            OnTaskTimerTick?.Invoke(index, task.state, task.elapsedTime, task.breakElapsedTime);
        }
        else
        {
            Debug.LogWarning($"[TaskTimerManager] RequestStartBreak called for task {index} but it was in state {task.state}.");
        }
    }

    public void RequestStopBreakAndResume(int index)
    {
        if (!IsValidIndex(index)) return;

        TaskData task = taskSystem.Tasks[index];
        if (task.state == TaskState.OnBreak)
        {
            task.state = TaskState.Running;
            task.breakElapsedTime = 0;
            Debug.Log($"[TaskTimerManager] Stopping break and resuming task {index}.");
            OnTaskTimerTick?.Invoke(index, task.state, task.elapsedTime, task.breakElapsedTime);
        }
        else
        {
            Debug.LogWarning($"[TaskTimerManager] RequestStopBreakAndResume called for task {index} but it was in state {task.state}.");
        }
    }

    public void RequestResetTimer(int index)
    {
        if (!IsValidIndex(index)) return;

        TaskData task = taskSystem.Tasks[index];
        task.state = TaskState.Stopped;
        task.elapsedTime = 0f;
        task.breakElapsedTime = 0f;
        task.breakDuration = 0f;
        Debug.Log($"[TaskTimerManager] Resetting timer for task {index}.");
        OnTaskTimerTick?.Invoke(index, task.state, task.elapsedTime, task.breakElapsedTime);
    }
}
