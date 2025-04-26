using UnityEngine;
using System;
using System.Collections.Generic;

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

        for (int i = 0; i < tasks.Count; i++)
        {
            TaskData task = tasks[i];
            if (task == null) continue;

            float deltaTime = Time.deltaTime;
            bool taskChangedThisFrame = false;
            TaskState previousState = task.state;
            float previousElapsed = task.elapsedTime;
            float previousBreakElapsed = task.breakElapsedTime;

            switch (task.state)
            {
                case TaskState.Running:
                    task.elapsedTime += deltaTime;
                    taskChangedThisFrame = true;
                    break;

                case TaskState.OnBreak:
                    task.breakElapsedTime += deltaTime;
                    taskChangedThisFrame = true;
                    if (task.breakElapsedTime >= task.breakDuration)
                    {
                        task.state = TaskState.Paused;
                        task.breakElapsedTime = 0;
                        Debug.Log($"[TaskTimerManager] Break finished for task {i} ('{task.title}'). Task automatically paused.");
                    }
                    break;

                case TaskState.Stopped:
                case TaskState.Paused:
                case TaskState.Completed:
                    break;
            }

            if (taskChangedThisFrame || task.state != previousState)
            {
                OnTaskTimerTick?.Invoke(i, task.state, task.elapsedTime, task.breakElapsedTime);
                // Inform TaskSystem about the updated timer state (optional, depends on architecture)
                // taskSystem?.UpdateTaskStateFromManager(i, task.state, task.elapsedTime, task.breakElapsedTime, task.breakDuration);
            }
        }
    }

    private void HandleTaskListChanged()
    {
        if (!_isInitialized) return;
        Debug.Log("[TaskTimerManager] Task list changed notification received.");
    }

    private bool IsValidIndex(int index)
    {
        if (taskSystem == null || taskSystem.Tasks == null || index < 0 || index >= taskSystem.Tasks.Count)
        {
            Debug.LogError($"[TaskTimerManager] Invalid task index requested: {index}");
            return false;
        }
         if (taskSystem.Tasks[index] == null) {
            Debug.LogError($"[TaskTimerManager] Task data at index {index} is null.");
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
            bool wasOnBreak = task.state == TaskState.OnBreak;
            task.state = TaskState.Running;
            if(wasOnBreak) task.breakElapsedTime = 0;

            Debug.Log($"[TaskTimerManager] Starting/Resuming timer for task {index} ('{task.title}').");
            OnTaskTimerTick?.Invoke(index, task.state, task.elapsedTime, task.breakElapsedTime);
            // taskSystem?.UpdateTaskStateFromManager(index, task.state, task.elapsedTime, task.breakElapsedTime, task.breakDuration);
        }
        else
        {
            Debug.LogWarning($"[TaskTimerManager] RequestStartTimer called for task {index} ('{task.title}') but it was already in state {task.state}. No action taken.");
        }
    }

    public void RequestPauseTimer(int index)
    {
        if (!IsValidIndex(index)) return;

        TaskData task = taskSystem.Tasks[index];
        if (task.state == TaskState.Running)
        {
            task.state = TaskState.Paused;
            Debug.Log($"[TaskTimerManager] Pausing timer for task {index} ('{task.title}').");
            OnTaskTimerTick?.Invoke(index, task.state, task.elapsedTime, task.breakElapsedTime);
            // taskSystem?.UpdateTaskStateFromManager(index, task.state, task.elapsedTime, task.breakElapsedTime, task.breakDuration);
        }
        else
        {
            Debug.LogWarning($"[TaskTimerManager] RequestPauseTimer called for task {index} ('{task.title}') but it was in state {task.state}. No action taken.");
        }
    }

    public void RequestStartBreak(int index, float durationSeconds)
    {
        if (!IsValidIndex(index)) return;
        if (durationSeconds <= 0) {
             Debug.LogWarning($"[TaskTimerManager] RequestStartBreak called for task {index} ('{taskSystem.Tasks[index]?.title}') with invalid duration ({durationSeconds}s). Ignoring.", this);
             return;
        }


        TaskData task = taskSystem.Tasks[index];
        if (task.state == TaskState.Running || task.state == TaskState.Paused)
        {
            task.state = TaskState.OnBreak;
            task.breakDuration = durationSeconds;
            task.breakElapsedTime = 0f;
            Debug.Log($"[TaskTimerManager] Starting break for task {index} ('{task.title}') (Duration: {durationSeconds}s).");
            OnTaskTimerTick?.Invoke(index, task.state, task.elapsedTime, task.breakElapsedTime);
            // taskSystem?.UpdateTaskStateFromManager(index, task.state, task.elapsedTime, task.breakElapsedTime, task.breakDuration);
        }
        else
        {
            Debug.LogWarning($"[TaskTimerManager] RequestStartBreak called for task {index} ('{task.title}') but it was in state {task.state}. No action taken.");
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
            Debug.Log($"[TaskTimerManager] Stopping break and resuming task {index} ('{task.title}').");
            OnTaskTimerTick?.Invoke(index, task.state, task.elapsedTime, task.breakElapsedTime);
            // taskSystem?.UpdateTaskStateFromManager(index, task.state, task.elapsedTime, task.breakElapsedTime, task.breakDuration);
        }
        else
        {
            Debug.LogWarning($"[TaskTimerManager] RequestStopBreakAndResume called for task {index} ('{task.title}') but it was in state {task.state}. No action taken.");
        }
    }

    public void RequestResetTimer(int index)
    {
        if (!IsValidIndex(index)) return;

        TaskData task = taskSystem.Tasks[index];
        if (task.state == TaskState.Completed) {
             Debug.LogWarning($"[TaskTimerManager] Resetting timer for task {index} ('{task.title}') which was Completed. TaskSystem's completion status might need separate handling.");
        }

        task.state = TaskState.Stopped;
        task.elapsedTime = 0f;
        task.breakElapsedTime = 0f;
        task.breakDuration = 0f;

        Debug.Log($"[TaskTimerManager] Resetting timer state for task {index} ('{task.title}').");
        OnTaskTimerTick?.Invoke(index, task.state, task.elapsedTime, task.breakElapsedTime);
        // taskSystem?.UpdateTaskStateFromManager(index, task.state, task.elapsedTime, task.breakElapsedTime, task.breakDuration);
        // Optionally, tell TaskSystem to reset its timer data too and save
        // taskSystem?.ResetTaskTimerState(index);
    }

    public void RequestCompleteTask(int index)
    {
        if (!IsValidIndex(index)) return;

        TaskData task = taskSystem.Tasks[index];

        if (task.state != TaskState.Completed)
        {
            TaskState previousState = task.state;
            task.state = TaskState.Completed;

            taskSystem?.SetTaskCompleted(index, true);

            Debug.Log($"[TaskTimerManager] Marking task {index} ('{task.title}') as Completed. Previous state: {previousState}.");

            OnTaskTimerTick?.Invoke(index, task.state, task.elapsedTime, task.breakElapsedTime);
            // taskSystem?.UpdateTaskStateFromManager(index, task.state, task.elapsedTime, task.breakElapsedTime, task.breakDuration);
        }
        else
        {
            Debug.LogWarning($"[TaskTimerManager] RequestCompleteTask called for task {index} ('{task.title}') but it was already in state {task.state}. No action taken.");
        }
    }
}


// --- Summary Block ---
// ScriptRole: Manages the core timing logic (task time, break time) based on TaskData state. Fires tick events and responds to requests to change timer states (Start, Pause, Break, Reset, Complete).
// RelatedScripts: TaskSystem, TaskListUI, TaskData, TaskState
// SendsTo: TaskListUI (via OnTaskTimerTick event)
// ReceivesFrom: TaskListUI (requests via public Request... methods), TaskSystem (via OnTaskListChanged event)

