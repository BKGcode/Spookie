
using System;
using UnityEngine;

namespace Core
{
    /// <summary>
    /// Singleton that manages the global game time, day/night cycles, and related events.
    /// </summary>
    public class TimeManager : MonoBehaviour
    {
        private static TimeManager _instance;
        public static TimeManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<TimeManager>();
                    if (_instance == null)
                    {
                        Debug.LogError("No TimeManager found in scene. Please ensure one exists.");
                    }
                }
                return _instance;
            }
            private set => _instance = value;
        }

        [Header("Cycle Settings")]
        [SerializeField]
        [Tooltip("Total duration of a full day-night cycle in real-time seconds.")]
        [Range(30f, 3600f)]
        private float totalDayDurationInSeconds = 120f;
        public float TotalDayDuration => totalDayDurationInSeconds;

        [Header("Sleep Window")]
        [SerializeField]
        [Tooltip("How far into the night cycle dwarfs should start going to bed (0-1)")]
        [Range(0f, 1f)]
        private float sleepWindowStart = 0.2f;
        public float SleepWindowStart => sleepWindowStart;
        
        [SerializeField]
        [Tooltip("How long before day starts dwarfs should wake up (in seconds)")]
        [Range(1f, 30f)]
        private float wakeUpBuffer = 10f;
        public float WakeUpBuffer => wakeUpBuffer;

        // --- Events ---
        public static event Action OnDayStart;
        public static event Action OnNightStart;
        public static event Action OnSleepWindowStart;
        public static event Action OnWakeUpTime;

        // --- Private State ---
        private float _dayCycleDuration;
        private float _nightCycleDuration;
        private float _currentTimeOfDay;
        private bool _isNight;
        private bool _isSleepWindow;
        private bool _isWakeUpTime;

        public bool IsNight => _isNight;
        public bool IsSleepWindow => _isSleepWindow;
        public bool IsWakeUpTime => _isWakeUpTime;

        public float DayCycleDuration => _dayCycleDuration;
        public float NightCycleDuration => _nightCycleDuration;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("Another instance of TimeManager already exists. Destroying this one.");
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            ValidateSettings();
            Debug.Log("TimeManager initialized.");
        }

        private void ValidateSettings()
        {
            if (totalDayDurationInSeconds < 30f)
            {
                Debug.LogWarning($"Day duration too short ({totalDayDurationInSeconds}s), setting to minimum (30s)");
                totalDayDurationInSeconds = 30f;
            }

            if (sleepWindowStart <= 0f || sleepWindowStart >= 1f)
            {
                Debug.LogWarning($"Invalid sleep window start ({sleepWindowStart}), clamping to range [0.1, 0.9]");
                sleepWindowStart = Mathf.Clamp(sleepWindowStart, 0.1f, 0.9f);
            }

            if (wakeUpBuffer <= 0f)
            {
                Debug.LogWarning($"Invalid wake up buffer ({wakeUpBuffer}s), setting to minimum (1s)");
                wakeUpBuffer = 1f;
            }
        }

        private void Start()
        {
            CalculateCycleDurations();
            _currentTimeOfDay = 0f;
            _isNight = false;
            _isSleepWindow = false;
            _isWakeUpTime = false;
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
            // Transición día/noche
            if (!_isNight && _currentTimeOfDay >= _dayCycleDuration)
            {
                StartNight();
            }
            else if (_currentTimeOfDay >= totalDayDurationInSeconds)
            {
                StartDay();
            }

            // Solo procesar estados nocturnos si es de noche
            if (_isNight)
            {
                float timeIntoNight = _currentTimeOfDay - _dayCycleDuration;
                float timeUntilDay = totalDayDurationInSeconds - _currentTimeOfDay;

                // Ventana de sueño
                if (!_isSleepWindow && timeIntoNight >= _nightCycleDuration * sleepWindowStart)
                {
                    StartSleepWindow();
                }

                // Tiempo de despertar
                if (!_isWakeUpTime && timeUntilDay <= wakeUpBuffer)
                {
                    StartWakeUpTime();
                }

                // Debug de estados nocturnos
                if (Debug.isDebugBuild)
                {
                    Debug.Log($"Night state - Sleep: {_isSleepWindow}, WakeUp: {_isWakeUpTime}, " +
                            $"TimeIntoNight: {timeIntoNight:F1}s, TimeUntilDay: {timeUntilDay:F1}s");
                }
            }
        }

        private void StartNight()
        {
            _isNight = true;
            OnNightStart?.Invoke();
            Debug.Log("Night has begun.");
        }

        private void StartSleepWindow()
        {
            _isSleepWindow = true;
            OnSleepWindowStart?.Invoke();
            Debug.Log("Sleep window has started. Dwarfs should head to bed.");
        }

        private void StartWakeUpTime()
        {
            _isWakeUpTime = true;
            OnWakeUpTime?.Invoke();
            Debug.Log("Wake up time. Dwarfs should prepare for the new day.");
        }

        private void StartDay()
        {
            _isNight = false;
            _isSleepWindow = false;
            _isWakeUpTime = false;
            _currentTimeOfDay = 0f;
            OnDayStart?.Invoke();
            Debug.Log("A new day has begun.");
        }
    }
}

// ScriptRole: Manages the global game time, day/night cycles, and related events. Handles sleep and wake up cycles for dwarfs.
// Dependencies: None. Singleton that persists across scenes.
// TriggersEvents: OnDayStart, OnNightStart, OnSleepWindowStart, OnWakeUpTime
// NeedsSetup: Create a GameObject named "TimeManager" in the scene and attach this script.
// Configuration: 
//   - TotalDayDuration: Set the total duration of a day-night cycle (30s-3600s)
//   - SleepWindowStart: When during night cycle dwarfs should sleep (0.1-0.9)
//   - WakeUpBuffer: How many seconds before day dwarfs wake up (1s-30s) 