using UnityEngine;
using Dwarf.FSM;

public class FindWanderPointState : IState
{
    private readonly DwarfStateManager _stateManager;
    private readonly MapGenerator _mapGenerator;
    private readonly Pathfinding _pathfinding;

    public FindWanderPointState(DwarfStateManager manager)
    {
        _stateManager = manager;
        _mapGenerator = manager.MapGenerator;
        _pathfinding = manager.Pathfinding;
    }

    public void OnEnter()
    {
        Debug.Log($"Dwarf '{_stateManager.DwarfController.DwarfData.DwarfName}' is looking for a new spot to wander.");
        
        Vector3Int targetCell = (Vector3Int)_mapGenerator.GetRandomCampPosition();
        Vector3 targetPosition = _mapGenerator.GetForegroundTilemap().GetCellCenterWorld(targetCell);

        var path = _pathfinding.FindPath(_stateManager.transform.position, targetPosition);

        if (path != null && path.Count > 0)
        {
            _stateManager.DwarfMovement.FollowPath(path, 
                () => _stateManager.ChangeState(new WaitingInCampState(_stateManager)), // OnSuccess
                () => _stateManager.ChangeState(new WaitingInCampState(_stateManager))  // OnFailure, just wait
            );
        }
        else
        {
            // If no path, just go back to waiting
            _stateManager.ChangeState(new WaitingInCampState(_stateManager));
        }
    }

    public void OnUpdate() { }
    public void OnExit() { }
}

// ScriptRole: Finds a random point in the camp and moves the dwarf there.
// Dependencies: DwarfStateManager, MapGenerator, Pathfinding
// NeedsSetup: None. 