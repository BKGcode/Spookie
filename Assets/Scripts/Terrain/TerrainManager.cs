using UnityEngine;
using System.Collections;

/// <summary>
/// Singleton manager that coordinates all terrain-related systems.
/// Provides a public API for interacting with the terrain.
/// </summary>
[RequireComponent(typeof(TerrainGenerator), typeof(TerrainRenderer))]
public class TerrainManager : MonoBehaviour
{
    public static TerrainManager Instance { get; private set; }
    
    [Header("System Dependencies")]
    [SerializeField] private MaterialDatabase materialDatabase;
    [SerializeField] private FeedbackMessagesSO feedbackMessages;
    
    [Header("Save/Load Settings")]
    [SerializeField] private bool loadOnStart = true;
    [SerializeField] private float autoSaveIntervalSeconds = 300f; // 5 minutes

    [Header("Debug Settings")]
    [SerializeField] private Vector2Int tileToMineForDebug = new Vector2Int(50, 50);

    [ContextMenu("Debug: Mine Specific Tile")]
    private void DebugMineTile()
    {
        MineTileAt(tileToMineForDebug.x, tileToMineForDebug.y);
    }

    // Internal component references
    private TerrainGenerator terrainGenerator;
    private TerrainRenderer terrainRenderer;
    private TerrainData currentTerrainData;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("TerrainManager: Another instance exists. Destroying this one.", this);
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        terrainGenerator = GetComponent<TerrainGenerator>();
        terrainRenderer = GetComponent<TerrainRenderer>();
        
        if (terrainGenerator == null || terrainRenderer == null)
        {
            Debug.LogError("TerrainManager: Missing TerrainGenerator or TerrainRenderer component.", this);
            enabled = false;
            return;
        }
    }

    IEnumerator Start()
    {
        if (loadOnStart)
        {
            yield return StartCoroutine(LoadTerrain());
        }
        else
        {
            terrainGenerator.GenerateTerrain();
        }
        
        if (autoSaveIntervalSeconds > 0)
        {
            yield return new WaitForSeconds(autoSaveIntervalSeconds);
            StartCoroutine(AutoSaveRoutine());
        }
    }

    private IEnumerator AutoSaveRoutine()
    {
        while(true)
        {
            yield return new WaitForSeconds(autoSaveIntervalSeconds);
            SaveTerrain();
        }
    }

    private void OnEnable()
    {
        TerrainEvents.OnTerrainGenerated.AddListener(HandleTerrainGenerated);
        TerrainEvents.OnTileMined.AddListener(HandleTileMined);
    }

    private void OnDisable()
    {
        TerrainEvents.OnTerrainGenerated.RemoveListener(HandleTerrainGenerated);
        TerrainEvents.OnTileMined.RemoveListener(HandleTileMined);
    }

    private void HandleTerrainGenerated(TerrainData data)
    {
        Debug.Log("TerrainManager: Caching newly generated terrain data.");
        currentTerrainData = data;
        TerrainEvents.OnTerrainGenerated?.Invoke(data);
    }

    private void HandleTileMined(TileMinedEventData data)
    {
        if (!IsValidPosition(data.x, data.y))
        {
            Debug.LogWarning(feedbackMessages.GetMessage("mining_invalid_position"));
            return;
        }
        
        TerrainTile tile = currentTerrainData.GetTile(data.x, data.y);
        if (!tile.IsMineable())
        {
            Debug.Log(feedbackMessages.GetMessage("mining_not_mineable"));
            return;
        }

        // For now, we'll assume one-hit mining. Durability will be handled later.
        tile.SetMiningState(MiningState.Mined);
        currentTerrainData.SetTile(data.x, data.y, tile);
        
        Debug.Log($"TerrainManager: Tile at ({data.x},{data.y}) was mined.");

        // Trigger events
        var eventData = new TileMinedEventData { x = data.x, y = data.y, material = tile.GetMaterial(), wasCompletelyMined = true };
        TerrainEvents.OnTileMined?.Invoke(eventData);
        
        // Optional: Re-render the entire chunk or just update the specific tile.
        // For simplicity, we can trigger a full re-render for now.
        // A more optimized approach would be to update only the affected meshes.
        // terrainRenderer.UpdateTile(x, y);
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

    /// <summary>
    /// Saves the current state of the terrain to a file.
    /// </summary>
    [ContextMenu("Save Terrain")]
    public void SaveTerrain()
    {
        if (currentTerrainData == null)
        {
            Debug.LogError(feedbackMessages.GetMessage("terrain_not_generated"));
            return;
        }

        // For now, let's assume we save all tiles for simplicity.
        // A more optimized version would save only modified tiles.
        var saveData = new TerrainSaveData(currentTerrainData.MapSeed, Application.version);
        for (int y = 0; y < currentTerrainData.Height; y++)
        {
            for (int x = 0; x < currentTerrainData.Width; x++)
            {
                var tile = currentTerrainData.GetTile(x, y);
                // In an optimized system, we would compare to the original state.
                // For now, we save any tile that is not in its default state (e.g., mined).
                if (tile.GetMiningState() != MiningState.Intact || tile.GetSpecialProperty() != SpaceType.None)
                {
                    saveData.AddModifiedTile(x, y, tile.GetMiningState(), tile.GetSpecialProperty());
                }
            }
        }
        
        TerrainSaveSystem.SaveTerrain(saveData);
    }

    /// <summary>
    /// Loads the terrain state from a file, or generates a new one if no file exists.
    /// </summary>
    [ContextMenu("Load Terrain")]
    public IEnumerator LoadTerrain()
    {
        TerrainSaveData saveData = TerrainSaveSystem.LoadTerrain();
        if (saveData != null)
        {
            Debug.Log($"TerrainManager: Loading from save file with seed {saveData.MapSeed}.");
            // Generate the base terrain from the save file's seed
            terrainGenerator.GenerateTerrain(saveData.MapSeed);
            
            // Apply modifications from the save file
            // Note: HandleTerrainGenerated will have already cached the base terrain data
            foreach(var modifiedTile in saveData.ModifiedTiles)
            {
                var originalTile = currentTerrainData.GetTile(modifiedTile.X, modifiedTile.Y);
                originalTile.SetMiningState(modifiedTile.MiningState);
                originalTile.SetSpecialProperty(modifiedTile.SpecialProperty);
                currentTerrainData.SetTile(modifiedTile.X, modifiedTile.Y, originalTile);
            }

            Debug.Log($"TerrainManager: Applied {saveData.ModifiedTiles.Count} modifications from save file.");
            
            // The TerrainRenderer already re-renders when OnTerrainGenerated is invoked,
            // so the loaded state will be displayed correctly.
        }
        else
        {
            Debug.Log("TerrainManager: No save file found or failed to load. Generating new terrain.");
            terrainGenerator.GenerateTerrain(); // Generate with inspector seed
        }
        yield return null;
    }

    #endregion
}

// ScriptRole: Singleton coordinator for all terrain systems, providing a unified public API.
// Dependencies: TerrainGenerator, TerrainRenderer, MaterialDatabase, FeedbackMessagesSO
// HandlesEvents: OnTerrainGenerated (from TerrainGenerator)
// TriggersEvents: OnTerrainGenerated, OnTileMined (via static TerrainEvents)
// UsesSO: MaterialDatabase, FeedbackMessagesSO
// NeedsSetup: Attach to a GameObject with TerrainGenerator and TerrainRenderer. Assign database and message SOs. 