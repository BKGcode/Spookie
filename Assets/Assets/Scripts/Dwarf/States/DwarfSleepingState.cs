using UnityEngine;

public class DwarfSleepingState : DwarfBaseState
{
    public DwarfSleepingState(DwarfStateManager context) : base(context) { }

    public override void EnterState()
    {
        Debug.Log("Dwarf is now sleeping and recovering stamina.");
    }

    public override void UpdateState()
    {
        // Recover stamina while sleeping
        context.Stats.RecoverStamina(Time.deltaTime);
    }

    public override void ExitState()
    {
        Debug.Log("Dwarf woke up.");
    }
}

// ScriptRole: Manages dwarf behavior while sleeping to recover stamina. 