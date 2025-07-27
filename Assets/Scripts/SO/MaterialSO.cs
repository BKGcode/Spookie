using UnityEngine;

[CreateAssetMenu(fileName = "NewMaterial", menuName = "Spookie/Terrain/Material")]
public class MaterialSO : ScriptableObject
{
    [Header("Identity")]
    [Tooltip("The unique, persistent ID for this material. Do not change this value.")]
    [SerializeField] private string guid;
    public string Guid => guid;
    public int MaterialId => guid.GetHashCode();
    
    [Tooltip("Display name for this material.")]
    public string MaterialName;

    [Header("Generation")]
    [Range(0, 100)]
    [Tooltip("The exact percentage of the map this material should cover during procedural generation. The Base Material will fill the rest.")]
    public int CoveragePercentage = 5;

    [Header("Gameplay")]
    public bool IsMineable = true;
    [Range(1f, 100f)]
    public float Durability = 10f;

    [Header("Visuals")]
    public Material Material;
    public Color EditorColor = Color.white;

    public void GenerateGuid()
    {
        if (string.IsNullOrEmpty(guid))
        {
            guid = System.Guid.NewGuid().ToString();
        }
    }
}

// ScriptRole: Defines a unique type of minable material in the game.
// Dependencies: None
// HandlesEvents: None
// TriggersEvents: None
// UsesSO: None
// NeedsSetup: Set materialId (unique), material, configure rarity and durability values 