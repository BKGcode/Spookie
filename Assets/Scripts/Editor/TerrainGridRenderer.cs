using UnityEngine;
using UnityEditor;
using Terrain;
using Terrain.Data;
using SO;

namespace EditorScripts
{
    public class TerrainGridRenderer
    {
        private Vector2 pan = Vector2.zero;
        private float zoom = 1f;
        private TerrainEditorSettingsSO editorSettings;

        public TerrainGridRenderer(TerrainEditorSettingsSO settings)
        {
            this.editorSettings = settings;
        }

        public void HandleZoom(float delta)
        {
            zoom = Mathf.Clamp(zoom + delta * editorSettings.ZoomSpeed, editorSettings.MinZoom, editorSettings.MaxZoom);
        }

        public void HandlePan(Vector2 delta)
        {
            // Invertimos el delta para que el movimiento sea natural
            pan -= delta * editorSettings.PanSpeed;
        }

        public void DrawGrid(Rect gridRect, Terrain.TerrainData data)
        {
            if (data == null) return;

            // Calculamos el aspect ratio del terreno
            float terrainAspect = (float)data.Width / data.Height;
            float viewAspect = gridRect.width / gridRect.height;

            // Ajustamos el rect para mantener el aspect ratio
            float adjustedWidth, adjustedHeight;
            float adjustedX = gridRect.x, adjustedY = gridRect.y;

            if (viewAspect > terrainAspect)
            {
                // La vista es más ancha que el terreno
                adjustedHeight = gridRect.height;
                adjustedWidth = adjustedHeight * terrainAspect;
                adjustedX += (gridRect.width - adjustedWidth) * 0.5f;
            }
            else
            {
                // La vista es más alta que el terreno
                adjustedWidth = gridRect.width;
                adjustedHeight = adjustedWidth / terrainAspect;
                adjustedY += (gridRect.height - adjustedHeight) * 0.5f;
            }

            Rect adjustedGridRect = new Rect(adjustedX, adjustedY, adjustedWidth, adjustedHeight);

            // Calculate the scaled grid size
            float scaledWidth = adjustedWidth * zoom;
            float scaledHeight = adjustedHeight * zoom;

            // Calculate the visible area
            Rect visibleRect = new Rect(
                adjustedGridRect.x - pan.x,
                adjustedGridRect.y - pan.y,
                scaledWidth,
                scaledHeight
            );

            // Batch drawing for efficiency
            Handles.BeginGUI();

            // Draw tile backgrounds
            for (int y = 0; y < data.Height; y++)
            {
                for (int x = 0; x < data.Width; x++)
                {
                    Rect tileRect = GetTileRect(visibleRect, x, y, data);
                    if (IsRectVisible(tileRect, gridRect)) // Only draw visible tiles
                    {
                        DrawTileBackground(tileRect, data.GetTile(x, y));
                    }
                }
            }

            // Draw Dwarf Spawn Points on top
            if (data.DwarfSpawnPoints != null)
            {
                Handles.color = editorSettings.DwarfSpawnerColor;
                foreach (var spawnPoint in data.DwarfSpawnPoints)
                {
                    Rect tileRect = GetTileRect(visibleRect, spawnPoint.x, spawnPoint.y, data);
                    if (IsRectVisible(tileRect, gridRect))
                    {
                        // Draw a small circle in the center of the tile
                        Vector2 center = tileRect.center;
                        float radius = Mathf.Min(tileRect.width, tileRect.height) * 0.25f;
                        Handles.DrawSolidDisc(center, Vector3.forward, radius);
                    }
                }
            }

            // Draw Bed Spawn Points on top
            if (data.BedSpawnPoints != null)
            {
                Handles.color = editorSettings.BedSpawnerColor;
                foreach (var spawnPoint in data.BedSpawnPoints)
                {
                    Rect tileRect1 = GetTileRect(visibleRect, spawnPoint.x, spawnPoint.y, data);
                    if (IsRectVisible(tileRect1, gridRect))
                    {
                        Rect tileRect2 = GetTileRect(visibleRect, spawnPoint.x, spawnPoint.y + 1, data);
                        Rect bedRect = new Rect(tileRect1.x, tileRect1.y, tileRect1.width, tileRect1.height * 2);
                        Handles.DrawSolidRectangleWithOutline(bedRect, Handles.color, Color.black);
                        
                        GUIStyle style = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, normal = { textColor = Color.black } };
                        GUI.Label(bedRect, "B", style);
                    }
                }
            }

