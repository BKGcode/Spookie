using UnityEngine;
using Dwarf.FSM;

public class SleepingState : IState
{
    private readonly DwarfStateManager _stateManager;
    private readonly float _staminaRegenRate = 10f; // Stamina per second

    public SleepingState(DwarfStateManager manager)
    {
        _stateManager = manager;
    }

    public void OnEnter()
    {
        Debug.Log($"Dwarf '{_stateManager.DwarfController.DwarfData.DwarfName}' is now sleeping.");
        // If there's a status property on DwarfData or a relevant component, update it here.
        // For example: _stateManager.DwarfController.Status = "Sleeping";
    }

    public void OnUpdate()
    {
        // Stamina logic might be moved to a dedicated DwarfStats component in the future.
        // For now, we'll leave it out of the state to keep it simple, assuming another system handles it.
    }

    public void OnExit()
    {
        Debug.Log($"Dwarf '{_stateManager.DwarfController.DwarfData.DwarfName}' has woken up.");
    }
}

// ScriptRole: Handles the dwarf's behavior while sleeping.
// Dependencies: DwarfStateManager
// NeedsSetup: None. 