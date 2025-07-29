using UnityEngine;
using UnityEditor;

public class TerrainGridRenderer
{
    public void DrawGrid(Rect gridRect, TerrainData data)
    {
        if (data == null) return;

        // Batch drawing for efficiency
        Handles.BeginGUI();

        // Draw tile backgrounds
        for (int y = 0; y < data.Height; y++)
        {
            for (int x = 0; x < data.Width; x++)
            {
                Rect tileRect = GetTileRect(gridRect, x, y, data);
                DrawTileBackground(tileRect, data.GetTile(x, y));
            }
        }

        // Draw Dwarf Spawn Points on top
        DrawDwarfSpawners(gridRect, data);

        Handles.EndGUI();
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

    private void DrawDwarfSpawners(Rect gridRect, TerrainData data)
    {
        if (data.DwarfSpawnPoints == null) return;

        Handles.color = Color.cyan;
        foreach (var spawnPoint in data.DwarfSpawnPoints)
        {
            Rect tileRect = GetTileRect(gridRect, spawnPoint.x, spawnPoint.y, data);
            // Draw a small circle in the center of the tile
            Vector2 center = tileRect.center;
            float radius = Mathf.Min(tileRect.width, tileRect.height) * 0.25f; // 25% of the tile size
            Handles.DrawSolidDisc(center, Vector3.forward, radius);
        }
    }

    public Vector2Int GetTileCoordinatesFromMouse(Rect gridRect, Vector2 mousePosition, TerrainData data)
    {
        if (data == null || data.Width == 0 || data.Height == 0) return new Vector2Int(-1, -1);

        float tileWidth = gridRect.width / data.Width;
        float tileHeight = gridRect.height / data.Height;

        int x = (int)((mousePosition.x - gridRect.x) / tileWidth);
        int y = (int)((mousePosition.y - gridRect.y) / tileHeight);

        return new Vector2Int(x, y);
    }

    private Rect GetTileRect(Rect gridRect, int x, int y, TerrainData data)
    {
        float tileWidth = gridRect.width / data.Width;
        float tileHeight = gridRect.height / data.Height;
        return new Rect(gridRect.x + x * tileWidth, gridRect.y + y * tileHeight, tileWidth, tileHeight);
    }
    
    private Color GetColorForSpaceType(SpaceType type)
    {
        switch (type)
        {
            case SpaceType.Empty: return Color.gray;
            case SpaceType.Treasure: return Color.yellow;
            case SpaceType.Enemy: return Color.red;
            default: return Color.clear;
        }
    }
} 