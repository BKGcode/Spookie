using UnityEngine;
using System;
using System.Collections.Generic;
using Dwarf.FSM;

public class MoveToExactPositionState : IState
{
    private readonly DwarfStateManager _stateManager;
    private readonly Vector3 _targetPosition;
    private readonly Action _onArrival;

    public MoveToExactPositionState(DwarfStateManager manager, Vector3 target, Action onArrivalCallback)
    {
        _stateManager = manager;
        _targetPosition = target;
        _onArrival = onArrivalCallback;
    }

    public void OnEnter()
    {
        Debug.Log($"Dwarf '{_stateManager.DwarfController.DwarfData.DwarfName}' is moving to occupy mined space at {_targetPosition}.");
        
        var directPath = new List<Vector3> { _targetPosition };
        _stateManager.DwarfMovement.FollowPath(directPath, HandleArrival, HandleFailure);
    }

    public void OnUpdate()
    {
        // Movement is handled by DwarfMovement.
    }

    public void OnExit()
    {
        _stateManager.DwarfMovement.Stop();
    }

    private void HandleArrival()
    {
        _onArrival?.Invoke();
    }
    
    private void HandleFailure()
    {
        Debug.LogWarning("MoveToExactPositionState failed. Transitioning to wait.");
        // If it fails for some reason, just go back to waiting to prevent getting stuck.
        _stateManager.ChangeState(new WaitingInCampState(_stateManager));
    }
}

// ScriptRole: Moves the dwarf to an exact adjacent (and now empty) tile.
// Dependencies: DwarfStateManager, DwarfMovement
// NeedsSetup: None. 