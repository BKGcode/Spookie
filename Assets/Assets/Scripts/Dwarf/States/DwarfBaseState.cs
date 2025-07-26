public abstract class DwarfBaseState
{
    protected readonly DwarfStateManager context;

    protected DwarfBaseState(DwarfStateManager context)
    {
        this.context = context;
    }

    public abstract void EnterState();
    public abstract void UpdateState();
    public abstract void ExitState();
}

// ScriptRole: Abstract base class for all dwarf states, defining the state machine structure. 