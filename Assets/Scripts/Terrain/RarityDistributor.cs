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
    /// Distributes materials based on their defined coverage percentage.
    /// </summary>
    public void DistributeAllMaterials(MaterialSO baseMaterial)
    {
        Debug.Log("RarityDistributor: Distributing materials...");

        var materialsToDistribute = materialDatabase.GetCoverageMaterials()
            .OrderByDescending(m => m.CoveragePercentage)
            .ToList();

        int totalTiles = terrainData.Width * terrainData.Height;
        foreach (var material in materialsToDistribute)
        {
            int requiredTiles = (int)(totalTiles * (material.CoveragePercentage / 100f));
            for (int i = 0; i < requiredTiles; i++)
            {
                if (availableCoords.Count == 0)
                {
                    Debug.LogWarning($"RarityDistributor: Ran out of available coordinates while placing {material.MaterialName}.");
                    break;
                }

                int randomIndex = noise.GetNext(0, availableCoords.Count);
                Vector2Int coord = availableCoords[randomIndex];
                availableCoords.RemoveAt(randomIndex); // Ensure this coordinate is not picked again

                // Only place the new material if the tile is still the base material
                if (terrainData.GetTile(coord.x, coord.y).GetMaterial() == baseMaterial)
                {
                    terrainData.SetTile(coord.x, coord.y, new TerrainTile(material));
                }
            }
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
        for (int i = positions.Count - 1; i > 0; i--)
        {
            int j = noise.GetNext(0, i + 1);
            Vector2Int temp = positions[i];
            positions[i] = positions[j];
            positions[j] = temp;
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