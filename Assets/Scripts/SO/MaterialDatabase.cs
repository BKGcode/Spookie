using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(fileName = "MaterialDatabase", menuName = "Spookie/Terrain/Material Database")]
public class MaterialDatabase : ScriptableObject
{
    [Header("Base Layer")]
    [Tooltip("The material that will fill the entire map by default.")]
    [SerializeField] private MaterialSO baseMaterial;
    
    [Header("Coverage Layers")]
    [Tooltip("List of materials that will be placed on top of the base material according to their coverage percentage.")]
    [SerializeField] private List<MaterialSO> coverageMaterials = new List<MaterialSO>();
    
    private Dictionary<int, MaterialSO> materialLookup;

    private void OnEnable()
    {
        BuildLookupCache();
    }

    private void BuildLookupCache()
    {
        materialLookup = new Dictionary<int, MaterialSO>();
        
        // Add base material to lookup
        if (baseMaterial != null)
        {
            if (!materialLookup.ContainsKey(baseMaterial.MaterialId))
            {
                materialLookup.Add(baseMaterial.MaterialId, baseMaterial);
            }
            else
            {
                Debug.LogError($"MaterialDatabase: ID duplicado entre el material base y la lista de cobertura: {baseMaterial.MaterialId}", this);
            }
        }

        // Add coverage materials to lookup
        if (coverageMaterials != null)
        {
            foreach (var material in coverageMaterials)
            {
                if (material == null)
                {
                    Debug.LogWarning("MaterialDatabase: Material nulo encontrado en la lista de cobertura.", this);
                    continue;
                }
                
                if (materialLookup.ContainsKey(material.MaterialId))
                {
                    Debug.LogError($"MaterialDatabase: ID duplicado encontrado en la lista de cobertura: {material.MaterialId} ({material.MaterialName}). Ignorando duplicado.", this);
                    continue;
                }
                
                materialLookup.Add(material.MaterialId, material);
            }
        }
    }

    public MaterialSO GetMaterialById(int id)
    {
        if (materialLookup == null) BuildLookupCache();
        
        if (materialLookup.TryGetValue(id, out MaterialSO material))
        {
            return material;
        }
        
        Debug.LogWarning($"MaterialDatabase: Material con ID {id} no encontrado.", this);
        return null;
    }

    public List<MaterialSO> GetAllMaterials()
    {
        var allMaterials = new List<MaterialSO>();
        if (baseMaterial != null) allMaterials.Add(baseMaterial);
        if (coverageMaterials != null) allMaterials.AddRange(coverageMaterials);
        return allMaterials?.Where(m => m != null).ToList() ?? new List<MaterialSO>();
    }

    public List<MaterialSO> GetCoverageMaterials()
    {
        return coverageMaterials?.Where(m => m != null).ToList() ?? new List<MaterialSO>();
    }

    public bool HasMaterialWithId(int id)
    {
        if (materialLookup == null) BuildLookupCache();
        return materialLookup.ContainsKey(id);
    }

    public int GetMaterialCount()
    {
        return materialLookup?.Count ?? 0;
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        var allMaterials = new List<MaterialSO>();
        if (baseMaterial != null) allMaterials.Add(baseMaterial);
        if (coverageMaterials != null) allMaterials.AddRange(coverageMaterials);
        
        if (allMaterials.Count > 0)
        {
            var ids = new HashSet<int>();
            foreach (var material in allMaterials)
            {
                if (material == null) continue;
                
                if (!ids.Add(material.MaterialId))
                {
                    Debug.LogError($"MaterialDatabase: ID duplicado detectado: {material.MaterialId} ({material.MaterialName})", this);
                }
            }
        }
        
        if (Application.isPlaying)
        {
            BuildLookupCache();
        }
    }

    [ContextMenu("Rebuild Cache")]
    public void RebuildCache()
    {
        BuildLookupCache();
    }
#endif
    
    public MaterialSO GetBaseMaterial()
    {
        if (baseMaterial == null)
        {
            Debug.LogError("MaterialDatabase: No se ha asignado un material base en el Inspector.", this);
            return null;
        }
        return baseMaterial;
    }
} 