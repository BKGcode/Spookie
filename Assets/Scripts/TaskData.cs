using System; // Needed for [Serializable] and Guid
using UnityEngine;

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
    public float remainingBreakTime = 0f; // Current time left for the break countdown (in seconds)

    public TaskState state = TaskState.Pending; // Current state of the task

    public float initialDurationSeconds; // Duración estimada inicial
    public DateTime creationTime; // Para ordenar si es necesario

    // --- Campos para el temporizador de descanso ---
    public float initialBreakDurationSeconds;

    // --- Nuevos campos para el historial ---
    public bool isCompleted = false; // Para filtrar fácilmente
    public DateTime completionTime; // Cuándo se marcó como completada
    public float completionDurationSeconds; // Tiempo activo total invertido (sin descansos)
    public float totalDurationSeconds; // Tiempo total desde el primer inicio hasta la finalización (incluye descansos)
    public DateTime firstStartTime; // Para calcular el tiempo total transcurrido
    public float totalActiveTimeAccumulatedSeconds; // Acumulador del tiempo activo
    public float totalBreakTimeAccumulatedSeconds; // Acumulador del tiempo de descanso

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
        remainingBreakTime = 0f; // Initialize
        state = TaskState.Pending; // Default state for new tasks
        initialDurationSeconds = totalSeconds;
        creationTime = DateTime.UtcNow;
        isCompleted = false; // Asegurar estado inicial

        // Inicializar otros campos
        initialBreakDurationSeconds = 0;
        completionTime = default;
        completionDurationSeconds = 0;
        totalDurationSeconds = 0;
        firstStartTime = default;
        totalActiveTimeAccumulatedSeconds = 0;
        totalBreakTimeAccumulatedSeconds = 0;
    }

    // Default constructor might be needed for serialization frameworks
    public TaskData()
    {
        id = Guid.NewGuid().ToString();
        remainingBreakTime = 0f; // Initialize
        state = TaskState.Pending;
        initialDurationSeconds = 0f;
        creationTime = DateTime.UtcNow;
        isCompleted = false;
        initialBreakDurationSeconds = 0f;
        completionTime = default;
        completionDurationSeconds = 0f;
        totalDurationSeconds = 0f;
        firstStartTime = default;
        totalActiveTimeAccumulatedSeconds = 0f;
        totalBreakTimeAccumulatedSeconds = 0f;
    }
}

// --- Summary Block ---
// ScriptRole: Holds all runtime and persistent data for a single task, including timing, state, identifiers, and completion details for history.
// RelatedScripts: TaskManager, TaskState, IconSetSO, SaveLoadManager, GameData, TaskHistoryUI
// UsesSO: Indirectly via iconIndex referencing an IconSetSO 