// FILE: TaskTimerManager.cs
using UnityEngine;
using System.Collections.Generic; // Needed for IReadOnlyList

/// <summary>
/// Manages the time tracking for individual tasks.
/// It reads task data from TaskSystem and updates elapsed time.
/// It receives commands (Toggle, Reset) and forwards them to TaskSystem.
/// </summary>
public class TaskTimerManager : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Reference to the TaskSystem to access task data and request changes.")]
    public TaskSystem taskSystem; // Assign this in the Unity Inspector

    // We need a small interval to avoid saving excessively often if timers update rapidly.
    private const float SAVE_INTERVAL = 2.0f; // Save accumulated time every 2 seconds
    private float _timeSinceLastSave = 0f;

    void Update()
    {
        // Ensure TaskSystem is assigned before proceeding
        if (taskSystem == null)
        {
            // Log error only once to avoid spamming the console
            if (Time.frameCount % 100 == 0) // Log roughly every few seconds if still null
            {
               Debug.LogError("[TaskTimerManager] TaskSystem reference is not set in the Inspector!");
            }
            return; // Stop execution if TaskSystem is missing
        }

        // Get the current list of tasks (read-only is fine for iteration)
        IReadOnlyList<TaskData> tasks = taskSystem.Tasks;
        bool timeUpdated = false;

        // Iterate through all tasks to update timers
        for (int i = 0; i < tasks.Count; i++)
        {
            // Check if the timer for THIS specific task should be running
            if (tasks[i].isTimerRunning)
            {
                // Increment the elapsed time for this task.
                // We can modify the field directly because TaskData is a class (reference type).
                tasks[i].elapsedTime += Time.deltaTime;
                timeUpdated = true; // Mark that some time was updated this frame
            }
        }

        // --- Save accumulated time periodically ---
        // If any timer was updated, accumulate time towards the next save
        if (timeUpdated)
        {
             _timeSinceLastSave += Time.deltaTime;
        }

        // If enough time has passed since the last save, and time was updated, save now.
        // Also save if time was *not* updated this frame but might have been updated previously and needs saving.
        if (_timeSinceLastSave >= SAVE_INTERVAL && timeUpdated)
        {
             // We don't call SaveTasks directly. The modification happens above.
             // We trigger save here to persist the accumulated time periodically.
             taskSystem.SaveTasks(); // Ask TaskSystem to persist all current data
             _timeSinceLastSave = 0f; // Reset save timer
             // Optionally: Trigger UI refresh less frequently if needed, but SaveTasks() in TaskSystem
             // currently doesn't trigger OnTaskListChanged unless data is modified *by its methods*.
             // If real-time UI update during ticking is needed, we'll need Option B events later.
        }
    }

    // --- Public Methods Called by TaskListUI (or other systems) ---

    /// <summary>
    /// Toggles the running state (Start/Pause) of a specific task's timer.
    /// Delegates the action to TaskSystem.
    /// </summary>
    public void ToggleTaskTimer(int taskIndex)
    {
        if (taskSystem == null) return; // Safety check

        // Check index validity (TaskSystem methods also check, but good practice here too)
         if (taskIndex < 0 || taskIndex >= taskSystem.Tasks.Count)
         {
            Debug.LogWarning($"[TaskTimerManager] ToggleTaskTimer: Invalid task index {taskIndex}");
            return;
         }

        // Find the current state and ask TaskSystem to set the opposite state
        bool currentRunningState = taskSystem.Tasks[taskIndex].isTimerRunning;
        taskSystem.SetTaskTimerRunning(taskIndex, !currentRunningState);

        // TaskSystem.SetTaskTimerRunning handles logging, saving, and invoking OnTaskListChanged
    }

    /// <summary>
    /// Resets the elapsed time of a specific task's timer to zero and stops it.
    /// Delegates the action to TaskSystem.
    /// </summary>
    public void ResetTaskTimer(int taskIndex)
    {
        if (taskSystem == null) return; // Safety check

         if (taskIndex < 0 || taskIndex >= taskSystem.Tasks.Count)
         {
             Debug.LogWarning($"[TaskTimerManager] ResetTaskTimer: Invalid task index {taskIndex}");
            return;
         }

        // Ask TaskSystem to stop the timer AND reset the elapsed time
        taskSystem.SetTaskTimerRunning(taskIndex, false); // Ensure timer is stopped
        taskSystem.ResetTaskElapsedTime(taskIndex);       // Reset the time counter

         // TaskSystem methods handle logging, saving, and invoking OnTaskListChanged
    }
}
