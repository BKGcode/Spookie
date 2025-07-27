using UnityEngine;
using System.Linq;
using System.Collections.Generic;

/// <summary>
/// Handles the distribution of materials using a "tile budget" system based on percentages.
/// </summary>
public class RarityDistributor
{
    private TerrainData terrainData;
    private MaterialDatabase materialDatabase;
    private int seed;

    public RarityDistributor(TerrainData data, MaterialDatabase database, int generationSeed)
    {
        terrainData = data;
        materialDatabase = database;
        seed = generationSeed;
    }

    /// <summary>
    /// Distributes materials based on their defined coverage percentage.
    /// </summary>
    public void DistributeAllMaterials(MaterialSO baseMaterialToExclude)
    {
        var materialsToDistribute = materialDatabase.GetCoverageMaterials();

        int totalTiles = terrainData.Width * terrainData.Height;
        float totalPercentage = materialsToDistribute.Sum(m => m.CoveragePercentage);

        if (totalPercentage > 100)
        {
            Debug.LogWarning($"RarityDistributor: La suma de porcentajes ({totalPercentage}%) excede el 100%. Algunos materiales podrían no generarse completamente.");
        }

        List<Vector2Int> availablePositions = GetShuffledPositions();

        foreach (var material in materialsToDistribute)
        {
            int tilesToPlace = Mathf.FloorToInt(totalTiles * (material.CoveragePercentage / 100f));
            
            if (availablePositions.Count < tilesToPlace)
            {
                Debug.LogWarning($"RarityDistributor: No hay suficientes tiles disponibles para {material.MaterialName}. Faltan {tilesToPlace - availablePositions.Count} tiles.");
                tilesToPlace = availablePositions.Count;
            }

            PlaceMaterialVeins(material, tilesToPlace, ref availablePositions);
        }
        
    }

    private void PlaceMaterialVeins(MaterialSO material, int tilesToPlace, ref List<Vector2Int> availablePositions)
    {
        System.Random random = new System.Random(seed + material.MaterialId);
        int placedCount = 0;

        while (placedCount < tilesToPlace && availablePositions.Count > 0)
        {
            // Start a new vein from a random available position
            int startIndex = availablePositions.Count - 1;
            Vector2Int startPos = availablePositions[startIndex];
            availablePositions.RemoveAt(startIndex);

            terrainData.SetTile(startPos.x, startPos.y, new TerrainTile(material));
            placedCount++;

            // Expand the vein with a random walk
            Vector2Int currentPos = startPos;
            int veinSize = random.Next(5, 20); // Each vein will have a random size

            for (int i = 0; i < veinSize && placedCount < tilesToPlace && availablePositions.Count > 0; i++)
            {
                var neighbors = GetShuffledNeighbors(currentPos, random);
                bool foundSpot = false;
                foreach (var neighbor in neighbors)
                {
                    if (availablePositions.Contains(neighbor))
                    {
                        currentPos = neighbor;
                        terrainData.SetTile(currentPos.x, currentPos.y, new TerrainTile(material));
                        placedCount++;
                        availablePositions.Remove(currentPos);
                        foundSpot = true;
                        break;
                    }
                }
                if (!foundSpot) break; // No available neighbors, end this vein
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
        System.Random random = new System.Random(seed);
        for (int i = positions.Count - 1; i > 1; i--)
        {
            int j = random.Next(i + 1);
            var temp = positions[j];
            positions[j] = positions[i];
            positions[i] = temp;
        }
        return positions;
    }
    
    private List<Vector2Int> GetShuffledNeighbors(Vector2Int pos, System.Random random)
    {
        var neighbors = new List<Vector2Int>
        {
            new Vector2Int(pos.x + 1, pos.y),
            new Vector2Int(pos.x - 1, pos.y),
            new Vector2Int(pos.x, pos.y + 1),
            new Vector2Int(pos.x, pos.y - 1)
        };

        return neighbors.OrderBy(p => random.Next()).ToList();
    }
}


// ScriptRole: Manages the placement of materials on the terrain grid according to their rarity.
// Dependencies: TerrainData, MaterialDatabase, MaterialSO
// HandlesEvents: None
// TriggersEvents: None
// UsesSO: MaterialSO (indirectly via MaterialDatabase)
// NeedsSetup: Instantiated by TerrainGenerator with required data. 