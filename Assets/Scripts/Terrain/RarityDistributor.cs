using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using SO;
using Terrain.Data;

namespace Terrain
{
    /// <summary>
    /// Handles the distribution of materials using a "tile budget" system based on percentages.
    /// </summary>
    public class RarityDistributor
    {
        private TerrainData terrainData;
        private MaterialDatabase materialDatabase;
        private int seed;
        private NoiseGenerator noise;
        private List<Vector2Int> availableCoords;

        public RarityDistributor(TerrainData data, MaterialDatabase database, int generationSeed)
        {
            terrainData = data;
            materialDatabase = database;
            seed = generationSeed;
            if (terrainData == null || materialDatabase == null)
            {
                Debug.LogError("RarityDistributor: TerrainData or MaterialDatabase is null.");
                return;
            }
            this.noise = new NoiseGenerator(seed);
            this.availableCoords = GetShuffledPositions();
        }

        /// <summary>
        /// Distributes materials based on their defined coverage percentage by iterating through a pre-shuffled list of coordinates.
        /// </summary>
        public void DistributeAllMaterials(MaterialSO baseMaterial)
        {
            Debug.Log("RarityDistributor: Distributing materials...");

            var materialsToDistribute = materialDatabase.GetCoverageMaterials();
            int totalTiles = terrainData.Width * terrainData.Height;
            int coordIndex = 0;

            foreach (var material in materialsToDistribute)
            {
                int requiredTiles = (int)(totalTiles * (material.CoveragePercentage / 100f));
                int placedCount = 0;
                
                while (placedCount < requiredTiles && coordIndex < availableCoords.Count)
                {
                    Vector2Int coord = availableCoords[coordIndex];
                    
                    // We check if the tile is still the base material to avoid overwriting other procedural materials.
                    if (terrainData.GetTile(coord.x, coord.y).GetMaterial() == baseMaterial)
                    {
                        terrainData.SetTile(coord.x, coord.y, new TerrainTile(material));
                        placedCount++;
                    }
                    coordIndex++;
                }

                if (placedCount < requiredTiles)
                {
                    Debug.LogWarning($"RarityDistributor: Could only place {placedCount}/{requiredTiles} tiles for {material.MaterialName}. The map might be full or percentages might exceed 100%.");
                }
            }
        }

        private List<Vector2Int> GetShuffledPositions()
        {
            List<Vector2Int> positions = new List<Vector2Int>();
            for (int y = 0; y < terrainData.Height; y++)
            {
                for (int x = 0; x < terrainData.Width; x++)
                {
                    positions.Add(new Vector2Int(x, y));
                }
            }

            // Fisher-Yates shuffle
            for (int i = positions.Count - 1; i > 0; i--)
            {
                int j = noise.GetNext(0, i + 1);
                Vector2Int temp = positions[i];
                positions[i] = positions[j];
                positions[j] = temp;
            }

            return positions;
        }
        
    }
}


// ScriptRole: Manages the placement of materials on the terrain grid according to their rarity.
// Dependencies: TerrainData, MaterialDatabase, MaterialSO
// HandlesEvents: None
// TriggersEvents: None
// UsesSO: MaterialSO (indirectly via MaterialDatabase)
// NeedsSetup: Instantiated by TerrainGenerator with required data.