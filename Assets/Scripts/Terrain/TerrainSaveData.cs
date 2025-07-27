using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Serializable class that holds the necessary data to save and load the terrain state.
/// </summary>
[System.Serializable]
public class TerrainSaveData
{
    [SerializeField] private string gameVersion;
    [SerializeField] private int mapSeed;
    [SerializeField] private List<ModifiedTileData> modifiedTiles;

    public string GameVersion => gameVersion;
    public int MapSeed => mapSeed;
    public List<ModifiedTileData> ModifiedTiles => modifiedTiles;

    public TerrainSaveData(int seed, string version)
    {
        mapSeed = seed;
        gameVersion = version;
        modifiedTiles = new List<ModifiedTileData>();
        Debug.Log($"TerrainSaveData: Created for seed {seed}, version {version}.");
    }

    /// <summary>
    /// Adds a tile's modified data to the save list.
    /// </summary>
    public void AddModifiedTile(int x, int y, MiningState miningState, SpaceType specialProperty)
    {
        modifiedTiles.Add(new ModifiedTileData(x, y, miningState, specialProperty));
    }
}

/// <summary>
/// Serializable struct representing a tile that has been changed from its original state.
/// </summary>
[System.Serializable]
public struct ModifiedTileData
{
    [SerializeField] private int x;
    [SerializeField] private int y;
    [SerializeField] private MiningState miningState;
    [SerializeField] private SpaceType specialProperty;

    public int X => x;
    public int Y => y;
    public MiningState MiningState => miningState;
    public SpaceType SpecialProperty => specialProperty;

    public ModifiedTileData(int posX, int posY, MiningState state, SpaceType property)
    {
        x = posX;
        y = posY;
        miningState = state;
        specialProperty = property;
    }
}

// ScriptRole: Defines the serializable data structure for saving the terrain's state.
// Dependencies: TerrainEnums
// HandlesEvents: None
// TriggersEvents: None
// UsesSO: None
// NeedsSetup: This class is a data container, instantiated and populated by the TerrainSaveSystem. 