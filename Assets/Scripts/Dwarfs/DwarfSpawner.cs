using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Dwarfs
{
    /// <summary>
    /// Handles the spawning of dwarfs based on the spawn points defined in the level's TerrainData.
    /// </summary>
    public class DwarfSpawner : MonoBehaviour
    {
        [Header("Spawning Configuration")]
        [SerializeField] private GameObject dwarfPrefab;
        [SerializeField] private float delayBetweenSpawns = 0.5f;
        private bool hasSpawned = false;

        private void Start()
        {
            // Check if terrain is already generated when this spawner becomes active.
            // This handles cases where the spawner is instantiated after terrain generation.
            if (TerrainManager.Instance != null && TerrainManager.Instance.IsTerrainReady)
            {
                SpawnDwarfs(TerrainManager.Instance.GetTerrainData());
            }
        }

        private void OnEnable()
        {
            Debug.Log("DwarfSpawner: OnEnable() called. Subscribing to terrain generation events.", this);
            TerrainEvents.OnTerrainGenerated.AddListener(SpawnDwarfs);
        }

        private void OnDisable()
        {
            TerrainEvents.OnTerrainGenerated.RemoveListener(SpawnDwarfs);
        }

        private void SpawnDwarfs(TerrainData terrainData)
        {
            if (hasSpawned) return;
            hasSpawned = true;

            if (dwarfPrefab == null)
            {
                Debug.LogError("DwarfSpawner: Dwarf Prefab is not assigned!", this);
                return;
            }

            if (terrainData.DwarfSpawnPoints == null || terrainData.DwarfSpawnPoints.Count == 0)
            {
                Debug.LogWarning("DwarfSpawner: No dwarf spawn points found in the terrain data. No dwarfs will be spawned.", this);
                return;
            }

            Debug.Log($"DwarfSpawner: Found {terrainData.DwarfSpawnPoints.Count} spawn points. Starting spawn sequence.");
            StartCoroutine(SpawnSequence(terrainData.DwarfSpawnPoints));
        }

        private IEnumerator SpawnSequence(List<Vector2Int> spawnPoints)
        {
            foreach (var point in spawnPoints)
            {
                // We place the dwarf at Y=0.5 to be correctly positioned on top of the floor tile.
                Vector3 spawnPosition = new Vector3(point.x, 0.5f, point.y);
                
                Instantiate(dwarfPrefab, spawnPosition, Quaternion.identity, this.transform);
                Debug.Log($"Spawned dwarf at {spawnPosition}");

                yield return new WaitForSeconds(delayBetweenSpawns);
            }
            
            Debug.Log("DwarfSpawner: Spawn sequence complete.");
        }
    }
}

// ScriptRole: Instantiates dwarfs at predefined spawn points after the terrain is generated.
// Dependencies: None
// HandlesEvents: TerrainEvents.OnTerrainGenerated
// TriggersEvents: None
// UsesSO: None
// NeedsSetup: Attach to a GameObject in the scene (e.g., a 'Spawners' manager). Assign the 'Dwarf Prefab' and configure the spawn delay. 