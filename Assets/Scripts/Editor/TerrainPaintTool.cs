using UnityEngine;
using UnityEditor;
using System.Linq;

/// <summary>
/// Provides tools for "painting" properties onto the terrain grid in the editor.
/// </summary>
public class TerrainPaintTool
{
    private enum PaintMode { Material, SpecialProperty }
    private PaintMode currentPaintMode = PaintMode.Material;
    
    // Brush settings
    private int brushSize = 1;
    
    // Material paint settings
    private int selectedMaterialIndex = 0;
    
    // Property paint settings
    private SpaceType selectedSpaceType = SpaceType.None;

    public void DrawUI(MaterialDatabase materialDatabase)
    {
        GUILayout.Label("Paint Tool", EditorStyles.boldLabel);
        
        currentPaintMode = (PaintMode)EditorGUILayout.EnumPopup("Paint Mode:", currentPaintMode);
        
        if (currentPaintMode == PaintMode.Material)
        {
            DrawMaterialPainterUI(materialDatabase);
        }
        else // currentPaintMode == PaintMode.SpecialProperty
        {
            DrawPropertyPainterUI();
        }

        brushSize = EditorGUILayout.IntSlider("Brush Size:", brushSize, 1, 5);
    }

    private void DrawMaterialPainterUI(MaterialDatabase materialDatabase)
    {
        if (materialDatabase == null || materialDatabase.GetAllMaterials().Count == 0)
        {
            EditorGUILayout.HelpBox("Assign a Material Database with materials to paint.", MessageType.Warning);
            return;
        }

        string[] materialNames = materialDatabase.GetAllMaterials().Select(m => m.MaterialName).ToArray();
        selectedMaterialIndex = EditorGUILayout.Popup("Material:", selectedMaterialIndex, materialNames);
    }
    
    private void DrawPropertyPainterUI()
    {
        selectedSpaceType = (SpaceType)EditorGUILayout.EnumPopup("Space Type:", selectedSpaceType);
    }

    /// <summary>
    /// Applies the painting action to the terrain data.
    /// </summary>
    public void Paint(TerrainData terrainData, Vector2Int centerCoords, MaterialDatabase materialDatabase)
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
                    if (currentPaintMode == PaintMode.Material)
                    {
                        PaintMaterial(terrainData, targetX, targetY, materialDatabase);
                    }
                    else // currentPaintMode == PaintMode.SpecialProperty
                    {
                        PaintProperty(terrainData, targetX, targetY, materialDatabase);
                    }
                }
            }
        }
    }
    
    private void PaintMaterial(TerrainData terrainData, int x, int y, MaterialDatabase materialDatabase)
    {
        if (materialDatabase == null || materialDatabase.GetAllMaterials().Count == 0) return;
        
        MaterialSO selectedMaterial = materialDatabase.GetAllMaterials()[selectedMaterialIndex];
        
        // When painting a material, we overwrite the tile's state completely
        // to ensure it becomes a full, solid block of the selected type.
        TerrainTile tile = terrainData.GetTile(x, y);
        tile.SetMaterial(selectedMaterial);
        tile.SetMiningState(MiningState.Intact);
        tile.SetSpecialProperty(SpaceType.None);
        
        terrainData.SetTile(x, y, tile);
    }
    
    private void PaintProperty(TerrainData terrainData, int x, int y, MaterialDatabase materialDatabase)
    {
        TerrainTile tile = terrainData.GetTile(x, y);

        switch (selectedSpaceType)
        {
            case SpaceType.Empty:
                // When painting 'Empty', we effectively mine the tile and ensure it has the base material.
                tile.SetMiningState(MiningState.Mined);
                tile.SetMaterial(materialDatabase.GetBaseMaterial());
                break;
            
            case SpaceType.None:
                // When clearing a property, we just reset the property itself, leaving mining state as is.
                break;

            default: // Treasure, Enemy, etc.
                // Ensure the tile is solid before placing a marker on it.
                tile.SetMiningState(MiningState.Intact);
                break;
        }

        tile.SetSpecialProperty(selectedSpaceType);
        terrainData.SetTile(x, y, tile);
    }
}

// ScriptRole: Provides UI and logic for the terrain painting tool in the editor.
// Dependencies: TerrainData, TerrainEnums, MaterialDatabase, MaterialSO
// HandlesEvents: None
// TriggersEvents: None
// UsesSO: MaterialDatabase, MaterialSO
// NeedsSetup: Instantiated by TerrainEditorWindow. 