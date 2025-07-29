using UnityEngine;

[CreateAssetMenu(fileName = "DwarfStats", menuName = "ScriptableObjects/Dwarf Stats")]
public class DwarfStatsSO : ScriptableObject
{
    [Header("Movement")]
    public float moveSpeed = 3f;
    public float rotationSpeed = 10f;

    [Header("Work")]
    public float WorkCycleTime = 2f; // Time in seconds to complete one work action

    [Header("Time Budgets")]
    [Range(0, 1)]
    public float WorkTimePercentage = 0.7f; // 70% of the day
    [Range(0, 1)]
    public float LivingTimePercentage = 0.3f; // 30% of the day
}

// ScriptRole: Contains all the configurable stats for a dwarf.
// NeedsSetup: Create instances from Assets > Create > ScriptableObjects > Dwarf Stats. Adjust values as needed. 