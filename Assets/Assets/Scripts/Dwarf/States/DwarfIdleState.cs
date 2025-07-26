using UnityEngine;

public class DwarfIdleState : DwarfBaseState
{
    public DwarfIdleState(DwarfStateManager context) : base(context) { }

    public override void EnterState()
    {
        Debug.Log("Dwarf is now Idle.");
    }

    public override void UpdateState()
    {
        // In a real scenario, we might check for work availability
        // or other triggers to transition to another state.
        // For now, it just stays idle.
    }

    public override void ExitState()
    {
        // No specific cleanup needed for Idle state.
    }
}

// ScriptRole: Represents the Idle behavior of a dwarf. 