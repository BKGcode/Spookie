using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Handles the visual representation of the terrain by generating and managing meshes.
/// </summary>
// The RequireComponent attribute is removed to decouple the renderer from the generator.
public class TerrainRenderer : MonoBehaviour
{
    // This dictionary will hold references to the GameObject of each tile for individual updates.
    private Dictionary<Vector2Int, GameObject> tileObjects = new Dictionary<Vector2Int, GameObject>();

    // These dictionaries for mesh combining are no longer needed and have been removed.
    
    // Noise offsets for deterministic grass generation, initialized once per level load.
    private float grassNoiseOffsetX;
    private float grassNoiseOffsetY;

    void OnValidate()
    {
        // This is no longer needed.
    }

    /// <summary>
    /// Renders the entire terrain from scratch. Called once on level load.
    /// </summary>
    public void RenderTerrain(TerrainData data, GrassDatabaseSO grassDatabase)
    {
        ClearExistingMeshes();
        
        // Initialize noise offsets for this terrain instance to ensure consistent grass patterns.
        System.Random prng = new System.Random(data.MapSeed);
        grassNoiseOffsetX = prng.Next(0, 10000);
        grassNoiseOffsetY = prng.Next(0, 10000);
        
        GenerateTileGameObjects(data, grassDatabase);
        
        // Special properties are drawn once on top of the base terrain.
        DrawSpecialProperties(data);
    }
    
    /// <summary>
    /// Updates the visual representation of a single tile and its direct neighbors.
    /// </summary>
    public void UpdateTileVisual(int x, int y, TerrainData data, GrassDatabaseSO grassDatabase)
    {
        Debug.Log($"TerrainRenderer: Updating visual for tile ({x}, {y}) and its neighbors.");

        // A list of coordinates to update. Start with the target tile.
        List<Vector2Int> coordsToUpdate = new List<Vector2Int> { new Vector2Int(x, y) };

        // Add direct neighbors to the update list, as their mesh faces might change.
        if (data.IsValidPosition(x, y + 1)) coordsToUpdate.Add(new Vector2Int(x, y + 1));
        if (data.IsValidPosition(x, y - 1)) coordsToUpdate.Add(new Vector2Int(x, y - 1));
        if (data.IsValidPosition(x + 1, y)) coordsToUpdate.Add(new Vector2Int(x + 1, y));
        if (data.IsValidPosition(x - 1, y)) coordsToUpdate.Add(new Vector2Int(x - 1, y));

        // Regenerate each affected tile
        foreach (var coord in coordsToUpdate)
        {
            // Remove the old GameObject
            if (tileObjects.TryGetValue(coord, out GameObject oldObj))
            {
                Destroy(oldObj);
                tileObjects.Remove(coord);
            }

            // Create the new mesh and GameObject for the tile
            CreateTileGameObject(coord.x, coord.y, data, grassDatabase);
        }
    }
    
    private void ClearExistingMeshes()
    {
        // Now also clears the tile dictionary
        foreach (var pair in tileObjects)
        {
            Destroy(pair.Value);
        }
        tileObjects.Clear();
        
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        // cubeMeshesToCombine.Clear(); // Removed
        // floorMeshesToCombine.Clear(); // Removed
        // grassMeshesToCombine.Clear(); // Removed
    }
    
    private void GenerateTileGameObjects(TerrainData data, GrassDatabaseSO grassDatabase)
    {
        // This method will now generate individual GameObjects instead of combining them.
        for (int y = 0; y < data.Height; y++)
        {
            for (int x = 0; x < data.Width; x++)
            {
                CreateTileGameObject(x, y, data, grassDatabase);
            }
        }
    }

