using UnityEngine;
using UnityEngine.Events; // Needed for UnityEvents
using System.Collections.Generic;
using System.Linq; // Needed for Count() with lambda
using System; // Needed for TimeSpan in timer updates/logging (if used)

public class TaskManager : MonoBehaviour
{
    [Header("Task Data")]
    [SerializeField]
    private List<TaskData> allTasks = new List<TaskData>(); // Holds all tasks (Pending, Active, Paused, Stopped, Break)
    // Note: Completed tasks might be moved to a separate history list later

    [Header("Configuration")]
    [SerializeField]
    private int maxActiveTasks = 5; // Limit for tasks in Active or Break state
    [SerializeField]
    private IconSetSO iconSet; // Reference to the ScriptableObject holding icons

    // Runtime state for breaks (Task ID -> Remaining Break Time)
    private Dictionary<string, float> currentBreaks = new Dictionary<string, float>();

    // --- Events ---
    // Called whenever the task list changes (add, remove, state change affecting lists)
    public UnityEvent OnTaskListUpdated;
    // Called when a task needs to display specific feedback (Task ID, Message Key/Text)
    public UnityEvent<string, string> OnTaskFeedbackRequested;
    // Called when a task timer reaches zero (Task ID)
    public UnityEvent<string> OnTaskTimerFinished;
    // Called when a task break timer reaches zero (Task ID)
    public UnityEvent<string> OnTaskBreakFinished;
     // Called when a task is completed and reward should be given (TaskData)
    public UnityEvent<TaskData> OnTaskCompleted;


    // --- Public Access ---
    // Provides read-only access to the tasks for UI display
    public IReadOnlyList<TaskData> GetAllTasks() => allTasks.AsReadOnly();

    // --- Unity Lifecycle ---
    void Update()
    {
        float deltaTime = Time.deltaTime;
        // Use a copy of keys for safe iteration while potentially modifying dictionary
        List<string> tasksInBreak = new List<string>(currentBreaks.Keys);

        // Update Break Timers
        foreach (string taskId in tasksInBreak)
        {
            if (currentBreaks.ContainsKey(taskId))
            {
                TaskData task = FindTaskById(taskId);
                if (task != null && task.state == TaskState.Break)
                {
                    currentBreaks[taskId] -= deltaTime;
                    task.breakTime += deltaTime; // Accumulate total break time

                    if (currentBreaks[taskId] <= 0f)
                    {
                        HandleBreakFinished(task);
                    }
                }
                else
                {
                    // Task not found or not in break state anymore, remove from break tracking
                    currentBreaks.Remove(taskId);
                }
            }
        }


        // Update Active Task Timers
        foreach (TaskData task in allTasks)
        {
            if (task.state == TaskState.Active)
            {
                task.remainingTime -= deltaTime;
                task.elapsedTime += deltaTime; // Accumulate total active time

                if (task.remainingTime <= 0f)
                {
                    HandleTimerFinished(task);
                }
            }
        }
    }

    // --- Core Task Management ---

    public void AddNewTask(string title, int iconIndex, float assignedSeconds)
    {
        TaskData newTask = new TaskData(title, iconIndex, assignedSeconds);
        allTasks.Add(newTask);
        Debug.Log($"Task Added: {title} ({newTask.id})");
        OnTaskListUpdated?.Invoke();
    }

    public void ActivateTask(string taskId)
    {
        TaskData task = FindTaskById(taskId);
        if (task == null)
        {
            Debug.LogError($"ActivateTask: Task with ID {taskId} not found.");
            return;
        }

        if (task.state == TaskState.Active) return; // Already active

        // Check limit BEFORE activating
        int currentActiveCount = allTasks.Count(t => t.state == TaskState.Active || t.state == TaskState.Break);
        if (currentActiveCount >= maxActiveTasks)
        {
            Debug.LogWarning($"Cannot activate task {task.title}. Max active task limit ({maxActiveTasks}) reached.");
            OnTaskFeedbackRequested?.Invoke(taskId, "Feedback_MaxTasksReached"); // Use a key for localization later
            return;
        }

        // If it was in break, stop the break first
        if (task.state == TaskState.Break)
        {
            StopBreakTimer(task.id);
        }

        task.state = TaskState.Active;
        Debug.Log($"Task Activated: {task.title}");
        OnTaskListUpdated?.Invoke();
        OnTaskFeedbackRequested?.Invoke(taskId, "Feedback_TaskStarted");
    }

