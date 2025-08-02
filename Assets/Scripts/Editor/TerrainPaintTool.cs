using UnityEngine;
using UnityEditor;
using System.Linq;
using Terrain;
using Terrain.Data;
using SO;

namespace EditorScripts
{
    public class TerrainPaintTool
    {
        private enum PaintMode { Material, SpecialProperty, DwarfSpawner, BedSpawner }
        private PaintMode currentPaintMode = PaintMode.Material;
        
        private int brushSize = 1;
        private int selectedMaterialIndex = 0;
        private SpaceType selectedSpaceType = SpaceType.None;
        private string[] tabNames = { "Materials", "Properties", "Spawners" };
        private int selectedTab = 0;

        public void DrawUI(MaterialDatabase materialDatabase)
        {
            GUILayout.Label("Paint Tool", EditorStyles.boldLabel);
            selectedTab = GUILayout.Toolbar(selectedTab, tabNames);
            EditorGUILayout.Space();

            switch (selectedTab)
            {
                case 0:
                    currentPaintMode = PaintMode.Material;
                    DrawMaterialPainterUI(materialDatabase);
                    DrawBrushSettings(true);
                    break;

                case 1:
                    currentPaintMode = PaintMode.SpecialProperty;
                    DrawPropertyPainterUI();
                    DrawBrushSettings(true);
                    break;

                case 2:
                    DrawSpawnerUI();
                    DrawBrushSettings(false);
                    break;
            }
        }

        private void DrawBrushSettings(bool allowSizeChange)
        {
            EditorGUILayout.Space();
            if (allowSizeChange)
            {
                brushSize = EditorGUILayout.IntSlider("Brush Size:", brushSize, 1, 5);
            }
            else
            {
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.IntField("Brush Size:", 1);
                EditorGUI.EndDisabledGroup();
                EditorGUILayout.HelpBox("Spawners use fixed brush size of 1", MessageType.Info);
            }
        }

        private void DrawMaterialPainterUI(MaterialDatabase materialDatabase)
        {
            if (materialDatabase == null || materialDatabase.GetAllMaterials().Count == 0)
            {
                EditorGUILayout.HelpBox("Assign a Material Database with materials to paint.", MessageType.Warning);
                return;
            }

            var materials = materialDatabase.GetAllMaterials();
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            for (int i = 0; i < materials.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                var rect = EditorGUILayout.GetControlRect(GUILayout.Width(20), GUILayout.Height(20));
                EditorGUI.DrawRect(rect, materials[i].EditorColor);
                
                if (GUILayout.Toggle(selectedMaterialIndex == i, materials[i].MaterialName, EditorStyles.miniButton))
                {
                    selectedMaterialIndex = i;
                }
                
                EditorGUILayout.EndHorizontal();
            }
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawPropertyPainterUI()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            foreach (SpaceType type in System.Enum.GetValues(typeof(SpaceType)))
            {
                EditorGUILayout.BeginHorizontal();
                var rect = EditorGUILayout.GetControlRect(GUILayout.Width(20), GUILayout.Height(20));
                EditorGUI.DrawRect(rect, GetColorForSpaceType(type));
                
                if (GUILayout.Toggle(selectedSpaceType == type, type.ToString(), EditorStyles.miniButton))
                {
                    selectedSpaceType = type;
                }
                
                EditorGUILayout.EndHorizontal();
            }
            
            EditorGUILayout.EndVertical();
        }

        private void DrawSpawnerUI()
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Toggle(currentPaintMode == PaintMode.DwarfSpawner, "Dwarf Spawner", EditorStyles.miniButtonLeft))
                currentPaintMode = PaintMode.DwarfSpawner;
            if (GUILayout.Toggle(currentPaintMode == PaintMode.BedSpawner, "Bed Spawner", EditorStyles.miniButtonRight))
                currentPaintMode = PaintMode.BedSpawner;
            EditorGUILayout.EndHorizontal();

            string message = currentPaintMode == PaintMode.DwarfSpawner
                ? "Click on a walkable tile to add/remove a dwarf spawn point."
                : "Click on a walkable tile to add/remove a bed (occupies 2 vertical tiles).";
            
