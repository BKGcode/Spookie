using System; // Needed for [Serializable] and Guid

[Serializable]
public class TaskData
{
    public string id;             // Unique identifier for the task
    public string title = "New Task";
    public int iconIndex = 0;     // Index in the IconSetSO list

    public float assignedTime = 0f; // Total time set by the player (in seconds)
    public float remainingTime = 0f;// Current time left for the countdown (in seconds)
    public float elapsedTime = 0f;  // Accumulated time spent in Active state (for stats/history)
    public float breakTime = 0f;    // Accumulated time spent in Break state (for stats/history)

    public TaskState state = TaskState.Pending; // Current state of the task

    // Constructor for creating a new task
    public TaskData(string newTitle, int newIconIndex, float totalSeconds)
    {
        id = Guid.NewGuid().ToString(); // Generate a unique ID
        title = newTitle;
        iconIndex = newIconIndex;
        assignedTime = totalSeconds;
        remainingTime = totalSeconds; // Start with the full assigned time
        elapsedTime = 0f;
        breakTime = 0f;
        state = TaskState.Pending; // Default state for new tasks
    }

    // Default constructor might be needed for serialization frameworks
    public TaskData()
    {
        id = Guid.NewGuid().ToString();
        state = TaskState.Pending;
    }
}

// --- Summary Block ---
// ScriptRole: Holds all runtime and persistent data for a single task, including timing, state, and identifiers.
// RelatedScripts: TaskManager (manages lists of TaskData), TaskState (defines states), IconSetSO (referenced by iconIndex), SaveLoadManager (saves/loads TaskData)
// UsesSO: Indirectly via iconIndex referencing an IconSetSO 