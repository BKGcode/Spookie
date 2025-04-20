using UnityEngine;
using UnityEngine.Events;
using System;

public class TimerSystem : MonoBehaviour
{
    public event Action<TimerType> OnTimerStarted;
    public event Action<float, TimerType> OnTimerTick;
    public event Action<TimerType> OnTimerCompleted;
    public event Action OnTimerPaused;
    public event Action OnTimerResumed;
    public event Action OnTimerStopped;

    [Header("UnityEvents (opcional para UI)")]
    public UnityEvent<TimerType> unityOnTimerStarted;
    public UnityEvent<float, TimerType> unityOnTimerTick;
    public UnityEvent<TimerType> unityOnTimerCompleted;
    public UnityEvent unityOnTimerPaused;
    public UnityEvent unityOnTimerResumed;
    public UnityEvent unityOnTimerStopped;

    [Header("Timer Settings")]
    public TimerSettingsSO timerSettings;
    private float _currentDuration;
    private float _timeRemaining;
    private bool _isRunning = false;
    private TimerType _currentType;

    public void StartWorkTimer()
    {
        StartTimer(TimerType.Work, timerSettings.defaultWorkDuration,
            timerSettings.minWorkDuration, timerSettings.maxWorkDuration);
    }

    public void StartBreakTimer()
    {
        StartTimer(TimerType.Break, timerSettings.defaultBreakDuration,
            timerSettings.minBreakDuration, timerSettings.maxBreakDuration);
    }

    public void PauseTimer()
    {
        if (_isRunning)
        {
            _isRunning = false;
            Debug.Log("[TimerSystem] Timer paused (" + _currentType + ")");
            OnTimerPaused?.Invoke();
            unityOnTimerPaused?.Invoke();
        }
    }

    public void ResumeTimer()
    {
        if (!_isRunning && _timeRemaining > 0f)
        {
            _isRunning = true;
            Debug.Log("[TimerSystem] Timer resumed (" + _currentType + ")");
            OnTimerResumed?.Invoke();
            unityOnTimerResumed?.Invoke();
        }
    }

    public void StopTimer()
    {
        _isRunning = false;
        _timeRemaining = 0f;
        _currentDuration = 0f;
        Debug.Log("[TimerSystem] Timer stopped.");
        OnTimerStopped?.Invoke();
        unityOnTimerStopped?.Invoke();
    }

    public void ResetTimer()
    {
        _isRunning = false;
        _timeRemaining = _currentDuration;
        Debug.Log("[TimerSystem] Timer reset (" + _currentType + ")");
    }

    public bool IsTimerRunning() => _isRunning;
    public float GetTimeRemaining() => _timeRemaining;
    public float GetProgress()
    {
        if (_currentDuration <= 0f) return 0f;
        return 1f - (_timeRemaining / _currentDuration);
    }
    public TimerType GetCurrentTimerType() => _currentType;

    private void StartTimer(TimerType type, float requested, float min, float max)
    {
        float duration = Mathf.Clamp(requested, min, max);
        _currentDuration = duration;
        _timeRemaining = duration;
        _isRunning = true;
        _currentType = type;
        Debug.Log("[TimerSystem] Timer started: " + type + " (" + duration + "s)");
        OnTimerStarted?.Invoke(type);
        unityOnTimerStarted?.Invoke(type);
    }

    private void Update()
    {
        if (!_isRunning || _timeRemaining <= 0f) return;
        _timeRemaining -= Time.deltaTime;
        OnTimerTick?.Invoke(Mathf.Max(_timeRemaining, 0f), _currentType);
        unityOnTimerTick?.Invoke(Mathf.Max(_timeRemaining, 0f), _currentType);
        if (_timeRemaining <= 0f)
        {
            _isRunning = false;
            Debug.Log("[TimerSystem] Timer complete! (" + _currentType + ")");
            OnTimerCompleted?.Invoke(_currentType);
            unityOnTimerCompleted?.Invoke(_currentType);
        }
    }
}

public enum TimerType
{
    Work,
    Break
}