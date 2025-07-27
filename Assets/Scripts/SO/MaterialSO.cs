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
        Debug.Log($"MaterialSO: OnValidate para {materialName}");
        
        // Validate rarity is between 0-1
        if (coveragePercentage < 0)
        {
            Debug.LogWarning($"MaterialSO ({materialName}): Coverage Percentage no puede ser menor que 0. Ajustando a 0.", this);
            coveragePercentage = 0;
        }
        else if (coveragePercentage > 100)
        {
            Debug.LogWarning($"MaterialSO ({materialName}): Coverage Percentage no puede ser mayor que 100. Ajustando a 100.", this);
            coveragePercentage = 100;
        }
        
        // Validate durability is positive
        if (durability <= 0f)
        {
            Debug.LogWarning($"MaterialSO ({materialName}): Durability debe ser mayor que 0. Ajustando a 1.", this);
            durability = 1f;
        }
        
        // Validate material ID is positive
        if (materialId < 0)
        {
            Debug.LogWarning($"MaterialSO ({materialName}): Material ID debe ser mayor o igual que 0. Ajustando a 0.", this);
            materialId = 0;
        }
        
        // Validate economic value consistency
        if (hasEconomicValue && baseValue <= 0)
        {
            Debug.LogWarning($"MaterialSO ({materialName}): Material con valor económico debe tener baseValue > 0. Ajustando a 1.", this);
            baseValue = 1;
        }
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