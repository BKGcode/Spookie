using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapGenerator : MonoBehaviour
{
    public static Vector2Int CampfirePosition { get; private set; }

    [Header("Map Settings")]
    [SerializeField] private int mapWidth = 100;
    public int MapWidth => mapWidth;

    [SerializeField] private int mapHeight = 100;
    public int MapHeight => mapHeight;

    [SerializeField] private float noiseScale = 0.1f;
    [SerializeField] private int campSize = 6;

    [Header("Tilemaps")]
    [SerializeField] private Tilemap backgroundLayer;
    [SerializeField] private Tilemap foregroundLayer;
    [SerializeField] private TileBase foregroundTile; // A single RuleTile for all foreground tiles

    [Header("Tile Types")]
    [SerializeField] private List<TileTypeMapping> tileTypes;

    [SerializeField] private List<TileDataSO> tileDatas;
    private Dictionary<Vector2Int, TileDataSO> tilemapData;
    private Dictionary<Vector2Int, int> tileHealth;
    
    private void Start()
    {
        GenerateMap();
        Pathfinding.Instance.CreateGrid();
    }

    public void GenerateMap()
    {
        Debug.Log("Starting map generation...");
        tilemapData = new Dictionary<Vector2Int, TileDataSO>();
        tileHealth = new Dictionary<Vector2Int, int>();
        float offsetX = Random.Range(0f, 9999f);
        float offsetY = Random.Range(0f, 9999f);
        
        int campStartX = (mapWidth - campSize) / 2;
        int campStartY = (mapHeight - campSize) / 2;

        CampfirePosition = new Vector2Int(campStartX + campSize / 2, campStartY + campSize / 2);

        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                Vector3Int position = new Vector3Int(x, y, 0);
                
                // Check for camp area
                if (x >= campStartX && x < campStartX + campSize && 
                    y >= campStartY && y < campStartY + campSize)
                {
                    backgroundLayer.SetTile(position, GetCampBackgroundTile());
                    foregroundLayer.SetTile(position, null); // Clear foreground for camp
                    continue;
                }

                float perlinValue = Mathf.PerlinNoise((x + offsetX) * noiseScale, (y + offsetY) * noiseScale);
                TileDataSO selectedTileData = GetTileDataForPerlinValue(perlinValue);

                if (selectedTileData != null)
                {
                    backgroundLayer.SetTile(position, selectedTileData.backgroundTile);
                    foregroundLayer.SetTile(position, selectedTileData.foregroundTile);
                    SetTileData((Vector2Int)position, selectedTileData);
                    tileHealth[(Vector2Int)position] = selectedTileData.hardness;
                }
            }
        }
        
        // Ensure Pathfinding grid is created AFTER map generation is complete
        Pathfinding.Instance.CreateGrid();

        Debug.Log("Map generation complete.");
    }

    private TileDataSO GetTileDataForPerlinValue(float value)
    {
        foreach (var mapping in tileTypes)
        {
            if (value <= mapping.perlinThreshold)
            {
                return mapping.tileData;
            }
        }
        return tileTypes.Count > 0 ? tileTypes[tileTypes.Count - 1].tileData : null;
    }

    private TileBase GetCampBackgroundTile()
    {
        // Simple logic: use the background of the most common tile for the camp
        if (tileTypes.Count > 0)
        {
            return tileTypes[0].tileData.backgroundTile;
        }
        return null;
    }

    public Vector2Int GetRandomCampPosition()
    {
        int campStartX = (mapWidth - campSize) / 2;
        int campStartY = (mapHeight - campSize) / 2;
        int x = Random.Range(campStartX, campStartX + campSize);
        int y = Random.Range(campStartY, campStartY + campSize);
        return new Vector2Int(x, y);
    }

    private void SetTileData(Vector2Int position, TileDataSO tileData)
    {
        if (tileData == null) return;
        foregroundLayer.SetTile((Vector3Int)position, tileData.foregroundTile);
        tilemapData[position] = tileData;
    }
    
    public TileDataSO GetTileDataAt(Vector2Int position)
    {
        if (tilemapData.TryGetValue(position, out TileDataSO data))
        {
            return data;
        }
        return null;
    }

    public void DestroyTileAt(Vector2Int position)
    {
        foregroundLayer.SetTile((Vector3Int)position, null);
        tilemapData.Remove(position);
        Debug.Log($"Tile at {position} destroyed and removed from data.");
    }

    public void ApplyDamage(Vector2Int position, int damage)
    {
        if (tileHealth.ContainsKey(position))
        {
            tileHealth[position] -= damage;
            Debug.Log($"Applied {damage} damage to tile at {position}. New health: {tileHealth[position]}");
            if (tileHealth[position] <= 0)
            {
                DestroyTileAt(position);
                GameEvents.ReportTileDestroyed(position);
                tileHealth.Remove(position);
            }
        }
    }
}

[System.Serializable]
public class TileTypeMapping
{
    public string name;
    public TileDataSO tileData;
    [Range(0, 1)]
    public float perlinThreshold;
}


// ScriptRole: Generates the procedural tilemap at the start of the game.
// Dependencies: MapDataManager
// HandlesEvents: None
// TriggersEvents: None
// UsesSO: TileDataSO
// NeedsSetup: Assign Background/Foreground Tilemaps, Foreground RuleTile, and configure TileTypeMappings in the Inspector. 