            Handles.EndGUI();
        }

        private bool IsRectVisible(Rect rect, Rect viewport)
        {
            return rect.Overlaps(viewport);
        }

        private void DrawTileBackground(Rect tileRect, TerrainTile tile)
        {
            MaterialSO material = tile.GetMaterial();
            Color color = (material != null) ? material.EditorColor : Color.magenta;

            if (tile.GetMiningState() == MiningState.Mined)
            {
                color *= 0.5f; // Darken mined tiles
            }

            GUI.backgroundColor = color;
            GUI.Box(tileRect, GUIContent.none, GUI.skin.box);
            
            // Draw special property indicator
            SpaceType property = tile.GetSpecialProperty();
            if (property != SpaceType.None)
            {
                Handles.color = GetColorForSpaceType(property);
                float padding = tileRect.width * 0.2f;
                Handles.DrawSolidRectangleWithOutline(
                    new Rect(tileRect.x + padding, tileRect.y + padding, tileRect.width - padding * 2, tileRect.height - padding * 2),
                    new Color(Handles.color.r, Handles.color.g, Handles.color.b, 0.4f),
                    Color.black
                );
            }
        }



        public Vector2Int GetTileCoordinatesFromMouse(Rect gridRect, Vector2 mousePosition, Terrain.TerrainData data)
        {
            if (data == null || data.Width == 0 || data.Height == 0) return new Vector2Int(-1, -1);

            // Calculamos el rect ajustado (igual que en DrawGrid)
            float terrainAspect = (float)data.Width / data.Height;
            float viewAspect = gridRect.width / gridRect.height;
            
            float adjustedWidth, adjustedHeight;
            float adjustedX = gridRect.x, adjustedY = gridRect.y;

            if (viewAspect > terrainAspect)
            {
                adjustedHeight = gridRect.height;
                adjustedWidth = adjustedHeight * terrainAspect;
                adjustedX += (gridRect.width - adjustedWidth) * 0.5f;
            }
            else
            {
                adjustedWidth = gridRect.width;
                adjustedHeight = adjustedWidth / terrainAspect;
                adjustedY += (gridRect.height - adjustedHeight) * 0.5f;
            }

            Rect adjustedGridRect = new Rect(adjustedX, adjustedY, adjustedWidth, adjustedHeight);

            // Calculate the scaled grid size (igual que en DrawGrid)
            float scaledWidth = adjustedWidth * zoom;
            float scaledHeight = adjustedHeight * zoom;

            // Calculate the visible area (igual que en DrawGrid)
            Rect visibleRect = new Rect(
                adjustedGridRect.x - pan.x,
                adjustedGridRect.y - pan.y,
                scaledWidth,
                scaledHeight
            );

            // Convertir posición del ratón a coordenadas relativas al área visible
            Vector2 relativePos = new Vector2(
                mousePosition.x - visibleRect.x,
                mousePosition.y - visibleRect.y
            );

            // Convertir a coordenadas de tile usando las dimensiones escaladas
            float tileWidth = scaledWidth / data.Width;
            float tileHeight = scaledHeight / data.Height;

            int x = (int)(relativePos.x / tileWidth);
            int y = (int)(relativePos.y / tileHeight);

            return new Vector2Int(x, y);
        }

        private Rect GetTileRect(Rect gridRect, int x, int y, Terrain.TerrainData data)
        {
            // Calculamos el aspect ratio y ajustamos el rect
            float terrainAspect = (float)data.Width / data.Height;
            float viewAspect = gridRect.width / gridRect.height;
            
            float adjustedWidth, adjustedHeight;
            float adjustedX = gridRect.x, adjustedY = gridRect.y;

            if (viewAspect > terrainAspect)
            {
                adjustedHeight = gridRect.height;
                adjustedWidth = adjustedHeight * terrainAspect;
                adjustedX += (gridRect.width - adjustedWidth) * 0.5f;
            }
            else
            {
                adjustedWidth = gridRect.width;
                adjustedHeight = adjustedWidth / terrainAspect;
                adjustedY += (gridRect.height - adjustedHeight) * 0.5f;
            }

            float tileWidth = adjustedWidth / data.Width;
            float tileHeight = adjustedHeight / data.Height;
            return new Rect(adjustedX + x * tileWidth, adjustedY + y * tileHeight, tileWidth, tileHeight);
        }
        
        private Color GetColorForSpaceType(SpaceType type)
        {
            return editorSettings.GetSpaceTypeColor(type);
        }
    }
}