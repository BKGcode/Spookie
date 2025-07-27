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
    /// Gets a list of all unique materials contained in this database (Base, Procedural, and Special).
    /// </summary>
    public List<MaterialSO> GetAllMaterials()
    {
        var allMaterials = new HashSet<MaterialSO>();
        
        if (baseMaterial != null)
        {
            allMaterials.Add(baseMaterial);
        }
        if (proceduralMaterials != null)
        {
            foreach (var material in proceduralMaterials)
            {
                if(material != null) allMaterials.Add(material);
            }
        }
        if (specialMaterials != null)
        {
            foreach (var material in specialMaterials)
            {
                if(material != null) allMaterials.Add(material);
            }
        }
        return allMaterials.ToList();
    }
    
    /// <summary>
    /// Gets a material from the database by its unique name.
    /// </summary>
    public MaterialSO GetMaterialByName(string materialName)
    {
        if (string.IsNullOrEmpty(materialName)) return null;
        return GetAllMaterials().FirstOrDefault(m => m != null && m.MaterialName == materialName);
    }

    /// <summary>
    /// Gets a material from the database by its unique GUID.
    /// </summary>
    public MaterialSO GetMaterialByGuid(string guid)
    {
        if (string.IsNullOrEmpty(guid)) return null;
        return GetAllMaterials().FirstOrDefault(m => m != null && m.Guid == guid);
    }
} 