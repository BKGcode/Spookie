using UnityEngine;

namespace SO
{
    [CreateAssetMenu(fileName = "DwarfTimeBudgets", menuName = "Spookie/Dwarfs/Time Budgets")]
    public class DwarfTimeBudgetsSO : ScriptableObject
    {
        [Header("Budget Settings")]
        [Range(0, 1f)]
        [Tooltip("Percentage of day duration allocated to work (0-1)")]
        public float workBudgetPercentage = 0.5f;

        [Range(0, 1f)]
        [Tooltip("Percentage of day duration allocated to living activities (0-1)")]
        public float livingBudgetPercentage = 0.5f;

        private void OnValidate()
        {
            // Ensure percentages always sum to 1
            float total = workBudgetPercentage + livingBudgetPercentage;
            if (Mathf.Abs(total - 1f) > 0.01f)
            {
                Debug.LogWarning($"Time budgets must sum to 1. Current sum: {total}");
            }
        }
    }
}

// ScriptRole: Configurable time budget settings for dwarf daily activities
// UsesSO: None
// NeedsSetup: Create via Assets > Create > Spookie > Dwarfs > Time Budgets