using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewDwarfData", menuName = "Spookie/Data/Dwarf Data")]
public class DwarfDataSO : ScriptableObject
{
    [Header("Procedural Identity")]
    [Tooltip("Possible names for procedural generation.")]
    public List<string> possibleNames;
    
    [Tooltip("Possible icons for procedural generation.")]
    public List<Sprite> possibleIcons;

    [Header("Behavior")]
    [Tooltip("The minimum number of adjacent tiles a dwarf will mine predictably before making a random choice.")]
    [Range(1, 10)] public int minTilesBeforeRandomSearch = 2;
    [Tooltip("The maximum number of adjacent tiles a dwarf will mine predictably before making a random choice.")]
    [Range(2, 20)] public int maxTilesBeforeRandomSearch = 5;
    
    [Header("Stats")]
    [Tooltip("How fast the dwarf moves across the map.")]
    public float movementSpeed = 3f;

    [Tooltip("How much damage the dwarf deals to a tile per action.")]
    public int miningPower = 10;
    
    [Tooltip("Maximum amount of stamina the dwarf can have.")]
    public float maxStamina = 100f;

    public DwarfState CreateStateInstance()
    {
        return new DwarfState(this);
    }
}

// ScriptRole: Defines the base stats for a type of dwarf.
// UsesSO: None
// NeedsSetup: Create instances from Assets menu to define different types of dwarfs. 