using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapDataManager : MonoBehaviour
{
    [SerializeField] private Tilemap foregroundLayer;
    private Dictionary<Vector2Int, TileState> tileStates;

    public void InitializeMapData(Dictionary<Vector2Int, TileState> initialStates)
    {
        tileStates = new Dictionary<Vector2Int, TileState>(initialStates);
        Debug.Log($"Map data initialized with {tileStates.Count} tiles.");
    }

    public TileState GetTileStateAt(Vector2Int position)
    {
        tileStates.TryGetValue(position, out TileState state);
        return state;
    }

    public void ApplyDamage(Vector2Int position, int damage)
    {
        if (tileStates.TryGetValue(position, out TileState state))
        {
            state.CurrentHealth -= damage;
            Debug.Log($"Tile at {position} took {damage} damage. New health: {state.CurrentHealth}");

            if (state.CurrentHealth <= 0)
            {
                DestroyTile(position);
            }
        }
    }

    private void DestroyTile(Vector2Int position)
    {
        Debug.Log($"Destroying tile at {position}.");
        foregroundLayer.SetTile((Vector3Int)position, null);
        tileStates.Remove(position);
        
        GameEvents.ReportTileDestroyed(position);
    }
}

// ScriptRole: Manages the runtime state of all tiles in the map.
// Dependencies: None
// HandlesEvents: None (but is called by MapGenerator)
// TriggersEvents: GameEvents.OnTileDestroyed
// UsesSO: None
// NeedsSetup: Assign the Foreground Tilemap in the Inspector. 