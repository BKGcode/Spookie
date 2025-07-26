using UnityEngine;

public class DwarfReturningToBaseState : DwarfBaseState
{
    private Vector3 basePosition = Vector3.zero; // Assuming base is at (0,0,0)

    public DwarfReturningToBaseState(DwarfStateManager context) : base(context) { }

    public override void EnterState()
    {
        Debug.Log("Dwarf is returning to base.");
        context.Movement.MoveTo(basePosition);
        context.Movement.OnTargetReached += OnBaseReached;
    }

    public override void UpdateState()
    {
        // Movement is handled by DwarfMovement.
    }

    public override void ExitState()
    {
        context.Movement.OnTargetReached -= OnBaseReached;
    }

    private void OnBaseReached()
    {
        context.TransitionToState(context.SleepingState);
    }
}

// ScriptRole: Manages dwarf behavior when returning to the base camp. 