using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapGenerator : MonoBehaviour
{
    [Header("Map Settings")]
    [SerializeField] private int mapWidth = 100;
    [SerializeField] private int mapHeight = 100;
    [SerializeField] private float noiseScale = 0.1f;
    [SerializeField] private int campSize = 6;

    [Header("Tilemaps")]
    [SerializeField] private Tilemap backgroundLayer;
    [SerializeField] private Tilemap foregroundLayer;
    [SerializeField] private TileBase foregroundTile; // A single RuleTile for all foreground tiles

    [Header("Tile Types")]
    [SerializeField] private List<TileTypeMapping> tileTypes;

    private MapDataManager mapDataManager;

    private void Awake()
    {
        mapDataManager = GetComponent<MapDataManager>();
        if (mapDataManager == null)
        {
            Debug.LogError("MapDataManager component not found on the same GameObject!");
        }
    }

    void Start()
    {
        GenerateMap();
    }

    private void GenerateMap()
    {
        Debug.Log("Starting map generation...");
        Dictionary<Vector2Int, TileState> generatedTiles = new Dictionary<Vector2Int, TileState>();
        float offsetX = Random.Range(0f, 9999f);
        float offsetY = Random.Range(0f, 9999f);
        
        int campStartX = (mapWidth - campSize) / 2;
        int campStartY = (mapHeight - campSize) / 2;

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
                    foregroundLayer.SetTile(position, foregroundTile);
                    generatedTiles.Add((Vector2Int)position, new TileState(selectedTileData));
                }
            }
        }
        
        mapDataManager.InitializeMapData(generatedTiles);
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