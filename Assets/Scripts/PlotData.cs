using System; // Needed for [Serializable] and Guid
using UnityEngine; // Needed for warning suppression if cropSO is unused initially

[Serializable]
public class PlotData
{
    public string id; // Unique identifier for the plot
    public string assignedCropId = null; // ID or name of the assigned CropSO, null if empty
    public float currentGrowthTime = 0f; // Time the current crop has been growing (in seconds)

    // Constructor for creating a new, empty plot
    public PlotData()
    {
        id = Guid.NewGuid().ToString();
        assignedCropId = null;
        currentGrowthTime = 0f;
    }
}

// --- Summary Block ---
// ScriptRole: Holds the runtime and persistent state for a single farming plot, including its assigned crop and current growth progress.
// RelatedScripts: FarmingManager (manages lists of PlotData), CropSO (referenced by assignedCropId), SaveLoadManager (saves/loads PlotData)
// UsesSO: Indirectly via assignedCropId referencing a CropSO (likely looked up by ID/name in FarmingManager) 