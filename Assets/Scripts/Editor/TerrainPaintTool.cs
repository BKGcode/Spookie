using UnityEngine;
using UnityEditor;

/// <summary>
/// Provides tools for "painting" properties onto the terrain grid in the editor.
/// </summary>
public class TerrainPaintTool
{
    private SpaceType selectedSpaceType = SpaceType.None;
    private int brushSize = 1;

    public void DrawUI()
    {
        GUILayout.Label("Paint Tool", EditorStyles.boldLabel);
        
        selectedSpaceType = (SpaceType)EditorGUILayout.EnumPopup("Space Type:", selectedSpaceType);
        brushSize = EditorGUILayout.IntSlider("Brush Size:", brushSize, 1, 5);
    }

    /// <summary>
    /// Applies the painting action to the terrain data.
    /// </summary>
    public void Paint(TerrainData terrainData, Vector2Int centerCoords)
    {
        if (terrainData == null) return;
        
        int extent = (brushSize - 1) / 2;
        for (int y = -extent; y <= extent; y++)
        {
            for (int x = -extent; x <= extent; x++)
            {
                int targetX = centerCoords.x + x;
                int targetY = centerCoords.y + y;
                
                if (terrainData.IsValidPosition(targetX, targetY))
                {
                    TerrainTile tile = terrainData.GetTile(targetX, targetY);
                    tile.SetSpecialProperty(selectedSpaceType);
                    terrainData.SetTile(targetX, targetY, tile);
                }
            }
        }
        Debug.Log($"Painted {selectedSpaceType} at ({centerCoords.x},{centerCoords.y}) with brush size {brushSize}.");
    }
}

// ScriptRole: Provides UI and logic for the terrain painting tool in the editor.
// Dependencies: TerrainData, TerrainEnums
// HandlesEvents: None
// TriggersEvents: None
// UsesSO: None
// NeedsSetup: Instantiated by TerrainEditorWindow. 