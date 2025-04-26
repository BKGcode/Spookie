using System;
using UnityEngine; // Needed for [Serializable]

[Serializable]
public class TaskData
{
    // Core Task Information
    public string title = "New Task";
    public int iconIndex = 0; // Index related to TaskIconSO

    // State & Status
    public bool isCompleted = false;
    public TaskState state = TaskState.Stopped;

    // Timing
    public float elapsedTime = 0f; // Time spent in Running state
    public float targetDuration = 0f; // Optional: Target time for completion (0 means no target)
    public float breakElapsedTime = 0f; // Time spent in current break
    public float breakDuration = 300f; // Default 5 minutes, can be overridden when break starts

    // UI State (Transient - might not always need saving, but included for flexibility)
    public bool isSelected = false;

    // Constructor for creating new tasks programmatically
    public TaskData(string taskTitle, int taskIconIndex = 0, float taskTargetDuration = 0f)
    {
        title = taskTitle;
        iconIndex = taskIconIndex;
        targetDuration = taskTargetDuration > 0f ? taskTargetDuration : 0f; // Ensure target is non-negative

        // Initialize default states
        isCompleted = false;
        isSelected = false;
        state = TaskState.Stopped;
        elapsedTime = 0f;
        breakElapsedTime = 0f;
        breakDuration = 300f; // Default break duration
    }

    // Default constructor required for serialization (e.g., by JsonUtility)
    public TaskData()
    {
        // Set default values consistent with the parameterized constructor
        // Although serialization usually overwrites these, it's good practice
        title = "New Task";
        iconIndex = 0;
        isCompleted = false;
        isSelected = false;
        state = TaskState.Stopped;
        elapsedTime = 0f;
        targetDuration = 0f;
        breakElapsedTime = 0f;
        breakDuration = 300f;
    }
}

// --- Summary Block ---
// ScriptRole: Holds all data for a single task (title, icon, state, timers, completion, UI selection). Serializable for persistence.
// RelatedScripts: TaskSystem (Manages list), TaskListUI (Displays list), TaskItemMinimal (Displays minimal view), TaskItemActive (Displays active view), TaskTimerManager (Updates timers/state), TaskState (Enum)
// UsesSO: Indirectly via iconIndex referencing TaskIconSO in UI/System scripts.

