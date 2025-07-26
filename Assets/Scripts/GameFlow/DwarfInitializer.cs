using UnityEngine;

public class DwarfInitializer : MonoBehaviour
{
    private void Start()
    {
        DwarfRegistry.ClearRegistry();

        DwarfController[] allDwarves = FindObjectsOfType<DwarfController>();
        Debug.Log($"Found {allDwarves.Length} dwarves in the scene to initialize.");
        
        foreach (DwarfController dwarf in allDwarves)
        {
            dwarf.Initialize();
            
            // Register initial position
            Vector2Int startPosition = Pathfinding.Instance.WorldToGridPosition(dwarf.transform.position);
            DwarfRegistry.RegisterDwarf(dwarf, startPosition);
            Debug.Log($"Registered {dwarf.DwarfData.DwarfName} at {startPosition}");

            // The DwarfStateManager now starts itself in its own Start() method.
            // No need to call StartFSM().
        }

        // Signal that the game is ready for player input
        GameEvents.ReportGameReady();

        // This script's job is done, so we can destroy it.
        Destroy(gameObject);
    }
}

// ScriptRole: A temporary utility to find and initialize all dwarves present in the scene at startup.
// Dependencies: DwarfController, Pathfinding, DwarfRegistry
// HandlesEvents: None
// TriggersEvents: GameEvents.OnGameReady
// UsesSO: None
// NeedsSetup: Add this script to any manager-type GameObject in the scene. 