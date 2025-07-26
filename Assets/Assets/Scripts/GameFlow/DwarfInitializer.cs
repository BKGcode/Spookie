using UnityEngine;

public class DwarfInitializer : MonoBehaviour
{
    private void Start()
    {
        DwarfStats[] allDwarves = FindObjectsOfType<DwarfStats>();
        Debug.Log($"Found {allDwarves.Length} dwarves in the scene to initialize.");
        
        foreach (DwarfStats dwarf in allDwarves)
        {
            dwarf.Initialize();
        }

        // This script's job is done, so we can destroy it.
        Destroy(this);
    }
}

// ScriptRole: A temporary utility to find and initialize all dwarves present in the scene at startup.
// Dependencies: None
// HandlesEvents: None
// TriggersEvents: None
// UsesSO: None
// NeedsSetup: Add this script to any manager-type GameObject in the scene (like GameClock). 