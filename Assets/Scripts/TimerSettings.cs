using UnityEngine;

[CreateAssetMenu(fileName = "TimerSettings", menuName = "GameSettings/TimerSettings")]
public class TimerSettingsSO : ScriptableObject
{
    [Header("Default Durations (in seconds)")]
    public float workDuration = 1500f;  // 25 minutes
    public float breakDuration = 300f; // 5 minutes

    [Header("Limits")]
    public float minDuration = 60f;  // Minimum of 1 minute
    public float maxDuration = 3600f; // Maximum of 1 hour
}
