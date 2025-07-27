using UnityEngine;
using System.Collections;

/// <summary>
/// Singleton manager that coordinates all terrain-related systems.
/// Provides a public API for interacting with the terrain.
/// </summary>
public class TerrainManager : MonoBehaviour
{
    public static TerrainManager Instance { get; private set; }
    
    [Header("Level Data")]
    [Tooltip("The terrain level asset to load when the game starts.")]
    [SerializeField] private TerrainDataAsset levelToLoad;

    [Header("System Dependencies")]
    [SerializeField] private MaterialDatabase materialDatabase;
    [SerializeField] private FeedbackMessagesSO feedbackMessages;
    [SerializeField] private GrassDatabaseSO grassDatabase;

    // Internal component references
    private TerrainGenerator terrainGenerator; // Still useful for creating new terrains in editor
    private TerrainRenderer terrainRenderer;
    private TerrainData currentTerrainData;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        terrainGenerator = GetComponent<TerrainGenerator>();
        terrainRenderer = GetComponent<TerrainRenderer>();
    }

    void Start()
    {
        if (levelToLoad == null)
        {
            Debug.LogError("TerrainManager: No TerrainDataAsset assigned to 'levelToLoad'. Cannot start game.", this);
            enabled = false;
            return;
        }
        
        LoadTerrainFromAsset(levelToLoad);
        
        if (currentTerrainData != null && terrainRenderer != null)
        {
            terrainRenderer.RenderTerrain(currentTerrainData, grassDatabase);
        }
    }

    private void LoadTerrainFromAsset(TerrainDataAsset asset)
    {
        Debug.Log($"TerrainManager: Loading level '{asset.name}'.");
        
        var saveData = asset.LevelData;
        if (saveData == null)
        {
            Debug.LogError($"Asset '{asset.name}' has no level data.", this);
            return;
        }

        currentTerrainData = new TerrainData(saveData.Width, saveData.Height, saveData.MapSeed);
        
        for (int i = 0; i < saveData.AllTiles.Count; i++)
        {
            var savedTile = saveData.AllTiles[i];
            int x = i % saveData.Width;
            int y = i / saveData.Width;

            MaterialSO material = materialDatabase.GetMaterialByGuid(savedTile.MaterialGuid);
            if (material == null && !string.IsNullOrEmpty(savedTile.MaterialGuid))
            {
                Debug.LogWarning($"Could not find material with GUID '{savedTile.MaterialGuid}'. Using base material.");
                material = materialDatabase.GetBaseMaterial();
            }

            TerrainTile newTile = new TerrainTile(material);
            newTile.SetMiningState(savedTile.MiningState);
            newTile.SetSpecialProperty(savedTile.SpecialProperty);
            
            currentTerrainData.SetTile(x, y, newTile);
        }
        
        Debug.Log("TerrainManager: Level loaded successfully.");
    }
    
    #region Public API

    /// <summary>
    /// Gets the material at a specific world coordinate.
    /// </summary>
    /// <returns>The MaterialSO at the position, or null if invalid.</returns>
    public MaterialSO GetMaterialAt(int x, int y)
    {
        if (!IsValidPosition(x, y)) return null;
        return currentTerrainData.GetTile(x, y).GetMaterial();
    }
    
    /// <summary>
    /// Processes a mining action at a specific world coordinate.
    /// </summary>
    public void MineTileAt(int x, int y)
    {
        if (!IsValidPosition(x, y))
        {
            Debug.LogWarning(feedbackMessages.GetMessage("mining_invalid_position"));
            return;
        }
        
        TerrainTile tile = currentTerrainData.GetTile(x, y);
        if (!tile.IsMineable())
        {
            Debug.Log(feedbackMessages.GetMessage("mining_not_mineable"));
            return;
        }

        // For now, we'll assume one-hit mining. Durability will be handled later.
        tile.SetMiningState(MiningState.Mined);
        currentTerrainData.SetTile(x, y, tile);
        
        Debug.Log($"TerrainManager: Tile at ({x},{y}) was mined.");

        // Trigger events
        var eventData = new TileMinedEventData { x = x, y = y, material = tile.GetMaterial(), wasCompletelyMined = true };
        TerrainEvents.OnTileMined?.Invoke(eventData);
        
        // Optional: Re-render the entire chunk or just update the specific tile.
        // For simplicity, we can trigger a full re-render for now.
        // A more optimized approach would be to update only the affected meshes.
        // terrainRenderer.UpdateTile(x, y);
    }
    
    /// <summary>
    /// Checks if a given coordinate is within the valid terrain boundaries.
    /// </summary>
    public bool IsValidPosition(int x, int y)
    {
        if (currentTerrainData == null)
        {
            Debug.LogError(feedbackMessages.GetMessage("terrain_not_generated"));
            return false;
        }
        return currentTerrainData.IsValidPosition(x, y);
    }
    
    /// <summary>
    /// Gets the current terrain data object.
    /// </summary>
    public TerrainData GetTerrainData()
    {
        return currentTerrainData;
    }

    // Note: The player's game save/load logic will be a separate system.
    // The TerrainManager is now only responsible for loading the initial level template.

    #endregion
}

// ScriptRole: Loads and manages the state of the terrain for a playable level.
// Dependencies: TerrainDataAsset, MaterialDatabase, TerrainRenderer
// HandlesEvents: None
// TriggersEvents: None
// UsesSO: TerrainDataAsset, MaterialDatabase, FeedbackMessagesSO, GrassDatabaseSO
// NeedsSetup: Assign the 'Level To Load' asset and other dependencies in the Inspector. 