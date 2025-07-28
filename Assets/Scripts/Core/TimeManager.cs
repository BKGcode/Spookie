
using System;
using UnityEngine;

namespace Core
{
    /// <summary>
    /// Singleton that manages the global game time, day/night cycles, and related events.
    /// </summary>
    public class TimeManager : MonoBehaviour
    {
        public static TimeManager Instance { get; private set; }

        [Header("Cycle Settings")]
        [Tooltip("Total duration of a full day-night cycle in real-time seconds.")]
        public float totalDayDurationInSeconds = 120f;

        // --- Events ---
        public static event Action OnDayStart;
        public static event Action OnNightStart;

        // --- Private State ---
        private float _dayCycleDuration;
        private float _nightCycleDuration;
        private float _currentTimeOfDay;
        private bool _isNight;

        public float DayCycleDuration => _dayCycleDuration;
        public float NightCycleDuration => _nightCycleDuration;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("Another instance of TimeManager already exists. Destroying this one.");
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
                Debug.Log("TimeManager initialized.");
            }
        }

        private void Start()
        {
            CalculateCycleDurations();
            _currentTimeOfDay = 0f;
            _isNight = false;
            OnDayStart?.Invoke();
            Debug.Log("World clock started. It is now Day.");
        }

        private void Update()
        {
            _currentTimeOfDay += Time.deltaTime;
            CheckForCycleChange();
        }

        private void CalculateCycleDurations()
        {
            _dayCycleDuration = totalDayDurationInSeconds * 0.7f;
            _nightCycleDuration = totalDayDurationInSeconds * 0.3f;
        }

        private void CheckForCycleChange()
        {
            if (!_isNight && _currentTimeOfDay >= _dayCycleDuration)
            {
                StartNight();
            }
            else if (_currentTimeOfDay >= totalDayDurationInSeconds)
            {
                StartDay();
            }
        }

        private void StartNight()
        {
            _isNight = true;
            OnNightStart?.Invoke();
            Debug.Log("Night has begun.");
        }

        private void StartDay()
        {
            _isNight = false;
            _currentTimeOfDay = 0f;
            OnDayStart?.Invoke();
            Debug.Log("A new day has begun.");
        }
    }
}

// ScriptRole: Manages the global day/night cycle and broadcasts key time-based events.
// Dependencies: None. It's a Singleton.
// TriggersEvents: OnDayStart, OnNightStart.
// NeedsSetup: Create a single empty GameObject in the scene and attach this script. Set 'totalDayDurationInSeconds' in the Inspector. 