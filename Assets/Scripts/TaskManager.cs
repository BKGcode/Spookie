using UnityEngine;
using UnityEngine.Events; // Needed for UnityEvents
using System.Collections.Generic;
using System.Linq; // Needed for Count() with lambda and OrderByDescending
using System; // Needed for TimeSpan, DateTime, Guid

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

    // Runtime state for breaks (No longer needed if TaskData.remainingBreakTime is reliable)
    // private Dictionary<string, float> currentBreaks = new Dictionary<string, float>();

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

    // New method to get completed tasks for history
    public List<TaskData> GetCompletedTasks(int maxCount)
    {
        return allTasks
            .Where(t => t.isCompleted)
            .OrderByDescending(t => t.completionTime)
            .Take(maxCount)
            .ToList();
    }

    // --- Unity Lifecycle ---
    void Update()
    {
        float deltaTime = Time.deltaTime;

        // Update Active Task Timers & Elapsed Time
        foreach (TaskData task in allTasks)
        {
            if (task.state == TaskState.Active)
            {
                task.remainingTime = Mathf.Max(0f, task.remainingTime - deltaTime);
                task.elapsedTime += deltaTime; // Accumulate total active time

                if (task.remainingTime <= 0f)
                {
                    HandleTimerFinished(task);
                }
            }
            // Update Break Timers & Break Time
            else if (task.state == TaskState.Break)
            {
                 task.remainingBreakTime = Mathf.Max(0f, task.remainingBreakTime - deltaTime);
                 task.breakTime += deltaTime; // Accumulate total break time

                 if (task.remainingBreakTime <= 0f)
                 {
                     HandleBreakFinished(task);
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

    public void PrepareTaskForActivation(string taskId)
    {
        TaskData task = FindTaskById(taskId);
        if (task == null || !(task.state == TaskState.Pending || task.state == TaskState.Stopped)) return;

        task.state = TaskState.Paused;
        Debug.Log($"Task Prepared for Activation: {task.title} - State: {TaskState.Paused}");
        OnTaskListUpdated?.Invoke();
        OnTaskFeedbackRequested?.Invoke(taskId, "Feedback_TaskReadyToStart");
    }

    // --- Active Task Controls (Right List) ---

    public void ActivateTask(string taskId)
    {
        TaskData task = FindTaskById(taskId);
        if (task == null) return;

        if (task.state != TaskState.Paused && task.state != TaskState.Break)
        {
            Debug.LogWarning($"Cannot activate task {task.title} from state {task.state}.");
            return;
        }

        int currentActiveCount = allTasks.Count(t => t.state == TaskState.Active || t.state == TaskState.Break);
        if (task.state == TaskState.Paused && currentActiveCount >= maxActiveTasks)
        {
            Debug.LogWarning($"Cannot activate task {task.title}. Max active task limit ({maxActiveTasks}) reached.");
            OnTaskFeedbackRequested?.Invoke(taskId, "Feedback_MaxTasksReached");
            return;
        }

        // Record first start time if it hasn't been started before
        if (task.firstStartTime == default)
        {
            task.firstStartTime = DateTime.UtcNow;
            Debug.Log($"Task '{task.title}' first started at: {task.firstStartTime}");
        }

        // If resuming from break, ensure remainingBreakTime is cleared
        if (task.state == TaskState.Break)
        {
            task.remainingBreakTime = 0f; // Clear remaining break time
        }

        task.state = TaskState.Active;
        Debug.Log($"Task Activated: {task.title}");
        OnTaskListUpdated?.Invoke();
        OnTaskFeedbackRequested?.Invoke(taskId, "Feedback_TaskStarted");
    }

    public void PauseTask(string taskId)
    {
         TaskData task = FindTaskById(taskId);
        if (task == null || task.state != TaskState.Active) return;

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

         if (task.state == TaskState.Active || task.state == TaskState.Paused || task.state == TaskState.Break)
        {
            // If it was in break, clear remaining time
            if (task.state == TaskState.Break)
            {
                 task.remainingBreakTime = 0f;
            }
            // If it was active, pause it first (conceptually) before stopping
            // This ensures the elapsedTime isn't running during the transition

            task.state = TaskState.Stopped; // Mark as 'parked' for the left list
            Debug.Log($"Task Returned to Pending List: {task.title} - State: {TaskState.Stopped}");
            OnTaskListUpdated?.Invoke();
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

        // Ask for confirmation before resetting progress
        OnConfirmationRequired?.Invoke(
            taskId,
            "Confirm_Reset", // Key for feedback message
            () => ActuallyResetTask(taskId) // Action to perform on confirmation
        );
    }

    private void ActuallyResetTask(string taskId)
    {
        TaskData task = FindTaskById(taskId);
        if (task == null) return;

        Debug.Log($"Resetting Task: {task.title}");

        // Reset timers and accumulators
        task.remainingTime = task.assignedTime;
        task.elapsedTime = 0f;
        task.breakTime = 0f;
        task.remainingBreakTime = 0f;
        task.firstStartTime = default; // Reset first start time

        // Reset history fields
        task.isCompleted = false;
        task.completionTime = default;
        task.completionDurationSeconds = 0f;
        task.totalDurationSeconds = 0f;

        // If the task was Active or in Break, set it back to Paused
        if (task.state == TaskState.Active || task.state == TaskState.Break)
        {
            task.state = TaskState.Paused;
        }
        // If it was Completed or Stopped, it might remain that way or go to Pending?
        // Let's put it back to Paused if it was active/break/stopped/completed, ready for the right panel.
        // If it was Pending, it stays Pending.
        if (task.state == TaskState.Stopped || task.state == TaskState.Completed)
        {
             task.state = TaskState.Paused; // Or perhaps Pending? Let's go with Paused for consistency.
        }


        OnTaskListUpdated?.Invoke();
        OnTaskFeedbackRequested?.Invoke(taskId, "Feedback_TaskReset");
    }

    public void StartBreak(string taskId, float breakDurationSeconds)
    {
        TaskData task = FindTaskById(taskId);
        if (task == null || !(task.state == TaskState.Active || task.state == TaskState.Paused))
        {
             Debug.LogWarning($"Cannot start break for task {taskId} in state {task?.state}.");
            return;
        }

         // Check limit - Break tasks count towards the active limit
        int currentActiveCount = allTasks.Count(t => t.state == TaskState.Active || t.state == TaskState.Break);
        // If the task was already Active, starting a break doesn't change the count.
        // If it was Paused, check if activating it (as Break) exceeds the limit.
        if (task.state == TaskState.Paused && currentActiveCount >= maxActiveTasks)
        {
            Debug.LogWarning($"Cannot start break for task {task.title}. Max active task limit ({maxActiveTasks}) reached.");
            OnTaskFeedbackRequested?.Invoke(taskId, "Feedback_MaxTasksReached");
            return;
        }

        task.state = TaskState.Break;
        task.remainingBreakTime = breakDurationSeconds;
        // task.initialBreakDurationSeconds = breakDurationSeconds; // Store if needed for display reset

        Debug.Log($"Task Break Started: {task.title} for {breakDurationSeconds}s");
        OnTaskListUpdated?.Invoke();
        OnTaskFeedbackRequested?.Invoke(taskId, "Feedback_BreakStarted");
    }

     // Called by UI when Start/Pause is pressed WHILE in break
    public void InterruptBreakAndResume(string taskId)
    {
        TaskData task = FindTaskById(taskId);
        if (task == null || task.state != TaskState.Break) return;

        task.remainingBreakTime = 0f; // Clear remaining break time
        // Transition back to Active state
        ActivateTask(taskId); // Reuse ActivateTask logic (handles state change, events)
        OnTaskFeedbackRequested?.Invoke(taskId, "Feedback_BreakInterrupted");
    }

     public void MarkTaskAsCompleted(string taskId)
    {
        TaskData task = FindTaskById(taskId);
        if (task == null) return;

        // Can complete from Active, Paused, or even Break (if allowed by design)
        if (task.state != TaskState.Active && task.state != TaskState.Paused && task.state != TaskState.Break)
        {
            Debug.LogWarning($"Cannot mark task {task.title} as completed from state {task.state}.");
            return;
        }

        Debug.Log($"Marking Task as Completed: {task.title}");

         // If it was in break, clear remaining time
         if (task.state == TaskState.Break)
        {
             task.remainingBreakTime = 0f;
        }

        // Finalize history data
        task.isCompleted = true;
        task.completionTime = DateTime.UtcNow;
        // Ensure elapsedTime and breakTime are up-to-date before recording
        // (Update loop handles this, but consider edge cases if completion is immediate)
        task.completionDurationSeconds = task.elapsedTime;
        task.totalDurationSeconds = task.elapsedTime + task.breakTime;
        task.state = TaskState.Completed;

        // Clean up potential break state (redundant?)
        // StopBreakTimer(taskId); // No longer using the dictionary

        OnTaskListUpdated?.Invoke();
        OnTaskCompleted?.Invoke(task); // Notify listeners (e.g., reward system, history UI update trigger?)
        OnTaskFeedbackRequested?.Invoke(taskId, "Feedback_TaskCompleted");

        // Optional: Remove from active list immediately? Or let UI handle it based on state?
        // Let UI handle filtering based on state.
    }

    // --- Deletion --- (Called from Left List UI)

    public void RequestDeleteTask(string taskId)
    {
        TaskData task = FindTaskById(taskId);
        if (task == null) return;

        // Confirmation is good practice for deletion
        OnConfirmationRequired?.Invoke(
            taskId,
            "Confirm_Delete", // Key for confirmation message
            () => ActuallyDeleteTask(taskId) // Action on confirm
        );
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
                 // StopBreakTimer(taskId);
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
        // currentBreaks.Clear();

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
        // StopBreakTimer(task.id);

        // Task timer finished, go back to Paused state
        task.state = TaskState.Paused;
        Debug.Log($"Break Finished for Task: {task.title}");
        OnTaskBreakFinished?.Invoke(task.id); // Notify UI/Sound
        OnTaskListUpdated?.Invoke();
        OnTaskFeedbackRequested?.Invoke(task.id, "Feedback_BreakFinished"); // Notify break is done
    }

    // No longer needed if not using the dictionary
    // private void StopBreakTimer(string taskId)
    // {
    //     if (currentBreaks.ContainsKey(taskId))
    //     {
    //         currentBreaks.Remove(taskId);
    //         Debug.Log($"Stopped break timer for task {taskId}");
    //     }
    // }

}

// --- Summary Block ---
// ScriptRole: Manages the lifecycle and state transitions of all tasks. Handles timers, breaks, and communicates changes via UnityEvents.
// RelatedScripts: TaskData, TaskState, AddTaskPanel (via event), TaskListUI, ActiveTasksUI, ConfirmationPanelUI (via events), EconomyManager (listens for OnTaskCompleted), SaveLoadManager
// UsesSO: IconSetSO (for icons), potentially FeedbackMessagesSO (indirectly via feedback keys)
// ReceivesFrom: AddTaskPanel (OnNewTaskRequested), TaskListUI (PrepareTaskForActivation, RequestDeleteTask), ActiveTasksUI (ActivateTask, PauseTask, ReturnTaskToPendingList, RequestResetTask, StartBreak, InterruptBreakAndResume, MarkTaskAsCompleted), ConfirmationPanelUI (triggers actions like ActuallyResetTask, ActuallyDeleteTask)
// SendsTo: TaskListUI, ActiveTasksUI (via OnTaskListUpdated), ConfirmationPanelUI (via OnConfirmationRequired), Feedback systems (via OnTaskFeedbackRequested, OnTaskTimerFinished, OnTaskBreakFinished), EconomyManager (via OnTaskCompleted)