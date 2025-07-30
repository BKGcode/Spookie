using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Represents the serializable data for a single tile.
/// </summary>
[System.Serializable]
public class SavedTileData
{
    public string MaterialGuid; // Changed from MaterialName
    public MiningState MiningState;
    public SpaceType SpecialProperty;
}

/// <summary>
/// Represents a full snapshot of the terrain state, ready for serialization.
/// </summary>
[System.Serializable]
public class TerrainSaveData
{
    public string GameVersion;
    public int MapSeed;
    public int Width;
    public int Height;
    public List<SavedTileData> AllTiles { get; private set; } = new List<SavedTileData>();
    public List<Vector2Int> DwarfSpawnPoints { get; private set; } = new List<Vector2Int>();
    public List<Vector2Int> BedSpawnPoints { get; private set; } = new List<Vector2Int>();

    // Parameterless constructor for serialization
    public TerrainSaveData() {}

    public TerrainSaveData(int seed, int width, int height, string createdBy)
    {
        MapSeed = seed;
        Width = width;
        Height = height;
        GameVersion = createdBy;
        AllTiles = new List<SavedTileData>(width * height);
        DwarfSpawnPoints = new List<Vector2Int>();
        BedSpawnPoints = new List<Vector2Int>();
    }
} 