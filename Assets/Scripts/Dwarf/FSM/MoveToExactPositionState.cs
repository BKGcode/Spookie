using UnityEngine;
using System;

public class MoveToExactPositionState : IState
{
    private readonly DwarfStateManager stateManager;
    private readonly Vector2Int targetPosition;
    private readonly Action onArrival;

    public MoveToExactPositionState(DwarfStateManager manager, Vector2Int target, Action onArrivalCallback)
    {
        stateManager = manager;
        targetPosition = target;
        onArrival = onArrivalCallback;
    }

    public void OnEnter()
    {
        // This state assumes the target position is now walkable.
        Debug.Log($"Dwarf '{stateManager.Controller.CurrentState.DwarfName}' is moving to occupy mined space at {targetPosition}.");
        
        // We don't use the full Pathfinding system here, as it's just one step.
        // We will create a simple, direct path.
        var directPath = new System.Collections.Generic.List<Vector2Int> { targetPosition };
        stateManager.Movement.OnPathCompleted += HandleArrival;
        stateManager.Movement.FollowPath(directPath);
    }

    public void OnUpdate()
    {
        // Movement is handled by DwarfMovement.
    }

    public void OnExit()
    {
        stateManager.Movement.OnPathCompleted -= HandleArrival;
        stateManager.Movement.Stop();
    }

    private void HandleArrival()
    {
        onArrival?.Invoke();
    }
}

// ScriptRole: Moves the dwarf to an exact adjacent (and now empty) tile.
// Dependencies: DwarfStateManager, DwarfMovement
// NeedsSetup: None. 