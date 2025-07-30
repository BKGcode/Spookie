using UnityEngine;
using UnityEditor;
using System.Linq;

/// <summary>
/// Provides tools for "painting" properties onto the terrain grid in the editor.
/// </summary>
public class TerrainPaintTool
{
    private enum PaintMode { Material, SpecialProperty, DwarfSpawner, BedSpawner }
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
        else if (currentPaintMode == PaintMode.SpecialProperty)
        {
            DrawPropertyPainterUI();
        }
        else if (currentPaintMode == PaintMode.DwarfSpawner)
        {
            DrawDwarfSpawnerUI();
        }
        else // currentPaintMode == PaintMode.BedSpawner
        {
            DrawBedSpawnerUI();
        }

        if (currentPaintMode != PaintMode.DwarfSpawner && currentPaintMode != PaintMode.BedSpawner)
        {
            brushSize = EditorGUILayout.IntSlider("Brush Size:", brushSize, 1, 5);
        }
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

    private void DrawDwarfSpawnerUI()
    {
        EditorGUILayout.HelpBox("Click on a walkable tile to add or remove a dwarf spawn point.", MessageType.Info);
    }

    private void DrawBedSpawnerUI()
    {
        EditorGUILayout.HelpBox("Click on a walkable tile to place a 1x2 bed. The bed will occupy this tile and the one above it.", MessageType.Info);
    }

    /// <summary>
    /// Applies the painting action to the terrain data.
    /// </summary>
    public void Paint(TerrainData terrainData, Vector2Int centerCoords, MaterialDatabase materialDatabase)
    {
        if (terrainData == null) return;

        if (currentPaintMode == PaintMode.DwarfSpawner)
        {
            PaintDwarfSpawn(terrainData, centerCoords.x, centerCoords.y);
            return; // Exit after handling the single tile
        }
        else if (currentPaintMode == PaintMode.BedSpawner)
        {
            PaintBedSpawn(terrainData, centerCoords.x, centerCoords.y);
            return; // Exit after handling the single tile
        }
        
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
                    else if (currentPaintMode == PaintMode.SpecialProperty)
                    {
                        PaintProperty(terrainData, targetX, targetY, materialDatabase);
                    }
                    // This is now handled above
                    // else // DwarfSpawner
                    // {
                    //     PaintDwarfSpawn(terrainData, targetX, targetY);
                    // }
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

    private void PaintDwarfSpawn(TerrainData terrainData, int x, int y)
    {
        TerrainTile tile = terrainData.GetTile(x, y);
        Vector2Int coords = new Vector2Int(x, y);

        // Can only place spawners on walkable ground (mined tiles)
        if (tile.GetMiningState() == MiningState.Mined)
        {
            if (terrainData.DwarfSpawnPoints.Contains(coords))
            {
                terrainData.DwarfSpawnPoints.Remove(coords);
                Debug.Log($"Removed dwarf spawn point at {coords}");
            }
            else
            {
                terrainData.DwarfSpawnPoints.Add(coords);
                Debug.Log($"Added dwarf spawn point at {coords}");
            }
        }
        else
        {
            Debug.LogWarning($"Cannot place spawn point at {coords}. Tile is not walkable (not mined).");
        }
    }

    private void PaintBedSpawn(TerrainData terrainData, int x, int y)
    {
        Vector2Int coords = new Vector2Int(x, y);
        Vector2Int adjacentCoords = new Vector2Int(x, y + 1);

        // Check if the bed is already at this position to allow removal
        if (terrainData.BedSpawnPoints.Contains(coords))
        {
            terrainData.BedSpawnPoints.Remove(coords);
            Debug.Log($"Removed bed spawn point at {coords}");
            return;
        }

        // --- Placement Validation ---
        // 1. Check if adjacent tile is valid
        if (!terrainData.IsValidPosition(adjacentCoords.x, adjacentCoords.y))
        {
            Debug.LogWarning($"Cannot place bed at {coords}. The adjacent tile {adjacentCoords} is out of bounds.");
            return;
        }

        // 2. Check if both tiles are walkable
        TerrainTile baseTile = terrainData.GetTile(coords.x, coords.y);
        TerrainTile adjacentTile = terrainData.GetTile(adjacentCoords.x, adjacentCoords.y);
        if (baseTile.GetMiningState() != MiningState.Mined || adjacentTile.GetMiningState() != MiningState.Mined)
        {
            Debug.LogWarning($"Cannot place bed at {coords}. Both this tile and {adjacentCoords} must be walkable (mined).");
            return;
        }

        // 3. Check if the space is already occupied by another spawner
        if (terrainData.DwarfSpawnPoints.Contains(coords) || terrainData.DwarfSpawnPoints.Contains(adjacentCoords) ||
            terrainData.BedSpawnPoints.Contains(adjacentCoords)) // Check if another bed starts on the second tile
        {
            Debug.LogWarning($"Cannot place bed at {coords}. The space is already occupied by another spawn point.");
            return;
        }
        
        // --- End of Validation ---

        terrainData.BedSpawnPoints.Add(coords);
        Debug.Log($"Added bed spawn point at {coords}. It will occupy {coords} and {adjacentCoords}.");
    }
}

// ScriptRole: Provides UI and logic for the terrain painting tool in the editor.
// Dependencies: TerrainData, TerrainEnums, MaterialDatabase, MaterialSO
// HandlesEvents: None
// TriggersEvents: None
// UsesSO: MaterialDatabase, MaterialSO
// NeedsSetup: Instantiated by TerrainEditorWindow. 