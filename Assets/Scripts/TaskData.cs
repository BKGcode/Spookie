// FILE: TaskData.cs
using System;
using System.Collections.Generic; // Needed for List<T> in TaskSaveData

/// <summary>
/// Represents a single task with its details, including icon index, timer state,
/// and selection state for the active list.
/// </summary>
[Serializable] // Ensures this class can be serialized (e.g., to JSON)
public class TaskData
{
    public string title;
    public bool isCompleted;
    public int iconIndex;
    public float elapsedTime = 0f;   // Time spent on this task in seconds
    public bool isTimerRunning = false; // Is the timer for THIS task currently active?

    // --- NEW FIELD FOR SELECTION STATE ---
    public bool isSelected = false; // Should this task appear in the right-hand active list?
    // --- END NEW FIELD ---

    /// <summary>
    /// Constructor for creating a new task with title and icon.
    /// Initializes timer and selection fields to default values.
    /// </summary>
    public TaskData(string title, int iconIndex)
    {
        this.title = title;
        this.isCompleted = false; // New tasks start as incomplete
        this.iconIndex = iconIndex;
        // elapsedTime, isTimerRunning, and isSelected get default values (0f, false, false)
    }

    /// <summary>
    /// Alternative constructor using only the title (uses default icon index 0).
    /// Initializes timer and selection fields to default values.
    /// </summary>
    public TaskData(string title)
        : this(title, 0) // Calls the main constructor with default icon index
    {
        // Initialization handled by the main constructor
    }

     // Parameterless constructor for serialization
    public TaskData() { }
}

/// <summary>
/// Wrapper class used specifically for serializing a list of TaskData objects to JSON.
/// </summary>
[Serializable]
public class TaskSaveData
{
    // This list will contain instances of the TaskData class defined above.
    public List<TaskData> tasks;
}
