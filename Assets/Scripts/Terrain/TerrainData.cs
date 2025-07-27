using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Holds the entire grid of terrain tiles and provides methods to access and modify it.
/// </summary>
[System.Serializable]
public class TerrainData
{
    [SerializeField] private TerrainTile[,] tileGrid;
    [SerializeField] private int width = 100;
    [SerializeField] private int height = 100;
    [SerializeField] private int mapSeed;

    public int Width => width;
    public int Height => height;
    public int MapSeed => mapSeed;
    
    /// <summary>
    /// Initializes a new TerrainData object with a specific size and seed.
    /// </summary>
    public TerrainData(int gridWidth, int gridHeight, int seed)
    {
        Debug.Log($"TerrainData: Initializing with size {gridWidth}x{gridHeight} and seed {seed}.");
        width = gridWidth;
        height = gridHeight;
        mapSeed = seed;
        tileGrid = new TerrainTile[width, height];
    }
    
    /// <summary>
    /// Gets the tile at a specific coordinate.
    /// </summary>
    /// <returns>The TerrainTile at the given position. Returns a default tile if position is invalid.</returns>
    public TerrainTile GetTile(int x, int y)
    {
        if (!IsValidPosition(x, y))
        {
            Debug.LogError($"TerrainData: GetTile position ({x},{y}) is out of bounds.");
            return default;
        }
        return tileGrid[x, y];
    }

    /// <summary>
    /// Sets the tile at a specific coordinate.
    /// </summary>
    public void SetTile(int x, int y, TerrainTile tile)
    {
        if (!IsValidPosition(x, y))
        {
            Debug.LogError($"TerrainData: SetTile position ({x},{y}) is out of bounds.");
            return;
        }
        tileGrid[x, y] = tile;
    }

    /// <summary>
    /// Checks if a given coordinate is within the grid boundaries.
    /// </summary>
    public bool IsValidPosition(int x, int y)
    {
        return x >= 0 && x < width && y >= 0 && y < height;
    }

    /// <summary>
    /// Gets all valid neighbors of a given tile.
    /// </summary>
    /// <returns>A list of coordinate pairs for the neighbors.</returns>
    public List<(int, int)> GetNeighbors(int x, int y)
    {
        var neighbors = new List<(int, int)>();
        int[] dx = { -1, 1, 0, 0 }; // Left, Right
        int[] dy = { 0, 0, -1, 1 }; // Down, Up

        for (int i = 0; i < 4; i++)
        {
            int nx = x + dx[i];
            int ny = y + dy[i];

            if (IsValidPosition(nx, ny))
            {
                neighbors.Add((nx, ny));
            }
        }
        return neighbors;
    }
}

// ScriptRole: Manages the entire 100x100 grid of terrain data.
// Dependencies: TerrainTile
// HandlesEvents: None
// TriggersEvents: None
// UsesSO: None
// NeedsSetup: This class is a data container, typically instantiated and managed by a terrain generator. 