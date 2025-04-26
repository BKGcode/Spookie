using UnityEngine;

[CreateAssetMenu(fileName = "NewCrop", menuName = "Spookie/Farming/Crop Type", order = 1)]
public class CropSO : ScriptableObject
{
    [Tooltip("Name of the crop displayed in the UI.")]
    public string cropName = "Default Crop";

    [Tooltip("Visual representation of the crop.")]
    public Sprite icon; // Puedes añadir más sprites para estados de crecimiento si quieres

    [Tooltip("Base time in seconds for the crop to grow fully.")]
    public float baseGrowthTime = 60f;

    [Tooltip("Amount of currency awarded when the crop is automatically harvested.")]
    public int rewardValue = 10;
}

// --- Summary Block ---
// ScriptRole: Defines the static properties of a specific type of crop (name, icon, growth time, reward).
// RelatedScripts: PlotData (references a CropSO), FarmingManager (uses data to manage growth and rewards), EconomyManager (receives reward value)
// UsesSO: None 