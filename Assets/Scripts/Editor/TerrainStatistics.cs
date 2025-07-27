using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Globalization;

/// <summary>
/// Analyzes terrain data and provides statistical information.
/// </summary>
public class TerrainStatistics
{
    private Dictionary<string, int> materialCounts;
    private int totalTiles;

    /// <summary>
    /// Analyzes the provided terrain data.
    /// </summary>
    public void Analyze(TerrainData terrainData)
    {
        if (terrainData == null) return;

        totalTiles = terrainData.Width * terrainData.Height;
        materialCounts = new Dictionary<string, int>();

        for (int y = 0; y < terrainData.Height; y++)
        {
            for (int x = 0; x < terrainData.Width; x++)
            {
                MaterialSO material = terrainData.GetTile(x, y).GetMaterial();
                if (material != null)
                {
                    if (!materialCounts.ContainsKey(material.MaterialName))
                    {
                        materialCounts[material.MaterialName] = 0;
                    }
                    materialCounts[material.MaterialName]++;
                }
            }
        }
    }

    /// <summary>
    /// Draws the statistics UI in the editor window.
    /// </summary>
    public void DrawUI()
    {
        GUILayout.Label("Statistics", EditorStyles.boldLabel);
        
        if (materialCounts == null || materialCounts.Count == 0)
        {
            EditorGUILayout.HelpBox("No data analyzed. Click 'Analyze' to process the current terrain.", MessageType.Info);
            return;
        }

        foreach (var pair in materialCounts.OrderBy(p => p.Key))
        {
            float percentage = (float)pair.Value / totalTiles * 100f;
            EditorGUILayout.LabelField(pair.Key, $"{pair.Value} tiles ({percentage:F2}%)");
        }
        
        if (GUILayout.Button("Export to CSV"))
        {
            ExportToCSV();
        }
    }

    private void ExportToCSV()
    {
        string path = EditorUtility.SaveFilePanel("Save Terrain Statistics", "", "terrain_stats.csv", "csv");
        
        if (string.IsNullOrEmpty(path)) return;
        
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("Material,TileCount,Percentage");
        
        foreach (var pair in materialCounts.OrderBy(p => p.Key))
        {
            float percentage = (float)pair.Value / totalTiles * 100f;
            // Use InvariantCulture to ensure '.' is the decimal separator
            sb.AppendLine($"{pair.Key},{pair.Value},{percentage.ToString("F2", CultureInfo.InvariantCulture)}");
        }
        
        try
        {
            File.WriteAllText(path, sb.ToString());
            EditorUtility.DisplayDialog("Export Successful", $"Statistics saved to {path}", "OK");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to export CSV: {e.Message}");
            EditorUtility.DisplayDialog("Export Failed", $"Could not save file. Error: {e.Message}", "OK");
        }
    }
}

// ScriptRole: Analyzes and displays statistical data about the terrain composition.
// Dependencies: TerrainData, MaterialSO
// HandlesEvents: None
// TriggersEvents: None
// UsesSO: MaterialSO
// NeedsSetup: Instantiated by TerrainEditorWindow. 