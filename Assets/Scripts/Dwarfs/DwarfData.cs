
using UnityEngine;
using System.Collections;
using Core;

namespace Dwarfs
{
    public class DwarfData : MonoBehaviour
    {
        public enum DwarfState { Idle, Working, Living, Sleeping, Walking }

        [Header("State")]
        public DwarfState currentState;
        public PlayerDirective? currentDirective;

        [Header("Attributes")]
        [SerializeField, Range(0.1f, 0.9f)] private float sleepinessFactor;

        // Public properties for the brain to access
        public float WorkTimeBudget { get; private set; }
        public float LivingTimeBudget { get; private set; }

        private Coroutine _sleepCoroutine;

        private void Awake()
        {
            sleepinessFactor = Random.Range(0.1f, 0.9f);
            currentState = DwarfState.Sleeping; 
            Debug.Log($"Dwarf {name} initialized with sleepinessFactor: {sleepinessFactor}");
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
            if (_sleepCoroutine != null)
            {
                StopCoroutine(_sleepCoroutine);
                _sleepCoroutine = null;
            }

            WorkTimeBudget = TimeManager.Instance.DayCycleDuration;
            LivingTimeBudget = TimeManager.Instance.NightCycleDuration;
            currentState = DwarfState.Idle;
            
            Debug.Log($"Dwarf {name} wakes up! State: {currentState}. Work Budget: {WorkTimeBudget}s.");
        }

        private void HandleNightStart()
        {
            Debug.Log($"Dwarf {name} feels sleepy as night falls.");
            // The brain will now be responsible for forcing the dwarf to sleep.
            currentDirective = null; // Forget directive when night starts
            currentState = DwarfState.Sleeping;
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

// ScriptRole: Holds the core data and vital cycle logic for a single dwarf.
// HandlesEvents: TimeManager.OnDayStart, TimeManager.OnNightStart.
// NeedsSetup: Attach to the Dwarf prefab. Requires a TimeManager in the scene. 