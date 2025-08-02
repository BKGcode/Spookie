using UnityEngine;
using System.IO;
using System;
using Terrain.Data;

namespace Terrain
{
    public static class TerrainSaveSystem
    {
        private static readonly string saveFileName = "terrain.json";
        
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
        
        public static bool SaveTerrain(TerrainSaveData saveData)
        {
            return SaveTerrain(saveData, GetDefaultSavePath());
        }

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
        
        public static TerrainSaveData LoadTerrain()
        {
            string path = GetDefaultSavePath();
            if (!File.Exists(path))
            {
                Debug.Log("TerrainSaveSystem: No default save file found.");
                return null;
            }
            return LoadTerrain(path);
        }

        public static bool DefaultSaveFileExists()
        {
            return File.Exists(GetDefaultSavePath());
        }

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
}
