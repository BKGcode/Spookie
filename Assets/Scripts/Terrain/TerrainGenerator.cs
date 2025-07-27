using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Main class responsible for generating the procedural terrain.
/// </summary>
public class TerrainGenerator : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private int mapWidth = 100;
    [SerializeField] private int mapHeight = 100;
    [SerializeField] private int seed = 0;
    [SerializeField] private bool generateOnStart = true;
    
    [Header("Dependencies")]
    [SerializeField] private MaterialDatabase materialDatabase;

    [Header("Events")]
    public UnityEvent<TerrainData> OnTerrainGenerated;
    
    private TerrainData currentTerrainData;
    
    void Start()
    {
        // This is now handled by TerrainManager to avoid double generation
        // if (generateOnStart)
        // {
        //     Debug.Log("TerrainGenerator: Auto-generating terrain on Start.");
        //     GenerateTerrain();
        // }
    }
    
    /// <summary>
    /// Generates a new terrain using a specific seed and the current database.
    /// </summary>
    public void GenerateTerrain(int? specificSeed = null)
    {
        if (materialDatabase == null)
        {
            Debug.LogError("TerrainGenerator: MaterialDatabase is not assigned.", this);
            return;
        }

        seed = specificSeed ?? this.seed; // Use the provided seed or the current seed
        Debug.Log($"TerrainGenerator: Starting terrain generation with specific seed {seed}.");
        
        currentTerrainData = new TerrainData(mapWidth, mapHeight, seed);
        
        GenerateBaseMaterials();
        ApplySpecialMaterials();
        PostProcess();

        LogMaterialDistribution();

        Debug.Log("TerrainGenerator: Terrain generation complete.");
        OnTerrainGenerated?.Invoke(currentTerrainData);
    }

    /// <summary>
    /// Generates a new terrain using the seed from the Inspector.
    /// </summary>
    [ContextMenu("Generate Terrain")]
    public void GenerateTerrain()
    {
        GenerateTerrain(this.seed);
    }
    
    private void GenerateBaseMaterials()
    {
        Debug.Log("TerrainGenerator: Phase 1 - Generating base materials.");
        MaterialSO baseMaterial = materialDatabase.GetBaseMaterial();
        if (baseMaterial == null)
        {
            Debug.LogError("TerrainGenerator: No se pudo obtener el material base de la base de datos. Asigna uno en el MaterialDatabase.", this);
            return;
        }

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                currentTerrainData.SetTile(x, y, new TerrainTile(baseMaterial));
            }
        }
        Debug.Log($"TerrainGenerator: Base material '{baseMaterial.MaterialName}' applied to all tiles.");
    }
    
    private void ApplySpecialMaterials()
    {
        Debug.Log("TerrainGenerator: Phase 2 - Applying special materials.");
        MaterialSO baseMaterial = materialDatabase.GetBaseMaterial();
        if (baseMaterial == null) return; // Error already logged
        
        RarityDistributor distributor = new RarityDistributor(currentTerrainData, materialDatabase, seed);
        distributor.DistributeAllMaterials(baseMaterial);
    }
    
    private void PostProcess()
    {
        Debug.Log("TerrainGenerator: Phase 3 - Post-processing.");
        // Future logic for adding treasures, enemies, etc. can go here.
        // For now, this phase is empty.
        Debug.Log("TerrainGenerator: Post-processing complete (no operations performed).");
    }

    private void LogMaterialDistribution()
    {
        if (currentTerrainData == null || materialDatabase == null)
        {
            Debug.LogWarning("Cannot log material distribution. TerrainData or MaterialDatabase is null.");
            return;
        }

        var materialCounts = new Dictionary<MaterialSO, int>();
        // Initialize counts for all possible materials to ensure they appear in the report
        foreach (var material in materialDatabase.GetAllMaterials())
        {
            if (material != null)
            {
                materialCounts[material] = 0;
            }
        }

        int width = currentTerrainData.Width;
        int height = currentTerrainData.Height;
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                MaterialSO tileMaterial = currentTerrainData.GetTile(x, y).GetMaterial();
                if (tileMaterial != null && materialCounts.ContainsKey(tileMaterial))
                {
                    materialCounts[tileMaterial]++;
                }
            }
        }

        int totalTiles = width * height;
        Debug.Log("--- Material Distribution Report ---");
        
        // Calculate the total configured percentage to find the expected base material percentage
        float totalCoveragePercentage = materialDatabase.GetCoverageMaterials().Sum(m => m.CoveragePercentage);
        
        foreach (var kvp in materialCounts.OrderBy(kv => kv.Key != materialDatabase.GetBaseMaterial()))
        {
            MaterialSO material = kvp.Key;
            int count = kvp.Value;
            float actualPercentage = (float)count / totalTiles * 100f;

            string configString;
            if (material == materialDatabase.GetBaseMaterial())
            {
                float expectedBasePercentage = Mathf.Max(0, 100f - totalCoveragePercentage);
                configString = $"Fills rest (Expected: ~{expectedBasePercentage:F2}%)";
            }
            else
            {
                configString = $"Config: {material.CoveragePercentage}%";
            }

            Debug.Log($"{material.MaterialName}: {configString} | Actual: {actualPercentage:F2}% ({count} / {totalTiles} tiles)");
        }
        Debug.Log("------------------------------------");
    }
    
    /// <summary>
    /// Returns the currently generated terrain data.
    /// </summary>
    public TerrainData GetCurrentTerrainData()
    {
        return currentTerrainData;
    }
}

// ScriptRole: Orchestrates the entire procedural terrain generation process.
// Dependencies: MaterialDatabase, NoiseGenerator, RarityDistributor, TerrainData
// HandlesEvents: None
// TriggersEvents: OnTerrainGenerated (passes the completed TerrainData)
// UsesSO: MaterialDatabase
// NeedsSetup: Assign MaterialDatabase in the Inspector. Set seed and dimensions. 