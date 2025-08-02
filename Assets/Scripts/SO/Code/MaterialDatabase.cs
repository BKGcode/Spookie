using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace SO
{
    [CreateAssetMenu(fileName = "MaterialDatabase", menuName = "Spookie/Terrain/Material Database")]
    public class MaterialDatabase : ScriptableObject
    {
        [Header("Core Materials")]
        [SerializeField] private MaterialSO baseMaterial;
        [SerializeField] private List<MaterialSO> proceduralMaterials;

        [Header("Other Materials")]
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

        public MaterialSO GetBaseMaterial()
        {
            if (baseMaterial == null)
            {
                Debug.LogError("MaterialDatabase: No Base Material assigned.", this);
            }
            return baseMaterial;
        }

        public List<MaterialSO> GetCoverageMaterials()
        {
            if (proceduralMaterials == null)
            {
                return new List<MaterialSO>();
            }
            return proceduralMaterials.Where(m => m != null && m.CoveragePercentage > 0).ToList();
        }
        
        public List<MaterialSO> GetAllMaterials()
        {
            if (allMaterialsCache == null) BuildCache();
            return allMaterialsCache;
        }
        
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
            if (Application.isPlaying)
            {
                BuildCache();
            }
        }
    #endif
    }
}