    /// <summary>
    /// Creates a single GameObject for a tile at the given coordinates.
    /// </summary>
    private void CreateTileGameObject(int x, int y, TerrainData data, GrassDatabaseSO grassDatabase)
    {
        TerrainTile currentTile = data.GetTile(x, y);
        MaterialSO materialSO = currentTile.GetMaterial();
        if (materialSO == null) return;
        
        GameObject tileObj = new GameObject($"Tile_{x}_{y}");
        tileObj.transform.position = new Vector3(x, 0, y);
        tileObj.transform.parent = this.transform;

        // We need a targetable component for player interaction and AI
        tileObj.AddComponent<World.Targetable>();

        if (currentTile.GetMiningState() == MiningState.Mined)
        {
            // This is a floor tile. It needs a thin collider so dwarfs can walk on it.
            Mesh floorMesh = CreateQuadMesh(Vector3.down * 0.5f);
            MeshFilter filter = tileObj.AddComponent<MeshFilter>();
            filter.mesh = floorMesh;
            MeshRenderer renderer = tileObj.AddComponent<MeshRenderer>();
            renderer.material = materialSO.Material;

            // Add a thin BoxCollider to act as the floor
            BoxCollider collider = tileObj.AddComponent<BoxCollider>();
            collider.size = new Vector3(1, 0.1f, 1);
            collider.center = new Vector3(0, -0.5f, 0);
        }
        else
        {
            // This is a solid, mineable block. It needs a collider.
            Mesh cubeMesh = CreateCubeMeshWithFaceCulling(x, y, data);
            MeshFilter filter = tileObj.AddComponent<MeshFilter>();
            filter.mesh = cubeMesh;
            MeshRenderer renderer = tileObj.AddComponent<MeshRenderer>();
            renderer.material = materialSO.Material;
            
            // Add the collider
            BoxCollider collider = tileObj.AddComponent<BoxCollider>();
            collider.size = Vector3.one; // Standard 1x1x1 block

            // Create the grass on top (no collider needed for grass)
            CreateGrassForTile(tileObj, x, y, grassDatabase);
        }
        
        tileObjects[new Vector2Int(x, y)] = tileObj;
    }

    private void CreateGrassForTile(GameObject parentTile, int x, int y, GrassDatabaseSO grassDatabase)
    {
        if (grassDatabase == null || grassDatabase.GrassTypes == null || grassDatabase.GrassTypes.Count == 0) return;

        // Use Perlin noise to select a grass type, ensuring a natural, deterministic pattern
        float noiseScale = 0.1f;
        float noiseValue = Mathf.PerlinNoise((x + grassNoiseOffsetX) * noiseScale, (y + grassNoiseOffsetY) * noiseScale);
        int grassIndex = Mathf.FloorToInt(noiseValue * grassDatabase.GrassTypes.Count);
        grassIndex = Mathf.Clamp(grassIndex, 0, grassDatabase.GrassTypes.Count - 1);
        
        GrassTypeSO grassType = grassDatabase.GrassTypes[grassIndex];
        
        if (grassType != null && grassType.Material != null)
        {
            GameObject grassObj = new GameObject("Grass");
            grassObj.transform.SetParent(parentTile.transform, false);

            Mesh grassQuad = CreateQuadMesh(Vector3.up * 0.5f);
            
            MeshFilter filter = grassObj.AddComponent<MeshFilter>();
            filter.mesh = grassQuad;
            MeshRenderer renderer = grassObj.AddComponent<MeshRenderer>();
            renderer.material = grassType.Material;
        }
    }

    private Mesh CreateCubeMeshWithFaceCulling(int x, int y, TerrainData data)
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>();

        // Note: Top and bottom faces are intentionally omitted for optimization.
        // The top is covered by the grass mesh, and the bottom is not visible.

