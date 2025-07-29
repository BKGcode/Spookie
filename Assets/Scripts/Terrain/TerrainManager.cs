using UnityEngine;
using System.Collections;
using Pathfinding;

/// <summary>
/// Singleton manager that coordinates all terrain-related systems.
/// Provides a public API for interacting with the terrain.
/// </summary>
public class TerrainManager : MonoBehaviour
{
    public static TerrainManager Instance { get; private set; }
    public bool IsTerrainReady { get; private set; } = false;
    
    [Header("Level Data")]
    [Tooltip("The terrain level asset to load when the game starts.")]
    [SerializeField] private TerrainDataAsset levelToLoad;

    [Header("System Dependencies")]
    [SerializeField] private MaterialDatabase materialDatabase;
    [SerializeField] private FeedbackMessagesSO feedbackMessages;
    [SerializeField] private GrassDatabaseSO grassDatabase;

    // Internal component references
    // The TerrainGenerator is no longer a component and is not used by the manager at runtime.
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
        
        // This line is removed as TerrainGenerator is no longer a MonoBehaviour.
        terrainRenderer = GetComponent<TerrainRenderer>();
    }

    private void OnEnable()
    {
        // PlayerActions.OnMineAttempt += HandleMineAttempt; // No longer needed
    }

    private void OnDisable()
    {
        // PlayerActions.OnMineAttempt -= HandleMineAttempt; // No longer needed
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

        IsTerrainReady = true;
        
        // Notify all listeners that the terrain is ready.
        TerrainEvents.OnTerrainGenerated?.Invoke(currentTerrainData);
    }

    private void LoadTerrainFromAsset(TerrainDataAsset asset)
    {
        Debug.Log($"TerrainManager: Loading level '{asset.name}'.");
        
        if (asset.LevelData == null)
        {
            Debug.LogError($"Asset '{asset.name}' has no level data.", this);
            return;
        }

        currentTerrainData = TerrainDataFactory.CreateFromSaveData(asset.LevelData, materialDatabase);
        
        // Initialize the pathfinding grid with the newly created terrain data
        PathfindingGrid.Instance.InitializeGrid(currentTerrainData);
        
        if (currentTerrainData == null)
        {
            Debug.LogError("TerrainManager: Failed to create terrain data from asset.", this);
            return;
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
        
        // The inefficient full re-render has been replaced with a targeted visual update.
        terrainRenderer.UpdateTileVisual(x, y, currentTerrainData, grassDatabase);
    }
    
    private void HandleMineAttempt(Vector2Int tileCoords)
    {
        // This handler is kept for now in case it's needed for other inputs,
        // but it's disconnected from PlayerActions.
        Debug.Log($"TerrainManager: Received OnMineAttempt for tile {tileCoords}. Processing...");
        MineTileAt(tileCoords.x, tileCoords.y);
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
// TriggersEvents: TerrainEvents.OnTileMined
// UsesSO: TerrainDataAsset, MaterialDatabase, FeedbackMessagesSO, GrassDatabaseSO
// NeedsSetup: Assign the 'Level To Load' asset and other dependencies in the Inspector. 