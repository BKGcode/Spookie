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
    
    // Properties
    public Vector2Int? TargetTile { get; private set; }

    private void Awake()
    {
        Stats = GetComponent<DwarfStats>();
        Movement = GetComponent<DwarfMovement>();

        // Initialize States
        IdleState = new DwarfIdleState(this);
        MiningState = new DwarfMiningState(this);
        // MovingToTargetState is instantiated with a target, so it's created on demand.
    }

    private void OnEnable()
    {
        GameEvents.OnDwarfAssigned += HandleDwarfAssignment;
    }

    private void OnDisable()
    {
        GameEvents.OnDwarfAssigned -= HandleDwarfAssignment;
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
        currentState.EnterState();
        Debug.Log($"Dwarf '{name}' transitioned to state: {newState.GetType().Name}");
    }

    private void HandleDwarfAssignment(DwarfStateManager dwarf, Vector2Int targetPosition)
    {
        if (dwarf != this) return; // This event is not for me

        TargetTile = targetPosition;
        MovingToTargetState = new DwarfMovingToTargetState(this, targetPosition);
        TransitionToState(MovingToTargetState);
    }
}

// ScriptRole: The 'brain' of the dwarf, managing its behavior via a state machine.
// Dependencies: DwarfStats, DwarfMovement
// HandlesEvents: GameEvents.OnDwarfAssigned
// TriggersEvents: None
// UsesSO: None
// NeedsSetup: Must be on a GameObject with DwarfStats and DwarfMovement components. 