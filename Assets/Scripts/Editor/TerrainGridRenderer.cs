using UnityEngine;
using UnityEditor;

/// <summary>
/// Handles rendering the 2D grid view inside the TerrainEditorWindow.
/// </summary>
public class TerrainGridRenderer
{
    private Vector2 scrollPosition = Vector2.zero;
    private float zoomFactor = 10.0f;
    private const float MinZoom = 2.0f;
    private const float MaxZoom = 20.0f;
    
    public void DrawGrid(Rect position, TerrainData terrainData)
    {
        if (terrainData == null) return;
        
        // Handle zoom and pan events
        HandleMouseEvents(position);

        // Begin scroll view
        Rect viewRect = new Rect(0, 0, terrainData.Width * zoomFactor, terrainData.Height * zoomFactor);
        scrollPosition = GUI.BeginScrollView(position, scrollPosition, viewRect, false, false);
        
        // Draw tiles
        for (int y = 0; y < terrainData.Height; y++)
        {
            for (int x = 0; x < terrainData.Width; x++)
            {
                Rect tileRect = new Rect(x * zoomFactor, (terrainData.Height - 1 - y) * zoomFactor, zoomFactor, zoomFactor);
                DrawTile(tileRect, terrainData.GetTile(x, y));
            }
        }
        
        GUI.EndScrollView();
    }
    
    private void DrawTile(Rect rect, TerrainTile tile)
    {
        Color color = Color.black; // Default for null material
        MaterialSO material = tile.GetMaterial();
        if (material != null)
        {
            color = material.EditorColor;
        }
        
        // Draw tile background
        EditorGUI.DrawRect(rect, color);
        
        // Draw special property indicator on top
        SpaceType property = tile.GetSpecialProperty();
        if (property != SpaceType.None)
        {
            Color propertyColor = GetColorForSpaceType(property);
            Rect propertyRect = new Rect(rect.x + rect.width * 0.25f, rect.y + rect.height * 0.25f, rect.width * 0.5f, rect.height * 0.5f);
            EditorGUI.DrawRect(propertyRect, propertyColor);
        }

        // Draw grid lines
        Handles.color = Color.gray;
        Handles.DrawLine(new Vector3(rect.xMin, rect.yMin), new Vector3(rect.xMax, rect.yMin));
        Handles.DrawLine(new Vector3(rect.xMin, rect.yMin), new Vector3(rect.xMin, rect.yMax));
    }

    private Color GetColorForSpaceType(SpaceType type)
    {
        switch (type)
        {
            case SpaceType.Empty:
                return new Color(0.5f, 0.5f, 0.5f, 0.8f); // Semi-transparent grey
            case SpaceType.Treasure:
                return new Color(1f, 0.84f, 0f, 0.8f); // Gold
            case SpaceType.Enemy:
                return new Color(1f, 0.2f, 0.2f, 0.8f); // Red
            default:
                return Color.clear;
        }
    }

    private void HandleMouseEvents(Rect position)
    {
        Event current = Event.current;

        if (position.Contains(current.mousePosition))
        {
            // Zoom with scroll wheel
            if (current.type == EventType.ScrollWheel)
            {
                zoomFactor -= current.delta.y * 0.1f;
                zoomFactor = Mathf.Clamp(zoomFactor, MinZoom, MaxZoom);
                current.Use(); // Consume the event
            }
        }
        
        // Pan with middle mouse button
        if (current.type == EventType.MouseDrag && current.button == 2)
        {
            scrollPosition -= current.delta;
            current.Use();
        }
    }

    public Vector2Int GetTileCoordinatesFromMouse(Rect gridRect, Vector2 mousePosition, TerrainData terrainData)
    {
        if (!gridRect.Contains(mousePosition) || terrainData == null) return new Vector2Int(-1, -1);
        
        Vector2 localMousePos = mousePosition - gridRect.position + scrollPosition;
        int x = Mathf.FloorToInt(localMousePos.x / zoomFactor);
        int y = Mathf.FloorToInt(localMousePos.y / zoomFactor);
        
        // Invert Y to match our grid data structure (0,0 is bottom-left)
        int invertedY = (int)(terrainData.Height - 1 - y);

        return new Vector2Int(x, invertedY);
    }
}

// ScriptRole: Renders an interactive 2D grid for the terrain editor window.
// Dependencies: TerrainData, MaterialSO
// HandlesEvents: None (processes GUI events internally)
// TriggersEvents: None
// UsesSO: MaterialSO (for editor colors)
// NeedsSetup: Instantiated by TerrainEditorWindow. 