        // Front Face (Z+)
        if (y == data.Height - 1 || data.GetTile(x, y + 1).GetMiningState() == MiningState.Mined)
        {
            AddFace(Vector3.forward, 0.5f, ref vertices, ref triangles, ref uvs);
        }
        // Back Face (Z-)
        if (y == 0 || data.GetTile(x, y - 1).GetMiningState() == MiningState.Mined)
        {
            AddFace(Vector3.back, 0.5f, ref vertices, ref triangles, ref uvs);
        }
        // Right Face (X+)
        if (x == data.Width - 1 || data.GetTile(x + 1, y).GetMiningState() == MiningState.Mined)
        {
            AddFace(Vector3.right, 0.5f, ref vertices, ref triangles, ref uvs);
        }
        // Left Face (X-)
        if (x == 0 || data.GetTile(x - 1, y).GetMiningState() == MiningState.Mined)
        {
            AddFace(Vector3.left, 0.5f, ref vertices, ref triangles, ref uvs);
        }
        
        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.RecalculateNormals();
        return mesh;
    }
    
    private void AddFace(Vector3 direction, float size, ref List<Vector3> vertices, ref List<int> triangles, ref List<Vector2> uvs)
    {
        int vCount = vertices.Count;
        
        Vector3[] faceVertices = new Vector3[4];
        Vector3 right = new Vector3(direction.y, direction.z, direction.x);
        Vector3 up = Vector3.Cross(direction, right);

        faceVertices[0] = (direction - right - up) * size;
        faceVertices[1] = (direction + right - up) * size;
        faceVertices[2] = (direction + right + up) * size;
        faceVertices[3] = (direction - right + up) * size;
        
        vertices.AddRange(faceVertices);
        
        // Corrected winding order for visible faces from the outside
        triangles.Add(vCount);
        triangles.Add(vCount + 1);
        triangles.Add(vCount + 2);
        triangles.Add(vCount);
        triangles.Add(vCount + 2);
        triangles.Add(vCount + 3);

        uvs.Add(new Vector2(0, 0));
        uvs.Add(new Vector2(1, 0));
        uvs.Add(new Vector2(1, 1));
        uvs.Add(new Vector2(0, 1));
    }
    
    private Mesh CreateQuadMesh(Vector3 offset)
    {
        Mesh mesh = new Mesh();
        mesh.vertices = new Vector3[] {
            new Vector3(-0.5f, 0, -0.5f) + offset,
            new Vector3(0.5f, 0, -0.5f) + offset,
            new Vector3(0.5f, 0, 0.5f) + offset,
            new Vector3(-0.5f, 0, 0.5f) + offset
        };
        mesh.triangles = new int[] { 0, 2, 1, 0, 3, 2 };
        mesh.uv = new Vector2[] { new Vector2(0,0), new Vector2(1,0), new Vector2(1,1), new Vector2(0,1) };
        mesh.RecalculateNormals();
        return mesh;
    }

    private void DrawSpecialProperties(TerrainData data)
    {
        GameObject specialPropertyGroup = new GameObject("TerrainMesh_SpecialProperties");
        specialPropertyGroup.transform.parent = transform;

        for (int y = 0; y < data.Height; y++)
        {
            for (int x = 0; x < data.Width; x++)
            {
                SpaceType type = data.GetTile(x, y).GetSpecialProperty();
                if (type != SpaceType.None && type != SpaceType.Empty)
                {
                    GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    marker.transform.position = new Vector3(x, 1, y); // Position it above the ground
                    marker.transform.parent = specialPropertyGroup.transform;
                    marker.GetComponent<Renderer>().material.color = GetColorForSpaceType(type);
                    marker.name = $"Special_{type}_{x}_{y}";
                }
            }
        }
    }

    private Color GetColorForSpaceType(SpaceType type)
    {
        switch (type)
        {
            // 'Empty' is now represented by the lack of a block, so it doesn't need a colored marker.
            case SpaceType.Treasure: return Color.yellow;
            case SpaceType.Enemy: return Color.red;
            default: return Color.clear;
        }
    }
}

// ScriptRole: Renders the terrain by generating individual meshes, allowing for single-tile updates.
// Dependencies: TerrainData, MaterialSO, GrassDatabaseSO
// HandlesEvents: None
// TriggersEvents: None
// UsesSO: MaterialSO (via TerrainData), GrassTypeSO (via GrassDatabase)
// NeedsSetup: Attach to a GameObject in the scene. It's called by TerrainManager. 