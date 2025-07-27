using UnityEngine;
using System.IO;
using System;

/// <summary>
/// Static class to handle saving and loading of terrain data to/from disk.
/// </summary>
public static class TerrainSaveSystem
{
    private static readonly string saveFileName = "terrain.json";
    
    /// <summary>
    /// Saves the provided terrain data to a file.
    /// </summary>
    /// <param name="saveData">The data to save.</param>
    /// <returns>True if saving was successful.</returns>
    public static bool SaveTerrain(TerrainSaveData saveData)
    {
        string path = GetSavePath();
        Debug.Log($"TerrainSaveSystem: Attempting to save terrain to {path}");
        
        try
        {
            string json = JsonUtility.ToJson(saveData, true);
            File.WriteAllText(path, json);
            Debug.Log("TerrainSaveSystem: Save successful.");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"TerrainSaveSystem: Failed to save terrain data. Error: {e.Message}");
            return false;
        }
    }

    /// <summary>
    /// Loads terrain data from a file.
    /// </summary>
    /// <returns>The loaded TerrainSaveData, or null if loading fails.</returns>
    public static TerrainSaveData LoadTerrain()
    {
        string path = GetSavePath();
        if (!FileExists())
        {
            Debug.Log("TerrainSaveSystem: No save file found.");
            return null;
        }

        Debug.Log($"TerrainSaveSystem: Attempting to load terrain from {path}");
        
        try
        {
            string json = File.ReadAllText(path);
            TerrainSaveData saveData = JsonUtility.FromJson<TerrainSaveData>(json);
            
            // Basic data integrity check
            if (saveData == null || saveData.ModifiedTiles == null)
            {
                throw new Exception("Loaded data is null or corrupted.");
            }
            
            Debug.Log($"TerrainSaveSystem: Load successful. Found {saveData.ModifiedTiles.Count} modified tiles.");
            return saveData;
        }
        catch (Exception e)
        {
            Debug.LogError($"TerrainSaveSystem: Failed to load or parse terrain data. Error: {e.Message}. A new map will be generated.");
            // Optional: Backup or delete the corrupted file
            // File.Move(path, path + ".corrupted");
            return null;
        }
    }

    /// <summary>
    /// Checks if a save file exists.
    /// </summary>
    public static bool FileExists()
    {
        return File.Exists(GetSavePath());
    }

    /// <summary>
    /// Deletes the current save file.
    /// </summary>
    public static void DeleteSaveFile()
    {
        string path = GetSavePath();
        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log($"TerrainSaveSystem: Deleted save file at {path}.");
        }
    }

    private static string GetSavePath()
    {
        return Path.Combine(Application.persistentDataPath, saveFileName);
    }
}


// ScriptRole: Provides static methods for saving and loading terrain data to disk using JSON.
// Dependencies: TerrainSaveData
// HandlesEvents: None
// TriggersEvents: None
// UsesSO: None
// NeedsSetup: None. This is a static utility class. 