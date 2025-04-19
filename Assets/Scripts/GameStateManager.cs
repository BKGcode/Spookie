// GameStateManager.cs
using UnityEngine;
using System;

public class GameStateManager : MonoBehaviour
{
    public enum GameStateType
    {
        Configuring,
        Working,
        Paused,
        Break,
        Summary
    }

    public event Action<GameStateType> OnStateChanged;

    private IGameState _currentState;
    private GameStateType _currentType;

    public ConfiguringState configuringState { get; private set; }
    public WorkingState workingState { get; private set; }
    public PausedState pausedState { get; private set; }
    public BreakState breakState { get; private set; }
    public SummaryState summaryState { get; private set; }

    private void Awake()
    {
        configuringState = new ConfiguringState(this);
        workingState     = new WorkingState(this);
        pausedState      = new PausedState(this);
        breakState       = new BreakState(this);
        summaryState     = new SummaryState(this);

        ChangeState(GameStateType.Configuring);
    }

    private void Update()
    {
        if (_currentState != null)
            _currentState.OnUpdate();
    }

    public void ChangeState(GameStateType newType)
    {
        if (newType == _currentType) return;

        if (_currentState != null)
            _currentState.OnExit();

        switch (newType)
        {
            case GameStateType.Configuring:
                _currentState = configuringState;
                break;
            case GameStateType.Working:
                _currentState = workingState;
                break;
            case GameStateType.Paused:
                _currentState = pausedState;
                break;
            case GameStateType.Break:
                _currentState = breakState;
                break;
            case GameStateType.Summary:
                _currentState = summaryState;
                break;
        }

        _currentType = newType;
        Debug.Log("[GameStateManager] State changed to: " + _currentType);
        OnStateChanged?.Invoke(_currentType);
        if (_currentState != null)
            _currentState.OnEnter();
    }

    public GameStateType GetStateType() => _currentType;
}
