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
    // ... other states will be added here

    private void Awake()
    {
        Stats = GetComponent<DwarfStats>();
        Movement = GetComponent<DwarfMovement>();

        // Initialize States
        IdleState = new DwarfIdleState(this);
        // ... initialize other states
    }

    private void Start()
    {
        Movement.Initialize(Stats.BaseStats.movementSpeed);
        
        // Initial State
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
        Debug.Log($"Dwarf transitioned to state: {newState.GetType().Name}");
    }
}


// ScriptRole: The 'brain' of the dwarf, managing its behavior via a state machine.
// Dependencies: DwarfStats, DwarfMovement
// HandlesEvents: None
// TriggersEvents: None
// UsesSO: None
// NeedsSetup: Must be on a GameObject with DwarfStats and DwarfMovement components. 