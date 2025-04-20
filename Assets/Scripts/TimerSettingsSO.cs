using UnityEngine;

[CreateAssetMenu(fileName = "TimerSettingsSO", menuName = "Configs/TimerSettingsSO")]
public class TimerSettingsSO : ScriptableObject
{
    [Header("Work Timer (seconds)")]
    public float defaultWorkDuration = 1500f;
    public float minWorkDuration = 300f;
    public float maxWorkDuration = 3600f;

    [Header("Break Timer (seconds)")]
    public float defaultBreakDuration = 300f;
    public float minBreakDuration = 60f;
    public float maxBreakDuration = 1800f;
}