    public void PauseTask(string taskId)
    {
         TaskData task = FindTaskById(taskId);
        if (task == null)
        {
            Debug.LogError($"PauseTask: Task with ID {taskId} not found.");
            return;
        }

        if (task.state != TaskState.Active) return; // Can only pause active tasks

        task.state = TaskState.Paused;
        Debug.Log($"Task Paused: {task.title}");
        OnTaskListUpdated?.Invoke();
        OnTaskFeedbackRequested?.Invoke(taskId, "Feedback_TaskPaused");
    }

     public void StopTask(string taskId) // "Park" the task
    {
         TaskData task = FindTaskById(taskId);
        if (task == null)
        {
            Debug.LogError($"StopTask: Task with ID {taskId} not found.");
            return;
        }

         // Only stop tasks that are currently active, paused, or in break
         if (task.state == TaskState.Active || task.state == TaskState.Paused || task.state == TaskState.Break)
        {
            // If it was in break, stop the break timer
            if (task.state == TaskState.Break)
            {
                 StopBreakTimer(task.id);
            }

            task.state = TaskState.Stopped;
            Debug.Log($"Task Stopped (Parked): {task.title}");
            OnTaskListUpdated?.Invoke();
            OnTaskFeedbackRequested?.Invoke(taskId, "Feedback_TaskStopped");
        }
         else
        {
             Debug.LogWarning($"Task {task.title} cannot be stopped from state {task.state}.");
        }
    }

    public void StartBreak(string taskId, float breakDurationSeconds)
    {
        TaskData task = FindTaskById(taskId);
        if (task == null)
        {
            Debug.LogError($"StartBreak: Task with ID {taskId} not found.");
            return;
        }

        // Can only start break if Active or Paused, and within limit
        if (task.state == TaskState.Active || task.state == TaskState.Paused)
        {
             // Check limit only if the task wasn't already counting towards it (i.e., was paused)
             if (task.state == TaskState.Paused)
             {
                int currentActiveCount = allTasks.Count(t => t.state == TaskState.Active || t.state == TaskState.Break);
                if (currentActiveCount >= maxActiveTasks)
                {
                    Debug.LogWarning($"Cannot start break for task {task.title}. Max active task limit ({maxActiveTasks}) reached.");
                    OnTaskFeedbackRequested?.Invoke(taskId, "Feedback_MaxTasksReached");
                    return;
                }
             }

            task.state = TaskState.Break;
            currentBreaks[taskId] = breakDurationSeconds; // Start break timer
            Debug.Log($"Break Started for Task: {task.title} ({breakDurationSeconds}s)");
            OnTaskListUpdated?.Invoke();
            OnTaskFeedbackRequested?.Invoke(taskId, "Feedback_BreakStarted");
        }
        else
        {
             Debug.LogWarning($"Task {task.title} cannot start break from state {task.state}.");
        }
    }

     // Manually stopping the break timer (e.g., by pressing Start/Pause again)
    public void InterruptBreakAndResume(string taskId)
    {
        TaskData task = FindTaskById(taskId);
        if (task == null) return;

        if (task.state == TaskState.Break)
        {
            StopBreakTimer(task.id);
            // Immediately activate it again
            ActivateTask(taskId); // ActivateTask handles state change and feedback
             Debug.Log($"Break Interrupted & Resumed for Task: {task.title}");
             // No need to call OnTaskListUpdated here, ActivateTask does it.
        }
    }

     public void MarkTaskAsCompleted(string taskId)
    {
        TaskData task = FindTaskById(taskId);
        if (task == null) return;

        if (task.state != TaskState.Completed)
        {
             // If it was in break, stop the break timer
            if (task.state == TaskState.Break)
            {
                 StopBreakTimer(task.id);
            }

            task.state = TaskState.Completed;
            // Reset remaining time potentially, or leave as is for history? Let's keep it for now.
            task.remainingTime = 0; // Explicitly set to 0 on completion
            Debug.Log($"Task Completed: {task.title}");

            OnTaskCompleted?.Invoke(task); // Notify economy system etc.

            // Remove from active list logic (or handle in UI filtering)
            // For now, just update state and let UI decide filtering
             OnTaskListUpdated?.Invoke();
             OnTaskFeedbackRequested?.Invoke(taskId, "Feedback_TaskCompleted");

            // TODO: Move to a separate history list if needed
        }
    }

