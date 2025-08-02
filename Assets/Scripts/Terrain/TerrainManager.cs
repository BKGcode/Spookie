using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Pathfinding;
using World;
using SO;
using Terrain.Data;

namespace Terrain
{
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
        [SerializeField] private GameObject bedPrefab;

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

            SpawnBeds();

            IsTerrainReady = true;
            
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
            
            // Extract rock positions for pathfinding grid
            var rockPositions = new HashSet<Vector2Int>();
            for (int x = 0; x < currentTerrainData.Width; x++)
            {
                for (int y = 0; y < currentTerrainData.Height; y++)
                {
                    if (currentTerrainData.GetTile(x, y).GetMiningState() == MiningState.Intact)
                    {
                        rockPositions.Add(new Vector2Int(x, y));
                    }
                }
            }

            PathfindingGrid.Instance.InitializeGrid(currentTerrainData.Width, currentTerrainData.Height, rockPositions);
            
            if (currentTerrainData == null)
            {
                Debug.LogError("TerrainManager: Failed to create terrain data from asset.", this);
                return;
            }
            
            Debug.Log("TerrainManager: Level loaded successfully.");
        }
        
        private void SpawnBeds()
        {
            if (bedPrefab == null)
            {
                Debug.LogWarning("TerrainManager: Bed Prefab is not assigned. No beds will be spawned.");
                return;
            }

            if (currentTerrainData.BedSpawnPoints == null) return;

            foreach (var spawnPoint in currentTerrainData.BedSpawnPoints)
            {
                Vector3 spawnPosition = new Vector3(spawnPoint.x, 0.5f, spawnPoint.y);
                Instantiate(bedPrefab, spawnPosition, Quaternion.identity, transform);

                PathfindingGrid.Instance.UpdateNodeWalkability(spawnPosition, false);
                PathfindingGrid.Instance.UpdateNodeWalkability(new Vector3(spawnPoint.x, 0, spawnPoint.y + 1), false);
                
                Debug.Log($"Spawned bed at {spawnPosition} and blocked pathfinding for its 1x2 area.");
            }
        }

        public MaterialSO GetMaterialAt(int x, int y)
        {
            if (!IsValidPosition(x, y)) return null;
            return currentTerrainData.GetTile(x, y).GetMaterial();
        }
        
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

            tile.SetMiningState(MiningState.Mined);
            currentTerrainData.SetTile(x, y, tile);
            
            Debug.Log($"TerrainManager: Tile at ({x},{y}) was mined.");

            var eventData = new TileMinedEventData { x = x, y = y, material = tile.GetMaterial(), wasCompletelyMined = true };
            TerrainEvents.OnTileMined?.Invoke(eventData);
            
            PathfindingGrid.Instance.UpdateNodeWalkability(new Vector3(x, 0, y), true);
            
            terrainRenderer.UpdateTileVisual(x, y, currentTerrainData, grassDatabase);
        }
        
        public bool IsValidPosition(int x, int y)
        {
            if (currentTerrainData == null)
            {
                Debug.LogError(feedbackMessages.GetMessage("terrain_not_generated"));
                return false;
            }
            return currentTerrainData.IsValidPosition(x, y);
        }
        
        public TerrainData GetTerrainData()
        {
            return currentTerrainData;
        }
    }
}
