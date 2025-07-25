using UnityEngine;

[CreateAssetMenu(fileName = "NewDwarfData", menuName = "Spookie/Data/Dwarf Data")]
public class DwarfDataSO : ScriptableObject
{
    [Tooltip("Name or type of the dwarf.")]
    public string dwarfName = "Dwarf";

    [Header("Stats")]
    [Tooltip("How fast the dwarf moves across the map.")]
    public float movementSpeed = 3f;

    [Tooltip("How much damage the dwarf deals to a tile per action.")]
    public int miningPower = 10;
    
    [Tooltip("Maximum amount of stamina the dwarf can have.")]
    public float maxStamina = 100f;
}

// ScriptRole: Defines the base stats for a type of dwarf.
// UsesSO: None
// NeedsSetup: Create instances from Assets menu to define different types of dwarfs. 