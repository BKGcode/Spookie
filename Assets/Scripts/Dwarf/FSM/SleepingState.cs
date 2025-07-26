using UnityEngine;

public class SleepingState : IState
{
    private readonly DwarfStateManager stateManager;
    private readonly float staminaRegenRate = 10f; // Stamina per second

    public SleepingState(DwarfStateManager manager)
    {
        stateManager = manager;
    }

    public void OnEnter()
    {
        Debug.Log($"Dwarf '{stateManager.Controller.CurrentState.DwarfName}' is now sleeping.");
        stateManager.Controller.CurrentState.CurrentStatus = "Sleeping";
    }

    public void OnUpdate()
    {
        var dwarfState = stateManager.Controller.CurrentState;
        if (dwarfState.CurrentStamina < dwarfState.BaseStats.maxStamina)
        {
            dwarfState.CurrentStamina += staminaRegenRate * Time.deltaTime;
            dwarfState.CurrentStamina = Mathf.Min(dwarfState.CurrentStamina, dwarfState.BaseStats.maxStamina);
        }
        // The decision to wake up is now handled by the Supervisor based on the time of day.
    }

    public void OnExit()
    {
        Debug.Log($"Dwarf '{stateManager.Controller.CurrentState.DwarfName}' has woken up.");
    }
}

// ScriptRole: Handles the dwarf's behavior while sleeping to regenerate stamina.
// Dependencies: DwarfStateManager, GameEvents
// NeedsSetup: None. 