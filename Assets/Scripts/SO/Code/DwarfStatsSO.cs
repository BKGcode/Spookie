using UnityEngine;

namespace SO
{
    [CreateAssetMenu(fileName = "DwarfStats", menuName = "ScriptableObjects/Dwarf Stats")]
    public class DwarfStatsSO : ScriptableObject
    {
        [Header("Movement")]
        public float moveSpeed = 3f;
        public float rotationSpeed = 10f;

        [Header("Work")]
        public float WorkCycleTime = 2f;

        [Header("Time Budgets")]
        [Range(0, 1)]
        public float WorkTimePercentage = 0.7f;
        [Range(0, 1)]
        public float LivingTimePercentage = 0.3f;

        [Header("Daily Time Limits")]
        [Tooltip("Maximum time in seconds a dwarf can work per day")]
        public float MaxDailyWorkTime = 60f;
        [Tooltip("Maximum time in seconds a dwarf can spend on living activities per day")]
        public float MaxDailyLivingTime = 30f;

        [Header("Lifecycle")]
        [Tooltip("Lifespan in days (min/max). A value is chosen randomly upon spawn.")]
        public Vector2Int LifespanDaysRange = new Vector2Int(5, 10);
    }
}
