using UnityEngine;

[CreateAssetMenu(fileName = "GameSettings", menuName = "Spookie/Data/Game Settings")]
public class GameSettingsSO : ScriptableObject
{
    [Header("Time Settings")]
    [Tooltip("Duration of the work shift in seconds.")]
    public float dayDurationSeconds = 60f;

    [Tooltip("Duration of the rest shift in seconds.")]
    public float nightDurationSeconds = 30f;

    [Header("Stamina Settings")]
    [Tooltip("Stamina consumed per second while working.")]
    public float staminaConsumptionRate = 5f;

    [Tooltip("Stamina recovered per second while resting.")]
    public float staminaRecoveryRate = 10f;
}

// ScriptRole: Holds global game settings for easy tuning.
// UsesSO: None
// NeedsSetup: Create a single instance from Assets menu to hold all global game configuration. 