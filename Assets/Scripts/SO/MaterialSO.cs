using UnityEngine;

[CreateAssetMenu(fileName = "NewMaterial", menuName = "Spookie/Terrain/Material")]
public class MaterialSO : ScriptableObject
{
    [Header("Basic Properties")]
    [SerializeField] private string materialName = "New Material";
    [SerializeField] private int materialId = 0;
    
    [Header("Visual")]
    [Tooltip("Material used for both the cube and floor (same visual for consistency)")]
    [SerializeField] private Material material;
    [SerializeField] private Color editorColor = Color.white;
    
    [Header("Generation")]
    [Range(0, 100)]
    [Tooltip("The exact percentage of the map this material should cover. The Base Material will fill the rest.")]
    [SerializeField] private int coveragePercentage = 5;
    
    [Range(1f, 100f)]
    [SerializeField] private float durability = 10f;
    
    [Header("Economics")]
    [SerializeField] private GameObject itemPrefab;
    [SerializeField] private bool hasEconomicValue = true;
    [SerializeField] private int baseValue = 1;

    // Public Properties
    public string MaterialName => materialName;
    public int MaterialId => materialId;
    public Material Material => material;
    public Color EditorColor => editorColor;
    public int CoveragePercentage => coveragePercentage;
    public float Durability => durability;
    public GameObject ItemPrefab => itemPrefab;
    public bool HasEconomicValue => hasEconomicValue;
    public int BaseValue => baseValue;

    void OnValidate()
    {
        if (durability <= 0)
        {
            durability = 1;
        }

        if (baseValue <= 0)
        {
            baseValue = 1;
        }

        if (coveragePercentage < 0) coveragePercentage = 0;
        if (coveragePercentage > 100) coveragePercentage = 100;
    }

    public override int GetHashCode()
    {
        return materialId.GetHashCode();
    }

    public override bool Equals(object obj)
    {
        if (obj is MaterialSO other)
        {
            return materialId == other.materialId;
        }
        return false;
    }
}

// ScriptRole: Defines the properties and behavior of a mineable material type
// Dependencies: None
// HandlesEvents: None
// TriggersEvents: None
// UsesSO: None
// NeedsSetup: Set materialId (unique), material, configure rarity and durability values 