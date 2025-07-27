using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Handles the visual representation of the terrain by generating and managing meshes.
/// </summary>
[RequireComponent(typeof(TerrainGenerator))]
public class TerrainRenderer : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private TerrainGenerator terrainGenerator;
    
    private Dictionary<Material, List<CombineInstance>> cubeMeshesToCombine = new Dictionary<Material, List<CombineInstance>>();
    private Dictionary<Material, List<CombineInstance>> floorMeshesToCombine = new Dictionary<Material, List<CombineInstance>>();
    private Dictionary<Material, List<CombineInstance>> grassMeshesToCombine = new Dictionary<Material, List<CombineInstance>>();

    void OnValidate()
    {
        if (terrainGenerator == null)
        {
            terrainGenerator = GetComponent<TerrainGenerator>();
        }
    }

    /// <summary>
    /// Public method to render the terrain. This is now called directly by TerrainManager.
    /// </summary>
    public void RenderTerrain(TerrainData data, GrassDatabaseSO grassDatabase)
    {
        ClearExistingMeshes();
        GenerateAndCombineMeshes(data);
        GenerateGrassSurface(data, grassDatabase);
        DrawSpecialProperties(data);
    }
    
    private void ClearExistingMeshes()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        cubeMeshesToCombine.Clear();
        floorMeshesToCombine.Clear();
        grassMeshesToCombine.Clear();
    }
    
    private void GenerateAndCombineMeshes(TerrainData data)
    {
        // First, generate all individual mesh instances
        for (int y = 0; y < data.Height; y++)
        {
            for (int x = 0; x < data.Width; x++)
            {
                TerrainTile currentTile = data.GetTile(x, y);
                if (currentTile.GetMiningState() == MiningState.Mined)
                {
                    CreateFloorMeshInstance(x, y, currentTile.GetMaterial());
                }
                else
                {
                    CreateCubeMeshInstance(x, y, data);
                }
            }
        }
        
        // Now, combine them into single meshes for performance
        CombineMeshes();
    }

    private void GenerateGrassSurface(TerrainData data, GrassDatabaseSO grassDatabase)
    {
        if (grassDatabase == null)
        {
            Debug.LogWarning("GrassDatabase is not assigned in TerrainManager. Skipping grass generation.");
            return;
        }

        if (grassDatabase.GrassTypes.Count == 0)
        {
            Debug.LogWarning("GrassDatabase is assigned, but its 'GrassTypes' list is empty. Skipping grass generation.", grassDatabase);
            return;
        }

        Debug.Log("TerrainRenderer: Generating grass surface.");

        // Use a noise function for natural-looking patches.
        // The offsets are derived from the map seed to ensure the grass pattern is deterministic.
        System.Random prng = new System.Random(data.MapSeed);
        float noiseScale = 0.1f;
        float noiseOffsetX = prng.Next(0, 10000);
        float noiseOffsetY = prng.Next(0, 10000);
        
        for (int y = 0; y < data.Height; y++)
        {
            for (int x = 0; x < data.Width; x++)
            {
                if (data.GetTile(x, y).GetMiningState() != MiningState.Mined)
                {
                    // Use Perlin noise to select a grass type
                    float noiseValue = Mathf.PerlinNoise((x + noiseOffsetX) * noiseScale, (y + noiseOffsetY) * noiseScale);
                    int grassIndex = Mathf.FloorToInt(noiseValue * grassDatabase.GrassTypes.Count);
                    grassIndex = Mathf.Clamp(grassIndex, 0, grassDatabase.GrassTypes.Count - 1);
                    
                    GrassTypeSO grassType = grassDatabase.GrassTypes[grassIndex];
                    CreateGrassQuadInstance(x, y, grassType);
                }
            }
        }
        CombineGrassMeshes();
    }

    private void CreateGrassQuadInstance(int x, int y, GrassTypeSO grassType)
    {
        if (grassType == null || grassType.Material == null) return;
        
        // This quad will be the top surface of our cubes
        Mesh grassQuad = CreateQuadMesh(Vector3.up * 0.5f);
        
        CombineInstance combine = new CombineInstance();
        combine.mesh = grassQuad;
        combine.transform = Matrix4x4.TRS(new Vector3(x, 0, y), Quaternion.identity, Vector3.one);

        if (!grassMeshesToCombine.ContainsKey(grassType.Material))
        {
            grassMeshesToCombine[grassType.Material] = new List<CombineInstance>();
        }
        grassMeshesToCombine[grassType.Material].Add(combine);
    }
    
    private void CreateCubeMeshInstance(int x, int y, TerrainData data)
    {
        TerrainTile tile = data.GetTile(x, y);
        MaterialSO materialSO = tile.GetMaterial();
        if (materialSO == null || materialSO.Material == null) return;
        
        Mesh cubeMesh = CreateCubeMeshWithFaceCulling(x, y, data);
        
        CombineInstance combine = new CombineInstance();
        combine.mesh = cubeMesh;
        combine.transform = Matrix4x4.TRS(new Vector3(x, 0, y), Quaternion.identity, Vector3.one);

        if (!cubeMeshesToCombine.ContainsKey(materialSO.Material))
        {
            cubeMeshesToCombine[materialSO.Material] = new List<CombineInstance>();
        }
        cubeMeshesToCombine[materialSO.Material].Add(combine);
    }
    
    private void CreateFloorMeshInstance(int x, int y, MaterialSO materialSO)
    {
        if (materialSO == null || materialSO.Material == null) return;

        Mesh floorMesh = CreateQuadMesh(Vector3.down * 0.5f); // Position floor slightly down
        
        CombineInstance combine = new CombineInstance();
        combine.mesh = floorMesh;
        combine.transform = Matrix4x4.TRS(new Vector3(x, 0, y), Quaternion.identity, Vector3.one);

        if (!floorMeshesToCombine.ContainsKey(materialSO.Material))
        {
            floorMeshesToCombine[materialSO.Material] = new List<CombineInstance>();
        }
        floorMeshesToCombine[materialSO.Material].Add(combine);
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

    private void CombineMeshes()
    {
        foreach (var pair in cubeMeshesToCombine)
        {
            GameObject combinedObject = new GameObject("TerrainMesh_Cubes_" + pair.Key.name);
            combinedObject.transform.parent = transform;
            var filter = combinedObject.AddComponent<MeshFilter>();
            var renderer = combinedObject.AddComponent<MeshRenderer>();
            
            renderer.material = pair.Key;
            
            Mesh combinedMesh = new Mesh();
            combinedMesh.CombineMeshes(pair.Value.ToArray(), true, true);
            filter.mesh = combinedMesh;
        }
        
        foreach (var pair in floorMeshesToCombine)
        {
            GameObject combinedObject = new GameObject("TerrainMesh_Floors_" + pair.Key.name);
            combinedObject.transform.parent = transform;
            var filter = combinedObject.AddComponent<MeshFilter>();
            var renderer = combinedObject.AddComponent<MeshRenderer>();
            
            renderer.material = pair.Key;
            
            Mesh combinedMesh = new Mesh();
            combinedMesh.CombineMeshes(pair.Value.ToArray(), true, true);
            filter.mesh = combinedMesh;
        }
    }

    private void CombineGrassMeshes()
    {
        foreach (var pair in grassMeshesToCombine)
        {
            GameObject combinedObject = new GameObject("TerrainMesh_Grass_" + pair.Key.name);
            combinedObject.transform.parent = transform;
            var filter = combinedObject.AddComponent<MeshFilter>();
            var renderer = combinedObject.AddComponent<MeshRenderer>();
            
            renderer.material = pair.Key;
            
            Mesh combinedMesh = new Mesh();
            combinedMesh.CombineMeshes(pair.Value.ToArray(), true, true);
            filter.mesh = combinedMesh;
        }
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

// ScriptRole: Renders the entire terrain by generating and combining meshes for performance.
// Dependencies: TerrainGenerator, TerrainData, MaterialSO, GrassDatabaseSO
// HandlesEvents: None
// TriggersEvents: None
// UsesSO: MaterialSO (via TerrainData), GrassTypeSO (via GrassDatabase)
// NeedsSetup: Attach to the same GameObject as TerrainGenerator. It's called by TerrainManager. 