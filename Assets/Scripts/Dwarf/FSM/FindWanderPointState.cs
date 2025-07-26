using UnityEngine;
using System;

public class FindWanderPointState : IState
{
    private readonly DwarfStateManager stateManager;

    public FindWanderPointState(DwarfStateManager manager)
    {
        stateManager = manager;
    }

    public void OnEnter()
    {
        Debug.Log($"Dwarf '{stateManager.Controller.CurrentState.DwarfName}' is looking for a new spot to wander.");
        
        Vector2Int targetPosition = Pathfinding.Instance.mapGenerator.GetRandomCampPosition();
        
        Action onArrival = () => {
            stateManager.ChangeState(new WaitingInCampState(stateManager));
        };

        var pathfindingState = new PathfindingToTargetState(stateManager, targetPosition, onArrival, onArrival);
        stateManager.ChangeState(pathfindingState);
    }

    public void OnUpdate() { }
    public void OnExit() { }
}

// ScriptRole: Finds a random point in the camp and transitions to pathfinding.
// Dependencies: DwarfStateManager, MapGenerator
// NeedsSetup: None. 