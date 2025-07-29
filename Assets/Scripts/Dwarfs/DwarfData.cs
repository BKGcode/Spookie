
using UnityEngine;
using System.Collections;
using Core;

namespace Dwarfs
{
    public class DwarfData : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private DwarfStatsSO _stats;
        public DwarfStatsSO Stats => _stats;

        [Header("State")]
        public PlayerDirective? currentDirective;

        // Public properties for the brain to access
        public float WorkTimeBudget { get; private set; }
        public float LivingTimeBudget { get; private set; }

        private void Awake()
        {
            if (_stats == null)
            {
                Debug.LogError($"Dwarf {name} is missing DwarfStatsSO reference!");
                return; // Prevent further execution without stats
            }
            Debug.Log($"Dwarf {name} initialized with stats: {_stats.name}");
        }

        private void OnEnable()
        {
            TimeManager.OnDayStart += HandleDayStart;
            TimeManager.OnNightStart += HandleNightStart;
            Debug.Log($"Dwarf {name} subscribed to time events.");
        }

        private void OnDisable()
        {
            TimeManager.OnDayStart -= HandleDayStart;
            TimeManager.OnNightStart -= HandleNightStart;
            Debug.Log($"Dwarf {name} unsubscribed from time events.");
        }

        private void HandleDayStart()
        {
            // According to GDD, both budgets are a percentage of the total day duration.
            float dayDuration = TimeManager.Instance.totalDayDurationInSeconds;
            WorkTimeBudget = dayDuration * _stats.WorkTimePercentage;
            LivingTimeBudget = dayDuration * _stats.LivingTimePercentage;
            
            Debug.Log($"Dwarf {name} wakes up! Work Budget: {WorkTimeBudget}s, Living Budget: {LivingTimeBudget}s.");
        }

        private void HandleNightStart()
        {
            Debug.Log($"Dwarf {name} feels sleepy as night falls.");
            // The brain will now be responsible for forcing the dwarf to sleep.
            currentDirective = null; // Forget directive when night starts
        }

        // Methods for the brain to modify budgets
        public void ConsumeWorkTime(float amount)
        {
            WorkTimeBudget = Mathf.Max(0, WorkTimeBudget - amount);
        }

        public void ConsumeLivingTime(float amount)
        {
            LivingTimeBudget = Mathf.Max(0, LivingTimeBudget - amount);
        }
    }
}

// ScriptRole: Holds the core data, stats reference, and vital cycle logic for a single dwarf.
// Dependencies: DwarfBrain, DwarfMovement
// HandlesEvents: TimeManager.OnDayStart, TimeManager.OnNightStart
// UsesSO: DwarfStatsSO
// NeedsSetup: Attach to the Dwarf prefab. Requires a TimeManager in the scene. Assign DwarfStatsSO in the Inspector. 