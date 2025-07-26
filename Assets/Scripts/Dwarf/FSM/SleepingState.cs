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
        
        // Subscribe to day start event to wake up
        GameEvents.OnDayStart += WakeUp;
    }

    public void OnUpdate()
    {
        var dwarfState = stateManager.Controller.CurrentState;
        if (dwarfState.CurrentStamina < dwarfState.BaseStats.maxStamina)
        {
            dwarfState.CurrentStamina += staminaRegenRate * Time.deltaTime;
            // Clamp the value to not exceed max stamina
            dwarfState.CurrentStamina = Mathf.Min(dwarfState.CurrentStamina, dwarfState.BaseStats.maxStamina);
        }
        else
        {
            // Fully rested, wake up
            Debug.Log("Dwarf is fully rested.");
            WakeUp();
        }
    }

    public void OnExit()
    {
        Debug.Log($"Dwarf '{stateManager.Controller.CurrentState.DwarfName}' has woken up.");
        GameEvents.OnDayStart -= WakeUp;
    }

    private void WakeUp()
    {
        // Avoid multiple calls if already transitioning
        if (stateManager.CurrentState is SleepingState)
        {
            stateManager.ChangeState(new IdleState(stateManager));
        }
    }
}

// ScriptRole: Handles the dwarf's behavior while sleeping to regenerate stamina.
// Dependencies: DwarfStateManager, GameEvents
// NeedsSetup: None. 