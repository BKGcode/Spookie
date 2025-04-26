using UnityEngine;
using System.IO; // Needed for File operations
using System.Collections; // Needed for IEnumerator (Coroutine)
using System.Collections.Generic; // Needed for List<>

public class SaveLoadManager : MonoBehaviour
{
    [Header("System References")]
    [SerializeField] private TaskManager taskManager;
    // Add references to other managers (EconomyManager, FarmingManager) here later

    [Header("Save Configuration")]
    [SerializeField] private string saveFileName = "gameData.json";
    [SerializeField] private float saveIntervalSeconds = 10f; // How often to auto-save
    [SerializeField] private bool enableAutoSave = true;
    [SerializeField] private bool loadOnStart = true;

    private string _savePath;
    private Coroutine _autoSaveCoroutine;
    private bool _isSaving = false; // Flag to prevent concurrent saves

    void Awake()
    {
        // Determine the persistent save path
        _savePath = Path.Combine(Application.persistentDataPath, saveFileName);
        Debug.Log($"Save path set to: {_savePath}");
    }

    void Start()
    {
        if (!ValidateReferences())
        {
            enabled = false;
            return;
        }

        if (loadOnStart)
        {
            LoadGame();
        }

        if (enableAutoSave)
        {
            StartAutoSave();
        }
    }

    private bool ValidateReferences()
    {
        if (taskManager == null) { Debug.LogError($"[{gameObject.name}] Task Manager reference not set!", this); return false; }
        // Validate other manager references here when added
        return true;
    }

    private void StartAutoSave()
    {
        if (_autoSaveCoroutine != null)
        {
            StopCoroutine(_autoSaveCoroutine);
        }
        Debug.Log($"Starting auto-save coroutine with interval: {saveIntervalSeconds}s");
        _autoSaveCoroutine = StartCoroutine(AutoSaveRoutine());
    }

    private IEnumerator AutoSaveRoutine()
    {
        // Wait a bit initially before the first save
        yield return new WaitForSeconds(saveIntervalSeconds);

        while (true)
        {
            SaveGame();
            yield return new WaitForSeconds(saveIntervalSeconds);
        }
    }

    public void SaveGame()
    {
        if (_isSaving)
        {
            Debug.LogWarning("Save already in progress. Skipping.");
            return;
        }
        _isSaving = true;
        Debug.Log("Attempting to save game...");

        try
        {
            // 1. Create a data object
            GameData data = new GameData();

            // 2. Populate data from managers
            // Important: Get a *copy* or ensure the manager returns data suitable for saving
            data.tasks = taskManager.GetTasksForSaving();
            // data.coins = economyManager.GetCurrentCoins(); // Example
            // data.plots = farmingManager.GetPlotsForSaving(); // Example

            // 3. Serialize to JSON
            string json = JsonUtility.ToJson(data, true); // Use 'true' for pretty print (debugging)

            // 4. Write JSON to file
            File.WriteAllText(_savePath, json);

            Debug.Log($"Game saved successfully to {_savePath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to save game: {e.Message}\n{e.StackTrace}");
        }
        finally
        {
             _isSaving = false; // Release the lock even if save failed
        }
    }

    public void LoadGame()
    {
        if (!File.Exists(_savePath))
        {
            Debug.Log($"No save file found at {_savePath}. Starting fresh.");
            // Optionally: Initialize managers with default state here
            return;
        }

        Debug.Log("Loading game data...");
        try
        {
            // 1. Read JSON from file
            string json = File.ReadAllText(_savePath);

            // 2. Deserialize JSON to data object
            GameData data = JsonUtility.FromJson<GameData>(json);

            if (data == null)
            {
                 Debug.LogError("Failed to deserialize save data. Data is null.");
                 // Consider deleting the corrupt file or backing it up
                 // File.Delete(_savePath);
                 return;
            }

            // 3. Apply loaded data to managers
            taskManager.LoadTasks(data.tasks);
            // economyManager.LoadCoins(data.coins); // Example
            // farmingManager.LoadPlots(data.plots); // Example

            Debug.Log("Game loaded successfully.");

            // Crucially, after loading, trigger UI updates
            taskManager.ForceTaskListUpdateNotification(); // Need to add this method to TaskManager
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to load game: {e.Message}\n{e.StackTrace}");
            // Consider handling corrupt save files (e.g., delete or backup)
            // if(File.Exists(_savePath)) File.Move(_savePath, _savePath + ".corrupt");
        }
    }

    // Ensure game is saved when the application quits
    void OnApplicationQuit()
    {
        Debug.Log("Application quitting. Saving game...");
        // Stop the coroutine first if it's running
        if (_autoSaveCoroutine != null)
        {
            StopCoroutine(_autoSaveCoroutine);
             _autoSaveCoroutine = null;
        }
        SaveGame(); // Perform a final save
    }
}

// --- Summary Block ---
// ScriptRole: Handles saving and loading the game state (tasks, currency, farming progress) to/from a local JSON file. Supports auto-saving and loading on start.
// RelatedScripts: GameData (data container), TaskManager (provides/receives task data), EconomyManager (will provide/receive coin data), FarmingManager (will provide/receive plot data)
// UsesSO: None
// ReceivesFrom: Game start (triggers LoadGame), Coroutine timer (triggers SaveGame), Application quit event (triggers SaveGame)
// SendsTo: TaskManager, EconomyManager, FarmingManager (calls methods like LoadTasks, LoadCoins, LoadPlots)
