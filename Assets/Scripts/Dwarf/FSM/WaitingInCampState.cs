using UnityEngine;
using System;

public class WaitingInCampState : IState
{
    private readonly DwarfStateManager stateManager;
    private float waitTimer;
    private readonly float waitDuration;

    public WaitingInCampState(DwarfStateManager manager)
    {
        stateManager = manager;
        waitDuration = UnityEngine.Random.Range(2f, 5f); // Wait for 2 to 5 seconds
    }

    public void OnEnter()
    {
        Debug.Log($"Dwarf '{stateManager.Controller.CurrentState.DwarfName}' is waiting in camp for {waitDuration:F1}s.");
        stateManager.Controller.CurrentState.CurrentStatus = "Wandering";
        GameEvents.OnNightStart += GoToSleep;
    }

    public void OnUpdate()
    {
        waitTimer += Time.deltaTime;
        if (waitTimer >= waitDuration)
        {
            stateManager.ChangeState(new FindWanderPointState(stateManager));
        }
    }

    public void OnExit()
    {
        GameEvents.OnNightStart -= GoToSleep;
    }

    private void GoToSleep()
    {
        if (stateManager.CurrentState is WaitingInCampState)
        {
            Debug.Log("Night has fallen. Time to go to sleep.");
            Action onArrivalAtCamp = () => stateManager.ChangeState(new SleepingState(stateManager));
            var goHomeState = new PathfindingToTargetState(stateManager, MapGenerator.CampfirePosition, onArrivalAtCamp, onArrivalAtCamp);
            stateManager.ChangeState(goHomeState);
        }
    }
}

// ScriptRole: Waits for a short duration in the camp before wandering again.
// Dependencies: DwarfStateManager
// NeedsSetup: None. 