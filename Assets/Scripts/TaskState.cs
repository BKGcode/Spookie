public enum TaskState
{
    Pending,    // In the left list, timer not started or stopped/reset.
    Active,     // In the right list, timer counting down.
    Paused,     // In the right list, timer paused.
    Stopped,    // In the left list, timer stopped but progress saved ("parked").
    Break,      // In the right list, main timer paused, break timer active.
    Completed   // Finished, moved to History. Not actively displayed in main lists.
}

// --- Summary Block ---
// ScriptRole: Defines the possible states a task can be in, dictating its behavior and location in the UI.
// RelatedScripts: TaskData (holds the state), TaskManager (manages state transitions), TaskItemUI (displays state visually)
// UsesSO: None 