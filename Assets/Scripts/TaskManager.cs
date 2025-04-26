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
    // Called when a task needs external confirmation (Task ID, Confirmation Type Key, Action On Confirm)
    public UnityEvent<string, string, UnityAction> OnConfirmationRequired; // Example: (taskId, "Confirm_Reset", () => ActuallyResetTask(taskId))
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
                    task.remainingBreakTime = Mathf.Max(0f, currentBreaks[taskId]);

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
        // New tasks start as Pending, ready for the left list
        TaskData newTask = new TaskData(title, iconIndex, assignedSeconds);
        allTasks.Add(newTask);
        Debug.Log($"Task Added: {title} ({newTask.id}) - State: {newTask.state}");
        OnTaskListUpdated?.Invoke();
    }

    // Called by UI when a Pending/Stopped task is selected from the left list
    public void PrepareTaskForActivation(string taskId)
    {
        TaskData task = FindTaskById(taskId);
        if (task == null) return;

        // Only Pending or Stopped tasks can be prepared
        if (task.state == TaskState.Pending || task.state == TaskState.Stopped)
        {
            // Move to Paused state, ready to appear in the right list
            task.state = TaskState.Paused;
            Debug.Log($"Task Prepared for Activation: {task.title} - State: {TaskState.Paused}");
            OnTaskListUpdated?.Invoke();
            OnTaskFeedbackRequested?.Invoke(taskId, "Feedback_TaskReadyToStart"); // Notify UI
        }
        else
        {
             Debug.LogWarning($"Task {task.title} cannot be prepared from state {task.state}.");
        }
    }

    // --- Active Task Controls (Right List) ---

    public void ActivateTask(string taskId)
    {
        TaskData task = FindTaskById(taskId);
        if (task == null) return;

        // Can only activate if Paused or resuming from Break
        if (task.state != TaskState.Paused && task.state != TaskState.Break)
        {
            Debug.LogWarning($"Cannot activate task {task.title} from state {task.state}.");
            return;
        }

        // Check limit BEFORE activating
        int currentActiveCount = allTasks.Count(t => t.state == TaskState.Active || t.state == TaskState.Break);
        // If the task was in break, it was already counting towards the limit.
        // If it was Paused, check if adding it exceeds the limit.
        if (task.state == TaskState.Paused && currentActiveCount >= maxActiveTasks)
        {
            Debug.LogWarning($"Cannot activate task {task.title}. Max active task limit ({maxActiveTasks}) reached.");
            OnTaskFeedbackRequested?.Invoke(taskId, "Feedback_MaxTasksReached");
            return;
        }

        // If it was in break, stop the break timer
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
        if (task == null) return;

        // Can only pause Active tasks
        if (task.state != TaskState.Active)
        {
            Debug.LogWarning($"Cannot pause task {task.title} from state {task.state}.");
            return;
        }

        task.state = TaskState.Paused;
        Debug.Log($"Task Paused: {task.title}");
        OnTaskListUpdated?.Invoke();
        OnTaskFeedbackRequested?.Invoke(taskId, "Feedback_TaskPaused");
    }

     // Renamed from StopTask - Called by UI when 'X' (Close) is clicked on an active task item
    public void ReturnTaskToPendingList(string taskId)
    {
         TaskData task = FindTaskById(taskId);
         if (task == null) return;

         // Can return tasks that are Active, Paused, or in Break
         if (task.state == TaskState.Active || task.state == TaskState.Paused || task.state == TaskState.Break)
        {
            // If it was in break, stop the break timer
            if (task.state == TaskState.Break)
            {
                 StopBreakTimer(task.id);
            }

            task.state = TaskState.Stopped; // Mark as 'parked' for the left list
            Debug.Log($"Task Returned to Pending List: {task.title} - State: {TaskState.Stopped}");
            OnTaskListUpdated?.Invoke();
            // OnTaskFeedbackRequested?.Invoke(taskId, "Feedback_TaskStopped"); // Optional feedback?
        }
         else
        {
             Debug.LogWarning($"Task {task.title} cannot be returned to pending list from state {task.state}.");
        }
    }

    public void RequestResetTask(string taskId) // Renamed from ResetTaskTimer
    {
        TaskData task = FindTaskById(taskId);
        if (task == null) return;

        // Request confirmation from the UI before proceeding
        OnConfirmationRequired?.Invoke(taskId, "Confirm_Reset", () => ActuallyResetTask(taskId));
    }

    // Internal method called after UI confirmation
    private void ActuallyResetTask(string taskId)
    {
        TaskData task = FindTaskById(taskId);
        if (task == null) return;

        // Reset times
        task.remainingTime = task.assignedTime;
        task.elapsedTime = 0f;
        task.breakTime = 0f;
        task.remainingBreakTime = 0f;

        // Stop break timer if it was running
        if (task.state == TaskState.Break)
        {
            StopBreakTimer(taskId);
        }

        // Set state to Paused, ready in the right list
        task.state = TaskState.Paused;

        Debug.Log($"Task Reset: {task.title} - State: {TaskState.Paused}");
        OnTaskListUpdated?.Invoke();
        OnTaskFeedbackRequested?.Invoke(taskId, "Feedback_TaskReset");
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
            task.remainingBreakTime = breakDurationSeconds;
            Debug.Log($"Break Started for Task: {task.title} ({breakDurationSeconds}s)");
            OnTaskListUpdated?.Invoke();
            OnTaskFeedbackRequested?.Invoke(taskId, "Feedback_BreakStarted");
        }
        else
        {
             Debug.LogWarning($"Task {task.title} cannot start break from state {task.state}.");
        }
    }

     // Called by UI when Start/Pause is pressed WHILE in break
    public void InterruptBreakAndResume(string taskId)
    {
        TaskData task = FindTaskById(taskId);
        if (task == null) return;

        if (task.state == TaskState.Break)
        {
            StopBreakTimer(task.id);
            // Immediately activate it again
            ActivateTask(taskId); // ActivateTask handles state change, limit check, and feedback
             Debug.Log($"Break Interrupted & Resumed for Task: {task.title}");
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

            // Task state is Completed, UI filtering will handle removing it from lists
             OnTaskListUpdated?.Invoke();
             OnTaskFeedbackRequested?.Invoke(taskId, "Feedback_TaskCompleted");

            // TODO: Move to a separate history list if needed
        }
    }

    // --- Deletion --- (Called from Left List UI)

    public void RequestDeleteTask(string taskId)
    {
         TaskData task = FindTaskById(taskId);
         if (task == null) return;

         // Can delete tasks from any state except Completed
         if (task.state != TaskState.Completed)
         {
            // Request confirmation from the UI before proceeding
            OnConfirmationRequired?.Invoke(taskId, "Confirm_Delete", () => ActuallyDeleteTask(taskId));
         }
         else
         {
             Debug.LogWarning($"Task {task.title} cannot be deleted from state {task.state}.");
             // Optionally, provide feedback that it needs to be closed first?
             // OnTaskFeedbackRequested?.Invoke(taskId, "Feedback_CloseTaskBeforeDelete");
         }
    }

     // Internal method called after UI confirmation
    private void ActuallyDeleteTask(string taskId)
    {
        TaskData taskToRemove = FindTaskById(taskId);
        if (taskToRemove != null)
        {
            // Clean up break timer if necessary before deleting
            if (taskToRemove.state == TaskState.Break)
            {
                 StopBreakTimer(taskId);
            }

            allTasks.Remove(taskToRemove);
            Debug.Log($"Task Deleted: {taskToRemove.title}");
            OnTaskListUpdated?.Invoke();
            // Optionally send feedback about deletion?
            // OnTaskFeedbackRequested?.Invoke(taskId, "Feedback_TaskDeleted");
        }
        else
        {
            Debug.LogError($"Failed to delete task {taskId}. Task not found or in invalid state.");
        }
    }


    // --- Utility & Internal Handlers ---

    private TaskData FindTaskById(string taskId)
    {
        return allTasks.FirstOrDefault(task => task.id == taskId);
    }

    // --- Save/Load Integration ---

    // Called by SaveLoadManager to replace the current task list
    public void SetAllTasks(List<TaskData> loadedTasks)
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

            // --- Ensure no task starts as Active after loading ---
            foreach (TaskData task in allTasks)
            {
                if (task.state == TaskState.Active)
                {
                    task.state = TaskState.Paused;
                    Debug.Log($"Task '{task.title}' loaded as Active, setting to Paused.");
                }
            }
            // -----------------------------------------------------
        }

        // Reset runtime state that isn't saved/loaded
        currentBreaks.Clear();

        // Trigger UI update AFTER data is loaded
        // SaveLoadManager should call OnTaskListUpdated.Invoke() after this, or we do it here.
        // Let's do it here for simplicity, assuming SaveLoadManager calls this last.
        OnTaskListUpdated?.Invoke();
        Debug.Log("Task list updated after loading.");
    }

     private void HandleTimerFinished(TaskData task)
    {
        task.remainingTime = 0f;
        task.state = TaskState.Paused; // Go to Paused state when timer finishes
        Debug.Log($"Timer Finished for Task: {task.title}");
        OnTaskTimerFinished?.Invoke(task.id); // Notify UI/Sound
        OnTaskListUpdated?.Invoke();
        OnTaskFeedbackRequested?.Invoke(task.id, "Feedback_TimerFinished"); // Notify timer is done
    }

     private void HandleBreakFinished(TaskData task)
    {
        // Stop the break timer tracking
        StopBreakTimer(task.id);

        // Task timer finished, go back to Paused state
        task.state = TaskState.Paused;
        Debug.Log($"Break Finished for Task: {task.title}");
        OnTaskBreakFinished?.Invoke(task.id); // Notify UI/Sound
        OnTaskListUpdated?.Invoke();
        OnTaskFeedbackRequested?.Invoke(task.id, "Feedback_BreakFinished"); // Notify break is done
    }

    private void StopBreakTimer(string taskId)
    {
        if (currentBreaks.ContainsKey(taskId))
        {
            currentBreaks.Remove(taskId);
            TaskData task = FindTaskById(taskId);
            if (task != null)
            {
                 task.remainingBreakTime = 0f;
            }
        }
    }

}

// --- Summary Block ---
// ScriptRole: Manages the lifecycle and state transitions of all tasks. Handles timers, breaks, and communicates changes via UnityEvents.
// RelatedScripts: TaskData, TaskState, AddTaskPanel (via event), TaskListUI, ActiveTasksUI, ConfirmationPanelUI (via events), EconomyManager (listens for OnTaskCompleted), SaveLoadManager
// UsesSO: IconSetSO (for icons), potentially FeedbackMessagesSO (indirectly via feedback keys)
// ReceivesFrom: AddTaskPanel (OnNewTaskRequested), TaskListUI (PrepareTaskForActivation, RequestDeleteTask), ActiveTasksUI (ActivateTask, PauseTask, ReturnTaskToPendingList, RequestResetTask, StartBreak, InterruptBreakAndResume, MarkTaskAsCompleted), ConfirmationPanelUI (triggers actions like ActuallyResetTask, ActuallyDeleteTask)
// SendsTo: TaskListUI, ActiveTasksUI (via OnTaskListUpdated), ConfirmationPanelUI (via OnConfirmationRequired), Feedback systems (via OnTaskFeedbackRequested, OnTaskTimerFinished, OnTaskBreakFinished), EconomyManager (via OnTaskCompleted)