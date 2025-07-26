using UnityEngine;
using System;
using System.Collections.Generic;

public class PathfindingToTargetState : IState
{
    private readonly DwarfStateManager stateManager;
    private readonly Vector2Int targetPosition;
    private readonly Action onPathCompleted;
    private readonly Action onPathFailed;

    public PathfindingToTargetState(DwarfStateManager manager, Vector2Int target, Action onPathCompleted, Action onPathFailed = null)
    {
        stateManager = manager;
        targetPosition = target;
        this.onPathCompleted = onPathCompleted;
        this.onPathFailed = onPathFailed;
    }

    public void OnEnter()
    {
        Debug.Log($"Dwarf '{stateManager.Controller.CurrentState.DwarfName}' is pathfinding to {targetPosition}.");
        stateManager.Controller.CurrentState.CurrentStatus = "Moving";
        stateManager.Movement.OnPathCompleted += HandlePathCompleted;

        Vector2Int startPosition = Pathfinding.Instance.WorldToGridPosition(stateManager.transform.position);
        List<Vector2Int> path = Pathfinding.Instance.FindPath(startPosition, targetPosition);

        if (path != null && path.Count > 0)
        {
            stateManager.Movement.FollowPath(path);
        }
        else
        {
            Debug.LogWarning($"Path to {targetPosition} could not be found.");
            stateManager.Movement.OnPathCompleted -= HandlePathCompleted;
            onPathFailed?.Invoke();
        }
    }

    public void OnUpdate()
    {
        // Movement is handled by DwarfMovement, so this state just waits.
    }

    public void OnExit()
    {
        stateManager.Movement.OnPathCompleted -= HandlePathCompleted;
        stateManager.Movement.Stop();
    }

    private void HandlePathCompleted()
    {
        Debug.Log($"Dwarf '{stateManager.Controller.CurrentState.DwarfName}' reached target.");
        onPathCompleted?.Invoke();
    }
}

// ScriptRole: A reusable state for moving a dwarf to a specific grid position.
// Dependencies: DwarfStateManager, Pathfinding
// NeedsSetup: None. 