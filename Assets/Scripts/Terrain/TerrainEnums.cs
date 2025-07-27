using UnityEngine;

/// <summary>
/// Defines special properties or occupants of a tile beyond its material.
/// </summary>
public enum SpaceType
{
    [Tooltip("Standard space with no special properties.")]
    None,
    
    [Tooltip("A designated empty area, potentially for building or pathing.")]
    Empty,
    
    [Tooltip("This tile contains a hidden treasure chest.")]
    Treasure,
    
    [Tooltip("This tile spawns or contains an enemy unit.")]
    Enemy
}

/// <summary>
/// Represents the different stages of the procedural terrain generation process.
/// </summary>
public enum GenerationPhase
{
    [Tooltip("Initial phase, filling the map with a base material.")]
    MaterialBase,
    
    [Tooltip("Second phase, adding veins and deposits of special materials.")]
    SpecialMaterials,
    
    [Tooltip("Final phase, adding special features like treasures, enemies, or structures.")]
    Postprocess
}

/// <summary>
/// Describes the current mining state of a tile.
/// </summary>
public enum MiningState
{
    [Tooltip("The tile is untouched and at full durability.")]
    Intact,
    
    [Tooltip("The tile has been partially mined but not destroyed.")]
    Damaged,
    
    [Tooltip("The tile has been fully mined and its resource extracted.")]
    Mined
}

// ScriptRole: Defines all enumerations related to the terrain system for clarity and consistency.
// Dependencies: None
// HandlesEvents: None
// TriggersEvents: None
// UsesSO: None
// NeedsSetup: None. This script is a pure data definition file. 