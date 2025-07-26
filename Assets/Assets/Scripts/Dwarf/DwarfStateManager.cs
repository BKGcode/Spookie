using System.Collections.Generic;
using UnityEngine;

public class DwarfStateManager : MonoBehaviour
{
    // Component References
    public DwarfStats Stats { get; private set; }
    public DwarfMovement Movement { get; private set; }

    // State Machine
    private DwarfBaseState currentState;
    public DwarfIdleState IdleState { get; private set; }
    public DwarfMovingToTargetState MovingToTargetState { get; private set; }
    public DwarfMiningState MiningState { get; private set; }
    public DwarfReturningToBaseState ReturningToBaseState { get; private set; }
    public DwarfSleepingState SleepingState { get; private set; }
    
    // Properties
    public Vector2Int? TargetTile { get; private set; }

    private void Awake()
    {
        Stats = GetComponent<DwarfStats>();
        Movement = GetComponent<DwarfMovement>();

        // Initialize States
        IdleState = new DwarfIdleState(this);
        MiningState = new DwarfMiningState(this);
        ReturningToBaseState = new DwarfReturningToBaseState(this);
        SleepingState = new DwarfSleepingState(this);
        // MovingToTargetState is instantiated with a target, so it's created on demand.
    }

    private void OnEnable()
    {
        GameEvents.OnDwarfAssigned += HandleDwarfAssignment;
        GameEvents.OnNightStart += HandleNightStart;
        GameEvents.OnDayStart += HandleDayStart;
    }

    private void OnDisable()
    {
        GameEvents.OnDwarfAssigned -= HandleDwarfAssignment;
        GameEvents.OnNightStart -= HandleNightStart;
        GameEvents.OnDayStart -= HandleDayStart;
    }

    private void Start()
    {
        Movement.Initialize(Stats.BaseStats.movementSpeed);
        TransitionToState(IdleState);
    }

    private void Update()
    {
        currentState?.UpdateState();
    }

    public void TransitionToState(DwarfBaseState newState)
    {
        currentState?.ExitState();
        currentState = newState;

        // Update the human-readable status in DwarfStats
        Stats.UpdateStatus(GetStateName(newState));

        currentState.EnterState();
        Debug.Log($"Dwarf '{name}' transitioned to state: {newState.GetType().Name}");
    }

    private string GetStateName(DwarfBaseState state)
    {
        // This can be expanded with a switch for more descriptive names
        return state.GetType().Name
            .Replace("Dwarf", "")
            .Replace("State", "");
    }

    private void HandleDwarfAssignment(DwarfStateManager dwarf, Vector2Int targetPosition)
    {
        if (dwarf != this) return; // This event is not for me

        TargetTile = targetPosition;
        MovingToTargetState = new DwarfMovingToTargetState(this, targetPosition);
        TransitionToState(MovingToTargetState);
    }

    private void HandleNightStart()
    {
        Debug.Log($"Dwarf '{name}' received night signal.");
        // If the dwarf is doing something, it should return to base.
        if (currentState is DwarfMiningState || currentState is DwarfMovingToTargetState)
        {
            TransitionToState(ReturningToBaseState);
        }
    }

    private void HandleDayStart()
    {
        Debug.Log($"Dwarf '{name}' received day signal.");
        // Wake up and go to idle, ready for new assignments.
        if (currentState is DwarfSleepingState)
        {
            TransitionToState(IdleState);
        }
    }
}

// ScriptRole: The 'brain' of the dwarf, managing its behavior via a state machine.
// Dependencies: DwarfStats, DwarfMovement
// HandlesEvents: GameEvents.OnDwarfAssigned, GameEvents.OnNightStart, GameEvents.OnDayStart
// TriggersEvents: None
// UsesSO: None
// NeedsSetup: Must be on a GameObject with DwarfStats and DwarfMovement components. 