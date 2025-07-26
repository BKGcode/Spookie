using UnityEngine;
using System;

// Note: This state is a placeholder for a more complex decision-making process.
// It currently does not find the "nearest" tile, but demonstrates the flow.
public class FindNearestTileState : IState
{
    private readonly DwarfStateManager stateManager;

    public FindNearestTileState(DwarfStateManager manager)
    {
        stateManager = manager;
    }

    public void OnEnter()
    {
        Debug.Log($"Dwarf '{stateManager.Controller.CurrentState.DwarfName}' is looking for a tile to mine.");

        // TODO: Implement a more robust search for the nearest, reachable tile.
        // For now, this demonstrates the state transition. We'll find a random tile instead.
        Vector2Int randomTarget = new Vector2Int(
            UnityEngine.Random.Range(0, Pathfinding.Instance.mapGenerator.MapWidth),
            UnityEngine.Random.Range(0, Pathfinding.Instance.mapGenerator.MapHeight)
        );

        Action onArrival = () => {
            stateManager.ChangeState(new MiningState(stateManager, randomTarget));
        };
        
        Action onFail = () => {
            // If path fails, try finding another tile.
            stateManager.ChangeState(new FindNearestTileState(stateManager));
        };

        var pathfindingState = new PathfindingToTargetState(stateManager, randomTarget, onArrival, onFail);
        stateManager.ChangeState(pathfindingState);
    }

    public void OnUpdate() { }

    public void OnExit() { }
}

// ScriptRole: Finds a nearby minable tile and initiates pathfinding towards it.
// Dependencies: DwarfStateManager, Pathfinding
// NeedsSetup: None. 