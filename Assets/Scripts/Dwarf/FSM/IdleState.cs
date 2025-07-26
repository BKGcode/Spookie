using UnityEngine;
using System;

public class IdleState : IState
{
    private readonly DwarfStateManager stateManager;

    public IdleState(DwarfStateManager manager)
    {
        stateManager = manager;
    }

    public void OnEnter()
    {
        Debug.Log($"Dwarf '{stateManager.Controller.CurrentState.DwarfName}' is now Idle.");
        stateManager.Controller.CurrentState.CurrentStatus = "Idle";
        GameEvents.ReportDwarfIsIdle(stateManager.Controller.CurrentState);
        GameEvents.OnNightStart += GoToSleep;
    }

    public void OnUpdate()
    {
        // The dwarf will remain idle until a new command is given.
    }

    public void OnExit()
    {
        Debug.Log($"Dwarf '{stateManager.Controller.CurrentState.DwarfName}' is no longer Idle.");
        GameEvents.OnNightStart -= GoToSleep;
    }

    private void GoToSleep()
    {
        // Avoid multiple calls
        if (stateManager.CurrentState is IdleState)
        {
            Debug.Log("Night has fallen. Time to go to sleep.");
            Action onArrivalAtCamp = () => stateManager.ChangeState(new SleepingState(stateManager));
            var goHomeState = new PathfindingToTargetState(stateManager, MapGenerator.CampfirePosition, onArrivalAtCamp, onArrivalAtCamp);
            stateManager.ChangeState(goHomeState);
        }
    }
}

// ScriptRole: Represents the Idle state for a dwarf.
// Dependencies: DwarfStateManager, GameEvents
// NeedsSetup: None. 