            EditorGUILayout.HelpBox(message, MessageType.Info);
        }

        private Color GetColorForSpaceType(SpaceType type)
        {
            switch (type)
            {
                case SpaceType.Empty: return Color.gray;
                case SpaceType.Treasure: return Color.yellow;
                case SpaceType.Enemy: return Color.red;
                default: return Color.white;
            }
        }

        public void Paint(Terrain.TerrainData terrainData, Vector2Int centerCoords, MaterialDatabase materialDatabase)
        {
            if (terrainData == null) return;

            // Para spawners, forzamos brush size 1
            int actualBrushSize = (currentPaintMode == PaintMode.DwarfSpawner || currentPaintMode == PaintMode.BedSpawner) ? 1 : brushSize;
            
            int extent = (actualBrushSize - 1) / 2;
            for (int y = -extent; y <= extent; y++)
            {
                for (int x = -extent; x <= extent; x++)
                {
                    int targetX = centerCoords.x + x;
                    int targetY = centerCoords.y + y;
                    
                    if (terrainData.IsValidPosition(targetX, targetY))
                    {
                        PaintSingleTile(terrainData, new Vector2Int(targetX, targetY), materialDatabase);
                    }
                }
            }
        }

        private void PaintSingleTile(Terrain.TerrainData terrainData, Vector2Int coords, MaterialDatabase materialDatabase)
        {
            TerrainTile tile = terrainData.GetTile(coords.x, coords.y);

            switch (currentPaintMode)
            {
                case PaintMode.Material:
                    if (materialDatabase?.GetAllMaterials().Count > 0)
                    {
                        MaterialSO selectedMaterial = materialDatabase.GetAllMaterials()[selectedMaterialIndex];
                        tile.SetMaterial(selectedMaterial);
                        tile.SetMiningState(MiningState.Intact);
                        tile.SetSpecialProperty(SpaceType.None);
                        terrainData.SetTile(coords.x, coords.y, tile);
                    }
                    break;

                case PaintMode.SpecialProperty:
                    if (selectedSpaceType == SpaceType.Empty)
                    {
                        ClearTileCompletely(terrainData, coords.x, coords.y, materialDatabase);
                    }
                    else
                    {
                        if (selectedSpaceType != SpaceType.None)
                        {
                            tile.SetMiningState(MiningState.Intact);
                        }
                        tile.SetSpecialProperty(selectedSpaceType);
                        terrainData.SetTile(coords.x, coords.y, tile);
                    }
                    break;

                case PaintMode.DwarfSpawner:
                    if (tile.IsWalkable())
                    {
                        if (terrainData.DwarfSpawnPoints.Contains(coords))
                        {
                            terrainData.DwarfSpawnPoints.Remove(coords);
                            Debug.Log($"Removed dwarf spawn point at {coords}");
                        }
                        else if (!IsTileOccupied(terrainData, coords))
                        {
                            terrainData.DwarfSpawnPoints.Add(coords);
                            Debug.Log($"Added dwarf spawn point at {coords}");
                        }
                    }
                    break;

                case PaintMode.BedSpawner:
                    HandleBedSpawner(terrainData, coords);
                    break;
            }
        }

        private void HandleBedSpawner(Terrain.TerrainData terrainData, Vector2Int coords)
        {
            Vector2Int topCoords = new Vector2Int(coords.x, coords.y + 1);

            // Si hay una cama, la removemos
            if (terrainData.BedSpawnPoints.Contains(coords))
            {
                terrainData.BedSpawnPoints.Remove(coords);
                Debug.Log($"Removed bed at {coords}");
                return;
            }

            // Validaciones para colocar nueva cama
            if (!terrainData.IsValidPosition(topCoords.x, topCoords.y))
            {
                Debug.LogWarning($"Cannot place bed at {coords}. Top tile is out of bounds.");
                return;
            }

            TerrainTile baseTile = terrainData.GetTile(coords.x, coords.y);
            TerrainTile topTile = terrainData.GetTile(topCoords.x, topCoords.y);

            if (!baseTile.IsWalkable() || !topTile.IsWalkable())
            {
                Debug.LogWarning($"Cannot place bed at {coords}. Both tiles must be walkable.");
                return;
            }

            if (IsTileOccupied(terrainData, coords) || IsTileOccupied(terrainData, topCoords))
            {
                Debug.LogWarning($"Cannot place bed at {coords}. Space is occupied.");
                return;
            }

            terrainData.BedSpawnPoints.Add(coords);
            Debug.Log($"Added bed at {coords}");
        }

        private bool IsTileOccupied(Terrain.TerrainData terrainData, Vector2Int coords)
        {
            return terrainData.DwarfSpawnPoints.Contains(coords) || 
                   terrainData.BedSpawnPoints.Contains(coords) ||
                   terrainData.BedSpawnPoints.Any(bed => bed.y + 1 == coords.y && bed.x == coords.x);
        }

        private void ClearTileCompletely(Terrain.TerrainData terrainData, int x, int y, MaterialDatabase materialDatabase)
        {
            Vector2Int pos = new Vector2Int(x, y);
            
            TerrainTile tile = terrainData.GetTile(x, y);
            tile.SetMiningState(MiningState.Mined);
            tile.SetMaterial(materialDatabase.GetBaseMaterial());
            tile.SetSpecialProperty(SpaceType.Empty);
            terrainData.SetTile(x, y, tile);

            terrainData.DwarfSpawnPoints?.Remove(pos);
            terrainData.BedSpawnPoints?.Remove(pos);

            // Check if this tile is the top part of a bed
            Vector2Int lowerPos = new Vector2Int(x, y - 1);
            if (terrainData.BedSpawnPoints?.Contains(lowerPos) == true)
            {
                terrainData.BedSpawnPoints.Remove(lowerPos);
            }
        }
    }
}

// ScriptRole: Provides UI and logic for the terrain painting tool in the editor
// Dependencies: TerrainData, TerrainEnums, MaterialDatabase
// UsesSO: MaterialDatabase, MaterialSO
// NeedsSetup: Instantiated by TerrainEditorWindow