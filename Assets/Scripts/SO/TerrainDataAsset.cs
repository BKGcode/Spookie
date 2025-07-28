using UnityEngine;

/// <summary>
/// A ScriptableObject that acts as a container for a complete terrain level's data.
/// These assets represent the "level templates" of the game.
/// </summary>
[CreateAssetMenu(fileName = "NewTerrainLevel", menuName = "Spookie/Terrain/Terrain Level Asset")]
public class TerrainDataAsset : ScriptableObject
{
    [Tooltip("The actual data snapshot for this terrain level.")]
    public TerrainSaveData LevelData;
}

// ScriptRole: Acts as an asset container for a complete terrain level configuration.
// Dependencies: None
// HandlesEvents: None
// TriggersEvents: None
// UsesSO: TerrainSaveData
// NeedsSetup: Create instances via the Assets > Create menu. Data is populated by the Terrain Editor. 