using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using SO;

namespace Terrain
{
    public class TerrainGenerator
    {
        private int mapWidth = 100;
        private int mapHeight = 100;
        private int seed = 0;
        
        private MaterialDatabase materialDatabase;
        
        private TerrainData currentTerrainData;

        public TerrainGenerator(MaterialDatabase db, int width, int height)
        {
            this.materialDatabase = db;
            this.mapWidth = width;
            this.mapHeight = height;
        }
        
        public TerrainData GenerateTerrain(int? specificSeed = null)
        {
            if (materialDatabase == null)
            {
                Debug.LogError("TerrainGenerator: MaterialDatabase is not assigned.");
                return null;
            }

            seed = specificSeed ?? this.seed;
            Debug.Log($"TerrainGenerator: Starting terrain generation with specific seed {seed}.");
            
            currentTerrainData = new TerrainData(mapWidth, mapHeight, seed);
            
            GenerateBaseMaterials();
            ApplySpecialMaterials();
            PostProcess();

            LogMaterialDistribution();

            Debug.Log("TerrainGenerator: Terrain generation complete.");
            TerrainEvents.OnTerrainGenerated?.Invoke(currentTerrainData);
            
            return currentTerrainData;
        }

        public TerrainData GenerateTerrain()
        {
            return GenerateTerrain(this.seed);
        }
        
        private void GenerateBaseMaterials()
        {
            Debug.Log("TerrainGenerator: Phase 1 - Generating base materials.");
            MaterialSO baseMaterial = materialDatabase.GetBaseMaterial();
            if (baseMaterial == null)
            {
                Debug.LogError("TerrainGenerator: No se pudo obtener el material base de la base de datos. Asigna uno en el MaterialDatabase.");
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
            if (baseMaterial == null) return;
            
            RarityDistributor distributor = new RarityDistributor(currentTerrainData, materialDatabase, seed);
            distributor.DistributeAllMaterials(baseMaterial);
        }
        
        private void PostProcess()
        {
            Debug.Log("TerrainGenerator: Phase 3 - Post-processing.");
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
        
        public TerrainData GetCurrentTerrainData()
        {
            return currentTerrainData;
        }
    }
}

// ScriptRole: Orchestrates the entire procedural terrain generation process.
// Dependencies: MaterialDatabase, RarityDistributor, TerrainData
// TriggersEvents: TerrainEvents.OnTerrainGenerated
// UsesSO: MaterialDatabase
