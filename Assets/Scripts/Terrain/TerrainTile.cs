using UnityEngine;

/// <summary>
/// Struct representing the data for a single tile in the terrain grid.
/// </summary>
[System.Serializable]
public struct TerrainTile
{
    [SerializeField] private MaterialSO material;
    [SerializeField] private MiningState miningState;
    [SerializeField] private SpaceType specialProperty;

    /// <summary>
    /// Initializes a new tile with a specific material.
    /// </summary>
    public TerrainTile(MaterialSO initialMaterial)
    {
        material = initialMaterial;
        miningState = MiningState.Intact;
        specialProperty = SpaceType.None;
    }

    /// <summary>
    /// Checks if the tile can be mined.
    /// </summary>
    /// <returns>True if the tile has a material and is not already mined.</returns>
    public bool IsMineable()
    {
        return material != null && miningState != MiningState.Mined;
    }
    
    /// <summary>
    /// Gets the material of this tile.
    /// </summary>
    /// <returns>The MaterialSO of the tile.</returns>
    public MaterialSO GetMaterial()
    {
        return material;
    }
    
    /// <summary>
    /// Sets the material for this tile.
    /// </summary>
    public void SetMaterial(MaterialSO newMaterial)
    {
        material = newMaterial;
    }

    /// <summary>
    /// Sets a special property for this tile.
    /// </summary>
    /// <param name="newProperty">The new SpaceType property to assign.</param>
    public void SetSpecialProperty(SpaceType newProperty)
    {
        specialProperty = newProperty;
        Debug.Log($"TerrainTile: Special property set to {newProperty}.");
    }

    /// <summary>
    /// Updates the mining state of the tile.
    /// </summary>
    public void SetMiningState(MiningState newState)
    {
        if (miningState != newState)
        {
            miningState = newState;
            Debug.Log($"TerrainTile: Mining state changed to {newState}.");
        }
    }
    
    /// <summary>
    /// Returns the current mining state of the tile.
    /// </summary>
    public MiningState GetMiningState()
    {
        return miningState;
    }

    /// <summary>
    /// Returns the special property of the tile.
    /// </summary>
    public SpaceType GetSpecialProperty()
    {
        return specialProperty;
    }
}

// ScriptRole: Represents all data for a single terrain tile, including its material and state.
// Dependencies: MaterialSO, TerrainEnums
// HandlesEvents: None
// TriggersEvents: None
// UsesSO: MaterialSO
// NeedsSetup: None. This is a data struct initialized by TerrainData. 