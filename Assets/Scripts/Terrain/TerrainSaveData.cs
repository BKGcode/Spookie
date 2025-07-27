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
    public List<SavedTileData> AllTiles = new List<SavedTileData>();

    public TerrainSaveData(int seed, int width, int height, string version)
    {
        MapSeed = seed;
        Width = width;
        Height = height;
        GameVersion = version;
    }
} 