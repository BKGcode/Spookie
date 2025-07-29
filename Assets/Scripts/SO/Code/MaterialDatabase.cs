using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// A ScriptableObject that holds all available material types for the game.
/// </summary>
[CreateAssetMenu(fileName = "MaterialDatabase", menuName = "Spookie/Terrain/Material Database")]
public class MaterialDatabase : ScriptableObject
{
    [Header("Core Materials")]
    [Tooltip("The default material that fills the map before others are placed. This material does not need a Coverage Percentage.")]
    [SerializeField] private MaterialSO baseMaterial;

    [Tooltip("List of all materials that will be placed procedurally on top of the base material, according to their Coverage Percentage.")]
    [SerializeField] private List<MaterialSO> proceduralMaterials;

    [Header("Other Materials")]
    [Tooltip("List of special materials not used in procedural coverage (e.g., for quest items, manual painting).")]
    [SerializeField] private List<MaterialSO> specialMaterials;

    private Dictionary<string, MaterialSO> materialCacheByGuid;
    private Dictionary<string, MaterialSO> materialCacheByName;
    private List<MaterialSO> allMaterialsCache;

    private void OnEnable()
    {
        BuildCache();
    }

    private void BuildCache()
    {
        var allMaterials = new HashSet<MaterialSO>();
        if (baseMaterial != null) allMaterials.Add(baseMaterial);
        if (proceduralMaterials != null) allMaterials.UnionWith(proceduralMaterials.Where(m => m != null));
        if (specialMaterials != null) allMaterials.UnionWith(specialMaterials.Where(m => m != null));

        allMaterialsCache = allMaterials.ToList();

        materialCacheByGuid = new Dictionary<string, MaterialSO>();
        materialCacheByName = new Dictionary<string, MaterialSO>();

        foreach (var material in allMaterialsCache)
        {
            if (!string.IsNullOrEmpty(material.Guid) && !materialCacheByGuid.ContainsKey(material.Guid))
            {
                materialCacheByGuid.Add(material.Guid, material);
            }
            if (!string.IsNullOrEmpty(material.MaterialName) && !materialCacheByName.ContainsKey(material.MaterialName))
            {
                materialCacheByName.Add(material.MaterialName, material);
            }
        }
        Debug.Log("MaterialDatabase cache built.", this);
    }

    /// <summary>
    /// Gets the explicitly defined base material.
    /// </summary>
    public MaterialSO GetBaseMaterial()
    {
        if (baseMaterial == null)
        {
            Debug.LogError("MaterialDatabase: No Base Material assigned.", this);
        }
        return baseMaterial;
    }

    /// <summary>
    /// Returns all procedural materials intended for percentage-based coverage.
    /// </summary>
    public List<MaterialSO> GetCoverageMaterials()
    {
        if (proceduralMaterials == null)
        {
            return new List<MaterialSO>();
        }
        // Return a copy of the list containing only valid materials with coverage > 0
        return proceduralMaterials.Where(m => m != null && m.CoveragePercentage > 0).ToList();
    }
    
    /// <summary>
    /// Gets a cached list of all unique materials in this database.
    /// </summary>
    public List<MaterialSO> GetAllMaterials()
    {
        if (allMaterialsCache == null) BuildCache();
        return allMaterialsCache;
    }
    
    /// <summary>
    /// Gets a material from the cache by its unique name.
    /// </summary>
    public MaterialSO GetMaterialByName(string materialName)
    {
        if (string.IsNullOrEmpty(materialName)) return null;
        if (materialCacheByName == null) BuildCache();
        
        if (materialCacheByName.TryGetValue(materialName, out var material))
        {
            return material;
        }
        Debug.LogWarning($"MaterialDatabase: Material with name '{materialName}' not found in cache.", this);
        return null;
    }

    /// <summary>
    /// Gets a material from the cache by its unique GUID.
    /// </summary>
    public MaterialSO GetMaterialByGuid(string guid)
    {
        if (string.IsNullOrEmpty(guid)) return null;
        if (materialCacheByGuid == null) BuildCache();

        if (materialCacheByGuid.TryGetValue(guid, out var material))
        {
            return material;
        }
        Debug.LogWarning($"MaterialDatabase: Material with GUID '{guid}' not found in cache.", this);
        return null;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        // Rebuild cache if lists are changed in the inspector while the game is running.
        if (Application.isPlaying)
        {
            BuildCache();
        }
    }
#endif
}

// ScriptRole: Holds all available material types and provides efficient access via a cached lookup.
// Dependencies: None
// HandlesEvents: None
// TriggersEvents: None
// UsesSO: MaterialSO
// NeedsSetup: Create one instance in the Project. Populate the material lists. Base material is mandatory. 