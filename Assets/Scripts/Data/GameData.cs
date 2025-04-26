using System.Collections.Generic;
using System; // Needed for [Serializable]

[Serializable]
public class GameData
{
    public List<TaskData> tasks = new List<TaskData>();
    // public int coins = 0; // Example: Add later for EconomyManager
    // public List<PlotData> plots = new List<PlotData>(); // Example: Add later for FarmingManager
    // Add other persistent data fields here as needed

    // Constructor
    public GameData()
    {
        tasks = new List<TaskData>();
        // Initialize other fields if necessary
    }
}

// --- Summary Block ---
// ScriptRole: A simple container class holding all game state data that needs to be persisted between sessions. Marked as Serializable for JSON conversion.
// RelatedScripts: SaveLoadManager (creates, serializes, deserializes instances of this class), TaskManager (provides/receives task list), EconomyManager (will provide/receive coins), FarmingManager (will provide/receive plot data)
// UsesSO: None directly 