     public void ResetTaskTimer(string taskId)
    {
        TaskData task = FindTaskById(taskId);
        if (task == null) return;

        // Can reset from any state except maybe 'Completed' itself?
        // Resetting puts it back to 'Stopped' state in the left panel.
        if (task.state != TaskState.Completed)
        {
            // If it was in break, stop the break timer
            if (task.state == TaskState.Break)
            {
                 StopBreakTimer(task.id);
            }

            task.state = TaskState.Stopped; // Back to parked state
            task.remainingTime = task.assignedTime; // Restore full time
            task.elapsedTime = 0f; // Reset accumulated time
            task.breakTime = 0f;   // Reset accumulated break time
            Debug.Log($"Task Timer Reset: {task.title}");
            OnTaskListUpdated?.Invoke();
            OnTaskFeedbackRequested?.Invoke(taskId, "Feedback_TaskReset");
        }
    }


    // --- Public Accessors ---
    public Sprite GetIconByIndex(int index)
    {
        if (iconSet == null || iconSet.taskIcons == null || index < 0 || index >= iconSet.taskIcons.Count)
        {
            // Debug.LogWarning("Invalid icon index or IconSet not assigned.");
            return null; // Return null or a default sprite
        }
        return iconSet.taskIcons[index];
    }

    // --- Save/Load Integration ---

    public List<TaskData> GetTasksForSaving()
    {
        // Return a copy to prevent external modification? For JSON utility, direct list might be fine.
        // Consider filtering out 'Completed' tasks if they are handled separately in history.
        // For now, save all non-completed tasks.
        return allTasks.Where(t => t.state != TaskState.Completed).ToList();
        // Or simply return allTasks if Completed tasks should also be saved
        // return new List<TaskData>(allTasks);
    }

    public void LoadTasks(List<TaskData> loadedTasks)
    {
        if (loadedTasks == null)
        {
            allTasks = new List<TaskData>();
            Debug.LogWarning("Loaded task list was null. Initializing empty list.");
        }
        else
        {
            allTasks = loadedTasks;
            Debug.Log($"Loaded {allTasks.Count} tasks.");
        }

        // Reset runtime state that isn't saved
        currentBreaks.Clear();

        // It's crucial that the UI is updated after loading.
        // SaveLoadManager should call ForceTaskListUpdateNotification() after this.
    }

    // Used by SaveLoadManager to trigger UI refresh after loading data
    public void ForceTaskListUpdateNotification()
    {
         Debug.Log("ForceTaskListUpdateNotification called.");
         OnTaskListUpdated?.Invoke();
    }


    // --- Private Helpers ---

    private TaskData FindTaskById(string taskId)
    {
        return allTasks.FirstOrDefault(task => task.id == taskId);
    }

     private void HandleTimerFinished(TaskData task)
    {
        // Prevent finishing multiple times if Update runs again before state change
        if (task.state != TaskState.Active) return;

        task.remainingTime = 0f; // Clamp to zero
        task.state = TaskState.Paused; // Temporarily pause to stop timer logic
        Debug.Log($"Task Timer Finished: {task.title}");

        OnTaskTimerFinished?.Invoke(task.id);
        // The UI listening to OnTaskTimerFinished will handle showing options (Reset/Create New)
        // For now, we pause it. The user action will then either Reset or Complete it.
        OnTaskListUpdated?.Invoke(); // Update UI to reflect paused state maybe
    }

     private void HandleBreakFinished(TaskData task)
    {
         // State should be Break, remove from active break tracking
         StopBreakTimer(task.id);

         // Determine state to return to (was it Active or Paused before break?)
         // This simple implementation defaults to Paused. A more robust solution
         // might store the pre-break state. For now, returning to Paused is safe.
         // Edit: Let's try returning to Active, as pausing requires explicit action
         task.state = TaskState.Active; // Or could be TaskState.Paused
         Debug.Log($"Break Finished for Task: {task.title}. Returning to Active.");

         OnTaskBreakFinished?.Invoke(task.id);
         OnTaskListUpdated?.Invoke();
         OnTaskFeedbackRequested?.Invoke(task.id, "Feedback_BreakFinished");
    }

    private void StopBreakTimer(string taskId)
    {
        if (currentBreaks.ContainsKey(taskId))
        {
            currentBreaks.Remove(taskId);
        }
    }

}

// --- Summary Block ---
// ScriptRole: Central manager for creating, updating, and managing the state and timers of all tasks. Handles task limits and break logic. Notifies UI via events.
// RelatedScripts: TaskData, TaskState, IconSetSO, TaskListUI (listens to OnTaskListUpdated), TaskItemUI (calls methods, listens to feedback/finish events), SaveLoadManager (saves/loads allTasks list)
// UsesSO: IconSetSO
// ReceivesFrom: UI elements (TaskItemUI, AddTaskUI) triggering public methods.
// SendsTo: UI elements (via UnityEvents OnTaskListUpdated, OnTaskFeedbackRequested, OnTaskTimerFinished, OnTaskBreakFinished), EconomyManager (via OnTaskCompleted)