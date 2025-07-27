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
    /// Saves the provided terrain data to a specific file path.
    /// </summary>
    public static bool SaveTerrain(TerrainSaveData saveData, string path)
    {
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
            Debug.LogError($"TerrainSaveSystem: Failed to save terrain data to {path}. Error: {e.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// Saves the provided terrain data to the default application path.
    /// </summary>
    public static bool SaveTerrain(TerrainSaveData saveData)
    {
        return SaveTerrain(saveData, GetDefaultSavePath());
    }

    /// <summary>
    /// Loads terrain data from a specific file path.
    /// </summary>
    public static TerrainSaveData LoadTerrain(string path)
    {
        if (!File.Exists(path))
        {
            Debug.LogError($"TerrainSaveSystem: No file found at path {path}.");
            return null;
        }

        Debug.Log($"TerrainSaveSystem: Attempting to load terrain from {path}");
        
        try
        {
            string json = File.ReadAllText(path);
            TerrainSaveData saveData = JsonUtility.FromJson<TerrainSaveData>(json);
            
            if (saveData == null || saveData.AllTiles == null)
            {
                throw new Exception("Loaded data is null or corrupted.");
            }
            
            Debug.Log($"TerrainSaveSystem: Load successful from {path}. Found {saveData.AllTiles.Count} tiles.");
            return saveData;
        }
        catch (Exception e)
        {
            Debug.LogError($"TerrainSaveSystem: Failed to load or parse terrain data from {path}. Error: {e.Message}.");
            return null;
        }
    }
    
    /// <summary>
    /// Loads terrain data from the default application path.
    /// </summary>
    public static TerrainSaveData LoadTerrain()
    {
        string path = GetDefaultSavePath();
        if (!File.Exists(path))
        {
            // This is not an error in runtime, just means no save game exists yet.
            Debug.Log("TerrainSaveSystem: No default save file found.");
            return null;
        }
        return LoadTerrain(path);
    }

    /// <summary>
    /// Checks if a save file exists at the default path.
    /// </summary>
    public static bool DefaultSaveFileExists()
    {
        return File.Exists(GetDefaultSavePath());
    }

    /// <summary>
    /// Deletes the default save file.
    /// </summary>
    public static void DeleteDefaultSaveFile()
    {
        string path = GetDefaultSavePath();
        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log($"TerrainSaveSystem: Deleted default save file at {path}.");
        }
    }

    private static string GetDefaultSavePath()
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
// NeedsSetup: None. This is a static utility class. 